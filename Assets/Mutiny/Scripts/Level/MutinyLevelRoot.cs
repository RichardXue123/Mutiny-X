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
    }
}
