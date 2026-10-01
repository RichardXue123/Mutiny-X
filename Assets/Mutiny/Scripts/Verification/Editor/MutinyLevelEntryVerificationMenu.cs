using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class MutinyLevelEntryVerificationMenu
    {
        private const string Pending = "Mutiny.LevelEntry.Pending";
        private const string Batch = "Mutiny.LevelEntry.Batch";

        static MutinyLevelEntryVerificationMenu()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
                    Verify();
            };
        }

        [MenuItem("Mutiny/Parity/Validate Level Identity and GM Entry Play Mode")]
        public static void Run()
        {
            SessionState.SetBool(Pending, true);
            SessionState.SetBool(Batch, false);
            if (EditorApplication.isPlaying) EditorApplication.delayCall += Verify;
            else EditorApplication.EnterPlaymode();
        }

        // Run only against an isolated project, never the user's open main scene.
        public static void RunBatch()
        {
            Debug.Log("[LEVEL-ENTRY] Starting isolated Play Mode verification.");
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Pending, true);
            SessionState.SetBool(Batch, true);
            EditorApplication.EnterPlaymode();
        }

        private static void Verify()
        {
            if (!SessionState.GetBool(Pending, false)) return;
            SessionState.EraseBool(Pending);
            if (UnityEngine.Object.FindAnyObjectByType<Mutiny.Levels.MutinyLevelController>() != null ||
                UnityEngine.Object.FindAnyObjectByType<Mutiny.Presentation.MutinyFrontendController>() != null)
            {
                Debug.LogError("[LEVEL-ENTRY] Requires an empty scene; no live board was changed.");
                if (SessionState.GetBool(Batch, false)) EditorApplication.Exit(1);
                else EditorApplication.ExitPlaymode();
                return;
            }
            var reports = new System.Text.StringBuilder("Unity " + Application.unityVersion + " Play Mode\n");
            bool passed = true;
            try
            {
                (string, Func<MutinyLevel1VerificationResult>)[] checks = {
                    ("RESOURCE-SCAN", RunResourceScan),
                    ("LEVEL-ENTRY", MutinyLevelEntryVerificationTest.Run),
                    ("GM", MutinyTurnActionUiVerificationTest.RunGM),
                    ("TWO-PLAYER", MutinyTwoPlayerVerificationTest.Run),
                    ("CAMERA-INIT", MutinyTurnActionUiVerificationTest.RunCameraInitialization),
                    ("ENDING", MutinyTurnActionUiVerificationTest.RunEndingSequence) };
                foreach (var pair in checks)
                {
                    MutinyLevel1VerificationResult result = pair.Item2();
                    string summary = $"[{pair.Item1}] {(result.Passed ? "PASS" : "FAIL")} " +
                        $"{result.PassedAssertions}/{result.TotalAssertions}";
                    Debug.Log(summary);
                    reports.AppendLine(summary);
                    reports.AppendLine(string.Join("\n", result.Logs));
                    reports.AppendLine(string.Join("\n", result.Failures));
                    passed &= result.Passed;
                }
            }
            catch (Exception exception)
            {
                passed = false;
                Debug.LogException(exception);
                reports.AppendLine(exception.ToString());
            }
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "../level-entry-verification.txt")),
                reports.ToString());
            if (SessionState.GetBool(Batch, false)) EditorApplication.Exit(passed ? 0 : 1);
            else EditorApplication.ExitPlaymode();
        }

        private static MutinyLevel1VerificationResult RunResourceScan()
        {
            var result = new MutinyLevel1VerificationResult();
            string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../runtime-level-scan"));
            Mutiny.Levels.MutinyLevelScanResult scan = Mutiny.Levels.MutinyLevelScanner.Scan(
                Path.Combine(Application.dataPath, "Mutiny/Data/Levels"), output);
            result.Assert(scan.Files == 34 && scan.Parsed == 34 && scan.Errors == 0 &&
                File.ReadAllText(Path.Combine(output, "README.md")).Contains("NOT APPLICABLE"),
                "EXT-LVL-ID-01 runtime scanner accepts all 34 mode-scoped files without replacing the original evidence baseline");
            return result;
        }
    }
}
