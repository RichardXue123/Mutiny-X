using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mutiny.Levels;
using Mutiny.Levels.Editor;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class GalaxySplashVerification
    {
        const string Key = "Mutiny.GalaxySplash";
        static string Output => Path.GetFullPath(Path.Combine(Application.dataPath, "../galaxy-splash-output"));
        static string Report => Path.Combine(Output, "verification.txt");
        static double start, nextCapture;
        static int stage, pictures;
        static MutinyLevelController controller;
        static MutinyCharacter falling;
        static readonly HashSet<string> NaturalPoses = new HashSet<string>();
        static GalaxySplashVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                { stage = 0; start = EditorApplication.timeSinceStartup; EditorApplication.update += Tick; }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key, false);
                    File.AppendAllText(Report, "PASS all " + SessionState.GetInt(Key + "Checks", 0) + " assertions.\n");
                    EditorApplication.Exit(0);
                }
            };
        }
        static void Check(bool ok, string message)
        {
            if (!ok) throw new Exception(message);
            SessionState.SetInt(Key + "Checks", SessionState.GetInt(Key + "Checks", 0) + 1);
            File.AppendAllText(Report, "PASS " + message + "\n");
        }
        public static void RunBatch()
        {
            Directory.CreateDirectory(Output);
            File.WriteAllText(Report, "Unity " + Application.unityVersion + "; production GM / Character / CherryBomb physics / splash timer / GPU captures; 2026-10-01\n");
            SessionState.SetBool(Key, true); SessionState.SetInt(Key + "Checks", 0);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            try
            {
                for (int i = 1; i <= 6; i++)
                {
                    var sprite = Resources.Load<Sprite>($"Art/Space16/Splash/{i:D2}");
                    Check(sprite != null && sprite.rect.size == new Vector2(64, 64) && sprite.pivot == new Vector2(32, 12)
                        && sprite.pixelsPerUnit == 32 && sprite.texture.filterMode == FilterMode.Point,
                        "64px transparent pose with impact pivot/PPU/Point: " + i);
                }
                if (!AssetDatabase.IsValidFolder("Assets/GalaxySplashVerification")) AssetDatabase.CreateFolder("Assets", "GalaxySplashVerification");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, "Assets/GalaxySplashVerification/Main.unity");
                Check(MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16), out string error), "Production Scene load: " + error);
                EditorSceneManager.SaveScene(scene);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        // Initial kinematic fixture only. No splash/drowning flags are set, and all
        // crossing, death and presentation transitions run through production physics.
        static void PlaceAboveBoundary(MutinyPhysicsBody body, float gap = 3)
        {
            body.State.X = 64;
            body.State.Y = body.WaterPixelY - gap;
            body.SetVelocity(0, 4);
            body.transform.position = MutinyPhysics.PixelToUnity(body.State.X, body.State.Y);
        }
        static void Tick()
        {
            try
            {
                double now = EditorApplication.timeSinceStartup;
                if (stage == 0)
                {
                    if (now - start < 0.3) return;
                    var gm = MutinyGMManager.Instance ?? new GameObject("GM").AddComponent<MutinyGMManager>();
                    Check(gm.ExecuteCommand("enterlevel 16"), "GM enters space16");
                    controller = Object.FindAnyObjectByType<MutinyLevelController>();
                    var root = controller.CurrentLevel;
                    root.EnsureRuntimeWater();
                    var actor = root.Team1.Characters[0];
                    PlaceAboveBoundary(actor.PhysicsBody);
                    actor.PhysicsBody.AdvanceSimulationTick();
                    var effects = root.GetComponentsInChildren<MutinySplashEffect>();
                    Check(effects.Length == 1 && effects[0].IsSpaceSplash, "Real character crossing produces exactly one galaxy splash");
                    var effect = effects[0];
                    Check(effect.transform.position == MutinyPhysics.PixelToUnity(actor.PhysicsBody.State.X, actor.PhysicsBody.WaterPixelY), "Impact is at actual crossing X and boundary Y");
                    Check(actor.IsDrowned && !actor.IsAlive && actor.Health == 0, "Real water evaluation still drowns character");
                    var renderer = effect.GetComponent<SpriteRenderer>();
                    var spaceBackground = root.GetComponentInChildren<MutinySpaceBackground>();
                    spaceBackground.RefreshForCamera(Camera.main);
                    Check(renderer.sortingOrder > spaceBackground.transform.Find("Space_GalaxyFar").GetComponentInChildren<SpriteRenderer>().sortingOrder &&
                        renderer.sortingOrder < spaceBackground.transform.Find("Space_GalaxySurface").GetComponentInChildren<SpriteRenderer>().sortingOrder,
                        "Splash renders between far galaxy and foreground surface, like ocean");
                    for (int i = 0; i < 4; i++) actor.PhysicsBody.AdvanceSimulationTick();
                    Check(root.GetComponentsInChildren<MutinySplashEffect>().Length == 1, "Further underwater motion does not repeat entry splash");
                    for (int frame = 1; frame <= 18; frame++)
                    {
                        string expected = ((frame - 1) / 3 + 1).ToString("D2");
                        Check(effect.CurrentSourceFrame == frame && renderer.enabled && renderer.sprite.name == expected,
                            "Production splash display step " + frame + " -> pose " + expected);
                        effect.AdvanceOriginalTickForVerification();
                    }
                    Check(!renderer.enabled, "After 18 display steps effect hides before deferred destroy");
                    Object.DestroyImmediate(effect.gameObject);

                    var owner = root.Team1.Characters[1];
                    var weapon = MutinyWeaponFactory.SpawnAndFire("cherryBomb", owner, new Vector2(0, 4));
                    Check(weapon != null && weapon.IsFired, "Real weapon factory fires cherry bomb");
                    PlaceAboveBoundary(weapon.PhysicsBody);
                    weapon.PhysicsBody.AdvanceSimulationTick();
                    effects = root.GetComponentsInChildren<MutinySplashEffect>();
                    Check(effects.Length == 1 && effects[0].IsSpaceSplash && !weapon.IsFinished, "Real bomb crossing uses galaxy splash without exploding on entry");
                    var oldSplash = effects[0];
                    Object.DestroyImmediate(weapon.gameObject); // End the test projectile, retaining its splash for the unload check.
                    Check(gm.ExecuteCommand("enterlevel 6"), "Switch to original level6 with splash still active");
                    Check(oldSplash == null || !oldSplash.gameObject.activeInHierarchy, "Level change immediately removes old galaxy splash from view");
                    root = controller.CurrentLevel;
                    root.EnsureRuntimeWater();
                    actor = root.Team1.Characters[0];
                    PlaceAboveBoundary(actor.PhysicsBody);
                    actor.PhysicsBody.AdvanceSimulationTick();
                    var original = Object.FindObjectsByType<MutinySplashEffect>(FindObjectsSortMode.None).Single(e => !e.IsSpaceSplash);
                    Check(original.CurrentSourceFrame == 25 && AssetDatabase.GetAssetPath(original.GetComponent<SpriteRenderer>().sprite.texture).Contains("/Effects/Splash/"),
                        "Real level6 character still uses original sky2 splash sequence");
                    Object.DestroyImmediate(original.gameObject);
                    Check(gm.ExecuteCommand("enterlevel 16"), "Return to space16 for natural animation recording");
                    root = controller.CurrentLevel; root.EnsureRuntimeWater();
                    Check(root.Characters.Count == 21 && root.GravityScale == 0.5f && root.Team2.Characters.All(c => c.Luck == 50) && root.NextRobotVoiceNumber == 1,
                        "Teams, gravity, Luck and robot voice count unchanged");
                    var camera = Camera.main;
                    var follow = camera.GetComponent<MutinyCameraController>();
                    if (follow != null) follow.enabled = false;
                    camera.transform.position = new Vector3(2, -33.3f, -10); camera.orthographicSize = 2.7f; camera.aspect = 1.333333f;
                    start = now; stage = 1;
                    return;
                }
                if (stage == 1)
                {
                    // Let existing one-shot test smoke expire before recording the new fall.
                    if (now - start < 1) return;
                    falling = controller.CurrentLevel.Team1.Characters[0];
                    PlaceAboveBoundary(falling.PhysicsBody, 36);
                    start = now; nextCapture = now; stage = 2;
                }
                foreach (var effect in controller.CurrentLevel.GetComponentsInChildren<MutinySplashEffect>())
                    NaturalPoses.Add(effect.GetComponent<SpriteRenderer>().sprite.name);
                if (now >= nextCapture) { Capture(); nextCapture = now + 0.04; }
                if (now - start < 1.6) return;
                Check(NaturalPoses.Count == 6, "Natural Update rendered every pose after actual falling character crossed boundary");
                Check(falling.IsDrowned && controller.CurrentLevel.GetComponentsInChildren<MutinySplashEffect>().Length == 0,
                    "Natural playback finishes and removes effect; character remains drowned");
                Check(pictures >= 20, "GPU recorded natural animation frames: " + pictures);
                EditorApplication.update -= Tick; EditorApplication.ExitPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        static void Capture()
        {
            var camera = Camera.main;
            var target = new RenderTexture(640, 480, 24);
            var image = new Texture2D(640, 480, TextureFormat.RGB24, false);
            var active = RenderTexture.active; var oldTarget = camera.targetTexture; var oldRect = camera.rect;
            try
            {
                camera.targetTexture = target; camera.rect = new Rect(0, 0, 1, 1);
                controller.CurrentLevel.GetComponentInChildren<MutinySpaceBackground>().RefreshForCamera(camera);
                camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 640, 480), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(Output, "frame-" + pictures.ToString("D3") + ".png"), image.EncodeToPNG()); pictures++;
            }
            finally
            { RenderTexture.active = active; camera.targetTexture = oldTarget; camera.rect = oldRect; Object.DestroyImmediate(image); target.Release(); Object.DestroyImmediate(target); }
        }
        static void Fail(Exception ex)
        {
            EditorApplication.update -= Tick; SessionState.SetBool(Key, false);
            File.AppendAllText(Report, "FAIL " + ex + "\n"); Debug.LogException(ex); EditorApplication.Exit(1);
        }
    }
}
