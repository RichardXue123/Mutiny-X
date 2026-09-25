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
        private const string CannonEffectsVerificationKey = "Mutiny.CannonEffectsPlayModeVerification";
        private const string WeaponLifecycleVerificationKey = "Mutiny.WeaponLifecyclePlayModeVerification";
        private const string ScrollArrowVerificationKey = "Mutiny.ScrollArrowPlayModeVerification";
        private const string LevelLifecycleVerificationKey = "Mutiny.LevelLifecyclePlayModeVerification";
        private const string AnchorAnimationVerificationKey = "Mutiny.AnchorAnimationPlayModeVerification";

        static MutinyParityValidationMenu()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
                return;

            bool verifyCamera = SessionState.GetBool(CameraMovementVerificationKey, false);
            bool verifyCannon = SessionState.GetBool(CannonEffectsVerificationKey, false);
            bool verifyWeaponLifecycle = SessionState.GetBool(WeaponLifecycleVerificationKey, false);
            bool verifyScrollArrow = SessionState.GetBool(ScrollArrowVerificationKey, false);
            bool verifyLevelLifecycle = SessionState.GetBool(LevelLifecycleVerificationKey, false);
            bool verifyAnchorAnimation = SessionState.GetBool(AnchorAnimationVerificationKey, false);
            if (!verifyCamera && !verifyCannon && !verifyWeaponLifecycle && !verifyScrollArrow &&
                !verifyLevelLifecycle && !verifyAnchorAnimation)
                return;

            SessionState.EraseBool(CameraMovementVerificationKey);
            SessionState.EraseBool(CannonEffectsVerificationKey);
            SessionState.EraseBool(WeaponLifecycleVerificationKey);
            SessionState.EraseBool(ScrollArrowVerificationKey);
            SessionState.EraseBool(LevelLifecycleVerificationKey);
            SessionState.EraseBool(AnchorAnimationVerificationKey);
            bool passed = false;
            try
            {
                MutinyLevel1VerificationResult result =
                    verifyAnchorAnimation ? MutinyTurnActionUiVerificationTest.RunAnchorAnimation() :
                    verifyLevelLifecycle ? MutinyTurnActionUiVerificationTest.RunLevelLifecycle() :
                    verifyScrollArrow ? MutinyTurnActionUiVerificationTest.RunScrollArrows() :
                    verifyCannon ? MutinyTurnActionUiVerificationTest.RunCannonSmokeTrail() :
                        verifyWeaponLifecycle ? MutinyTurnActionUiVerificationTest.RunWeaponLifecycle() :
                            MutinyTurnActionUiVerificationTest.RunCameraMovement();
                passed = result.Passed;
                string label = verifyAnchorAnimation ? "Anchor animation" :
                    verifyLevelLifecycle ? "Level lifecycle" :
                    verifyScrollArrow ? "Scroll arrows" :
                    verifyCannon ? "Cannon effects" :
                    verifyWeaponLifecycle ? "Weapon lifecycle" : "Camera movement";
                if (passed)
                    Debug.Log($"[Mutiny Parity] {label} Play Mode verification passed: {result.PassedAssertions}/{result.TotalAssertions} assertions.");
                else
                    Debug.LogError($"[Mutiny Parity] {label} Play Mode verification failed:\n" +
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

        [MenuItem("Mutiny/Parity/Validate Level Lifecycle Play Mode")]
        public static void ValidateLevelLifecyclePlayMode()
        {
            SessionState.SetBool(LevelLifecycleVerificationKey, true);
            if (EditorApplication.isPlaying)
                OnPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);
            else
                EditorApplication.EnterPlaymode();
        }

        [MenuItem("Mutiny/Parity/Validate Anchor Animation Play Mode")]
        public static void ValidateAnchorAnimationPlayMode()
        {
            SessionState.SetBool(AnchorAnimationVerificationKey, true);
            if (EditorApplication.isPlaying)
                OnPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);
            else
                EditorApplication.EnterPlaymode();
        }

        [MenuItem("Mutiny/Parity/Validate Two Player Mode")]
        public static void ValidateTwoPlayerMode()
        {
            MutinyLevel1VerificationResult result = MutinyTwoPlayerVerificationTest.Run();
            if (result.Passed)
                Debug.Log($"[Mutiny Parity] Two-player mode verification passed: {result.PassedAssertions}/{result.TotalAssertions} assertions.");
            else
                Debug.LogError("[Mutiny Parity] Two-player mode verification failed:\n" +
                               string.Join("\n", result.Failures));
        }

        [MenuItem("Mutiny/Parity/Validate Cannon Effects Play Mode")]
        public static void ValidateCannonEffectsPlayMode()
        {
            SessionState.SetBool(CannonEffectsVerificationKey, true);
            if (EditorApplication.isPlaying)
                OnPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);
            else
                EditorApplication.EnterPlaymode();
        }

        [MenuItem("Mutiny/Parity/Validate Scroll Arrows Play Mode")]
        public static void ValidateScrollArrowsPlayMode()
        {
            SessionState.SetBool(ScrollArrowVerificationKey, true);
            if (EditorApplication.isPlaying)
                OnPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);
            else
                EditorApplication.EnterPlaymode();
        }

        [MenuItem("Mutiny/Parity/Validate Weapon Lifecycle Play Mode")]
        public static void ValidateWeaponLifecyclePlayMode()
        {
            SessionState.SetBool(WeaponLifecycleVerificationKey, true);
            if (EditorApplication.isPlaying)
                OnPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);
            else
                EditorApplication.EnterPlaymode();
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
