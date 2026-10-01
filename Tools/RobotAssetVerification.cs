// Copy this file into Assets/Editor in an isolated project, then invoke
// Mutiny.Verification.Editor.RobotAssetVerification.RunBatch with Unity batchmode.
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Mutiny.Levels;
using Mutiny.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class RobotAssetVerification
    {
        const string Pending = "Mutiny.RobotAssets.Pending";
        static readonly StringBuilder Report = new StringBuilder();
        static int checks;
        static RobotAssetVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false)) Verify();
            };
        }
        public static void RunBatch()
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Pending, true);
            EditorApplication.EnterPlaymode();
        }
        static void Check(bool condition, string description)
        {
            if (!condition) throw new Exception(description);
            checks++;
            Report.AppendLine("PASS " + description);
        }
        static void Verify()
        {
            SessionState.EraseBool(Pending);
            Report.AppendLine("Unity " + Application.unityVersion + " isolated batchmode/nographics Play Mode; 2026-10-01");
            bool success = false;
            try
            {
                var advance = typeof(MutinyCharacterAnimator).GetMethod("AdvanceOriginalTick", BindingFlags.Instance | BindingFlags.NonPublic);
                Check(advance != null, "Production animator tick entry exists");
                foreach (string type in new[] { "RobotCaptain", "Robot" })
                {
                    Sprite[] frames = MutinyCharacterAnimator.LoadFrames(type);
                    Check(frames.Length == 35, type + " production loader resolves all 35 frame slots");
                    for (int i = 0; i < frames.Length; i++)
                    {
                        Sprite sprite = frames[i];
                        Check(sprite != null && sprite.texture.width == 32 && sprite.texture.height == 36 &&
                            sprite.texture.filterMode == FilterMode.Point && sprite.texture.mipmapCount == 1 &&
                            Vector2.Distance(sprite.pivot, new Vector2(16, 15)) < 0.001f && sprite.pixelsPerUnit == 32,
                            type + " frame " + (i + 1) + " dimensions, point filtering, no mipmaps, pivot and PPU");
                        string path = "Assets/Mutiny/Resources/Art/Characters/Animations/" + type + "/" + (i + 1) + ".png";
                        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                        Check(importer != null && importer.textureCompression == TextureImporterCompression.Uncompressed &&
                            importer.alphaIsTransparency && importer.spriteImportMode == SpriteImportMode.Single,
                            type + " frame " + (i + 1) + " uncompressed transparent single sprite import");
                    }
                    Sprite preview = MutinyLevelBuilder.ResolveCharacterPreview(type);
                    Check(preview != null && Vector2.Distance(preview.pivot, frames[0].pivot) < 0.001f &&
                        preview.rect.size == frames[0].rect.size, type + " level-builder preview registration matches animation");
                    var host = new GameObject("RobotAssetVerification_" + type);
                    try
                    {
                        var renderer = host.AddComponent<SpriteRenderer>();
                        var animator = host.AddComponent<MutinyCharacterAnimator>();
                        animator.Initialize(type);
                        Check(renderer.sprite == frames[0], type + " Initialize displays idle frame 1");
                        for (int tick = 1; tick <= 12; tick++)
                        {
                            advance.Invoke(animator, null);
                            Check(renderer.sprite == frames[tick % 12], type + " production idle tick " + tick + " skips separator slots");
                        }
                        animator.PlayHit();
                        Check(renderer.sprite == frames[14], type + " PlayHit displays frame 15 immediately");
                        for (int tick = 1; tick <= 20; tick++)
                        {
                            advance.Invoke(animator, null);
                            Check(renderer.sprite == (tick == 20 ? frames[0] : frames[14 + tick]),
                                type + " production settled hit recovery tick " + tick);
                        }
                        animator.PlayHit();
                        Check(renderer.sprite == frames[14], type + " repeated hit restarts the hit pose");
                    }
                    finally { UnityEngine.Object.DestroyImmediate(host); }
                }
                success = true;
            }
            catch (Exception exception) { Report.AppendLine("FAIL " + exception); Debug.LogException(exception); }
            Report.AppendLine((success ? "PASS " : "FAIL ") + checks + " assertions");
            Report.AppendLine("Scope: resource import and production animation/preview entries. No battle placement, audio, manual Unity visual review or mobile build claim.");
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "../robot-asset-verification.txt")), Report.ToString());
            Debug.Log("[ROBOT-ASSETS] " + (success ? "PASS " : "FAIL ") + checks);
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
