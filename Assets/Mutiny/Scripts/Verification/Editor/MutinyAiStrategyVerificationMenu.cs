using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class MutinyAiStrategyVerificationMenu
    {
        private const string Pending = "Mutiny.AiStrategy.Pending";
        private const string Batch = "Mutiny.AiStrategy.Batch";
        private const string Capture = "Mutiny.AiStrategy.Capture";

        static MutinyAiStrategyVerificationMenu()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
                    EditorApplication.delayCall += Verify;
            };
        }

        public static void CaptureBaselineBatch()
        {
            StartBatch(capture: true);
        }

        public static void RunBatch() => StartBatch(capture: false);

        [MenuItem("Mutiny/Parity/Validate AI Strategy Boundary Play Mode")]
        public static void Run()
        {
            SessionState.SetBool(Capture, false);
            SessionState.SetBool(Batch, false);
            SessionState.SetBool(Pending, true);
            if (EditorApplication.isPlaying) EditorApplication.delayCall += Verify;
            else EditorApplication.EnterPlaymode();
        }

        private static void StartBatch(bool capture)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Capture, capture);
            SessionState.SetBool(Batch, true);
            SessionState.SetBool(Pending, true);
            double after = EditorApplication.timeSinceStartup + 2;
            EditorApplication.CallbackFunction enter = null;
            enter = () =>
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.timeSinceStartup < after) return;
                EditorApplication.update -= enter;
                EditorApplication.isPlaying = true;
            };
            EditorApplication.update += enter;
        }

        private static void Verify()
        {
            if (!SessionState.GetBool(Pending, false)) return;
            SessionState.SetBool(Pending, false);
            if (Object.FindObjectsByType<Mutiny.Levels.MutinyLevelRoot>().Length != 0)
            {
                Debug.LogError("[AI-STRATEGY] Requires an isolated empty Play Mode scene.");
                if (SessionState.GetBool(Batch, false)) EditorApplication.Exit(1);
                return;
            }
            if (SessionState.GetBool(Capture, false))
            {
                Complete(MutinyAiStrategyVerificationTest.CaptureBaseline(), "AI-STRATEGY-BASELINE");
                return;
            }
            var runner = new GameObject("AI Strategy Boundary Verification").AddComponent<MutinyAiStrategyVerificationRunner>();
            runner.Completed = result =>
            {
                Complete(result, "AI-STRATEGY");
                if (!SessionState.GetBool(Batch, false)) Object.Destroy(runner.gameObject);
            };
        }

        private static void Complete(MutinyLevel1VerificationResult result, string label)
        {
            foreach (string log in result.Logs) Debug.Log(log);
            Debug.Log("[" + label + "] " + (result.Passed ? "PASS " : "FAIL ") +
                result.PassedAssertions + "/" + result.TotalAssertions + " " + string.Join(" | ", result.Failures));
            if (SessionState.GetBool(Batch, false)) EditorApplication.Exit(result.Passed ? 0 : 1);
        }
    }
}
