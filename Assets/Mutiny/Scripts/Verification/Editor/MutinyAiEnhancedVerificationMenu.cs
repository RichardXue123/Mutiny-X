using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class MutinyAiEnhancedVerificationMenu
    {
        private const string Pending = "Mutiny.AiEnhanced.Pending", Batch = "Mutiny.AiEnhanced.Batch";
        static MutinyAiEnhancedVerificationMenu()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false)) EditorApplication.delayCall += Verify;
            };
        }
        [MenuItem("Mutiny/Parity/Validate Enhanced AI Effects Play Mode")]
        public static void Run()
        {
            SessionState.SetBool(Pending, true); SessionState.SetBool(Batch, false);
            if (EditorApplication.isPlaying) EditorApplication.delayCall += Verify; else EditorApplication.EnterPlaymode();
        }
        public static void RunBatch()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Pending, true); SessionState.SetBool(Batch, true);
            double after = EditorApplication.timeSinceStartup + 2;
            EditorApplication.CallbackFunction enter = null;
            enter = () =>
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.timeSinceStartup < after) return;
                EditorApplication.update -= enter; EditorApplication.isPlaying = true;
            };
            EditorApplication.update += enter;
        }
        private static void Verify()
        {
            if (!SessionState.GetBool(Pending, false)) return;
            SessionState.SetBool(Pending, false);
            if (Object.FindObjectsByType<Mutiny.Levels.MutinyLevelRoot>().Length != 0)
            { Debug.LogError("[AI-ENHANCED] Requires an isolated empty scene."); if (SessionState.GetBool(Batch, false)) EditorApplication.Exit(1); return; }
            var runner = new GameObject("Enhanced AI Effects Verification").AddComponent<MutinyAiStrategyVerificationRunner>();
            runner.TestFactory = MutinyAiEnhancedVerificationTest.Run;
            runner.Completed = result =>
            {
                foreach (string log in result.Logs) Debug.Log(log);
                Debug.Log("[AI-ENHANCED] " + (result.Passed ? "PASS " : "FAIL ") + result.PassedAssertions + "/" +
                    result.TotalAssertions + " " + string.Join(" | ", result.Failures));
                if (SessionState.GetBool(Batch, false)) EditorApplication.Exit(result.Passed ? 0 : 1);
                else Object.Destroy(runner.gameObject);
            };
        }
    }
}
