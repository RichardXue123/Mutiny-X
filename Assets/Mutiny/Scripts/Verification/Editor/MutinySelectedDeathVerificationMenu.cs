using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class MutinySelectedDeathVerificationMenu
    {
        private const string Pending = "Mutiny.SelectedDeath.Pending", Batch = "Mutiny.SelectedDeath.Batch";
        private static double s_Deadline;

        static MutinySelectedDeathVerificationMenu()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
                    EditorApplication.delayCall += Verify;
            };
        }

        [MenuItem("Mutiny/Parity/Validate Selected Death Settlement Play Mode")]
        public static void Run()
        {
            if (EditorSceneManager.GetActiveScene().rootCount != 0)
            {
                Debug.LogError("[TURN-DEATH] Open an empty scene for this isolated production-flow fixture.");
                return;
            }
            SessionState.SetBool(Pending, true);
            if (EditorApplication.isPlaying) EditorApplication.delayCall += Verify;
            else EditorApplication.EnterPlaymode();
        }

        // Only for an isolated verification checkout, never the user's live editor.
        public static void RunBatch()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
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
            s_Deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.update += Watchdog;
            var runner = new GameObject("Selected Death Verification").AddComponent<MutinySelectedDeathVerificationRunner>();
            runner.Completed = result =>
            {
                EditorApplication.update -= Watchdog;
                foreach (string log in result.Logs) Debug.Log(log);
                Debug.Log("[TURN-DEATH] " + (result.Passed ? "PASS " : "FAIL ") +
                    result.PassedAssertions + "/" + result.TotalAssertions + " " + string.Join(" | ", result.Failures));
                var aim = MutinyTurnActionUiVerificationTest.RunCharacterAimOverlay();
                var camera = MutinyTurnActionUiVerificationTest.RunCameraMovement();
                Debug.Log("[TURN-DEATH-COMPAT] aim=" + aim.PassedAssertions + "/" + aim.TotalAssertions +
                    " camera=" + camera.PassedAssertions + "/" + camera.TotalAssertions + " " +
                    string.Join(" | ", aim.Failures) + " " + string.Join(" | ", camera.Failures));
                if (SessionState.GetBool(Batch, false))
                {
                    SessionState.EraseBool(Batch);
                    EditorApplication.Exit(result.Passed && aim.Passed && camera.Passed ? 0 : 1);
                }
                else Object.Destroy(runner.gameObject);
            };
        }

        private static void Watchdog()
        {
            if (EditorApplication.timeSinceStartup < s_Deadline) return;
            EditorApplication.update -= Watchdog;
            Debug.LogError("[TURN-DEATH] Real Play Mode fixture timed out.");
            if (SessionState.GetBool(Batch, false)) EditorApplication.Exit(2);
        }
    }
}
