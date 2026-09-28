using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class MutinyAiResponsiveVerificationMenu
    {
        private const string Pending = "Mutiny.AiResponsive.Pending", Batch = "Mutiny.AiResponsive.Batch";
        static MutinyAiResponsiveVerificationMenu()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
                    EditorApplication.delayCall += Verify;
            };
        }

        [MenuItem("Mutiny/Parity/Validate AI Responsive Search Play Mode")]
        public static void Run()
        {
            SessionState.SetBool(Pending, true);
            if (EditorApplication.isPlaying) EditorApplication.delayCall += Verify;
            else EditorApplication.EnterPlaymode();
        }

        // Intended only for a separate empty verification project, never the user's live editor.
        public static void RunBatch()
        {
            Debug.Log("[AI-RESPONSIVE-STAGE] batch entry");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Batch, true);
            SessionState.SetBool(Pending, true);
            // The executeMethod callback can run during initial editor startup;
            // request Play Mode from a subsequent idle editor update instead.
            double after = EditorApplication.timeSinceStartup + 2;
            EditorApplication.CallbackFunction enter = null;
            enter = () =>
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.timeSinceStartup < after) return;
                EditorApplication.update -= enter;
                Debug.Log("[AI-RESPONSIVE-STAGE] requesting Play Mode");
                EditorApplication.isPlaying = true;
            };
            EditorApplication.update += enter;
        }

        private static void Verify()
        {
            Debug.Log("[AI-RESPONSIVE-STAGE] entered Play Mode");
            if (!SessionState.GetBool(Pending, false)) return;
            SessionState.SetBool(Pending, false);
            if (Object.FindObjectsByType<Mutiny.Levels.MutinyLevelRoot>().Length != 0)
            {
                Debug.LogError("[AI-RESPONSIVE] Requires an isolated empty Play Mode scene.");
                if (SessionState.GetBool(Batch, false)) EditorApplication.Exit(1);
                return;
            }
            var runner = new GameObject("AI Responsive Search Verification").AddComponent<MutinyAiResponsiveVerificationRunner>();
            runner.Completed = result =>
            {
                foreach (string log in result.Logs) Debug.Log(log);
                Debug.Log("[AI-RESPONSIVE] " + (result.Passed ? "PASS " : "FAIL ") +
                    result.PassedAssertions + "/" + result.TotalAssertions + " " + string.Join(" | ", result.Failures));
                // Compatibility suites exercise the existing real entry points.
                var prediction = MutinyAiPredictionVerificationTest.Run();
                var gm = MutinyTurnActionUiVerificationTest.RunGM();
                var boxes = MutinyTurnActionUiVerificationTest.RunAiBoxCamera();
                Debug.Log("[AI-RESPONSIVE-COMPAT] prediction=" + prediction.PassedAssertions + "/" + prediction.TotalAssertions +
                    " GM=" + gm.PassedAssertions + "/" + gm.TotalAssertions +
                    " boxes=" + boxes.PassedAssertions + "/" + boxes.TotalAssertions + " " +
                    string.Join(" | ", prediction.Failures) + " " + string.Join(" | ", gm.Failures) + " " + string.Join(" | ", boxes.Failures));
                if (SessionState.GetBool(Batch, false))
                {
                    SessionState.SetBool(Batch, false);
                    EditorApplication.Exit(result.Passed && prediction.Passed && gm.Passed && boxes.Passed ? 0 : 1);
                }
                else Object.Destroy(runner.gameObject);
            };
        }
    }
}
