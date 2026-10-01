using System;
using System.IO;
using System.Linq;
using Mutiny.Levels;
using Mutiny.Levels.Editor;
using Mutiny.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class Space16ArtVerification
    {
        const string Key = "Mutiny.SpaceArt.Running";
        static int frames;
        static string Output => Path.GetFullPath(Path.Combine(Application.dataPath, "../space-art-output"));
        static string Report => Path.Combine(Output, "verification.txt");
        static Space16ArtVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode) { frames = 0; EditorApplication.update += Tick; }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key, false);
                    File.AppendAllText(Report, "PASS all " + SessionState.GetInt(Key + ".Checks", 0) + " assertions.\n");
                    EditorApplication.Exit(0);
                }
            };
        }
        static void Check(bool ok, string message)
        {
            if (!ok) throw new Exception(message);
            SessionState.SetInt(Key + ".Checks", SessionState.GetInt(Key + ".Checks", 0) + 1);
            File.AppendAllText(Report, "PASS " + message + "\n");
        }
        public static void RunBatch()
        {
            Directory.CreateDirectory(Output);
            File.WriteAllText(Report, "Unity " + Application.unityVersion + " production Scene load/save/reopen/Play/GM; GPU camera renders; 2026-10-01\n");
            SessionState.SetBool(Key, true);
            SessionState.SetInt(Key + ".Checks", 0);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            try
            {
                foreach (string name in new[] { "hull_a", "hull_b", "edge_top", "edge_bottom", "edge_left", "edge_right", "interior", "cannon", "sky", "galaxy" })
                {
                    var sprite = Resources.Load<Sprite>(MutinySpaceVisuals.ResourcePath + name);
                    File.AppendAllText(Report, sprite == null ? "INFO missing Sprite: " + name + "\n" :
                        $"INFO {name}: ppu={sprite.pixelsPerUnit} filter={sprite.texture.filterMode} mips={sprite.texture.mipmapCount} pivot={sprite.pivot} rect={sprite.rect}\n");
                    Check(sprite != null && sprite.pixelsPerUnit == 32 && sprite.texture.filterMode == FilterMode.Point &&
                        sprite.texture.mipmapCount == 1 && sprite.pivot.x == 0 && sprite.pivot.y == sprite.rect.height,
                        "Imported pixel art, pivot and PPU: " + name);
                    if (name != "sky" && name != "galaxy") Check(sprite.rect.size == new Vector2(32, 32), "Tile dimensions: " + name);
                }
                if (!AssetDatabase.IsValidFolder("Assets/SpaceArtVerification")) AssetDatabase.CreateFolder("Assets", "SpaceArtVerification");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                const string path = "Assets/SpaceArtVerification/Main.unity";
                EditorSceneManager.SaveScene(scene, path);
                Check(MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16), out string error), "Editor production Load16: " + error);
                ValidateSpace(Object.FindAnyObjectByType<MutinyLevelController>().CurrentLevel, "editor");
                EditorSceneManager.SaveScene(scene, path);
                EditorSceneManager.OpenScene(path);
                var baked = Object.FindAnyObjectByType<MutinyLevelRoot>();
                Check(baked.VisualTheme == "space" && baked.GetComponentInChildren<MutinySpaceBackground>() != null,
                    "Theme and background component survive saved-scene reopen");
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        static void ValidateSpace(MutinyLevelRoot root, string phase)
        {
            Check(root != null && root.VisualTheme == "space", phase + " XML -> root theme");
            var sprites = root.TerrainHolder.GetComponentsInChildren<SpriteRenderer>();
            Check(sprites.Length == 712 && root.TerrainHolder.GetComponentsInChildren<BoxCollider2D>().Length == 712,
                phase + " 712 original solid cells and colliders retained");
            Check(sprites.All(sr => AssetDatabase.GetAssetPath(sr.sprite).Contains("/Art/Space16/")), phase + " all terrain uses space resources");
            foreach (string edge in new[] { "edge_top", "edge_bottom", "edge_left", "edge_right", "cannon" })
                Check(sprites.Any(sr => sr.sprite.name == edge), phase + " edge/cannon present: " + edge);
            var bg = root.BackgroundHolder.GetComponentsInChildren<SpriteRenderer>();
            Check(bg.Any(sr => sr.sprite.name == "interior") && bg.Any(sr => sr.sprite.name == "cannon"), phase + " recessed interiors and background gun ports");
            Check(root.GetComponentInChildren<MutinyBattleBackground>() == null && bg.Any(sr => sr.sprite.name == "deep_space") &&
                bg.Any(sr => sr.sprite.name == "galaxy") && bg.Any(sr => sr.sprite.name == "galaxy_surface"),
                phase + " layered space and foreground galaxy replace original clouds/mountains/ocean");
            root.EnsureRuntimeWater();
            Check(root.GetComponentInChildren<MutinyWaterSurface>() == null && root.WaterLevelY == -34 &&
                root.WaterHolder.Find("WaterTrigger").GetComponent<BoxCollider2D>().isTrigger,
                phase + " EnsureRuntimeWater preserves galaxy, trigger and fall boundary; no ocean recreated");
            Check(root.Width == 115 && root.Height == 36 && root.GravityScale == 0.5f && root.Team1.Characters.Count == 10 &&
                root.Team2.Characters.Count == 11 && root.Team2.Characters.All(c => c.Luck == 50), phase + " layout, teams, gravity and Luck unchanged");
        }
        static void Tick()
        {
            if (++frames < 20) return;
            EditorApplication.update -= Tick;
            try
            {
                var frontend = Object.FindAnyObjectByType<MutinyFrontendController>();
                Check(frontend != null && frontend.CurrentPage == MutinyFrontendPage.Title, "Main still enters Title");
                var gm = MutinyGMManager.Instance ?? new GameObject("GM").AddComponent<MutinyGMManager>();
                var controller = Object.FindAnyObjectByType<MutinyLevelController>();
                Check(gm.ExecuteCommand("enterlevel 16"), "Real GM enterlevel 16");
                ValidateSpace(controller.CurrentLevel, "play");
                Capture(controller.CurrentLevel, "overview", new Vector3(57.5f, -18, -10), 20, 1840, 640);
                Capture(controller.CurrentLevel, "deck-detail", new Vector3(32, -18.5f, -10), 6.25f, 1100, 800);
                Capture(controller.CurrentLevel, "galaxy-detail", new Vector3(55, -29, -10), 6.25f, 1100, 800);
                foreach (string command in new[] { "enterlevel 6", "enterlevel 2_16" })
                {
                    Check(gm.ExecuteCommand(command), "Real GM " + command);
                    var root = controller.CurrentLevel;
                    Check(root.GetComponentInChildren<MutinySpaceBackground>() == null &&
                        root.GetComponentInChildren<MutinyBattleBackground>() != null &&
                        root.GetComponentInChildren<MutinyWaterSurface>()?.LoadedFrameCount == 10 && root.GravityScale == 1,
                        command + " retains original sky/ocean/gravity");
                    Check(root.TerrainHolder.GetComponentsInChildren<SpriteRenderer>().All(sr => !AssetDatabase.GetAssetPath(sr.sprite).Contains("/Art/Space16/")), command + " retains original terrain");
                }
                Check(gm.ExecuteCommand("enterlevel 16") && controller.CurrentLevel.NextRobotVoiceNumber == 1, "GM reentry restores space and fresh voice sequence");
                ValidateSpace(controller.CurrentLevel, "reentry");
                EditorApplication.ExitPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        static void Capture(MutinyLevelRoot root, string name, Vector3 position, float ortho, int width, int height)
        {
            var camera = Camera.main;
            Vector3 previous = camera.transform.position;
            float oldOrtho = camera.orthographicSize;
            Rect oldRect = camera.rect;
            var target = new RenderTexture(width, height, 24);
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            var oldActive = RenderTexture.active;
            var oldTarget = camera.targetTexture;
            try
            {
                camera.targetTexture = target;
                camera.rect = new Rect(0, 0, 1, 1);
                camera.aspect = (float)width / height;
                camera.transform.position = position;
                camera.orthographicSize = ortho;
                root.GetComponentInChildren<MutinySpaceBackground>().RefreshForCamera(camera);
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.Combine(Output, name + ".png"), image.EncodeToPNG());
                Check(image.GetPixels32().Select(p => (int)p.r << 16 | p.g << 8 | p.b).Distinct().Count() > 100,
                    "GPU production camera image contains rendered art: " + name);
            }
            finally
            {
                RenderTexture.active = oldActive;
                camera.targetTexture = oldTarget;
                camera.rect = oldRect;
                camera.ResetAspect();
                camera.transform.position = previous;
                camera.orthographicSize = oldOrtho;
                root.GetComponentInChildren<MutinySpaceBackground>().RefreshForCamera(camera);
                Object.DestroyImmediate(image); target.Release(); Object.DestroyImmediate(target);
            }
        }
        static void Fail(Exception ex)
        {
            SessionState.SetBool(Key, false);
            File.AppendAllText(Report, "FAIL " + ex + "\n");
            Debug.LogException(ex); EditorApplication.Exit(1);
        }
    }
}
