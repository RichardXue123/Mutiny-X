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
    public static class Level16DuelVerification
    {
        const string Key = "Mutiny.Level16Duel";
        static string Report => Path.GetFullPath(Path.Combine(Application.dataPath, "../level16-duel-verification.txt"));
        static MutinyLevelController controller;
        static MutinyGMManager gm;
        static MutinyLevelData data;
        static double start;
        static Level16DuelVerification()
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
                if (!AssetDatabase.IsValidFolder("Assets/Level16DuelVerification")) AssetDatabase.CreateFolder("Assets", "Level16DuelVerification");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, "Assets/Level16DuelVerification/Main.unity");
                Check(MutinyLevelSceneMenu.TryLoadLevelToScene(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16), out string error), "Editor production scene load: " + error);
                var root = Object.FindAnyObjectByType<MutinyLevelRoot>();
                Check(root.Width == 87 && root.Height == 26 && root.Characters.Count == 13, "Editor scene contains new 87x26 / 13-member map");
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
            foreach (var actor in root.Characters) for (int tick = 0; tick < 50; tick++) actor.PhysicsBody.AdvanceSimulationTick();
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
        static bool Lower(Vector2 p) => p.x >= 45 * 32 + 6 && p.x <= 49 * 32 - 6 && Floor(p, 19);
        static bool OurBoat(Vector2 p) => p.x >= 2 * 32 + 6 && p.x <= 21 * 32 - 6 && (Floor(p, 17) || Floor(p, 18));
        static void Launch(MutinyLevelRoot root, MutinyCharacter actor, Vector2 velocity)
        {
            if (actor.TeamIndex == 1)
            {
                var input = root.GetComponent<MutinyPlayerInput>();
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
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
        static void Verify()
        {
            if (EditorApplication.timeSinceStartup - start < .5) return;
            EditorApplication.update -= Verify;
            try
            {
                controller = Object.FindAnyObjectByType<MutinyLevelController>();
                gm = MutinyGMManager.Instance ?? new GameObject("GM").AddComponent<MutinyGMManager>();
                var xml = Resources.Load<TextAsset>("Data/Levels/level_1_16");
                data = MutinyLevelXmlParser.Parse(xml.text, xml.name);
                Check(xml.text == File.ReadAllText(Path.Combine(Application.dataPath, "Mutiny/Data/Levels/level_1_16.xml")), "Authoring/runtime XML identical");
                var root = Fresh();
                Check(root.Width == 87 && root.Height == 26 && root.Team1.Characters.Count == 5 && root.Team2.Characters.Count == 8,
                    "Actual GM constructs two-ship 5-vs-8 map");
                foreach (var c in root.Characters)
                    File.AppendAllText(Report, $"INFO {c.name} team={c.TeamIndex} alive={c.IsAlive} hp={c.Health} rest={c.PhysicsBody.IsAtRest} gravity={c.PhysicsBody.State.EffectiveGravityScale} cannon={c.IsInfinite("cannon")} state=({c.PhysicsBody.State.X},{c.PhysicsBody.State.Y}; {c.PhysicsBody.State.VelocityX},{c.PhysicsBody.State.VelocityY})\n");
                Check(root.Characters.All(c => c.IsAlive && c.Health == 100 && c.PhysicsBody.IsAtRest && c.PhysicsBody.State.EffectiveGravityScale == .5f && c.IsInfinite("cannon")),
                    "All 13 actual bodies survive/stabilize at birth with half gravity and infinite cannon");
                Check(root.Team2.Characters.All(c => c.Luck == 50) && root.Team2.Characters.Count(c => c.CharacterType == "RobotCaptain") == 1,
                    "Eight robots retain Luck 50 and one captain");
                for (int x = 21; x <= 44; x++)
                    Check(Enumerable.Range(0, data.Height).All(y => data.Terrain[y, x] == null), "Empty gap column " + x);
                foreach (var actor in root.Characters)
                    Check(Enumerable.Range(0, actor.GridY).All(y => data.Terrain[y, actor.GridX] == null), "Birth column is open to sky: " + actor.name);
                var left = root.TerrainHolder.Find("tile_08_18_ship_top_middle").GetComponent<SpriteRenderer>();
                var right = root.TerrainHolder.Find("tile_55_07_ship_top_middle").GetComponent<SpriteRenderer>();
                Check(left.sprite == MutinyLevelBuilder.ResolveTileSprite("ship_top_middle") && right.sprite == Resources.Load<Sprite>("Art/Space16/edge_top"),
                    "Actual left wooden material / right spacecraft material coexist");
                Check(root.GetComponentInChildren<MutinySpaceBackground>() != null && root.GetComponent<MutinyGameHUD>().ResolveOpponentPortrait() == Resources.Load<Texture2D>("Art/Characters/Preview/Robot"),
                    "Space parallax, galaxy and silver Robot HUD retained");
                var captain = root.Team1.Characters[0];
                var state = captain.PhysicsBody.State;
                Check(!Find(state, 18, 1, Lower, out _), "18-force player launch cannot board enemy lower deck at any sampled upward angle");
                Check(Find(state, 20, 1, Lower, out var boarding), "20-force player launch has a narrow valid lower-deck landing");
                Check(!Find(state, 20, 1, p => p.x >= 45 * 32 && (Floor(p, 7) || Floor(p, 13)), out _),
                    "Player cannot directly jump onto enemy middle or high deck");
                float minForce = 20;
                for (float force = 18; force <= 20; force += .25f) if (Find(state, force, 1, Lower, out _)) { minForce = force; break; }
                File.AppendAllText(Report, "INFO Lowest sampled player boarding force=" + minForce + "/20; chosen velocity=" + boarding + "\n");
                Launch(root, captain, boarding);
                var landing = Settle(captain);
                Check(captain.IsAlive && Lower(landing), "Actual player jump lands on enemy bottom deck: " + landing);
                root = Fresh();
                var enemy = root.Team2.Characters[0];
                Check(Find(enemy.PhysicsBody.State, 20, -1, OurBoat, out var invasion), "Enemy high-deck attack has a valid left-ship landing");
                Launch(root, enemy, invasion); landing = Settle(enemy);
                Check(enemy.IsAlive && OurBoat(landing), "Actual production AI jump lands on wooden ship: " + landing);
                // Every original spawn must permit a useful actual jump away from it.
                for (int i = 0; i < 13; i++)
                {
                    root = Fresh(); var actor = root.Characters[i]; state = actor.PhysicsBody.State;
                    bool SameShip(Vector2 p) => (actor.TeamIndex == 1 ? OurBoat(p) : p.x >= 45 * 32 && p.x <= 83 * 32)
                        && Mathf.Abs(p.x - state.X) >= 48;
                    bool found = false; Vector2 v = default;
                    foreach (float force in new[] { 8f, 12f, 16f, 20f })
                    {
                        if (Find(state, force, 1, SameShip, out v) || Find(state, force, -1, SameShip, out v)) { found = true; break; }
                    }
                    Check(found, "Birth has reachable alternative deck position: " + actor.name);
                    Launch(root, actor, v); landing = Settle(actor);
                    Check(actor.IsAlive && SameShip(landing), "Actual jump escapes birth and lands safely: " + actor.name + " -> " + landing);
                }
                root = Fresh();
                foreach (int index in new[] { 6, 7, 5 })
                {
                    root = Fresh(); var actor = root.Team2.Characters[index]; state = actor.PhysicsBody.State;
                    int target = index == 5 ? 7 : 13;
                    Check(Find(state, 20, 1, p => Floor(p, target) && p.x >= (target == 7 ? 52 : 49) * 32 + 6, out var v),
                        "Enemy terrace connects upward: spawn " + index + " -> deck " + target);
                    Launch(root, actor, v); landing = Settle(actor);
                    Check(actor.IsAlive && Floor(landing, target), "Actual enemy jump reaches next deck: " + landing);
                }
                controller.RestartCurrentLevel(); root = controller.CurrentLevel;
                Check(root.Characters.Count == 13 && root.Characters.All(c => c.IsInfinite("cannon")), "Production restart preserves 5-vs-8 and unlimited cannon");
                foreach (string command in new[] { "6", "2_16" })
                {
                    Check(gm.ExecuteCommand("enterlevel " + command), "Original level regression GM " + command);
                    Check(controller.CurrentLevel.GravityScale == 1 && controller.CurrentLevel.GetComponentInChildren<MutinySpaceBackground>() == null,
                        "Original mode retains original gravity and background");
                }
                var ordinary = MutinyLevelXmlParser.Parse(Resources.Load<TextAsset>("Data/Levels/level_1_01").text);
                Check(ordinary.SpaceThemeMinX == 0 && MutinySpaceVisuals.ResolveTile(ordinary, "ship_top_middle", 4, 12, false) == null,
                    "Missing new XML attribute keeps ordinary visuals unchanged");
                File.AppendAllText(Report, "Scope: actual movement and landings validated; no complete-match AI strategy/balance or main-project GUI playthrough claim.\n");
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
