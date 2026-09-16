using System.Collections.Generic;
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
        public float WaterLevelY;

        public Transform BackgroundHolder;
        public Transform TerrainHolder;
        public Transform WaterHolder;
        public Transform ObjectsHolder;

        public MutinyTeam Team1;
        public MutinyTeam Team2;
        public List<MutinyCharacter> AllCharacters = new List<MutinyCharacter>();
        public List<MutinyCharacter> Characters => AllCharacters;

        private void Start()
        {
            EnsureRuntimeWater();
        }

        public void EnsureRuntimeWater()
        {
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

            MutinyWaterSurface surface = WaterHolder.GetComponent<MutinyWaterSurface>();
            if (surface == null)
                surface = WaterHolder.gameObject.AddComponent<MutinyWaterSurface>();
            if (surface.LoadedFrameCount == 0)
                surface.Initialize(Width * MutinyLevelBuilder.CellSize, WaterLevelY, 1);
        }
    }
}
