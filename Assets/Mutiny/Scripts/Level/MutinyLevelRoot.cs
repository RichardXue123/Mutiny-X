using System.Collections.Generic;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Levels
{
    [DisallowMultipleComponent]
    public sealed class MutinyLevelRoot : MonoBehaviour
    {
        public string LevelName;
        public int Width;
        public int Height;
        public int Players;
        public float GravityScale = 1f;
        public string VisualTheme = string.Empty;
        public List<MutinySpeechAudio> SpeechAudio = new();
        public int SkyColour = 1;
        public float WaterLevelY;

        public Transform BackgroundHolder;
        public Transform TerrainHolder;
        public Transform WaterHolder;
        public Transform ObjectsHolder;

        public MutinyTeam Team1;
        public MutinyTeam Team2;
        public List<MutinyCharacter> AllCharacters = new List<MutinyCharacter>();
        public List<MutinyCharacter> Characters => AllCharacters;

        // Runtime-only, owned by this particular level instance. A rebuilt or
        // scene-preview level always begins at 01, independent of the audio singleton.
        public int NextRobotVoiceNumber { get; private set; } = 1;
        internal void AdvanceRobotVoiceSequence() => NextRobotVoiceNumber = NextRobotVoiceNumber % 8 + 1;

        private void Start()
        {
            EnsureRuntimeWater();
        }

        public void EnsureRuntimeWater()
        {
            // Baked scenes can predate SkyColour serialization. Their controller
            // has already parsed the level index in Awake when Start reaches here.
            MutinyLevelController controller = GetComponent<MutinyLevelController>();
            if (controller == null)
                controller = GetComponentInParent<MutinyLevelController>();
            if (controller != null && controller.LevelXml != null)
                SkyColour = MutinyOriginalBackground.SkyColourForLevel(controller.OriginalLevelIndex);

            float waterPixelY = -WaterLevelY * MutinyPhysics.PixelsPerUnit;
            for (int i = 0; i < AllCharacters.Count; i++)
            {
                MutinyCharacter character = AllCharacters[i];
                if (character != null && character.PhysicsBody != null)
                    character.PhysicsBody.WaterPixelY = waterPixelY;
            }

            if (WaterHolder == null)
            {
                Transform water = transform.Find("Water");
                if (water != null)
                    WaterHolder = water;
            }

            if (WaterHolder == null)
                return;

            // Space retains the authoritative fall boundary but has no ocean surface.
            if (MutinySpaceVisuals.IsSpace(VisualTheme))
                return;

            MutinyWaterSurface surface = WaterHolder.GetComponent<MutinyWaterSurface>();
            if (surface == null)
                surface = WaterHolder.gameObject.AddComponent<MutinyWaterSurface>();
            if (surface.LoadedFrameCount == 0)
                surface.Initialize(Width * MutinyLevelBuilder.CellSize, WaterLevelY, SkyColour);
        }
    }
}
