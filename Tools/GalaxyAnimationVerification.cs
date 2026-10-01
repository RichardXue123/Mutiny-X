using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mutiny.Levels;
using Mutiny.Levels.Editor;
using Mutiny.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutiny.Verification.Editor
{
    [InitializeOnLoad]
    public static class GalaxyAnimationVerification
    {
        const string Key = "Mutiny.GalaxyAnimation";
        static string Output => Path.GetFullPath(Path.Combine(Application.dataPath, "../galaxy-animation-output"));
        static string Report => Path.Combine(Output, "verification.txt");
        static double start, nextCapture;
        static int initialFrame, previousFrame, pictures;
        static bool wrapped;
        static readonly HashSet<int> Frames = new HashSet<int>();
        static Color32[] firstPixels;
        static MutinyLevelController controller;
        static MutinySpaceBackground background;
        static SpriteRenderer galaxy;
        static Bounds originalBounds;
        static int stage;
        static GalaxyAnimationVerification()
        {
            if (SessionState.GetBool(Key, false) && !EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.delayCall += ResumeEditorCheck;
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                { start = EditorApplication.timeSinceStartup; stage = 1; EditorApplication.update += Tick; }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key, false);
                    File.AppendAllText(Report, "PASS all " + SessionState.GetInt(Key + "Checks", 0) + " assertions.\n");
                    EditorApplication.Exit(0);
                }
            };
        }
        static void ResumeEditorCheck()
        {
            if (!SessionState.GetBool(Key, false) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            background = Object.FindAnyObjectByType<MutinySpaceBackground>();
            if (background == null) return;
            initialFrame = background.CurrentAnimationFrame;
            start = EditorApplication.timeSinceStartup; stage = 0;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
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
            File.WriteAllText(Report, "Unity " + Application.unityVersion + "; production Scene/Play/GM + natural Update + GPU captures; 2026-10-01\n");
            SessionState.SetBool(Key, true); SessionState.SetInt(Key + "Checks", 0);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            try
            {
                if (!AssetDatabase.IsValidFolder("Assets/GalaxyVerification")) AssetDatabase.CreateFolder("Assets", "GalaxyVerification");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                const string path = "Assets/GalaxyVerification/Main.unity";
                EditorSceneManager.SaveScene(scene, path);
                Check(MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16), out string error), "Production editor load: " + error);
                EditorSceneManager.SaveScene(scene, path);
                EditorSceneManager.OpenScene(path);
                background = Object.FindAnyObjectByType<MutinySpaceBackground>();
                initialFrame = background.CurrentAnimationFrame;
                start = EditorApplication.timeSinceStartup; stage = 0;
                EditorApplication.update += Tick;
            }
            catch (Exception ex) { Fail(ex); }
        }
        static void Tick()
        {
            try
            {
                double now = EditorApplication.timeSinceStartup;
                if (stage == 0)
                {
                    if (now - start < 0.6) return;
                    Check(background.CurrentAnimationFrame != initialFrame, "Scene preview advances naturally after save/reopen");
                    Check(!background.gameObject.scene.isDirty, "Preview phase does not dirty saved scene");
                    Check(background.transform.Find("Space_GalaxySurface").GetComponentInChildren<SpriteRenderer>().sharedMaterial.shader.name == "Mutiny/GalaxyFlow", "Saved scene uses galaxy animation shader");
                    EditorApplication.update -= Tick;
                    EditorApplication.EnterPlaymode(); return;
                }
                if (stage == 1)
                {
                    if (now - start < 0.4) return;
                    Check(Object.FindAnyObjectByType<MutinyFrontendController>().CurrentPage == MutinyFrontendPage.Title, "Main still starts at menu");
                    var gm = MutinyGMManager.Instance ?? new GameObject("GM").AddComponent<MutinyGMManager>();
                    Check(gm.ExecuteCommand("enterlevel 16"), "Production GM enters space16");
                    controller = Object.FindAnyObjectByType<MutinyLevelController>();
                    background = controller.CurrentLevel.GetComponentInChildren<MutinySpaceBackground>();
                    Check(background.CurrentAnimationFrame == 0, "New level begins at animation frame zero");
                    background.RefreshForCamera(Camera.main);
                    galaxy = background.transform.Find("Space_GalaxySurface").GetComponentInChildren<SpriteRenderer>();
                    Check(galaxy.sharedMaterial.shader.isSupported && !ShaderUtil.ShaderHasError(galaxy.sharedMaterial.shader), "Galaxy shader compiles and is supported on GPU");
                    var camera = Camera.main;
                    var follow = camera.GetComponent<MutinyCameraController>();
                    if (follow != null) follow.enabled = false; // Lock only the review camera; animation uses its normal Update.
                    camera.transform.position = new Vector3(55, -29, -10);
                    camera.orthographicSize = 6.25f;
                    camera.aspect = 1.375f;
                    background.RefreshForCamera(camera);
                    originalBounds = galaxy.bounds;
                    firstPixels = Capture();
                    start = now; nextCapture = now + 0.08; stage = 2; previousFrame = 0;
                    return;
                }
                Frames.Add(background.CurrentAnimationFrame);
                if (background.CurrentAnimationFrame < previousFrame) wrapped = true;
                previousFrame = background.CurrentAnimationFrame;
                if (now >= nextCapture)
                {
                    var pixels = Capture(); nextCapture = now + 0.08;
                    if (pictures == 10)
                    {
                        int changedGalaxy = 0, changedHull = 0;
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            if (pixels[i].Equals(firstPixels[i])) continue;
                            if (i < 768 * 200) changedGalaxy++;
                            if (i >= 768 * 380) changedHull++;
                        }
                        File.AppendAllText(Report, $"INFO Changed pixels: galaxy={changedGalaxy}, upper hull={changedHull}\n");
                        Check(changedGalaxy > 1000, "Actual GPU galaxy pixels change with natural animation");
                        Check(changedHull == 0, "Actual GPU hull region stays static while galaxy flows");
                    }
                }
                if (now - start < 4.5) return;
                Check(wrapped && Frames.Count > 40, "Natural Update traverses frames and wraps through a full 4-second loop; observed=" + Frames.Count);
                Check(galaxy.bounds == originalBounds && controller.CurrentLevel.WaterLevelY == -34 && controller.CurrentLevel.GravityScale == 0.5f,
                    "Animation leaves bounds, fall boundary and gravity fixed");
                Check(controller.CurrentLevel.Team1.Characters.Count == 10 && controller.CurrentLevel.Team2.Characters.Count == 11 && controller.CurrentLevel.Team2.Characters.All(c => c.Luck == 50), "Teams and enemy Luck unchanged");
                Check(Resources.Load<Material>("Art/Space16/GalaxyAnimated").GetFloat("_GalaxyPhase") == 0, "Shared material is not mutated by per-level phase");
                var manager = MutinyGMManager.Instance;
                foreach (string command in new[] { "enterlevel 6", "enterlevel 2_16" })
                {
                    Check(manager.ExecuteCommand(command), "Production " + command);
                    Check(controller.CurrentLevel.GetComponentInChildren<MutinySpaceBackground>() == null && controller.CurrentLevel.GetComponentInChildren<MutinyWaterSurface>().LoadedFrameCount == 10,
                        command + " retains original ocean renderer");
                }
                Check(manager.ExecuteCommand("enterlevel 16") && controller.CurrentLevel.GetComponentInChildren<MutinySpaceBackground>().CurrentAnimationFrame == 0,
                    "GM reentry starts fresh independent animation");
                EditorApplication.update -= Tick;
                EditorApplication.ExitPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        static Color32[] Capture()
        {
            var camera = Camera.main;
            var target = new RenderTexture(768, 560, 24);
            var image = new Texture2D(768, 560, TextureFormat.RGB24, false);
            var active = RenderTexture.active; var oldTarget = camera.targetTexture; var oldRect = camera.rect;
            try
            {
                camera.targetTexture = target; camera.rect = new Rect(0, 0, 1, 1);
                background.RefreshForCamera(camera); camera.Render();
                RenderTexture.active = target; image.ReadPixels(new Rect(0, 0, 768, 560), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(Output, "frame-" + pictures.ToString("D3") + ".png"), image.EncodeToPNG());
                pictures++; return image.GetPixels32();
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
