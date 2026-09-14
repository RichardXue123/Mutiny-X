using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Mutiny.Levels
{
    public enum MutinyTileKind
    {
        Visual,
        Logic
    }

    [Serializable]
    public sealed class MutinyTileDefinition
    {
        public string TileName;
        public MutinyTileKind Kind;
        public int SymbolId;
        public string LinkageName;
        public int FrameCount;
        public int Width;
        public int Height;
        public float OriginX;
        public float OriginY;
        public Vector2 Pivot;
        public string AssetPath;
        public string Notes;

        public bool IsAnimated => FrameCount > 1;
        public bool IsLogic => Kind == MutinyTileKind.Logic;

        public override string ToString()
        {
            return $"{TileName} ({Kind}, {Width}x{Height}, Pivot={Pivot})";
        }
    }

    public static class MutinyTileCatalog
    {
        private static readonly Dictionary<string, MutinyTileDefinition> s_Definitions = new Dictionary<string, MutinyTileDefinition>(StringComparer.Ordinal);
        private static bool s_IsInitialized;

        public static IReadOnlyDictionary<string, MutinyTileDefinition> Definitions
        {
            get
            {
                EnsureInitialized();
                return s_Definitions;
            }
        }

        public static bool TryGetDefinition(string tileName, out MutinyTileDefinition definition)
        {
            EnsureInitialized();
            if (tileName == null)
            {
                definition = null;
                return false;
            }
            return s_Definitions.TryGetValue(tileName, out definition);
        }

        public static void EnsureInitialized()
        {
            if (s_IsInitialized)
                return;

            string path = Path.Combine(Application.dataPath, "Mutiny", "Data", "Tiles", "tile-mapping.csv");
            if (File.Exists(path))
            {
                LoadFromCsv(File.ReadAllText(path));
            }
        }

        public static void LoadFromCsv(string csvContent)
        {
            s_Definitions.Clear();
            using var reader = new StringReader(csvContent);
            string header = reader.ReadLine();
            if (header == null)
                return;

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] cols = line.Split(',');
                if (cols.Length < 12)
                    continue;

                var def = new MutinyTileDefinition
                {
                    TileName = cols[0].Trim(),
                    Kind = string.Equals(cols[1].Trim(), "logic", StringComparison.OrdinalIgnoreCase) 
                        ? MutinyTileKind.Logic 
                        : MutinyTileKind.Visual,
                    SymbolId = int.TryParse(cols[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int symId) ? symId : 0,
                    LinkageName = cols[3].Trim(),
                    FrameCount = int.TryParse(cols[4].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int frames) ? frames : 0,
                    Width = int.TryParse(cols[5].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int w) ? w : 0,
                    Height = int.TryParse(cols[6].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int h) ? h : 0,
                    OriginX = float.TryParse(cols[7].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float ox) ? ox : 0f,
                    OriginY = float.TryParse(cols[8].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float oy) ? oy : 0f,
                    Pivot = new Vector2(
                        float.TryParse(cols[9].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float px) ? px : 0f,
                        float.TryParse(cols[10].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float py) ? py : 1f
                    ),
                    AssetPath = cols[11].Trim(),
                    Notes = cols.Length > 12 ? cols[12].Trim() : string.Empty
                };

                s_Definitions[def.TileName] = def;
            }

            s_IsInitialized = true;
        }
    }
}
