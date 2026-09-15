using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mutiny.Levels.Editor
{
    public sealed class MutinyTilePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.StartsWith("Assets/Mutiny/Resources/Art/Characters/Preview/", StringComparison.OrdinalIgnoreCase))
            {
                var charImporter = (TextureImporter)assetImporter;
                charImporter.textureType = TextureImporterType.Sprite;
                charImporter.spriteImportMode = SpriteImportMode.Single;
                charImporter.spritePixelsPerUnit = 32f;
                charImporter.filterMode = FilterMode.Point;
                charImporter.textureCompression = TextureImporterCompression.Uncompressed;
                var charSettings = new TextureImporterSettings();
                charImporter.ReadTextureSettings(charSettings);
                charSettings.spriteAlignment = (int)SpriteAlignment.Center;
                charImporter.SetTextureSettings(charSettings);
                return;
            }

            if (!assetPath.StartsWith("Assets/Mutiny/Resources/Art/Tiles/", StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string tileName = fileName;
            if (assetPath.IndexOf("/Animated/", StringComparison.OrdinalIgnoreCase) >= 0 &&
                fileName.Length > 3 && fileName[fileName.Length - 3] == '_')
            {
                tileName = fileName.Substring(0, fileName.Length - 3);
            }

            MutinyTileCatalog.EnsureInitialized();
            if (MutinyTileCatalog.TryGetDefinition(tileName, out var def) && !def.IsLogic)
            {
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = def.Pivot;
                importer.SetTextureSettings(settings);
            }
        }
    }

    public static class MutinyTileImporter
    {
        [MenuItem("Mutiny/Tiles/Configure All Tile Sprites")]
        public static void ConfigureAllTileSprites()
        {
            try
            {
                string csvPath = Path.Combine(Application.dataPath, "Mutiny", "Data", "Tiles", "tile-mapping.csv");
                if (!File.Exists(csvPath))
                {
                    Debug.LogError($"[MutinyTileImporter] Mapping CSV not found: {csvPath}");
                    return;
                }

                MutinyTileCatalog.LoadFromCsv(File.ReadAllText(csvPath));
                var defs = MutinyTileCatalog.Definitions;

                int totalVisual = 0;
                int configuredTextures = 0;
                int missingFiles = 0;

                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var kvp in defs)
                    {
                        var def = kvp.Value;
                        if (def.IsLogic)
                            continue;

                        totalVisual++;

                        if (def.FrameCount <= 1)
                        {
                            string assetPath = $"Assets/Mutiny/Resources/Art/Tiles/Single/{def.TileName}.png";
                            if (ConfigureTexture(assetPath, def.Pivot))
                                configuredTextures++;
                            else
                                missingFiles++;
                        }
                        else
                        {
                            for (int frame = 1; frame <= def.FrameCount; frame++)
                            {
                                string assetPath = $"Assets/Mutiny/Resources/Art/Tiles/Animated/{def.TileName}/{def.TileName}_{frame:D2}.png";
                                if (ConfigureTexture(assetPath, def.Pivot))
                                    configuredTextures++;
                                else
                                    missingFiles++;
                            }
                        }
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                string msg = $"[MutinyTileImporter] Complete. Configured {configuredTextures} textures across {totalVisual} visual tiles. Missing files: {missingFiles}.";
                if (missingFiles > 0)
                    Debug.LogError(msg);
                else
                    Debug.Log(msg);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [MenuItem("Mutiny/Tiles/Validate All Tile Sprites")]
        public static void ValidateAllTileSprites()
        {
            try
            {
                string csvPath = Path.Combine(Application.dataPath, "Mutiny", "Data", "Tiles", "tile-mapping.csv");
                if (!File.Exists(csvPath))
                {
                    Debug.LogError($"[MutinyTileImporter] Mapping CSV not found: {csvPath}");
                    return;
                }

                MutinyTileCatalog.LoadFromCsv(File.ReadAllText(csvPath));
                var defs = MutinyTileCatalog.Definitions;

                int totalChecked = 0;
                int errors = 0;

                foreach (var kvp in defs)
                {
                    var def = kvp.Value;
                    if (def.IsLogic)
                        continue;

                    string checkPath = def.FrameCount <= 1
                        ? $"Assets/Mutiny/Resources/Art/Tiles/Single/{def.TileName}.png"
                        : $"Assets/Mutiny/Resources/Art/Tiles/Animated/{def.TileName}/{def.TileName}_01.png";

                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(checkPath);
                    if (sprite == null)
                    {
                        Debug.LogError($"[MutinyTileImporter] Sprite failed to load: {checkPath}");
                        errors++;
                        continue;
                    }

                    if (Mathf.Abs(sprite.pixelsPerUnit - 32f) > 0.001f)
                    {
                        Debug.LogError($"[MutinyTileImporter] Sprite {def.TileName} PPU is {sprite.pixelsPerUnit}, expected 32!");
                        errors++;
                    }

                    if (sprite.texture.filterMode != FilterMode.Point)
                    {
                        Debug.LogWarning($"[MutinyTileImporter] Sprite {def.TileName} FilterMode is {sprite.texture.filterMode}, expected Point!");
                    }

                    totalChecked++;
                }

                if (errors == 0)
                    Debug.Log($"[MutinyTileImporter] Validation passed! All {totalChecked} visual tiles loaded successfully as 32 PPU Sprites.");
                else
                    Debug.LogError($"[MutinyTileImporter] Validation finished with {errors} errors out of {totalChecked} checked tiles.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static bool ConfigureTexture(string assetPath, Vector2 pivot)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return false;
            }

            bool modified = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                modified = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                modified = true;
            }

            if (Math.Abs(importer.spritePixelsPerUnit - 32f) > 0.001f)
            {
                importer.spritePixelsPerUnit = 32f;
                modified = true;
            }

            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                modified = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                modified = true;
            }

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);

            if (settings.spriteAlignment != (int)SpriteAlignment.Custom ||
                Vector2.Distance(settings.spritePivot, pivot) > 0.0001f)
            {
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = pivot;
                importer.SetTextureSettings(settings);
                modified = true;
            }

            if (modified)
            {
                importer.SaveAndReimport();
            }

            return true;
        }
    }
}
