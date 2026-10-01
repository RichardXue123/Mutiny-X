using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public static class Level16FreighterBayVerification
    {
        const string Key = "Mutiny.Level16FreighterBay";
        static string Report => Path.GetFullPath(Path.Combine(Application.dataPath, "../level16-freighter-bay-verification.txt"));
        static MutinyLevelController controller;
        static MutinyGMManager gm;
        static MutinyLevelData data;
        static double start;
        static Level16FreighterBayVerification()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                { start = EditorApplication.timeSinceStartup; EditorApplication.update += Verify; }
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
            File.WriteAllText(Report, "Unity " + Application.unityVersion + "; production GM, editor scene load, player jump / AI ExecuteMove, actual PhysicsBody ticks; 2026-10-01\n");
            SessionState.SetBool(Key, true); SessionState.SetInt(Key + "Checks", 0);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            try
            {
                if (!AssetDatabase.IsValidFolder("Assets/Level16FreighterBayVerification")) AssetDatabase.CreateFolder("Assets", "Level16FreighterBayVerification");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, "Assets/Level16FreighterBayVerification/Main.unity");
                Check(MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16), out string error), "Editor production scene load: " + error);
                var root = Object.FindAnyObjectByType<MutinyLevelRoot>();
                Check(root.Width == 80 && root.Height == 27 && root.Characters.Count == 13, "Editor scene contains new 80x27 / 13-member map");
                EditorSceneManager.SaveScene(scene); EditorSceneManager.OpenScene(scene.path);
                Check(Object.FindAnyObjectByType<MutinyLevelRoot>().Team2.Characters.Count == 8, "Saved/reopened scene retains new enemies");
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        static MutinyLevelRoot Fresh()
        {
            if (!gm.ExecuteCommand("enterlevel 16")) throw new Exception("GM enter failed");
            var root = controller.CurrentLevel;
            foreach (var actor in root.Characters) { if (!actor.PhysicsBody.TryGetTerrain(out _, out _, out _)) throw new Exception("Production terrain cache unavailable"); for (int tick = 0; tick < 50; tick++) actor.PhysicsBody.AdvanceSimulationTick(); }
            root.GetComponent<MutinyTurnManager>().StartGame();
            return root;
        }
        static bool Predict(PhysicsBodyState state, Vector2 velocity, out Vector2 landing)
        {
            state.VelocityX = velocity.x; state.VelocityY = velocity.y;
            for (int tick = 0; tick < 180; tick++)
            {
                MutinyPhysics.Step(ref state, data.Terrain, data.Width, data.Height);
                if (state.Y >= 24 * 32 || state.X < 0 || state.X > data.Width * 32) break;
                if (tick > 2 && state.IsAtRest) { landing = new Vector2(state.X, state.Y); return true; }
            }
            landing = new Vector2(state.X, state.Y); return false;
        }
        static bool Find(PhysicsBodyState state, float force, int direction, Func<Vector2, bool> accept, out Vector2 velocity)
        {
            for (int angle = 5; angle <= 85; angle++)
            {
                var v = new Vector2(direction * force * Mathf.Cos(angle * Mathf.Deg2Rad), -force * Mathf.Sin(angle * Mathf.Deg2Rad));
                if (Predict(state, v, out var landing) && accept(landing)) { velocity = v; return true; }
            }
            velocity = default; return false;
        }
        static bool Floor(Vector2 p, int row) => Mathf.Abs(p.y - (row * 32 - 8 - .1f)) < .2f;
        static void Launch(MutinyLevelRoot root, MutinyCharacter actor, Vector2 velocity)
        {
            if (actor.TeamIndex == 1)
            {
                var input = root.GetComponent<MutinyPlayerInput>();
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                // Run the production frame sync after batched fixed ticks, so
                // pointer selection sees the current, landed Transform position.
                typeof(MutinyPhysicsBody).GetMethod("AdvanceSimulationFrameForVerification", flags).Invoke(actor.PhysicsBody, new object[] { 0f });
                var startPx = new Vector2(actor.PhysicsBody.State.X, actor.PhysicsBody.State.Y);
                bool selected = (bool)typeof(MutinyPlayerInput).GetMethod("TrySelectCharacterForVerification", flags).Invoke(input, new object[] { root.Team1, startPx });
                Check(selected && input.SelectCharacterThrow(), "Production player selects jump: " + actor.name);
                bool committed = (bool)typeof(MutinyPlayerInput).GetMethod("TryCommitCharacterThrow", flags).Invoke(input, new object[] { actor, startPx, startPx - velocity * 4 });
                Check(committed && !actor.CanThrow && actor.IsSelfThrown, "Production player submits actual capped Twang");
            }
            else
            {
                var ai = root.Team2.GetComponent<MutinyAIController>();
                var move = new AIMove { Character = actor, MoveType = AIMoveType.SelfThrow, LaunchVelocity = velocity };
                typeof(MutinyAIController).GetMethod("ExecuteMove", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ai, new object[] { move });
                Check(!actor.CanThrow && actor.IsSelfThrown, "Production AI ExecuteMove submits jump: " + actor.name);
            }
        }
        static Vector2 Settle(MutinyCharacter actor)
        {
            for (int tick = 0; tick < 180; tick++)
            {
                actor.PhysicsBody.AdvanceSimulationTick();
                if (tick > 2 && (actor.PhysicsBody.IsAtRest || actor.IsDrowned)) break;
            }
            return new Vector2(actor.PhysicsBody.State.X, actor.PhysicsBody.State.Y);
        }
        static bool FindAny(PhysicsBodyState state, Func<Vector2, bool> accept, out Vector2 velocity)
        {
            foreach (float force in new[] { 8f, 12f, 16f, 20f })
                if (Find(state, force, 1, accept, out velocity) || Find(state, force, -1, accept, out velocity)) return true;
            velocity = default; return false;
        }
        static void Capture(MutinyLevelRoot root)
        {
            var camera = Camera.main;
            if (camera == null) camera = new GameObject("Preview Camera", typeof(Camera)).GetComponent<Camera>();
            var follow = camera.GetComponent<MutinyCameraController>();
            if (follow != null) follow.enabled = false;
            camera.orthographic = true; camera.orthographicSize = 13.5f;
            camera.aspect = 80f / 27f; camera.transform.position = new Vector3(40, -13.5f, -10);
            var bg = root.GetComponentInChildren<MutinySpaceBackground>();
            bg.RefreshForCamera(camera);
            var rt = new RenderTexture(1600, 540, 24);
            var tex = new Texture2D(1600, 540, TextureFormat.RGB24, false);
            var previous = RenderTexture.active; var target = camera.targetTexture; var rect = camera.rect;
            try
            {
                camera.targetTexture = rt; camera.rect = new Rect(0, 0, 1, 1); camera.Render(); RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0,0,1600,540),0,0); tex.Apply();
                File.WriteAllBytes(Path.GetFullPath(Path.Combine(Application.dataPath, "../level16-freighter-bay-preview.png")), tex.EncodeToPNG());
            }
            finally { camera.targetTexture = target; camera.rect = rect; RenderTexture.active = previous; Object.DestroyImmediate(tex); rt.Release(); Object.DestroyImmediate(rt); }
        }
        static void Route(string name, params Vector3[] stops)
        {
            var root = Fresh(); var actor = root.Team1.Characters[2];
            foreach (var stop in stops)
            {
                bool Accept(Vector2 p) => p.x >= stop.x * 32 + 6 && p.x <= (stop.y + 1) * 32 - 6 && Floor(p, (int)stop.z);
                Check(FindAny(actor.PhysicsBody.State, Accept, out var v), name + " reachable platform " + stop);
                // Start a fresh production turn for each movement-only fixture.
                // No action flags or collision arrays are rewritten by the test.
                root.GetComponent<MutinyTurnManager>().StartGame();
                Launch(root, actor, v); var landed = Settle(actor);
                File.AppendAllText(Report, "ROUTE " + name + " launch=" + v + " landing=" + landed + "\n");
                Check(actor.IsAlive && Accept(landed), name + " actual landing " + landed);
            }
        }
        static void Verify()
        {
            if (EditorApplication.timeSinceStartup - start < .5) return;
            EditorApplication.update -= Verify;
            try
            {
                controller = Object.FindAnyObjectByType<MutinyLevelController>();
                gm = MutinyGMManager.Instance ?? new GameObject("GM").AddComponent<MutinyGMManager>();
                var xml = Resources.Load<TextAsset>("Data/Levels/level_1_16"); data = MutinyLevelXmlParser.Parse(xml.text, xml.name);
                Check(xml.text == File.ReadAllText(Path.Combine(Application.dataPath, "Mutiny/Data/Levels/level_1_16.xml")), "Authoring/runtime XML identical");
                var root = Fresh();
                Check(root.Width == 80 && root.Height == 27 && root.Team1.Characters.Count == 5 && root.Team2.Characters.Count == 8, "Actual GM builds compact 80x27 map, 5-vs-8");
                foreach (var actor in root.Characters)
                    Check(actor.IsAlive && actor.Health == 100 && actor.PhysicsBody.IsAtRest && actor.PhysicsBody.State.EffectiveGravityScale == .5f && actor.IsInfinite("cannon"), "Stable living birth, half gravity and infinite cannon: " + actor.name);
                Check(root.Team2.Characters.All(c => c.Luck == 50) && root.Team2.Characters.Count(c => c.CharacterType == "RobotCaptain") == 1, "Eight robots, Luck 50, exactly one captain");
                Check(data.Terrain[23,10] != null && data.Terrain[20,60] != null && root.WaterLevelY == -24,
                    "Wooden hull remains on waterline; freighter underside ends at y=21, three cells above galaxy");
                Check(Enumerable.Range(21,6).All(y => Enumerable.Range(52,28).All(x => data.Terrain[y,x] == null)), "No enemy hull or support extends down into hover gap");
                Check(Enumerable.Range(17,2).All(y => Enumerable.Range(52,6).All(x => data.Terrain[y,x] == null)) && data.Terrain[16,53] != null && data.Terrain[19,53] != null,
                    "Forward twin mandibles have a genuine open two-cell notch");
                Check(Enumerable.Range(17,2).All(y => Enumerable.Range(52,13).All(x => data.Terrain[y,x] == null)), "Forward bay is open from the notch through x64");
                Check(Enumerable.Range(14,5).All(y => Enumerable.Range(63,2).All(x => data.Terrain[y,x] == null)), "Two-cell roof hatch has no hidden collision");
                Check(root.Team2.Characters.Count(c => c.GridY == 18 && (c.GridX == 59 || c.GridX == 63) && c.CharacterType == "Robot") == 2, "Exactly two ordinary robots spawn inside the forward bay");
                Check(root.Team2.Characters.Where(c => c.GridY != 18).All(c => Enumerable.Range(0,c.GridY).All(y => data.Terrain[y,c.GridX] == null)), "Other six robots retain open exterior spawn surfaces");
                var left = root.TerrainHolder.Find("tile_07_22_ship_top_middle").GetComponent<SpriteRenderer>();
                var right = root.TerrainHolder.Find("tile_60_14_ship_top_middle").GetComponent<SpriteRenderer>();
                Check(left.sprite == MutinyLevelBuilder.ResolveTileSprite("ship_top_middle") && right.sprite == Resources.Load<Sprite>("Art/Space16/edge_top"), "Old wooden ship / new spacecraft remain visually distinct");
                var rocks = root.TerrainHolder.GetComponentsInChildren<SpriteRenderer>().Where(r => r.sharedMaterial != null && r.sharedMaterial.shader.name == "Mutiny/AsteroidRock").ToArray();
                Check(rocks.Length > 70 && rocks.All(r => r.transform.position.x >= 25 && r.transform.position.x < 52), "Asteroid palette is restricted to irregular belt rock tiles");
                Check(!ShaderUtil.ShaderHasError(rocks[0].sharedMaterial.shader) && rocks[0].sharedMaterial.shader.isSupported, "Asteroid shader compiles on actual GPU");
                Check(root.GetComponentInChildren<MutinySpaceBackground>() != null && root.GetComponent<MutinyGameHUD>().ResolveOpponentPortrait() == Resources.Load<Texture2D>("Art/Characters/Preview/Robot"), "Space parallax, animated galaxy and Robot HUD retained");
                Capture(root);
                for (int i = 0; i < 13; i++)
                {
                    root = Fresh(); var actor = root.Characters[i]; var state = actor.PhysicsBody.State;
                    bool Accept(Vector2 p) => p.y < 24*32-8 && p.x > 0 && p.x < 80*32 && Mathf.Abs(p.x-state.X) >= 48;
                    Check(FindAny(state, Accept, out var v), "Birth can jump to another standing position: " + actor.name);
                    Launch(root, actor, v); var landing = Settle(actor);
                    Check(actor.IsAlive && actor.PhysicsBody.IsAtRest && Accept(landing), "Actual birth escape jump survives and lands: " + actor.name + " -> " + landing);
                }
                // Both actual interior actors can use the shaft, with a floor hop for the deeper actor.
                foreach (int index in new[] { 1, 2 })
                {
                    root = Fresh(); var actor = root.Team2.Characters[index];
                    if (index == 1)
                    {
                        bool Shaft(Vector2 p) => p.x >= 63*32+10 && p.x <= 65*32-10 && Floor(p,19);
                        Check(FindAny(actor.PhysicsBody.State, Shaft, out var v), "Interior robot can reposition into roof shaft");
                        Launch(root, actor, v); var landing = Settle(actor);
                        Check(actor.IsAlive && Shaft(landing), "Actual interior floor hop reaches shaft: " + landing);
                    }
                    bool Roof(Vector2 p) => p.x >= 57*32 && p.x < 76*32 && p.y < 16*32-8;
                    root.Team2.StartTurn();
                    Check(FindAny(actor.PhysicsBody.State, Roof, out var up), "Interior robot has a roof exit: " + index);
                    Launch(root, actor, up); var roof = Settle(actor);
                    Check(actor.IsAlive && actor.PhysicsBody.IsAtRest && Roof(roof), "Actual interior robot exits onto exterior roof: " + roof);
                }
                {
                    root = Fresh(); var actor = root.Team2.Characters[1];
                    bool Front(Vector2 p) => p.x >= 52*32+6 && p.x <= 57*32+26 && Floor(p,19);
                    Check(FindAny(actor.PhysicsBody.State, Front, out var v), "Interior bay has a forward exit");
                    Launch(root, actor, v); var landing = Settle(actor);
                    Check(actor.IsAlive && Front(landing), "Actual interior robot exits onto lower fork: " + landing);
                    root.Team2.StartTurn();
                    bool Bay(Vector2 p) => p.x >= 58*32+6 && p.x <= 62*32+26 && Floor(p,19);
                    Check(FindAny(actor.PhysicsBody.State, Bay, out var back), "Front notch can re-enter bay");
                    Launch(root, actor, back); var returned = Settle(actor);
                    Check(actor.IsAlive && Bay(returned), "Actual robot enters bay from notch: " + returned);
                }
                Route("lower route", new Vector3(27,29,19), new Vector3(34,36,21), new Vector3(44,47,19), new Vector3(52,53,19), new Vector3(48,50,16), new Vector3(52,56,16), new Vector3(60,70,14));
                Route("middle route", new Vector3(31,35,14), new Vector3(42,45,11), new Vector3(48,50,16), new Vector3(52,56,16));
                Route("upper branch", new Vector3(26,28,8), new Vector3(34,37,6), new Vector3(46,48,4), new Vector3(60,70,14));
                // Original wooden bottom-deck exit remains valid; the freighter lower-fork exit is in the route above.
                foreach (int i in new[] { 4 })
                {
                    root = Fresh(); var actor = root.Characters[i]; var originalY = actor.PhysicsBody.State.Y;
                    bool Up(Vector2 p) => p.y < originalY - 64 && (i == 4 ? p.x < 25*32 : p.x >= 52*32);
                    Check(FindAny(actor.PhysicsBody.State, Up, out var v), "Bottom deck has an upward exit: " + actor.name);
                    Launch(root, actor, v); var landing = Settle(actor);
                    Check(actor.IsAlive && Up(landing), "Actual bottom-deck jump reaches higher deck: " + landing);
                }
                controller.RestartCurrentLevel(); root = controller.CurrentLevel;
                Check(root.Characters.Count == 13 && root.Characters.All(c => c.IsInfinite("cannon")), "Restart retains 5-vs-8 and infinite cannon");
                foreach (var actor in root.Characters) { actor.ConsumeWeapon("cannon"); actor.ConsumeWeapon("cannon"); }
                Check(root.Characters.All(c => c.IsInfinite("cannon") && c.GetAmmunition("cannon") == -1), "Actual inventory consumption preserves unlimited cannon for everyone");
                foreach (string command in new[] { "6", "2_16" })
                {
                    Check(gm.ExecuteCommand("enterlevel " + command), "Original level GM regression: " + command);
                    Check(controller.CurrentLevel.GravityScale == 1 && controller.CurrentLevel.GetComponentInChildren<MutinySpaceBackground>() == null, "Original gravity and background retained");
                    Check(!controller.CurrentLevel.TerrainHolder.GetComponentsInChildren<SpriteRenderer>().Any(r => r.sharedMaterial.shader.name == "Mutiny/AsteroidRock"), "Original rock colors retained");
                }
                File.AppendAllText(Report, "Scope: production movement and GPU overview, not a full-match AI strategy/balance or main-project GUI playthrough. Routes use production StartGame between jump actions; no claim of complete turn-flow regression.\n");
                EditorApplication.ExitPlaymode();
            }
            catch (Exception ex) { Fail(ex); }
        }
        static void Fail(Exception ex)
        {
            File.AppendAllText(Report, "FAIL " + ex + "\n"); SessionState.SetBool(Key, false);
            Debug.LogException(ex); EditorApplication.Exit(1);
        }
    }
}
