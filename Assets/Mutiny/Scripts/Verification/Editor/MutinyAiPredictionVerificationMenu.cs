using System;
using UnityEditor;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class MutinyAiPredictionVerificationMenu
    {
        private const string PendingKey = "Mutiny.AiPredictionParity.Pending";

        static MutinyAiPredictionVerificationMenu()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(PendingKey, false))
                    EditorApplication.delayCall += Verify;
            };
        }

        [MenuItem("Mutiny/Parity/Validate AI Prediction Fixes Play Mode")]
        public static void Run()
        {
            SessionState.SetBool(PendingKey, true);
            if (EditorApplication.isPlaying) EditorApplication.delayCall += Verify;
            else EditorApplication.EnterPlaymode();
        }

        private static void Verify()
        {
            if (!SessionState.GetBool(PendingKey, false)) return;
            SessionState.SetBool(PendingKey, false);
            if (UnityEngine.Object.FindObjectsByType<Mutiny.Levels.MutinyLevelRoot>().Length > 0)
            {
                Debug.LogError("[AI-PREDICTION-FIXES] Use an isolated empty Play Mode scene; do not run on an active game level.");
                return;
            }
            try
            {
                var result = MutinyAiPredictionVerificationTest.Run();
                string message = "[AI-PREDICTION-FIXES] " + (result.Passed ? "PASS " : "FAIL ") +
                    result.PassedAssertions + "/" + result.TotalAssertions + " " + string.Join(" | ", result.Failures);
                if (result.Passed) Debug.Log(message);
                else Debug.LogError(message);
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }
    }
}
