// Isolated Editor runner: real scene load/clear, save/reopen and Play lifecycle.
using System;
using System.IO;
using System.Linq;
using Mutiny.Levels;
using Mutiny.Levels.Editor;
using Mutiny.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class MainSceneStartupVerification
    {
        const string Key = "Mutiny.MainStartup.";
        const string Folder = "Assets/MainStartupVerification";
        static int frames;
        static string ReportPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../main-startup-verification.txt"));
        static MainSceneStartupVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key + "Running", false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    frames = 0; EditorApplication.update += Tick;
                }
                if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += Next;
            };
        }
        static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            SessionState.SetInt(Key + "Checks", SessionState.GetInt(Key + "Checks", 0) + 1);
            File.AppendAllText(ReportPath, "PASS " + message + "\n");
        }
        public static void RunBatch()
        {
            File.WriteAllText(ReportPath, "Unity " + Application.unityVersion + " isolated batchmode/nographics saved scenes and natural Play; 2026-10-01\n");
            SessionState.SetBool(Key + "Running", true);
            SessionState.SetInt(Key + "Checks", 0);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            try { Prepare(0); } catch (Exception ex) { Fail(ex); }
        }
        static void Prepare(int stage)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "MainStartupVerification");
            string path = Folder + (stage == 3 ? "/DedicatedPreview.unity" : "/Main.unity");
            // Assign the scene name before the editor load tool determines its startup policy.
            EditorSceneManager.SaveScene(scene, path);
            var id = new MutinyLevelId(stage == 1 ? MutinyGameMode.LocalTwoPlayer : MutinyGameMode.SinglePlayer,
                stage == 1 ? 18 : 16);
            Check(MutinyLevelSceneMenu.TryLoadLevelToScene(id, out string error), "Production load fixture stage=" + stage + " " + error);
            var controller = Object.FindAnyObjectByType<MutinyLevelController>();
            Check(controller.StartInGameplay == (stage == 3), "Main load does not enable auto-gameplay; dedicated scene does");
            if (stage == 0)
            {
                // Model the actual persisted setting created by the previous tool,
                // not a fake gameplay state. Save/reopen then use real bootstrap.
                controller.StartInGameplay = true;
                EditorUtility.SetDirty(controller);
            }
            if (stage == 2)
            {
                MutinyLevelSceneMenu.ClearLoadedLevel();
                Check(Object.FindAnyObjectByType<MutinyLevelController>() == null &&
                    Object.FindAnyObjectByType<MutinyLevelRoot>() == null, "Actual Clear removes level/controller before Main Play");
            }
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.OpenScene(path);
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
                var frontend = Object.FindAnyObjectByType<MutinyFrontendController>();
                var controller = Object.FindAnyObjectByType<MutinyLevelController>();
                Check(frontend != null && controller != null, "Runtime frontend and session controller exist stage=" + stage);
                var audio = MutinyAudioManager.Instance;
                if (stage < 3)
                {
                    Check(frontend.CurrentPage == MutinyFrontendPage.Title &&
                        controller.CurrentLevel != null && !controller.CurrentLevel.gameObject.activeInHierarchy,
                        "FIX-MAIN-START-01/02 Main shows Title and hides battle stage=" + stage);
                    Check(audio.MusicSource.clip != null && audio.MusicSource.clip.name == "menu_music",
                        "Main selects menu_music stage=" + stage);
                    if (stage == 0)
                        Check(controller.StartInGameplay && controller.CurrentLevelIndex == 16,
                            "Old serialized auto-preview=true cannot bypass Main title");
                    if (stage == 1)
                        Check(controller.CurrentLevelId.Equals(new MutinyLevelId(MutinyGameMode.LocalTwoPlayer, 18)),
                            "Main baked dual18 retains identity behind title");
                    var gm = MutinyGMManager.Instance;
                    if (gm == null) gm = new GameObject("MainStartup_GM").AddComponent<MutinyGMManager>();
                    Check(gm.ExecuteCommand("enterlevel 16") && frontend.CurrentPage == MutinyFrontendPage.Gameplay &&
                        controller.CurrentLevel.gameObject.activeInHierarchy && controller.CurrentLevel.GravityScale == 0.5f &&
                        controller.CurrentLevel.Team1.Characters.Count == 10 &&
                        controller.CurrentLevel.Team2.Characters.Count == 11 &&
                        controller.CurrentLevel.Team2.Characters.All(c => c.Luck == 50f),
                        "FIX-MAIN-START-03 real GM leaves title for single16 with intended crews/settings");
                    bool originallyEnabled = audio.SfxEnabled;
                    string actual = null;
                    Action<string> listener = clip => actual = clip;
                    audio.SfxPlayed += listener;
                    try
                    {
                        if (!audio.SfxEnabled) audio.ToggleSFX();
                        controller.CurrentLevel.Team2.SelectCharacter(controller.CurrentLevel.Team2.Characters[0]);
                        Check(actual == "daftpunk_01", "Main menu -> GM16 -> actual robot selection starts voice01");
                    }
                    finally
                    {
                        audio.SfxPlayed -= listener;
                        if (audio.SfxEnabled != originallyEnabled) audio.ToggleSFX();
                    }
                    Check(gm.ExecuteCommand("enterlevel 2_18") && frontend.CurrentPage == MutinyFrontendPage.Gameplay &&
                        controller.ActiveGameMode == MutinyGameMode.LocalTwoPlayer &&
                        !controller.CurrentLevel.Team1.IsAiControlled && !controller.CurrentLevel.Team2.IsAiControlled,
                        "GM dual18 after Main title remains two-human gameplay");
                }
                else
                {
                    Check(frontend.CurrentPage == MutinyFrontendPage.Gameplay &&
                        controller.CurrentLevelIndex == 16 && controller.CurrentLevel.gameObject.activeInHierarchy,
                        "FIX-MAIN-START-03 dedicated preview retains direct gameplay");
                    Check(audio.MusicSource.clip.name == "game_music", "Dedicated preview selects game_music");
                }
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
                if (stage == 2)
                    Check(Object.FindAnyObjectByType<MutinyLevelController>() == null &&
                        Object.FindAnyObjectByType<MutinyLevelRoot>() == null,
                        "Exit Play leaves cleared editor Main empty; runtime fallback did not alter authoring scene");
                if (stage < 3) { Prepare(stage + 1); return; }
                File.AppendAllText(ReportPath, "PASS " + SessionState.GetInt(Key + "Checks", 0) + " assertions.\n" +
                    "Scope: production editor load/clear, serialized old preview flag, natural Play, frontend page/music/battle eligibility and real GM. No mouse rendering claim.\n");
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
