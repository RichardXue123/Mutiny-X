using System;
using UnityEditor;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class MutinyEndingVerificationMenu
    {
        private const string VerificationKey = "Mutiny.EndingSequencePlayModeVerification";

        static MutinyEndingVerificationMenu()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("Mutiny/Parity/Validate Ending Sequence Play Mode")]
        public static void Validate()
        {
            SessionState.SetBool(VerificationKey, true);
            if (EditorApplication.isPlaying)
                OnPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);
            else
                EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode ||
                !SessionState.GetBool(VerificationKey, false))
                return;

            SessionState.EraseBool(VerificationKey);
            bool passed = false;
            try
            {
                MutinyLevel1VerificationResult result =
                    MutinyTurnActionUiVerificationTest.RunEndingSequence();
                passed = result.Passed;
                if (passed)
                    Debug.Log($"[Mutiny Parity] Ending sequence Play Mode verification passed: {result.PassedAssertions}/{result.TotalAssertions} assertions.");
                else
                    Debug.LogError("[Mutiny Parity] Ending sequence Play Mode verification failed:\n" +
                                   string.Join("\n", result.Failures));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            if (Application.isBatchMode)
                EditorApplication.Exit(passed ? 0 : 1);
            else
                EditorApplication.ExitPlaymode();
        }
    }
}
