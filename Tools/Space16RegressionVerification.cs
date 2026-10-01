// Isolated Play Mode regressions after adding per-world gravity.
using System;
using System.IO;
using System.Text;
using Mutiny.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class Space16RegressionVerification
    {
        const string Pending = "Mutiny.Space16Regression";
        static Space16RegressionVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false)) Verify();
            };
        }
        public static void RunBatch()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }
        static void Verify()
        {
            SessionState.EraseBool(Pending);
            var report = new StringBuilder("Unity " + Application.unityVersion + " isolated Play Mode; 2026-10-01\n");
            bool success = true;
            try
            {
                foreach (string invalid in new[] { "0", "-1", "NaN", "Infinity", "oops" })
                {
                    string xml = UnityEngine.Resources.Load<TextAsset>("Data/Levels/level_1_16").text
                        .Replace("gravityScale=\"0.5\"", "gravityScale=\"" + invalid + "\"");
                    bool rejected = false;
                    try { MutinyLevelXmlParser.Parse(xml); } catch (FormatException) { rejected = true; }
                    if (!rejected) throw new Exception("Invalid gravity accepted: " + invalid);
                    report.AppendLine("PASS parser rejects gravityScale=" + invalid);
                }
                var entry = MutinyLevelEntryVerificationTest.Run();
                report.AppendLine("Level identity / GM: " + entry.PassedAssertions + "/" + entry.TotalAssertions);
                report.AppendLine(string.Join("\n", entry.Logs));
                report.AppendLine(string.Join("\n", entry.Failures));
                success &= entry.Passed;
                var dual = MutinyTwoPlayerVerificationTest.Run();
                report.AppendLine("Two player: " + dual.PassedAssertions + "/" + dual.TotalAssertions);
                report.AppendLine(string.Join("\n", dual.Logs));
                report.AppendLine(string.Join("\n", dual.Failures));
                success &= dual.Passed;
            }
            catch (Exception ex) { success = false; report.AppendLine("FAIL " + ex); }
            report.AppendLine(success ? "PASS all requested regressions." : "FAIL regressions.");
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "../space16-regression-verification.txt")), report.ToString());
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
