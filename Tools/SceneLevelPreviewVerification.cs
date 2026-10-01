// Copy into an isolated project's Assets/Editor and execute
// Mutiny.Verification.Editor.SceneLevelPreviewVerification.RunBatch.
using System;
using System.IO;
using System.Linq;
using System.Text;
using Mutiny.Levels;
using Mutiny.Levels.Editor;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class SceneLevelPreviewVerification
    {
        const string Key = "Mutiny.ScenePreview.Verify.";
        const string Folder = "Assets/ScenePreviewVerification";
        static int frames;
        static SceneLevelPreviewVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key + "Running", false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    frames = 0;
                    EditorApplication.update += Tick;
                }
                else if (state == PlayModeStateChange.EnteredEditMode)
                    EditorApplication.delayCall += Next;
            };
        }
        static string ReportPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../scene-level-preview-verification.txt"));
        static void Check(bool condition, string description)
        {
            if (!condition) throw new Exception(description);
            SessionState.SetInt(Key + "Checks", SessionState.GetInt(Key + "Checks", 0) + 1);
            File.AppendAllText(ReportPath, "PASS " + description + Environment.NewLine);
        }
        static T[] Active<T>() where T : Component =>
            Resources.FindObjectsOfTypeAll<T>().Where(c => c.gameObject.scene == SceneManager.GetActiveScene()).ToArray();
        static MutinyLevelController Preview => Active<MutinyLevelController>().Single(c => c.StartInGameplay);
        public static void RunBatch()
        {
            File.WriteAllText(ReportPath, "Unity " + Application.unityVersion + " isolated Editor + serialized scenes + natural Play Mode; 2026-10-01\n");
            SessionState.SetInt(Key + "Checks", 0);
            SessionState.SetInt(Key + "Stage", 0);
            SessionState.SetBool(Key + "Running", true);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            try
            {
                EditChecks();
                Prepare(0);
            }
            catch (Exception ex) { Fail(ex); }
        }
        static void EditChecks()
        {
            var active = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Check(MutinyLevelSceneMenu.GetAvailableLevels(MutinyGameMode.SinglePlayer).Count == 16 &&
                MutinyLevelSceneMenu.GetAvailableLevels(MutinyGameMode.LocalTwoPlayer).Count == 18,
                "EXT-SCENE-LVL-01 discovers 16 single / 18 dual unique authoring+runtime maps");
            foreach (var mode in new[] { MutinyGameMode.SinglePlayer, MutinyGameMode.LocalTwoPlayer })
                foreach (var id in MutinyLevelSceneMenu.GetAvailableLevels(mode))
                {
                    Check(MutinyLevelSceneMenu.TryLoadLevelToScene(id, out string error), "Production editor load " + id + ": " + error);
                    Check(Preview.CurrentLevelId.Equals(id) && Preview.ActiveGameMode == mode &&
                        Preview.CurrentLevel.Players == MutinyLevelXmlParser.Parse(Preview.LevelXml.text).Players &&
                        Active<MutinyLevelRoot>().Length == 1 &&
                        Preview.CurrentLevel.TerrainHolder.childCount > 0 && Preview.CurrentLevel.Characters.Count > 0 &&
                        Preview.CurrentLevel.Team2.IsAiControlled == (mode == MutinyGameMode.SinglePlayer),
                        "EXT-SCENE-LVL-01/02 exact mode, index, tiles, crews and single replacement " + id);
                }
            MutinyLevelRoot prior = Preview.CurrentLevel;
            Check(!MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 0), out _) &&
                !MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 997), out _) &&
                Preview.CurrentLevel == prior, "Invalid/missing targets preserve prior level");
            string badPath = "Assets/Mutiny/Data/Levels/level_1_998.xml";
            string mismatchPath = "Assets/Mutiny/Data/Levels/level_2_999.xml";
            try
            {
                File.WriteAllText(badPath, "<broken");
                File.WriteAllText(mismatchPath, MutinyLevelSceneMenu.FindLevelXml(new MutinyLevelId(MutinyGameMode.SinglePlayer, 1)).text);
                AssetDatabase.ImportAsset(badPath, ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.ImportAsset(mismatchPath, ImportAssetOptions.ForceSynchronousImport);
                Check(!MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 998), out _) &&
                    Preview.CurrentLevel == prior, "Malformed XML preserves prior level");
                Check(MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.LocalTwoPlayer, 999), out _) &&
                    Preview.CurrentLevelId.Mode == MutinyGameMode.LocalTwoPlayer && !Preview.CurrentLevel.Team2.IsAiControlled,
                    "Future dual999 with original players=1 still uses selected dual mode");
                Check(MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.LocalTwoPlayer, 18), out _),
                    "Restore dual18 after future-map fixture");
            }
            finally { AssetDatabase.DeleteAsset(badPath); AssetDatabase.DeleteAsset(mismatchPath); }
            Check(Camera.main != null && Camera.main.orthographic, "EXT-SCENE-LVL-04 missing camera created and configured");
            var camera = Camera.main;
            var cameraPosition = camera.transform.position;
            Check(MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16), out _),
                "Replace dual18 with single16");
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Check(Preview.CurrentLevelId.Equals(new MutinyLevelId(MutinyGameMode.LocalTwoPlayer, 18)) &&
                Active<MutinyLevelRoot>().Length == 1 && Camera.main == camera && camera.transform.position == cameraPosition,
                "EXT-SCENE-LVL-02 load Undo restores prior mode/level and preserves camera");
            Undo.PerformRedo();
            Check(Preview.CurrentLevelId.Equals(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16)) &&
                Preview.CurrentLevel.Characters.Count == 21 && Active<MutinyLevelRoot>().Length == 1,
                "Load Redo restores complete single16");
            Preview.gameObject.SetActive(false);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "ScenePreviewVerification");
            EditorSceneManager.SaveScene(active, Folder + "/EditChecks.unity");
            var otherScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var otherHost = new GameObject("UnrelatedOtherSceneLevel");
            SceneManager.MoveGameObjectToScene(otherHost, otherScene);
            var otherRoot = otherHost.AddComponent<MutinyLevelRoot>();
            SceneManager.SetActiveScene(active);
            var customHost = new GameObject("CustomHost");
            var sentinel = new GameObject("UnrelatedCustomChild");
            sentinel.transform.SetParent(customHost.transform);
            var custom = customHost.AddComponent<MutinyLevelController>();
            custom.LevelXml = MutinyLevelSceneMenu.FindLevelXml(new MutinyLevelId(MutinyGameMode.SinglePlayer, 1));
            MutinyLevelSceneMenu.ClearLoadedLevel();
            Check(Active<MutinyLevelRoot>().Length == 0 && otherRoot != null && sentinel != null && custom != null &&
                custom.LevelXml == null, "EXT-SCENE-LVL-02 clear includes inactive roots, preserves other scene and custom host");
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Check(Active<MutinyLevelRoot>().Length == 1 && Preview.CurrentLevel.Characters.Count == 21 &&
                !Preview.gameObject.activeSelf && otherRoot != null && custom.LevelXml != null,
                "Clear Undo restores inactive level and custom assignment");
            Preview.gameObject.SetActive(true);
            EditorSceneManager.CloseScene(otherScene, true);
            MutinyLevelSceneMenu.ClearLoadedLevel();
            Check(Active<MutinyLevelRoot>().Length == 0 && Active<MutinyLevelController>().All(c => c.LevelXml == null),
                "Clear leaves no root or automatic level rebuild assignment");
            Undo.ClearAll();
        }
        static void Prepare(int stage)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "ScenePreviewVerification");
            string path = Folder + (stage == 0 ? "/SpacePreview.unity" : stage == 1 ? "/DualPreview.unity" : "/Main.unity");
            if (stage < 2)
            {
                var id = new MutinyLevelId(stage == 0 ? MutinyGameMode.SinglePlayer : MutinyGameMode.LocalTwoPlayer,
                    stage == 0 ? 16 : 18);
                Check(MutinyLevelSceneMenu.TryLoadLevelToScene(id, out string error), "Prepare saved scene " + id + ": " + error);
            }
            else
            {
                var cameraHost = new GameObject("Main Camera");
                cameraHost.tag = "MainCamera";
                cameraHost.AddComponent<Camera>().orthographic = true;
                var host = new GameObject("MutinyGame");
                host.AddComponent<MutinyLevelController>().LevelXml =
                    MutinyLevelSceneMenu.FindLevelXml(new MutinyLevelId(MutinyGameMode.SinglePlayer, 1));
            }
            Check(EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path), "Save preview fixture");
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            if (stage < 2)
                Check(Preview.StartInGameplay && Preview.LevelXml != null && Preview.CurrentLevel != null,
                    "EXT-SCENE-LVL-03 scene serialization preserves preview flag, XML and baked root");
            SessionState.SetInt(Key + "Stage", stage);
            EditorApplication.EnterPlaymode();
        }
        static void Tick()
        {
            if (++frames < 20) return;
            EditorApplication.update -= Tick;
            try
            {
                int stage = SessionState.GetInt(Key + "Stage", 0);
                var front = Object.FindAnyObjectByType<MutinyFrontendController>();
                Check(front != null, "EXT-SCENE-LVL-04 frontend bootstrap in " + SceneManager.GetActiveScene().name);
                if (stage < 2)
                {
                    var controller = Preview;
                    var root = controller.CurrentLevel;
                    var turns = root.GetComponent<MutinyTurnManager>();
                    Check(front.CurrentPage == MutinyFrontendPage.Gameplay && root.gameObject.activeInHierarchy &&
                        controller.CurrentLevelIndex == (stage == 0 ? 16 : 18) &&
                        controller.ActiveGameMode == (stage == 0 ? MutinyGameMode.SinglePlayer : MutinyGameMode.LocalTwoPlayer),
                        "EXT-SCENE-LVL-03 Play direct gameplay preserves serialized mode/index");
                    Check(turns.CurrentPhase == TurnPhase.TurnActive && turns.CurrentTeam != null &&
                        root.Characters.All(c => c.IsAlive && c.PhysicsBody != null && c.PhysicsBody.TryGetTerrain(out _, out _, out _)),
                        "Natural Start/Update produces a live turn and valid character physics");
                    Check(root.Team2.IsAiControlled == (stage == 0) &&
                        (stage != 0 || (root.Team1.Characters.Count == 10 && root.Team2.Characters.Count == 11)) &&
                        (stage != 1 || (!root.Team1.IsAiControlled && !root.Team2.IsAiControlled)),
                        "Serialized preview crews and single AI / dual human controls");
                    if (stage == 0)
                    {
                        Level16SpaceSettingsVerification.Verify(controller, Check);
                        root = controller.CurrentLevel;
                    }
                    Check(!MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 1), out _) &&
                        controller.CurrentLevel == root, "EXT-SCENE-LVL-04 editor load rejects Play Mode without mutation");
                    MutinyLevelSceneMenu.ClearLoadedLevel();
                    Check(controller.CurrentLevel == root && root.gameObject.activeInHierarchy,
                        "Editor clear rejects Play Mode without mutation");
                    controller.RestartCurrentLevel();
                    Check(controller.CurrentLevel != root && controller.CurrentLevelIndex == (stage == 0 ? 16 : 18) &&
                        controller.CurrentLevel.Team2.IsAiControlled == (stage == 0) &&
                        front.CurrentPage == MutinyFrontendPage.Gameplay,
                        "Production restart retains scene preview mode and gameplay");
                }
                else
                    Check(front.CurrentPage == MutinyFrontendPage.Title &&
                        !Object.FindAnyObjectByType<MutinyLevelController>().StartInGameplay,
                        "EXT-SCENE-LVL-04 ordinary Main still starts at title");
                EditorApplication.ExitPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        static void Next()
        {
            if (!SessionState.GetBool(Key + "Running", false)) return;
            try
            {
                int stage = SessionState.GetInt(Key + "Stage", 0);
                if (stage < 2)
                    Check(Preview.CurrentLevel != null && Preview.CurrentLevel.gameObject.activeSelf,
                        "Exit Play restores baked editor preview");
                if (stage < 2) { Prepare(stage + 1); return; }
                File.AppendAllText(ReportPath, "PASS " + SessionState.GetInt(Key + "Checks", 0) + " assertions.\n" +
                    "Scope: production editor load/clear/Undo, all current maps, serialized scene roundtrip and natural Play bootstrap. No mouse UI, visible graphics or full-match claim.\n");
                SessionState.SetBool(Key + "Running", false);
                EditorApplication.Exit(0);
            }
            catch (Exception ex) { Fail(ex); }
        }
        static void Fail(Exception ex)
        {
            SessionState.SetBool(Key + "Running", false);
            File.AppendAllText(ReportPath, "FAIL " + ex + "\n");
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }
}
