using System;
using System.IO;
using Mutiny.Levels;
using Mutiny.Levels.Editor;
using Mutiny.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class RobotPortraitVerification
    {
        const string Key = "Mutiny.RobotPortraitVerification";
        static string Report => Path.GetFullPath(Path.Combine(Application.dataPath, "../robot-portrait-verification.txt"));
        static double start;
        static RobotPortraitVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                { start = EditorApplication.timeSinceStartup; EditorApplication.update += Tick; }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key, false);
                    File.AppendAllText(Report, "PASS all " + SessionState.GetInt(Key + "Checks", 0) + " assertions.\n");
                    EditorApplication.Exit(0);
                }
            };
        }
        static void Check(bool success, string message)
        {
            if (!success) throw new Exception(message);
            SessionState.SetInt(Key + "Checks", SessionState.GetInt(Key + "Checks", 0) + 1);
            File.AppendAllText(Report, "PASS " + message + "\n");
        }
        public static void RunBatch()
        {
            File.WriteAllText(Report, "Unity " + Application.unityVersion + "; actual production GM -> level build -> HUD portrait resolver; 2026-10-01\n");
            SessionState.SetBool(Key, true); SessionState.SetInt(Key + "Checks", 0);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            try
            {
                if (!AssetDatabase.IsValidFolder("Assets/RobotPortraitVerification"))
                    AssetDatabase.CreateFolder("Assets", "RobotPortraitVerification");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, "Assets/RobotPortraitVerification/Main.unity");
                Check(MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16), out string error), "Editor production load: " + error);
                EditorSceneManager.SaveScene(scene);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        static void Tick()
        {
            if (EditorApplication.timeSinceStartup - start < 0.5) return;
            EditorApplication.update -= Tick;
            try
            {
                var gm = MutinyGMManager.Instance ?? new GameObject("GM").AddComponent<MutinyGMManager>();
                var commands = new[] { "16", "1", "6", "15", "2_1", "2_16", "16" };
                foreach (string command in commands)
                {
                    Check(gm.ExecuteCommand("enterlevel " + command), "Production GM enters " + command);
                    var controller = Object.FindAnyObjectByType<MutinyLevelController>();
                    var hud = controller.CurrentLevel.GetComponent<MutinyGameHUD>();
                    bool robot = command == "16";
                    var expected = Resources.Load<Texture2D>(robot ? "Art/Characters/Preview/Robot" :
                        "UI/BattleHUD/Opponents/" + controller.OriginalLevelIndex);
                    Check(expected != null && hud.ResolveOpponentPortrait() == expected,
                        "Actual HUD uses " + (robot ? "silver Robot" : "original portrait " + controller.OriginalLevelIndex));
                    if (robot) Check(expected.filterMode == FilterMode.Point && expected.width == 32 && expected.height == 36,
                        "Existing Robot pixel art retained without distortion or replacement art");
                }
                EditorApplication.ExitPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        static void Fail(Exception ex)
        {
            File.AppendAllText(Report, "FAIL " + ex + "\n");
            SessionState.SetBool(Key, false); Debug.LogException(ex); EditorApplication.Exit(1);
        }
    }
}
