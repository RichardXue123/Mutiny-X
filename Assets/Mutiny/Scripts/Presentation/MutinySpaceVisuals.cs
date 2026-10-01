using System;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Presentation
{
    // User-authored theme: visual substitutions leave XML collision identities intact.
    public static class MutinySpaceVisuals
    {
        public const string ResourcePath = "Art/Space16/";
        public static bool IsSpace(string theme) => string.Equals(theme, "space", StringComparison.OrdinalIgnoreCase);

        public static bool IsAsteroidTile(string tile) => tile != null &&
            (tile.StartsWith("earth", StringComparison.Ordinal) || tile.StartsWith("eartyh", StringComparison.Ordinal));

        public static void ApplyRockMaterial(MutinyLevelData level, string tile, SpriteRenderer renderer)
        {
            if (IsSpace(level.VisualTheme) && IsAsteroidTile(tile))
                renderer.sharedMaterial = Resources.Load<Material>(ResourcePath + "AsteroidRock");
        }

        public static Sprite ResolveTile(MutinyLevelData level, string tile, int x, int y, bool background)
        {
            if (!IsSpace(level.VisualTheme) || x < level.SpaceThemeMinX) return null;
            string name;
            if (tile.StartsWith("cannon_port_", StringComparison.Ordinal)) name = "cannon";
            else if (tile == "ship_tile_1" || tile == "ship_tile_2" || tile == "ship_top_middle" || tile == "cave_middle_2")
            {
                if (background) name = "interior";
                else if (!Solid(level, x, y - 1)) name = "edge_top";
                else if (!Solid(level, x, y + 1)) name = "edge_bottom";
                else if (!Solid(level, x - 1, y)) name = "edge_left";
                else if (!Solid(level, x + 1, y)) name = "edge_right";
                else name = tile == "ship_tile_2" ? "hull_b" : "hull_a";
            }
            else return null;
            Sprite sprite = Resources.Load<Sprite>(ResourcePath + name);
            if (sprite == null) Debug.LogError("[Mutiny:Space] Missing tile: " + name);
            return sprite;
        }

        private static bool Solid(MutinyLevelData level, int x, int y)
        {
            if (x < 0 || y < 0 || x >= level.Width || y >= level.Height) return false;
            string tile = level.Terrain[y, x];
            return !string.IsNullOrEmpty(tile) && tile != "-" && !tile.Contains("ripple");
        }
    }
}
