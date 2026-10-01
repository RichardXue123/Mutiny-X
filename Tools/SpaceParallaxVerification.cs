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
    public static class SpaceParallaxVerification
    {
        const string Key = "Mutiny.SpaceParallax";
        static string Output => Path.GetFullPath(Path.Combine(Application.dataPath, "../space-parallax-output"));
        static string Report => Path.Combine(Output, "verification.txt");
        static double start;
        static bool pendingAnimation;
        static int animationStart;
        static MutinyLevelController controller;
        static MutinySpaceBackground bg;
        static Camera camera;
        static SpaceParallaxVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                { start = EditorApplication.timeSinceStartup; EditorApplication.update += Tick; }
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
            File.WriteAllText(Report, "Unity " + Application.unityVersion + "; legacy saved scene migration + production editor/GM/physics + real camera/GPU parallax and occlusion; 2026-10-01\n");
            SessionState.SetBool(Key, true); SessionState.SetInt(Key + "Checks", 0);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            try
            {
                // This fixture was saved by the old flat-background verification run.
                EditorSceneManager.OpenScene("Assets/SpaceArtVerification/Main.unity");
                bg = Object.FindAnyObjectByType<MutinySpaceBackground>(); camera = Camera.main;
                bg.RefreshForCamera(camera);
                Check(bg.transform.Find("Space_sky").GetComponent<SpriteRenderer>().sprite.name == "deep_space", "Legacy baked flat sky migrates to body-free deep space");
                Check(bg.GetComponentsInChildren<MutinySpaceParallaxLayer>().Length == 6 &&
                    !bg.transform.Find("Space_galaxy").GetComponent<SpriteRenderer>().enabled, "Legacy flat galaxy disabled; six independent planes created");
                if (!AssetDatabase.IsValidFolder("Assets/SpaceParallaxVerification")) AssetDatabase.CreateFolder("Assets", "SpaceParallaxVerification");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                const string path = "Assets/SpaceParallaxVerification/Main.unity";
                EditorSceneManager.SaveScene(scene, path);
                Check(MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16), out string error), "Production Scene load: " + error);
                EditorSceneManager.SaveScene(scene, path); EditorSceneManager.OpenScene(path);
                bg = Object.FindAnyObjectByType<MutinySpaceBackground>(); camera = Camera.main;
                bg.RefreshForCamera(camera); bg.RefreshForCamera(camera);
                Check(bg.GetComponentsInChildren<MutinySpaceParallaxLayer>().Length == 6, "Saved/reopened scene refresh is idempotent, no duplicate planes");
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        static MutinySpaceParallaxLayer Layer(string name) => bg.transform.Find("Space_" + name).GetComponent<MutinySpaceParallaxLayer>();
        static Dictionary<string, Vector3> Positions(MutinySpaceParallaxLayer layer) => layer.GetComponentsInChildren<SpriteRenderer>()
            .ToDictionary(r => r.name, r => r.transform.position);
        static void SetCamera(float x, float y, float size = 6.25f, float aspect = 1.375f)
        {
            camera.transform.position = new Vector3(x, y, -10); camera.orthographicSize = size; camera.aspect = aspect;
            bg.RefreshForCamera(camera);
        }
        static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup - start < .4) return;
                if (pendingAnimation)
                {
                    Check(bg.CurrentAnimationFrame != animationStart, "Natural Play still advances galaxy foreground animation");
                    EditorApplication.update -= Tick; EditorApplication.ExitPlaymode(); return;
                }
                Check(Object.FindAnyObjectByType<MutinyFrontendController>().CurrentPage == MutinyFrontendPage.Title, "Main still starts with title");
                var gm = MutinyGMManager.Instance ?? new GameObject("GM").AddComponent<MutinyGMManager>();
                Check(gm.ExecuteCommand("enterlevel 16"), "Real GM enters16");
                controller = Object.FindAnyObjectByType<MutinyLevelController>();
                bg = controller.CurrentLevel.GetComponentInChildren<MutinySpaceBackground>(); camera = Camera.main;
                var follow = camera.GetComponent<MutinyCameraController>(); if (follow != null) follow.enabled = false;
                SetCamera(32, -18);
                Check(bg.GetComponentsInChildren<MutinySpaceParallaxLayer>().Length == 6, "Stars, sun, moon, ring planet, far galaxy and surface are separate planes");
                foreach (string name in new[] { "FarStars", "Sun", "Moon", "RingPlanet", "GalaxyFar", "GalaxySurface" })
                {
                    SetCamera(32, -18);
                    var layer = Layer(name); var before = Positions(layer);
                    Vector2 delta = new Vector2(3.25f, -2f);
                    SetCamera(32 + delta.x, -18 + delta.y);
                    var after = Positions(layer); int matched = 0;
                    foreach (var pair in before)
                    {
                        if (!after.TryGetValue(pair.Key, out var position)) continue;
                        Vector2 screenDelta = (Vector2)(position - pair.Value) - delta;
                        Check(Vector2.Distance(screenDelta, -Vector2.Scale(delta, layer.Parallax)) < .002f,
                            name + " actual camera-relative displacement matches depth " + layer.Parallax);
                        matched++;
                    }
                    Check(matched > 0, name + " keeps stable repeat cells across camera move");
                }
                SetCamera(32, -18);
                Check(Layer("GalaxySurface").GetComponentsInChildren<SpriteRenderer>().All(r => r.sortingOrder == 300), "Near galaxy uses ocean foreground order300");
                Check(Layer("GalaxyFar").GetComponentsInChildren<SpriteRenderer>().All(r => r.sortingOrder == -20), "Far galaxy stays behind ship/characters at -20");
                foreach (var p in new[] { new Vector2(0,0), new Vector2(115,0), new Vector2(0,-36), new Vector2(115,-36), new Vector2(57.5f,-18) })
                {
                    SetCamera(p.x,p.y,6.25f,3.2f);
                    var sky = bg.transform.Find("Space_sky").GetComponent<SpriteRenderer>();
                    float halfW = camera.orthographicSize * camera.aspect;
                    foreach (var corner in new[] { new Vector3(p.x-halfW+.01f,p.y-6.24f,0), new Vector3(p.x+halfW-.01f,p.y+6.24f,0) })
                    {
                        Check(sky.bounds.Contains(corner), "Opaque base covers wide-view corner at " + p);
                        Check(Layer("FarStars").GetComponentsInChildren<SpriteRenderer>().Any(r => r.bounds.Contains(corner)), "Starfield repeats cover corner at " + p);
                    }
                }
                SetCamera(57.5f,-18,20,2.875f); Capture("overview", 1840,640);
                SetCamera(32,-31);
                var surfaceSize = Layer("GalaxySurface").GetComponentsInChildren<SpriteRenderer>()[0].bounds.size;
                SetCamera(80,-31,6.25f,3.2f);
                Check(Vector3.Distance(surfaceSize, Layer("GalaxySurface").GetComponentsInChildren<SpriteRenderer>()[0].bounds.size) < .001f,
                    "Foreground galaxy retains world size under wide viewport; no viewport stretching");
                // Repeat seam crossings retain cell positions instead of shifting a whole plane.
                SetCamera(31.99f,-18); var seamBefore = Positions(Layer("RingPlanet"));
                SetCamera(32.01f,-18); var seamAfter = Positions(Layer("RingPlanet"));
                Check(seamBefore.Any(pair => seamAfter.ContainsKey(pair.Key) && Mathf.Abs(seamAfter[pair.Key].x - pair.Value.x - .013f) < .002f), "Repeat crossing has continuous visible celestial positions");

                var root = controller.CurrentLevel; root.EnsureRuntimeWater();
                var actor = root.Team1.Characters[0]; var body = actor.PhysicsBody;
                body.State.X = 32*32; body.State.Y = body.WaterPixelY-3; body.SetVelocity(0,4);
                body.AdvanceSimulationTick();
                var splash = root.GetComponentInChildren<MutinySplashEffect>();
                Check(actor.IsDrowned && splash != null && splash.IsSpaceSplash && splash.GetComponent<SpriteRenderer>().sortingOrder == 299,
                    "Production character entry creates galaxy splash behind surface, matching ocean order299/300");
                for(int i=0;i<30;i++) body.AdvanceSimulationTick();
                actor.transform.position = MutinyPhysics.PixelToUnity(body.State.X,body.State.Y);
                SetCamera(32,-34);
                var actorRenderer=actor.GetComponent<SpriteRenderer>();
                var submerged = Capture(null); actorRenderer.enabled=false; var hidden=Capture(null); actorRenderer.enabled=true;
                Check(Different(submerged,hidden)==0, "GPU: fully submerged character is occluded by foreground surface");
                var surfaces=Layer("GalaxySurface").GetComponentsInChildren<SpriteRenderer>();
                foreach(var r in surfaces) r.enabled=false;
                var exposed=Capture(null); actorRenderer.enabled=false; var noActor=Capture(null); actorRenderer.enabled=true;
                Check(Different(exposed,noActor)>20, "GPU control: same character is visible when foreground alone is hidden");
                foreach(var r in surfaces) r.enabled=true;
                Check(gm.ExecuteCommand("enterlevel 16"), "Fresh16 after immersion test");
                bg=controller.CurrentLevel.GetComponentInChildren<MutinySpaceBackground>();
                for(int i=0;i<60;i++)
                {
                    float t=(1-Mathf.Cos(i*2*Mathf.PI/60))*.5f;
                    SetCamera(26+16*t,-14+1.5f*t); Capture("pan-"+i.ToString("D3"));
                }
                for(int i=0;i<40;i++)
                {
                    float t=(1-Mathf.Cos(i*2*Mathf.PI/40))*.5f;
                    SetCamera(28+10*t,-30-2*t); Capture("depth-"+i.ToString("D3"));
                }
                foreach(string command in new[]{"enterlevel 6","enterlevel 2_16"})
                {
                    Check(gm.ExecuteCommand(command),"Real GM "+command);
                    Check(controller.CurrentLevel.GetComponentInChildren<MutinySpaceBackground>()==null && controller.CurrentLevel.GetComponentInChildren<MutinyWaterSurface>().LoadedFrameCount==10,
                        command+" retains original background and ocean");
                }
                Check(gm.ExecuteCommand("enterlevel 16"),"Reentry16");
                bg=controller.CurrentLevel.GetComponentInChildren<MutinySpaceBackground>(); SetCamera(32,-31);
                root=controller.CurrentLevel;
                Check(root.Characters.Count==21 && root.TerrainHolder.childCount==712 && root.GravityScale==.5f && root.WaterLevelY==-34 && root.Team2.Characters.All(c=>c.Luck==50),
                    "Geometry, crews, gravity, water boundary and Luck unchanged");
                Check(bg.GetComponentsInChildren<MutinySpaceParallaxLayer>().Length==6,"Reentry has exactly six planes");
                animationStart=bg.CurrentAnimationFrame;start=EditorApplication.timeSinceStartup;pendingAnimation=true;
            }
            catch(Exception ex){Fail(ex);}
        }
        static int Different(Color32[] a,Color32[] b){int n=0;for(int i=0;i<a.Length;i++)if(!a[i].Equals(b[i]))n++;return n;}
        static Color32[] Capture(string name,int width=660,int height=480)
        {
            var rt=new RenderTexture(width,height,24);var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            var active=RenderTexture.active;var oldTarget=camera.targetTexture;var oldRect=camera.rect;
            try
            {
                camera.targetTexture=rt;camera.rect=new Rect(0,0,1,1);bg.RefreshForCamera(camera);camera.Render();
                RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                if(name!=null)File.WriteAllBytes(Path.Combine(Output,name+".png"),image.EncodeToPNG());
                return image.GetPixels32();
            }
            finally{RenderTexture.active=active;camera.targetTexture=oldTarget;camera.rect=oldRect;Object.DestroyImmediate(image);rt.Release();Object.DestroyImmediate(rt);}
        }
        static void Fail(Exception ex){EditorApplication.update-=Tick;SessionState.SetBool(Key,false);File.AppendAllText(Report,"FAIL "+ex+"\n");Debug.LogException(ex);EditorApplication.Exit(1);}
    }
}
