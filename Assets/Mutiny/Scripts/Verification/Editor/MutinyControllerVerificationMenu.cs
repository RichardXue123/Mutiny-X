using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class MutinyControllerVerificationMenu
    {
        private const string PendingKey = "Mutiny.ControllerVerification.Pending";
        private const string BatchKey = "Mutiny.ControllerVerification.Batch";
        private static double s_Deadline;
        private static EditorWindow s_GameView;

        static MutinyControllerVerificationMenu()
        {
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        [MenuItem("Mutiny/Controller/Validate Initial Support Play Mode")]
        public static void Validate()
        {
            if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().rootCount != 0)
            {
                Debug.LogError("[Mutiny Controller] Open an empty scene outside Play Mode to run this isolated fixture.");
                return;
            }
            s_GameView = EditorWindow.GetWindow(System.Type.GetType("UnityEditor.GameView, UnityEditor"));
            s_GameView.Show();
            SessionState.SetBool(PendingKey, true);
            EditorApplication.EnterPlaymode();
        }

        // executeMethod entry: only use in an isolated verification project.
        public static void RunBatch()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode =
                UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            s_GameView = EditorWindow.GetWindow(System.Type.GetType("UnityEditor.GameView, UnityEditor"));
            s_GameView.Show();
            SessionState.SetBool(BatchKey, true);
            Validate();
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PendingKey, false)) return;
            SessionState.EraseBool(PendingKey);
            MutinyControllerVerification.Begin();
            s_Deadline = EditorApplication.timeSinceStartup + 90f;
            EditorApplication.update += AwaitResult;
        }

        private static void AwaitResult()
        {
            if (s_GameView != null) s_GameView.Repaint();
            if (!MutinyControllerVerification.Finished)
            {
                if (EditorApplication.timeSinceStartup < s_Deadline) return;
                Debug.LogError("[Mutiny Controller] Verification timed out; no completed result.");
                EditorApplication.update -= AwaitResult;
                if (SessionState.GetBool(BatchKey, false)) EditorApplication.Exit(2);
                return;
            }
            EditorApplication.update -= AwaitResult;
            MutinyLevel1VerificationResult result = MutinyControllerVerification.Result;
            string report = $"Unity {Application.unityVersion}; controller initial support\n" +
                $"Actual Play Mode result: {result.PassedAssertions}/{result.TotalAssertions}\n" +
                string.Join("\n", result.Failures);
            File.WriteAllText(Path.Combine(Application.dataPath, "../controller-verification.txt"), report);
            if (SessionState.GetBool(BatchKey, false))
            {
                SessionState.EraseBool(BatchKey);
                EditorApplication.Exit(result.Passed ? 0 : 1);
            }
            else EditorApplication.ExitPlaymode();
        }
    }
}
