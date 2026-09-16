using System;
using System.Collections.Generic;
using Mutiny.Diagnostics;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyTreasureChestManager : MonoBehaviour
    {
        // Temporary user-authorized debug switch. Keep the original implementation
        // below intact so P6 parity work can resume from one explicit switch.
        public static bool SystemEnabled => false;
        public const int MaximumChestCount = 3;

        private readonly List<string> m_PotentialWeapons = new List<string>();
        private readonly List<int> m_ValidDropColumns = new List<int>();
        private readonly List<MutinyTreasureChest> m_Chests = new List<MutinyTreasureChest>();
        private MutinyLevelData m_LevelData;
        private MutinyLevelRoot m_LevelRoot;
        private float m_TickAccumulator;
        private bool m_DisabledStateApplied;

        public IReadOnlyList<MutinyTreasureChest> Chests => m_Chests;

        public void Initialize(MutinyLevelData levelData, MutinyLevelRoot levelRoot)
        {
            m_LevelData = levelData;
            m_LevelRoot = levelRoot;
            if (!SystemEnabled)
            {
                DisableExistingChests();
                return;
            }
            BuildPotentialWeaponList();
            BuildValidDropColumns();
        }

        private void Update()
        {
            if (!SystemEnabled)
            {
                DisableExistingChests();
                return;
            }

            m_TickAccumulator += Time.deltaTime;
            while (m_TickAccumulator >= MutinyPhysics.TimeStep)
            {
                m_TickAccumulator -= MutinyPhysics.TimeStep;
                for (int i = m_Chests.Count - 1; i >= 0; i--)
                {
                    if (m_Chests[i] == null)
                        m_Chests.RemoveAt(i);
                    else
                        m_Chests[i].AdvanceOriginalTick();
                }
            }
        }

        public void TryDropNew()
        {
            if (!SystemEnabled || m_LevelData == null || m_LevelRoot == null ||
                m_Chests.Count >= MaximumChestCount ||
                m_ValidDropColumns.Count == 0 || m_PotentialWeapons.Count == 0)
                return;

            int column = m_ValidDropColumns[UnityEngine.Random.Range(0, m_ValidDropColumns.Count)];
            int floorRow = FindFirstSolidRow(column);
            if (floorRow < 0)
                return;

            float x = column * 32f + 16f;
            float floorY = floorRow * 32f;
            if (!IsDropPositionClear(x, floorY))
                return;

            int contentCount = 1 + UnityEngine.Random.Range(0, 3);
            var contents = new List<string>(contentCount);
            for (int i = 0; i < contentCount; i++)
                contents.Add(m_PotentialWeapons[UnityEngine.Random.Range(0, m_PotentialWeapons.Count)]);

            GameObject chestObject = new GameObject($"TreasureChest_{m_Chests.Count:D2}");
            chestObject.transform.SetParent(m_LevelRoot.ObjectsHolder, false);
            MutinyTreasureChest chest = chestObject.AddComponent<MutinyTreasureChest>();
            chest.Initialize(this, x, floorY, contents);
            m_Chests.Add(chest);
            MutinyDebugLog.Info("Chest",
                $"dropped name={chestObject.name} column={column} floorY={floorY:0} contents={string.Join(",", contents)}", chest);
        }

        private void DisableExistingChests()
        {
            if (m_DisabledStateApplied)
                return;

            m_DisabledStateApplied = true;
            MutinyTreasureChest[] existing = FindObjectsByType<MutinyTreasureChest>();
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i] != null)
                    Destroy(existing[i].gameObject);
            }
            m_Chests.Clear();
            MutinyDebugLog.Info("Chest",
                $"airdrop system disabled for debugging; removedExisting={existing.Length}", this);
        }

        public void Unregister(MutinyTreasureChest chest)
        {
            m_Chests.Remove(chest);
        }

        private void BuildPotentialWeaponList()
        {
            m_PotentialWeapons.Clear();
            for (int i = 0; i < m_LevelData.Objects.Count; i++)
            {
                MutinyLevelObject obj = m_LevelData.Objects[i];
                if (!string.Equals(obj.Type, "potentialWeapons", StringComparison.Ordinal))
                    continue;

                foreach (KeyValuePair<string, string> property in obj.Properties)
                {
                    if (property.Key == "x" || property.Key == "y" || property.Key == "type" ||
                        property.Key == "luck" || property.Key == "maxChests")
                        continue;

                    if (!int.TryParse(property.Value, out int weight) || weight <= 0)
                        continue;
                    for (int n = 0; n < weight; n++)
                        m_PotentialWeapons.Add(property.Key);
                }
                break;
            }
        }

        private void BuildValidDropColumns()
        {
            m_ValidDropColumns.Clear();
            for (int x = 0; x < m_LevelData.Width; x++)
            {
                for (int y = 0; y < m_LevelData.Height; y++)
                {
                    if (string.Equals(m_LevelData.Background[y, x], "antichest", StringComparison.Ordinal))
                        break;
                    if (IsSolid(m_LevelData.Terrain[y, x]))
                    {
                        m_ValidDropColumns.Add(x);
                        break;
                    }
                }
            }
        }

        private int FindFirstSolidRow(int column)
        {
            for (int y = 0; y < m_LevelData.Height; y++)
            {
                if (IsSolid(m_LevelData.Terrain[y, column]))
                    return y;
            }
            return -1;
        }

        private bool IsDropPositionClear(float x, float floorY)
        {
            for (int i = 0; i < m_LevelRoot.Characters.Count; i++)
            {
                MutinyCharacter character = m_LevelRoot.Characters[i];
                if (character == null || !character.IsAlive || character.PhysicsBody == null)
                    continue;
                PhysicsBodyState state = character.PhysicsBody.State;
                if (state.X >= x - 32f && state.X <= x + 32f && state.Y <= floorY + 32f)
                    return false;
            }

            for (int i = 0; i < m_Chests.Count; i++)
            {
                if (m_Chests[i] != null && Mathf.Approximately(m_Chests[i].PixelX, x))
                    return false;
            }

            MutinyWeapon[] weapons = FindObjectsByType<MutinyWeapon>();
            for (int i = 0; i < weapons.Length; i++)
            {
                if ((weapons[i] is MutinyWoodenCrate || weapons[i] is MutinyGunpowderBarrel) &&
                    weapons[i].PhysicsBody != null &&
                    Mathf.Abs(weapons[i].PhysicsBody.State.X - x) < 32f)
                    return false;
            }
            return true;
        }

        private static bool IsSolid(string tile)
        {
            return !string.IsNullOrEmpty(tile) && tile != "-" &&
                   tile.IndexOf("ripple", StringComparison.OrdinalIgnoreCase) < 0;
        }
    }
}
