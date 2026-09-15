using System;
using System.Collections.Generic;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Levels
{
    public static class MutinyLevelBuilder
    {
        public const float PixelsPerUnit = 32f;
        public const float CellSize = 1.0f; // 32 pixels / 32 PPU = 1 Unity unit

        public const int BackgroundSortingOrder = -10;
        public const int ObjectSortingOrder = 0;
        public const int TerrainSortingOrder = 10;
        public const int CharacterSortingOrder = 20;
        public const int WaterSortingOrder = 30;

        public static GameObject BuildLevel(MutinyLevelData levelData, Transform parent = null)
        {
            if (levelData == null)
                throw new ArgumentNullException(nameof(levelData));

            string rootName = string.IsNullOrEmpty(levelData.Name) || levelData.Name == "undefined"
                ? "MutinyLevel"
                : $"MutinyLevel_{levelData.Name}";

            GameObject levelRootObj = new GameObject(rootName);
            if (parent != null)
                levelRootObj.transform.SetParent(parent, false);

            MutinyLevelRoot levelRoot = levelRootObj.AddComponent<MutinyLevelRoot>();
            levelRoot.LevelName = levelData.Name;
            levelRoot.Width = levelData.Width;
            levelRoot.Height = levelData.Height;
            levelRoot.Players = levelData.Players;

            // 1. Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(levelRootObj.transform, false);
            levelRoot.BackgroundHolder = bgObj.transform;
            BuildBackground(levelData, bgObj.transform);

            // 2. Terrain
            GameObject terrainObj = new GameObject("Terrain");
            terrainObj.transform.SetParent(levelRootObj.transform, false);
            levelRoot.TerrainHolder = terrainObj.transform;
            BuildTerrain(levelData, terrainObj.transform);

            // 3. Water
            GameObject waterObj = new GameObject("Water");
            waterObj.transform.SetParent(levelRootObj.transform, false);
            levelRoot.WaterHolder = waterObj.transform;
            levelRoot.WaterLevelY = BuildWater(levelData, waterObj.transform);

            // 4. Objects (Characters & Items)
            GameObject objectsObj = new GameObject("Objects");
            objectsObj.transform.SetParent(levelRootObj.transform, false);
            levelRoot.ObjectsHolder = objectsObj.transform;
            BuildObjects(levelData, objectsObj.transform, levelRoot);

            var chestManager = levelRootObj.AddComponent<MutinyTreasureChestManager>();
            chestManager.Initialize(levelData, levelRoot);

            // 5. Turn Management & Input & HUD
            var turnManager = levelRootObj.AddComponent<MutinyTurnManager>();
            turnManager.Team1 = levelRoot.Team1;
            turnManager.Team2 = levelRoot.Team2;

            var playerInput = levelRootObj.AddComponent<MutinyPlayerInput>();
            playerInput.TurnManager = turnManager;

            var hud = levelRootObj.AddComponent<MutinyGameHUD>();
            hud.TurnManager = turnManager;
            hud.PlayerInput = playerInput;

            // Ensure AudioManager instance exists
            _ = MutinyAudioManager.Instance;

            return levelRootObj;
        }

        private static void BuildBackground(MutinyLevelData levelData, Transform parent)
        {
            if (levelData.Background == null)
                return;

            for (int y = 0; y < levelData.Height; y++)
            {
                for (int x = 0; x < levelData.Width; x++)
                {
                    string tileName = levelData.Background[y, x];
                    if (string.IsNullOrEmpty(tileName) || tileName == "-" || tileName == "antichest")
                        continue;

                    Sprite sprite = ResolveTileSprite(tileName);
                    if (sprite == null)
                        continue;

                    GameObject tileObj = new GameObject($"bg_{x:D2}_{y:D2}_{tileName}");
                    tileObj.transform.SetParent(parent, false);
                    tileObj.transform.localPosition = new Vector3(x * CellSize, -y * CellSize, 0f);

                    SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingOrder = BackgroundSortingOrder;
                }
            }
        }

        private static void BuildTerrain(MutinyLevelData levelData, Transform parent)
        {
            if (levelData.Terrain == null)
                return;

            for (int y = 0; y < levelData.Height; y++)
            {
                for (int x = 0; x < levelData.Width; x++)
                {
                    string tileName = levelData.Terrain[y, x];
                    if (string.IsNullOrEmpty(tileName) || tileName == "-")
                        continue;

                    Sprite sprite = ResolveTileSprite(tileName);
                    if (sprite == null)
                        continue;

                    GameObject tileObj = new GameObject($"tile_{x:D2}_{y:D2}_{tileName}");
                    tileObj.transform.SetParent(parent, false);
                    tileObj.transform.localPosition = new Vector3(x * CellSize, -y * CellSize, 0f);

                    SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingOrder = TerrainSortingOrder;

                    // Physical collision for solid terrain (exclude water ripples)
                    if (!tileName.Contains("ripple"))
                    {
                        BoxCollider2D collider = tileObj.AddComponent<BoxCollider2D>();
                        collider.size = new Vector2(CellSize, CellSize);
                        collider.offset = new Vector2(CellSize * 0.5f, -CellSize * 0.5f);
                    }
                }
            }
        }

        private static float BuildWater(MutinyLevelData levelData, Transform parent)
        {
            float waterY = -14f; // Default for level 1
            if (levelData.Objects != null)
            {
                foreach (var obj in levelData.Objects)
                {
                    if (obj.Type == "water")
                    {
                        waterY = -obj.Y * CellSize;
                        break;
                    }
                }
            }

            GameObject triggerObj = new GameObject("WaterTrigger");
            triggerObj.transform.SetParent(parent, false);
            triggerObj.transform.localPosition = new Vector3(levelData.Width * 0.5f * CellSize, waterY - 5f, 0f);

            BoxCollider2D collider = triggerObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(levelData.Width * CellSize + 20f, 10f);

            return waterY;
        }

        private static void BuildObjects(MutinyLevelData levelData, Transform parent, MutinyLevelRoot levelRoot)
        {
            if (levelData.Objects == null)
                return;

            GameObject team1Group = new GameObject("Team1_Player");
            team1Group.transform.SetParent(parent, false);
            levelRoot.Team1 = team1Group.AddComponent<MutinyTeam>();
            levelRoot.Team1.TeamNumber = 1;
            levelRoot.Team1.IsAiControlled = false;

            GameObject team2Group = new GameObject("Team2_Enemy");
            team2Group.transform.SetParent(parent, false);
            levelRoot.Team2 = team2Group.AddComponent<MutinyTeam>();
            levelRoot.Team2.TeamNumber = 2;
            levelRoot.Team2.IsAiControlled = (levelData.Players == 1);
            if (levelRoot.Team2.IsAiControlled)
            {
                team2Group.AddComponent<MutinyAIController>();
            }

            GameObject otherGroup = new GameObject("Misc");
            otherGroup.transform.SetParent(parent, false);

            int charIndex = 0;
            foreach (var obj in levelData.Objects)
            {
                if (obj.Type == "water")
                    continue;

                if (obj.Type == "potentialWeapons")
                {
                    GameObject weaponPoolObj = new GameObject("PotentialWeapons");
                    weaponPoolObj.transform.SetParent(otherGroup.transform, false);
                    weaponPoolObj.transform.localPosition = new Vector3(obj.X * CellSize, -obj.Y * CellSize, 0f);
                    continue;
                }

                // Character
                int teamIndex = (obj.Type == "redPirate" || obj.Type == "redPirateCaptain") ? 1 : 2;
                Transform teamParent = teamIndex == 1 ? team1Group.transform : team2Group.transform;

                float posX = (obj.X + 0.5f) * CellSize;
                float posY = -(obj.Y + 0.75f) * CellSize;

                GameObject charObj = new GameObject($"Char_{charIndex:D2}_{obj.Type}_T{teamIndex}");
                charObj.transform.SetParent(teamParent, false);
                charObj.transform.localPosition = new Vector3(posX, posY, 0f);

                var character = charObj.AddComponent<MutinyCharacter>();
                character.Initialize(obj.Type, teamIndex, obj.X, obj.Y, obj.Properties);
                levelRoot.Characters.Add(character);

                if (teamIndex == 1 && levelRoot.Team1 != null)
                {
                    levelRoot.Team1.RegisterCharacter(character);
                }
                else if (teamIndex == 2 && levelRoot.Team2 != null)
                {
                    levelRoot.Team2.RegisterCharacter(character);
                }

                // Add visual preview
                Sprite charSprite = ResolveCharacterPreview(obj.Type);
                SpriteRenderer sr = charObj.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    sr = charObj.AddComponent<SpriteRenderer>();
                }
                sr.sortingOrder = CharacterSortingOrder;
                if (charSprite != null)
                {
                    sr.sprite = charSprite;
                }

                MutinyCharacterAnimator characterAnimator = charObj.AddComponent<MutinyCharacterAnimator>();
                characterAnimator.Initialize(obj.Type);
                charObj.AddComponent<MutinyCharacterOverlay>();

                charIndex++;
            }
        }

        public static Sprite ResolveTileSprite(string tileName, int frame = 1)
        {
            MutinyTileCatalog.EnsureInitialized();
            if (!MutinyTileCatalog.TryGetDefinition(tileName, out var def) || def.IsLogic)
                return null;

            string path = def.FrameCount <= 1
                ? $"Art/Tiles/Single/{def.TileName}"
                : $"Art/Tiles/Animated/{def.TileName}/{def.TileName}_{frame:D2}";

            return Resources.Load<Sprite>(path);
        }

        public static Sprite ResolveCharacterPreview(string characterType)
        {
            return Resources.Load<Sprite>($"Art/Characters/Preview/{characterType}");
        }
    }
}
