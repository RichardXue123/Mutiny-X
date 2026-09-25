using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class MutinyParityValidationMenu
    {
        private const string CameraMovementVerificationKey = "Mutiny.CameraMovementPlayModeVerification";

        static MutinyParityValidationMenu()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode ||
                !SessionState.GetBool(CameraMovementVerificationKey, false))
                return;

            SessionState.EraseBool(CameraMovementVerificationKey);
            bool passed = false;
            try
            {
                MutinyLevel1VerificationResult result =
                    MutinyTurnActionUiVerificationTest.RunCameraMovement();
                passed = result.Passed;
                if (passed)
                    Debug.Log($"[Mutiny Parity] Camera movement Play Mode verification passed: {result.PassedAssertions}/{result.TotalAssertions} assertions.");
                else
                    Debug.LogError("[Mutiny Parity] Camera movement Play Mode verification failed:\n" +
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

        private static readonly string[] CharacterTypes =
        {
            "blindPirate", "blindPirateCaptain", "bluePirate", "bluePirateCaptain",
            "bossGuy", "bossGuyZombie", "cabinBoy", "cabinBoyCaptain", "crab",
            "femalePirate", "femalePirateCaptain", "monkey", "oldPirate",
            "oldPirateCaptain", "parrot", "rainbowBeard", "rainbowBeardCaptain",
            "redPirate", "redPirateCaptain", "shark", "skeletonPirate",
            "skeletonPirateCaptain", "soldier", "soldierCaptain", "squid", "tribe",
            "tribeChief"
        };

        [MenuItem("Mutiny/Parity/Validate Runtime Resources")]
        public static void ValidateRuntimeResources()
        {
            var failures = new List<string>();
            for (int level = 1; level <= 18; level++)
            {
                string path = $"Data/Levels/level_{level:D2}";
                if (Resources.Load<TextAsset>(path) == null)
                    failures.Add(path);
            }

            if (Resources.Load<TextAsset>("Data/Tiles/tile-mapping") == null)
                failures.Add("Data/Tiles/tile-mapping");
            if (Resources.LoadAll<AudioClip>("Audio/SFX").Length != 37)
                failures.Add("Audio/SFX (expected 37 clips)");
            if (Resources.LoadAll<AudioClip>("Audio/Music").Length != 2)
                failures.Add("Audio/Music (expected 2 clips)");

            for (int i = 0; i < CharacterTypes.Length; i++)
            {
                string path = $"Art/Characters/Animations/{CharacterTypes[i]}";
                if (Resources.LoadAll<Texture2D>(path).Length != 35)
                    failures.Add($"{path} (expected 35 frames)");
            }

            if (Resources.LoadAll<Texture2D>("Art/Objects/TreasureChest").Length != 90)
                failures.Add("Art/Objects/TreasureChest (expected 90 frames)");

            if (failures.Count == 0)
                Debug.Log("[Mutiny Parity] Runtime resources validated: 18 levels, tile catalog, 39 audio clips, 945 character frames and 90 chest frames.");
            else
                Debug.LogError("[Mutiny Parity] Missing runtime resources:\n" + string.Join("\n", failures));
        }

        [MenuItem("Mutiny/Parity/Validate Turn Action UI")]
        public static void ValidateTurnActionUi()
        {
            MutinyLevel1VerificationResult result = MutinyTurnActionUiVerificationTest.Run();
            if (result.Passed)
                Debug.Log($"[Mutiny Parity] Turn action UI verification passed: {result.PassedAssertions}/{result.TotalAssertions} assertions.");
            else
                Debug.LogError("[Mutiny Parity] Turn action UI verification failed:\n" +
                               string.Join("\n", result.Failures));
        }

        [MenuItem("Mutiny/Parity/Validate Cannon Smoke Trail")]
        public static void ValidateCannonSmokeTrail()
        {
            MutinyLevel1VerificationResult result =
                MutinyTurnActionUiVerificationTest.RunCannonSmokeTrail();
            if (result.Passed)
                Debug.Log($"[Mutiny Parity] Cannon smoke trail verification passed: {result.PassedAssertions}/{result.TotalAssertions} assertions.");
            else
                Debug.LogError("[Mutiny Parity] Cannon smoke trail verification failed:\n" +
                               string.Join("\n", result.Failures));
        }

        [MenuItem("Mutiny/Parity/Validate GM Commands")]
        public static void ValidateGMCommands()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Mutiny Parity] Enter Play Mode before validating GM commands; weapon components require Awake.");
                return;
            }
            MutinyLevel1VerificationResult result = MutinyTurnActionUiVerificationTest.RunGM();
            if (result.Passed)
                Debug.Log($"[Mutiny Parity] GM command verification passed: {result.PassedAssertions}/{result.TotalAssertions} assertions.");
            else
                Debug.LogError("[Mutiny Parity] GM command verification failed:\n" +
                               string.Join("\n", result.Failures));
        }

        [MenuItem("Mutiny/Parity/Validate Camera Movement")]
        public static void ValidateCameraMovement()
        {
            SessionState.SetBool(CameraMovementVerificationKey, true);
            if (EditorApplication.isPlaying)
                OnPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);
            else
                EditorApplication.EnterPlaymode();
        }

        [MenuItem("Mutiny/Parity/Validate Weapon Idle Animations")]
        public static void ValidateWeaponIdleAnimations()
        {
            MutinyLevel1VerificationResult result =
                MutinyTurnActionUiVerificationTest.RunWeaponIdleAnimations();
            if (result.Passed)
                Debug.Log($"[Mutiny Parity] Weapon idle animation verification passed: {result.PassedAssertions}/{result.TotalAssertions} assertions.");
            else
                Debug.LogError("[Mutiny Parity] Weapon idle animation verification failed:\n" +
                               string.Join("\n", result.Failures));
        }

        [MenuItem("Mutiny/Parity/Validate Battle HUD")]
        public static void ValidateBattleHud()
        {
            MutinyLevel1VerificationResult result = MutinyTurnActionUiVerificationTest.RunBattleHud();
            if (result.Passed)
                Debug.Log($"[Mutiny Parity] Battle HUD verification passed: {result.PassedAssertions}/{result.TotalAssertions} assertions.");
            else
                Debug.LogError("[Mutiny Parity] Battle HUD verification failed:\n" +
                               string.Join("\n", result.Failures));
        }

        [MenuItem("Mutiny/Parity/Validate Weapon Ready And Cancel")]
        public static void ValidateWeaponReadyAndCancel()
        {
            MutinyLevel1VerificationResult result =
                MutinyTurnActionUiVerificationTest.RunWeaponReadyAndCancel();
            if (result.Passed)
                Debug.Log($"[Mutiny Parity] Weapon ready/cancel verification passed: {result.PassedAssertions}/{result.TotalAssertions} assertions.");
            else
                Debug.LogError("[Mutiny Parity] Weapon ready/cancel verification failed:\n" +
                               string.Join("\n", result.Failures));
        }
    }
}
