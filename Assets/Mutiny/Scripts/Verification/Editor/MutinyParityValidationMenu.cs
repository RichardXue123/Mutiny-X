using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    public static class MutinyParityValidationMenu
    {
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
    }
}

