using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    internal static class MutinyBoxPlacementRules
    {
        internal static bool CanPlaceLive(Vector2 position, string[,] terrain, int width, int height, MutinyPhysicsBody self)
        {
            var chests = new List<Vector2>();
            foreach (var chest in Object.FindObjectsByType<MutinyTreasureChest>())
                if (chest != null) chests.Add(new Vector2(chest.PixelX, chest.PixelY));
            var characters = new List<PhysicsBodyState>();
            foreach (var character in Object.FindObjectsByType<MutinyCharacter>())
                if (character != null && character.IsAlive && character.PhysicsBody != null)
                    characters.Add(character.PhysicsBody.State);
            return CanPlace(position, terrain, width, height, MutinyBoxRegistry.GetObstacles(self), chests, characters);
        }

        internal static bool CanPlace(Vector2 pixelPosition, string[,] terrain, int gridWidth, int gridHeight,
            IReadOnlyList<PhysicsBoxObstacle> boxes, IReadOnlyList<Vector2> chests, IReadOnlyList<PhysicsBodyState> characters)
        {
            if (terrain == null || gridWidth <= 0 || gridHeight <= 0)
                return false;

            float px = pixelPosition.x;
            float py = pixelPosition.y;
            int left = Mathf.FloorToInt((px - MutinyWoodenCrate.LeftExtentPixels) / 32f);
            int right = Mathf.FloorToInt((px + MutinyWoodenCrate.LeftExtentPixels) / 32f);
            int top = Mathf.FloorToInt((py - MutinyWoodenCrate.LeftExtentPixels) / 32f);
            int bottom = Mathf.FloorToInt((py + MutinyWoodenCrate.LeftExtentPixels) / 32f);
            for (int x = left; x <= right; x++)
                for (int y = top; y <= bottom; y++)
                    if (IsSolidTile(terrain, x, y, gridWidth, gridHeight))
                        return false;

            // The original retains the first support surface as a vertical boundary.
            float obstructionY = (bottom + 1) * 32f;
            for (int y = bottom; y < gridHeight; y++)
            {
                bool foundSolid = false;
                for (int x = left; x <= right; x++)
                {
                    if (IsSolidTile(terrain, x, y, gridWidth, gridHeight))
                    {
                        // Original canPlace accepts the first supporting column;
                        // the complete box width does not need ground beneath it.
                        foundSolid = true;
                        break;
                    }
                }
                if (foundSolid)
                {
                    obstructionY = (y + 1) * 32f;
                    break;
                }
            }

            for (int i = 0; i < boxes.Count; i++)
            {
                PhysicsBodyState other = boxes[i].State;
                if (other.X - other.LeftExtent <= px + MutinyWoodenCrate.LeftExtentPixels &&
                    other.X + other.RightExtent >= px - MutinyWoodenCrate.RightExtentPixels &&
                    other.Y + other.BottomExtent >= py - MutinyWoodenCrate.RightExtentPixels)
                {
                    if (other.Y - other.TopExtent < obstructionY)
                        obstructionY = other.Y - other.TopExtent;
                    if (other.Y - other.TopExtent <= py + MutinyWoodenCrate.LeftExtentPixels)
                        return false;
                }
            }

            for (int i = 0; i < chests.Count; i++)
            {
                Vector2 chestPx = chests[i];
                if (chestPx.x - 16f <= px + MutinyWoodenCrate.LeftExtentPixels &&
                    chestPx.x + 16f >= px - MutinyWoodenCrate.RightExtentPixels &&
                    chestPx.y + 16f >= py - MutinyWoodenCrate.LeftExtentPixels)
                    return false;
            }

            for (int i = 0; i < characters.Count; i++)
            {
                PhysicsBodyState body = characters[i];
                if (body.X - body.LeftExtent <= px + MutinyWoodenCrate.LeftExtentPixels &&
                    body.X + body.RightExtent >= px - MutinyWoodenCrate.RightExtentPixels &&
                    body.Y + body.BottomExtent >= py - MutinyWoodenCrate.RightExtentPixels &&
                    body.Y - body.TopExtent <= obstructionY)
                    return false;
            }
            return true;
        }


        private static bool IsSolidTile(string[,] terrain, int x, int y, int width, int height)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return false;
            string tile = terrain[y, x];
            return !string.IsNullOrEmpty(tile) && tile != "-" &&
                tile.IndexOf("ripple", System.StringComparison.OrdinalIgnoreCase) < 0;
        }
    }
}

