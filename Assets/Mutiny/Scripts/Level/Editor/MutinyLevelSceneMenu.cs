using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mutiny.Levels.Editor
{
    public static class MutinyLevelSceneMenu
    {
        public static bool CanEditScene => !EditorApplication.isPlayingOrWillChangePlaymode;

        [MenuItem("Mutiny/Levels/Load Level to Active Scene...")]
        public static void OpenLevelWindow() => MutinyLevelSceneWindow.Open();

        [MenuItem("Mutiny/Levels/Load Level 1 to Active Scene")]
        public static void LoadLevel1ToScene()
        {
            if (!TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 1), out string error))
                Debug.LogError("[MutinyLevelSceneMenu] " + error);
        }

        [MenuItem("Mutiny/Levels/Load Level 1 to Active Scene", true)]
        [MenuItem("Mutiny/Levels/Clear Loaded Level", true)]
        private static bool ValidateEditScene() => CanEditScene;

        public static TextAsset FindLevelXml(MutinyLevelId id)
        {
            if (!id.IsValid) return null;
            return AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/Mutiny/Data/Levels/{id.AssetName}.xml") ??
                AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/Mutiny/Resources/Data/Levels/{id.AssetName}.xml");
        }

        public static List<MutinyLevelId> GetAvailableLevels(MutinyGameMode mode)
        {
            var ids = new HashSet<MutinyLevelId>();
            foreach (string guid in AssetDatabase.FindAssets("t:TextAsset", new[]
                { "Assets/Mutiny/Data/Levels", "Assets/Mutiny/Resources/Data/Levels" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) continue;
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name.StartsWith("level_", StringComparison.OrdinalIgnoreCase) &&
                    MutinyLevelId.TryParse(name, out MutinyLevelId id) && id.Mode == mode) ids.Add(id);
            }
            return ids.OrderBy(id => id.Number).ToList();
        }

        private static T[] InScene<T>(Scene scene) where T : Component =>
            Resources.FindObjectsOfTypeAll<T>().Where(c => c.gameObject.scene == scene &&
                !EditorUtility.IsPersistent(c)).ToArray();

        public static bool TryLoadLevelToScene(MutinyLevelId id, out string error)
        {
            error = null;
            if (!CanEditScene) { error = "Stop Play Mode before editing scene levels."; return false; }
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) { error = "No active loaded scene."; return false; }
            TextAsset xml = FindLevelXml(id);
            if (xml == null) { error = $"Level XML not found: {id.AssetName}.xml"; return false; }
            MutinyLevelData data;
            try
            {
                data = MutinyLevelXmlParser.Parse(xml.text, xml.name);
                // Original dual maps may also contain players=1. Mode comes
                // from the selected mode-scoped identity, not this legacy field.
            }
            catch (Exception exception) { error = exception.Message; return false; }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            string action = "Load " + id.AssetName;
            Undo.SetCurrentGroupName(action);
            GameObject host = null;
            try
            {
                // Build the replacement before retiring the old scene content.
                host = new GameObject("MutinyGame");
                SceneManager.MoveGameObjectToScene(host, scene);
                var controller = host.AddComponent<MutinyLevelController>();
                controller.StartInGameplay = scene.name != "Main";
                controller.ConfigureSession(id.Mode);
                controller.CurrentLevelIndex = id.Number;
                controller.LevelXml = xml;
                controller.BuildLevel();
                if (controller.CurrentLevel == null)
                    throw new InvalidOperationException("The level builder did not produce a level.");
                controller.CurrentLevel.gameObject.name = id.AssetName;
                Undo.RegisterCreatedObjectUndo(host, action);
                ClearSceneLevels(scene, host);

                Camera camera = InScene<Camera>(scene).FirstOrDefault(c => c.CompareTag("MainCamera"));
                if (camera == null)
                {
                    var cameraHost = new GameObject("Main Camera");
                    SceneManager.MoveGameObjectToScene(cameraHost, scene);
                    cameraHost.tag = "MainCamera";
                    camera = cameraHost.AddComponent<Camera>();
                    Undo.RegisterCreatedObjectUndo(cameraHost, action);
                }
                Undo.RecordObject(camera, action);
                Undo.RecordObject(camera.transform, action);
                camera.orthographic = true;
                camera.orthographicSize = 6.25f;
                camera.transform.position = new Vector3(8.6f, -6.25f, -10f);
                camera.backgroundColor = new Color(0.38f, 0.65f, 0.88f);

                Selection.activeGameObject = host;
                if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
                EditorSceneManager.MarkSceneDirty(scene);
                Undo.CollapseUndoOperations(undoGroup);
                Debug.Log($"[MutinyLevelSceneMenu] Loaded {id}: {data.Width}x{data.Height}, " +
                    $"{controller.CurrentLevel.Team1.Characters.Count} vs {controller.CurrentLevel.Team2.Characters.Count}. " +
                    (scene.name == "Main" ? $"Play starts at the menu; GM: enterlevel {id.AssetName}" : "Press Play to preview."));
                return true;
            }
            catch (Exception exception)
            {
                // Restore the grouped old level and camera if replacement fails.
                Undo.RevertAllDownToGroup(undoGroup);
                if (host != null) UnityEngine.Object.DestroyImmediate(host);
                error = exception.Message;
                return false;
            }
        }

        [MenuItem("Mutiny/Levels/Clear Loaded Level")]
        public static void ClearLoadedLevel()
        {
            if (!CanEditScene) return;
            Scene scene = SceneManager.GetActiveScene();
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Clear Loaded Level");
            ClearSceneLevels(scene, null);
            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void ClearSceneLevels(Scene scene, GameObject keep)
        {
            foreach (MutinyLevelController controller in InScene<MutinyLevelController>(scene))
            {
                if (controller.gameObject == keep) continue;
                // Preserve unrelated components and children on custom hosts.
                bool toolHost = controller.gameObject.name == "MutinyGame" &&
                    controller.GetComponents<Component>().All(c => c is Transform || c is MutinyLevelController) &&
                    controller.transform.Cast<Transform>().All(t => t.GetComponent<MutinyLevelRoot>() != null);
                if (toolHost)
                {
                    Undo.DestroyObjectImmediate(controller.gameObject);
                    continue;
                }
                Undo.RecordObject(controller, "Clear Level Assignment");
                var serialized = new SerializedObject(controller);
                serialized.FindProperty("m_CurrentLevel").objectReferenceValue = null;
                serialized.FindProperty("m_LevelXml").objectReferenceValue = null;
                serialized.FindProperty("m_StartInGameplay").boolValue = false;
                serialized.ApplyModifiedProperties();
            }
            foreach (MutinyLevelRoot root in InScene<MutinyLevelRoot>(scene))
            {
                if (keep != null && root.transform.IsChildOf(keep.transform)) continue;
                // Legacy tools attached the controller directly to the root.
                if (root.GetComponent<MutinyLevelController>() != null)
                {
                    foreach (Transform child in root.transform.Cast<Transform>().ToArray())
                        Undo.DestroyObjectImmediate(child.gameObject);
                    Undo.DestroyObjectImmediate(root);
                }
                else Undo.DestroyObjectImmediate(root.gameObject);
            }
        }
    }

    public sealed class MutinyLevelSceneWindow : EditorWindow
    {
        private const string Pref = "Mutiny.SceneLevel.";
        private MutinyGameMode mode;
        private int number = 1;
        private List<MutinyLevelId> available = new List<MutinyLevelId>();
        private string message;
        private bool failed;

        public static void Open()
        {
            var window = GetWindow<MutinyLevelSceneWindow>("Load Mutiny Level");
            window.minSize = new Vector2(360f, 290f);
            window.Show();
        }
        private void OnEnable()
        {
            mode = EditorPrefs.GetInt(Pref + "Mode", 1) == 2 ? MutinyGameMode.LocalTwoPlayer : MutinyGameMode.SinglePlayer;
            number = Mathf.Max(1, EditorPrefs.GetInt(Pref + "Number", 1));
            Refresh();
        }
        private void OnFocus() => Refresh();
        private void Refresh() => available = MutinyLevelSceneMenu.GetAvailableLevels(mode);
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Active Scene", SceneManager.GetActiveScene().name);
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            mode = EditorGUILayout.Popup("Mode", mode == MutinyGameMode.SinglePlayer ? 0 : 1,
                new[] { "Single Player / 单人", "Local Two Player / 双人" }) == 0
                ? MutinyGameMode.SinglePlayer : MutinyGameMode.LocalTwoPlayer;
            number = EditorGUILayout.IntField("Level Number / 关卡编号", number);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(Pref + "Mode", mode == MutinyGameMode.SinglePlayer ? 1 : 2);
                EditorPrefs.SetInt(Pref + "Number", number);
                Refresh(); message = null;
            }
            if (available.Count > 0)
            {
                int current = available.FindIndex(id => id.Number == number);
                var labels = new[] { "Choose existing XML..." }.Concat(available.Select(id => id.AssetName)).ToArray();
                int chosen = EditorGUILayout.Popup("Existing Levels", current + 1, labels) - 1;
                if (chosen >= 0 && chosen != current)
                {
                    number = available[chosen].Number;
                    EditorPrefs.SetInt(Pref + "Number", number); message = null;
                }
            }
            var target = new MutinyLevelId(mode, number);
            TextAsset xml = MutinyLevelSceneMenu.FindLevelXml(target);
            EditorGUILayout.LabelField("XML", target.AssetName + ".xml");
            EditorGUILayout.LabelField("Available", available.Count + " maps in this mode");
            EditorGUILayout.HelpBox(SceneManager.GetActiveScene().name == "Main"
                ? "Main按Play进入主菜单；临时关可在GM输入 enterlevel " + target.AssetName + "。清理只影响活动场景，可用Undo撤销。"
                : "独立预览Scene加载后按Play直接进入关卡。双人编号为模式内编号。清理只影响活动场景，可用Undo撤销。", MessageType.Info);
            using (new EditorGUI.DisabledScope(!MutinyLevelSceneMenu.CanEditScene))
            {
                using (new EditorGUI.DisabledScope(xml == null))
                    if (GUILayout.Button("Load to Active Scene"))
                    {
                        failed = !MutinyLevelSceneMenu.TryLoadLevelToScene(target, out message);
                        if (!failed) message = "Loaded " + target + (SceneManager.GetActiveScene().name == "Main"
                            ? ". Play starts at the main menu." : ". Press Play to preview.");
                    }
                if (GUILayout.Button("Clear Loaded Level"))
                {
                    MutinyLevelSceneMenu.ClearLoadedLevel(); failed = false; message = "Active scene level cleared.";
                }
            }
            if (GUILayout.Button("Refresh XML List")) Refresh();
            if (!MutinyLevelSceneMenu.CanEditScene)
                EditorGUILayout.HelpBox("Stop Play Mode to load or clear scene levels.", MessageType.Info);
            else if (xml == null)
                EditorGUILayout.HelpBox("XML not found for this mode and number.", MessageType.Warning);
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, failed ? MessageType.Error : MessageType.Info);
        }
    }
}
