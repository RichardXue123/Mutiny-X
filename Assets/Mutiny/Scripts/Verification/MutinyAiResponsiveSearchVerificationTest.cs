using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mutiny.Levels;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutiny.Verification
{
    public static class MutinyAiResponsiveSearchVerificationTest
    {
        public sealed class Fixture : IDisposable
        {
            public readonly GameObject Host = new GameObject("AIResponsiveFixture");
            public readonly MutinyTeam Team, EnemyTeam;
            public readonly MutinyCharacter Actor, Enemy;
            public readonly MutinyAIController Ai;
            public readonly string[,] Terrain = new string[20, 20];
            public readonly MutinyLevelController Controller;
            public readonly MutinyLevelRoot Root;
            private readonly TextAsset m_Xml;

            public Fixture(float luck, bool runtime = false, bool highPlatform = false)
            {
                MutinyBoxRegistry.ResetForLevel();
                var team = new GameObject("Team"); team.transform.SetParent(Host.transform);
                Team = team.AddComponent<MutinyTeam>(); Team.TeamNumber = 1;
                var opponent = new GameObject("EnemyTeam"); opponent.transform.SetParent(Host.transform);
                EnemyTeam = opponent.AddComponent<MutinyTeam>(); EnemyTeam.TeamNumber = 2;
                EnemyTeam.IsAiControlled = true;
                Actor = AddCharacter("Actor", Team, 96f, highPlatform ? 152f : 312f, runtime);
                Enemy = AddCharacter("Enemy", EnemyTeam, 176f, 312f, runtime);
                Actor.Luck = luck;
                for (int x = 0; x < 20; x++) Terrain[10, x] = "ground";
                if (highPlatform) Terrain[5, 2] = Terrain[5, 3] = "ground";
                Actor.PhysicsBody.SetTerrain(Terrain, 20, 20);
                Enemy.PhysicsBody.SetTerrain(Terrain, 20, 20);
                Ai = team.AddComponent<MutinyAIController>(); Ai.enabled = false;
                Ai.UseFixedDecisionSeed = true; Ai.FixedDecisionSeed = 13731;
                Ai.SaveDecisionTrace = false;
                Root = Host.AddComponent<MutinyLevelRoot>(); Root.enabled = false;
                Root.WaterLevelY = -608f / 32f; Root.Team1 = Team; Root.Team2 = EnemyTeam;
                // Uses the normal XML parser in PrepareDecisionWork. No rewritten prediction formula.
                var rows = new System.Text.StringBuilder();
                for (int row = 0; row < 20; row++) rows.Append("<row>").Append(row == 10 ? "ground:20" :
                    highPlatform && row == 5 ? "-:2,ground:2,-:16" : "-:20").Append("</row>");
                for (int row = 0; row < 20; row++) rows.Append("<bgRow>-:20</bgRow>");
                m_Xml = new TextAsset("<level width=\"20\" height=\"20\" players=\"1\">" + rows + "</level>");
                Controller = Host.AddComponent<MutinyLevelController>();
                Controller.enabled = false; Controller.LevelXml = m_Xml;
                Enemy.AddWeapon("cherryBomb", 5);
                Actor.AddWeapon("cherryBomb", 5);
            }

            internal MutinyCharacter AddCharacter(string name, MutinyTeam team, float x, float y, bool runtime)
            {
                var go = new GameObject(name); go.transform.SetParent(Host.transform);
                var character = go.AddComponent<MutinyCharacter>(); character.TeamIndex = team.TeamNumber;
                character.PhysicsBody.State = PhysicsBodyState.CreateDefault(x, y);
                character.transform.position = MutinyPhysics.PixelToUnity(x, y);
                character.PhysicsBody.State.Friction = 2f;
                character.PhysicsBody.State.HitsBoxes = true;
                character.PhysicsBody.WaterPixelY = 608f;
                character.PhysicsBody.IsActive = runtime;
                team.RegisterCharacter(character);
                return character;
            }

            public void Dispose()
            {
                Ai.enabled = false;
                foreach (var weapon in Object.FindObjectsByType<MutinyWeapon>())
                    if (weapon.Owner == Actor || weapon.Owner == Enemy) Object.DestroyImmediate(weapon.gameObject);
                foreach (var explosion in Object.FindObjectsByType<MutinyExplosion>())
                    if (explosion.Caster == Actor || explosion.Caster == Enemy) Object.DestroyImmediate(explosion.gameObject);
                Object.DestroyImmediate(Host); Object.DestroyImmediate(m_Xml);
                MutinyBoxRegistry.ResetForLevel();
            }
        }

        public static IEnumerator Run(MutinyLevel1VerificationResult result)
        {
            int force = MutinyAIController.ForcedWeaponId;
            bool logging = MutinyAIController.ActionLogEnabled;
            var random = UnityEngine.Random.state;
            try
            {
                MutinyAIController.TrySetForcedWeaponId(0);
                MutinyAIController.SetActionLogEnabled(false);
                // Each production dispatcher branch and its complete candidate sequence.
                string[] weapons = { "cherryBomb", "boulder", "dynamite", "piecesOfEight", "rumBottle", "banana",
                    "parachuteBomb", "woodenCrate", "gunpowderBarrel", "seagull", "mine", "cannon", "anchor", "voodooDoll", "tidalWave" };
                using (var fixture = new Fixture(7))
                {
                    foreach (string weapon in weapons)
                    {
                        Debug.Log("[AI-RESPONSIVE-STAGE] replay " + weapon);
                        fixture.Actor.WeaponInventory.Clear(); fixture.Actor.AddWeapon(weapon, 5);
                        AIMove synchronous = fixture.Ai.EvaluateBestMove();
                        var expected = fixture.Ai.LastDecisionTrace;
                        fixture.Ai.SetReplayTraceForVerification(expected);
                        AIMove sliced = default;
                        var steps = fixture.Ai.EvaluateBestMoveBudgetedForVerification(move => sliced = move);
                        // Do not advance the live fixture; pump real budget slices over the same board.
                        int slices = 0;
                        while (steps.MoveNext()) slices++;
                        var actual = fixture.Ai.LastDecisionTrace;
                        result.Assert(SameMove(synchronous, sliced) && actual.DrawCount == expected.Draws.Count &&
                            actual.CandidateCount == expected.Candidates.Count,
                            "EXT-AI-SLICE-02 " + weapon + " all draws/candidates replay exactly through the production budget pump");
                    }
                }
                yield return null;

                using (var fixture = new Fixture(33))
                {
                    Debug.Log("[AI-RESPONSIVE-STAGE] streaming roundtrip");
                    fixture.Ai.SaveDecisionTrace = true;
                    AIMove synchronous = fixture.Ai.EvaluateBestMove();
                    var expected = fixture.Ai.LastDecisionTrace;
                    var steps = fixture.Ai.EvaluateBestMoveBudgetedForVerification(_ => { }, streaming: true);
                    while (steps.MoveNext()) yield return null;
                    string path = fixture.Ai.LastDecisionTracePath;
                    double deadline = Time.realtimeSinceStartupAsDouble + 10;
                    while (!File.Exists(path) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                    result.Assert(fixture.Ai.LastDecisionTrace.RecordsStreamed && fixture.Ai.LastDecisionTrace.Candidates.Count == 0 &&
                        fixture.Ai.LastDecisionTrace.Draws.Count == 0 && File.Exists(path),
                        "EXT-AI-TRACE-02 runtime retains counters, not unbounded candidate/draw arrays; async JSON publishes");
                    if (File.Exists(path))
                    {
                        var saved = JsonUtility.FromJson<MutinyAIDecisionTrace>(File.ReadAllText(path));
                        fixture.Ai.SaveDecisionTrace = false;
                        fixture.Ai.SetReplayTraceForVerification(saved);
                        AIMove replayed = fixture.Ai.EvaluateBestMove();
                        result.Assert(SameMove(synchronous, replayed) && saved.Draws.Count == expected.Draws.Count &&
                            saved.Candidates.Count == expected.Candidates.Count,
                            "EXT-AI-TRACE-02 streamed JSON preserves schema, complete random stream and every candidate");
                    }
                }
                yield return null;

                foreach (int luck in new[] { 10000, 99999 })
                    foreach (object step in RunHighLuckTurn(result, luck)) yield return step;
                foreach (object step in RunCancellation(result)) yield return step;
                foreach (object step in RunFirstActionAndPass(result)) yield return step;
                foreach (object step in RunTraceIoFailure(result)) yield return step;
                foreach (object step in RunLevelRestart(result)) yield return step;
                foreach (object step in RunLevel12CorpseRegression(result)) yield return step;
                foreach (object step in RunAliveAndPresentationChecks(result)) yield return step;
            }
            finally
            {
                MutinyAIController.TrySetForcedWeaponId(force);
                MutinyAIController.SetActionLogEnabled(logging);
                UnityEngine.Random.state = random;
            }
        }

        private static IEnumerable<object> RunHighLuckTurn(MutinyLevel1VerificationResult result, int luck)
        {
            Debug.Log("[AI-RESPONSIVE-STAGE] high luck " + luck);
            using (var fixture = new Fixture(luck, runtime: true))
            {
                var manager = fixture.Host.AddComponent<MutinyTurnManager>();
                manager.Initialize(fixture.Team, fixture.EnemyTeam);
                fixture.Team.SelectCharacter(fixture.Actor);
                fixture.Ai.ExecuteMoveForVerification(new AIMove
                {
                    Character = fixture.Actor, MoveType = AIMoveType.SelfThrow, LaunchVelocity = new Vector2(0f, -3f)
                }, manager);
                fixture.Ai.SaveDecisionTrace = true;
                // The production GM is a persistent singleton. Adding another instance
                // to the fixture would destroy the fixture in the singleton's Awake.
                var gm = MutinyGMManager.Instance;
                bool takeover = gm != null && gm.ExecuteCommand("aitakeover " + luck);
                result.Assert(takeover, "EXT-AI-SLICE-02 real GM takeover accepts high Luck " + luck + " during committed jump");
                if (!takeover) yield break;
                int beforeFrames = Time.frameCount;
                double deadline = Time.realtimeSinceStartupAsDouble + 180;
                while (fixture.Ai.LastDecisionTrace == null && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                var trace = fixture.Ai.LastDecisionTrace;
                result.Assert(trace != null && fixture.Ai.LastDecisionCandidateCount == luck && fixture.Ai.SearchSliceCount > 1 &&
                    Time.frameCount - beforeFrames > 1 && trace.Phase == "continuation",
                    "EXT-AI-SLICE-02 Luck " + luck + " finishes every sample across real frames after real jump settlement");
                result.Assert(trace != null && trace.Candidates.Count == 0 && trace.Draws.Count == 0 && trace.DrawCount == 2 * luck,
                    "EXT-AI-TRACE-02 Luck " + luck + " preserves all random draws with bounded retained arrays");
                // This checks measured search slices, not an invented FPS formula.
                result.Logs.Add($"[PERF] luck={luck} samples={fixture.Ai.LastDecisionCandidateCount} frames={Time.frameCount - beforeFrames} slices={fixture.Ai.SearchSliceCount} maximumSearchSliceMs={fixture.Ai.MaximumSearchSliceMilliseconds:R}");
                deadline = Time.realtimeSinceStartupAsDouble + 10;
                while (fixture.Actor.CanShoot && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                result.Assert(!fixture.Actor.CanShoot && fixture.Actor.WeaponInventory["cherryBomb"] == 4,
                    "AI-EXE-01 Luck " + luck + " submits one real weapon only after complete search");
                string path = fixture.Ai.LastDecisionTracePath;
                deadline = Time.realtimeSinceStartupAsDouble + 15;
                while (path != null && !File.Exists(path) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                result.Assert(path != null && File.Exists(path), "EXT-AI-TRACE-02 high Luck " + luck + " complete trace saved off-thread");
                result.Logs.Add("[TRACE] " + path);
            }
        }

        private static IEnumerable<object> RunCancellation(MutinyLevel1VerificationResult result)
        {
            using (var fixture = new Fixture(99999, runtime: true))
            {
                fixture.Ai.SaveDecisionTrace = true;
                var manager = fixture.Host.AddComponent<MutinyTurnManager>(); manager.Initialize(fixture.Team, fixture.EnemyTeam);
                string error;
                manager.TryTakeOverCurrentPlayerTurn(99999, out error);
                double deadline = Time.realtimeSinceStartupAsDouble + 10;
                while (fixture.Ai.EvaluatedCandidateCount < 100 && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                int old = fixture.Ai.EvaluatedCandidateCount;
                string canceledPath = fixture.Ai.CurrentDecisionTracePath;
                // Use real inventory and GM APIs; production snapshot validation must discard work.
                fixture.Actor.AddWeapon("dynamite", 1);
                fixture.Ai.SetTakeoverLuckOverride(2);
                for (int frame = 0; frame < 4; frame++) yield return null;
                result.Assert(old >= 100 && fixture.Ai.InvalidatedDecisionCount > 0,
                    "EXT-AI-SNAPSHOT-01 live inventory/Luck change invalidates the previous production search");
                result.Logs.Add("[INVALIDATION] " + fixture.Ai.LastDecisionInvalidationReason);
                // Snapshot validation reports the first changed object. FindObjectsByType
                // does not promise that Actor precedes Enemy; the override affects both
                // effective snapshot values, while only own-team Luck controls samples.
                result.Assert(fixture.Ai.LastDecisionInvalidationReason != null &&
                    fixture.Ai.LastDecisionInvalidationReason.StartsWith("character-luck:") &&
                    fixture.Ai.LastDecisionInvalidationReason.EndsWith(":99999->2"),
                    "EXT-AI-SNAPSHOT-03 invalidation records the affected character and old/new Luck");
                fixture.Ai.enabled = false;
                int ammunition = fixture.Actor.WeaponInventory["cherryBomb"];
                for (int frame = 0; frame < 10; frame++) yield return null;
                result.Assert(!fixture.Ai.IsEvaluatingCandidates && fixture.Actor.WeaponInventory["cherryBomb"] == ammunition,
                    "EXT-AI-SNAPSHOT-01 disabled controller explicitly stops old coroutine without committing stale action");
                result.Assert(canceledPath != null && !File.Exists(canceledPath),
                    "EXT-AI-TRACE-02 incomplete canceled decision does not publish a complete JSON");
            }
            yield return null;
            using (var fixture = new Fixture(99999, runtime: true))
            {
                var manager = fixture.Host.AddComponent<MutinyTurnManager>(); manager.Initialize(fixture.Team, fixture.EnemyTeam);
                manager.TryTakeOverCurrentPlayerTurn(99999, out _);
                for (int frame = 0; frame < 6; frame++) yield return null;
                int ammunition = fixture.Actor.WeaponInventory["cherryBomb"];
                manager.PassTurn(); // Production turn switch, not a local CurrentPhase edit.
                double deadline = Time.realtimeSinceStartupAsDouble + 10;
                while (manager.CurrentTeam == fixture.Team && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                for (int frame = 0; frame < 4; frame++) yield return null;
                result.Assert(manager.CurrentTeam == fixture.EnemyTeam && !fixture.Ai.IsEvaluatingCandidates &&
                    fixture.Actor.WeaponInventory["cherryBomb"] == ammunition && !fixture.Team.IsAiControlled,
                    "EXT-AI-SNAPSHOT-01 real Pass/turn switch cancels search and restores human control");
            }
        }

        private static IEnumerable<object> RunFirstActionAndPass(MutinyLevel1VerificationResult result)
        {
            using (var fixture = new Fixture(1, runtime: true, highPlatform: true))
            {
                // Force only Tidal Wave; movement remains a candidate. A nearby enemy
                // below the wave band makes the production wave score positive.
                MutinyAIController.TrySetForcedWeaponId(15);
                fixture.Enemy.PhysicsBody.State.X = 592f;
                fixture.Enemy.PhysicsBody.State.Y = 312f;
                fixture.Enemy.transform.position = MutinyPhysics.PixelToUnity(592f, 312f);
                for (int i = 0; i < 4; i++)
                {
                    var enemyObject = new GameObject("WaveEnemy" + i);
                    enemyObject.transform.SetParent(fixture.Host.transform);
                    var enemy = enemyObject.AddComponent<MutinyCharacter>();
                    enemy.TeamIndex = 2;
                    enemy.PhysicsBody.State = PhysicsBodyState.CreateDefault(400f + 32f * i, 312f);
                    enemy.transform.position = MutinyPhysics.PixelToUnity(400f + 32f * i, 312f);
                    enemy.PhysicsBody.SetTerrain(fixture.Terrain, 20, 20);
                    enemy.PhysicsBody.WaterPixelY = 608f;
                    enemy.AddWeapon("cherryBomb", 5);
                    fixture.EnemyTeam.RegisterCharacter(enemy);
                }
                var manager = fixture.Host.AddComponent<MutinyTurnManager>(); manager.Initialize(fixture.Team, fixture.EnemyTeam);
                manager.TryTakeOverCurrentPlayerTurn(1, out _);
                double deadline = Time.realtimeSinceStartupAsDouble + 20;
                while (fixture.Ai.LastDecisionTrace == null && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                result.Assert(fixture.Ai.LastDecisionTrace != null && fixture.Ai.LastDecisionTrace.Phase == "first-action" &&
                    fixture.Ai.LastDecisionCandidateCount == 51 && !fixture.Actor.CanShoot &&
                    fixture.Ai.LastDecisionTrace.Winner.StartsWith("ShootWeapon:Actor:tidalWave:"),
                    "AI-SEL-01 first-action completes 50 jumps plus wave before real direct fire");
            }
            MutinyAIController.TrySetForcedWeaponId(0);
            yield return null;
            using (var fixture = new Fixture(0, runtime: true))
            {
                var manager = fixture.Host.AddComponent<MutinyTurnManager>(); manager.Initialize(fixture.Team, fixture.EnemyTeam);
                fixture.Team.SelectCharacter(fixture.Actor);
                fixture.Ai.ExecuteMoveForVerification(new AIMove { Character = fixture.Actor,
                    MoveType = AIMoveType.SelfThrow, LaunchVelocity = new Vector2(0, -3) }, manager);
                manager.TryTakeOverCurrentPlayerTurn(0, out _);
                double deadline = Time.realtimeSinceStartupAsDouble + 20;
                while (manager.CurrentTeam == fixture.Team && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                result.Assert(fixture.Ai.LastDecisionTrace != null && fixture.Ai.LastDecisionTrace.Phase == "continuation" &&
                    fixture.Ai.LastDecisionTrace.Winner.StartsWith("Pass:") && fixture.Actor.WeaponInventory["cherryBomb"] == 5 &&
                    manager.CurrentTeam == fixture.EnemyTeam && !fixture.Team.IsAiControlled,
                    "AI-SEL-03 real jump followed by zero-Luck continuation passes with ammunition unchanged");
            }
        }

        private static IEnumerable<object> RunTraceIoFailure(MutinyLevel1VerificationResult result)
        {
            string directory = Path.Combine(Application.temporaryCachePath, "mutiny-ai-io-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string blockingFile = Path.Combine(directory, "not-a-directory");
            File.WriteAllText(blockingFile, "AI trace IO failure fixture");
            var writer = new MutinyAITraceWriter(Path.Combine(blockingFile, "trace.json"));
            try
            {
                double deadline = Time.realtimeSinceStartupAsDouble + 10;
                while (!writer.Completion.IsCompleted && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                result.Assert(writer.Completion.IsCompleted && writer.Error != null && writer.CanAcceptRecords &&
                    writer.TryComplete(new MutinyAIDecisionTrace()) && !File.Exists(writer.Path),
                    "EXT-AI-TRACE-02 storage failure is reported without blocking the search or publishing invalid JSON");
            }
            finally
            {
                writer.Abort();
                File.Delete(blockingFile);
                Directory.Delete(directory); // Only this explicitly created, empty fixture directory.
            }
        }

        private static IEnumerable<object> RunLevelRestart(MutinyLevel1VerificationResult result)
        {
            // Build/restart the actual numbered level through its production controller.
            var host = new GameObject("AI Responsive Level Restart Fixture");
            var controller = host.AddComponent<MutinyLevelController>(); controller.enabled = false;
            try
            {
                bool loaded = controller.TryLoadLevel(7);
                result.Assert(loaded, "EXT-AI-SNAPSHOT-01 production level 7 loads for restart cancellation");
                if (!loaded) yield break;
                var manager = controller.CurrentLevel.GetComponent<MutinyTurnManager>();
                double deadline = Time.realtimeSinceStartupAsDouble + 20;
                while (manager.CurrentPhase != TurnPhase.TurnActive && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                bool takeover = MutinyGMManager.Instance.ExecuteCommand("aitakeover 99999");
                result.Assert(takeover, "EXT-AI-SNAPSHOT-01 real GM starts high-Luck search on loaded level 7");
                if (!takeover) yield break;
                var ai = controller.CurrentLevel.Team1.GetComponent<MutinyAIController>();
                deadline = Time.realtimeSinceStartupAsDouble + 10;
                while (ai.EvaluatedCandidateCount < 100 && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                bool searching = ai.IsEvaluatingCandidates && ai.EvaluatedCandidateCount >= 100;
                controller.RestartCurrentLevel();
                for (int frame = 0; frame < 5; frame++) yield return null;
                result.Assert(searching && ai == null && controller.CurrentLevel != null &&
                    !controller.CurrentLevel.Team1.IsAiControlled,
                    "EXT-AI-SNAPSHOT-01 real restart destroys old search and does not transfer takeover to rebuilt level");
            }
            finally
            {
                controller.ClearLevel();
                Object.DestroyImmediate(host);
                MutinyBoxRegistry.ResetForLevel();
            }
        }

        private static IEnumerable<object> RunLevel12CorpseRegression(MutinyLevel1VerificationResult result)
        {
            float previousCapture = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f; // Advance real 25 Hz physics across rendered search slices.
            try
            {
                foreach (int luck in new[] { 1000, 9999 })
                {
                    Debug.Log("[AI-RESPONSIVE-STAGE] level 12 corpse luck=" + luck);
                    var host = new GameObject("AI Level 12 Corpse Regression");
                    var controller = host.AddComponent<MutinyLevelController>(); controller.enabled = false;
                    try
                    {
                        bool loaded = controller.TryLoadLevel(12);
                        result.Assert(loaded, "EXT-AI-SNAPSHOT-02 real level 12 loads, Luck=" + luck);
                        if (!loaded) continue;
                        var root = controller.CurrentLevel;
                        result.Assert(Object.FindObjectsByType<Camera>().Length == 0 &&
                            Object.FindObjectsByType<MutinyCameraController>().Length == 0,
                            "EXT-AI-SNAPSHOT-02 native level 12 regression has no camera or camera panning, Luck=" + luck);
                        var manager = root.GetComponent<MutinyTurnManager>();
                        var ai = root.Team2.GetComponent<MutinyAIController>(); ai.SaveDecisionTrace = false;
                        ai.UseFixedDecisionSeed = true; ai.FixedDecisionSeed = 13731;
                        double deadline = Time.realtimeSinceStartupAsDouble + 20;
                        while (manager.CurrentTeam != root.Team1 || manager.CurrentPhase != TurnPhase.TurnActive)
                        {
                            if (Time.realtimeSinceStartupAsDouble >= deadline) break;
                            yield return null;
                        }
                        result.Assert(MutinyGMManager.Instance.ExecuteCommand("aisetluck " + luck),
                            "EXT-AI-SNAPSHOT-02 real GM sets native level 12 AI Luck=" + luck);
                        // Use real velocity/25 Hz contact/water/death callbacks, not IsAlive edits.
                        var victimTeam = luck == 1000 ? root.Team1 : root.Team2;
                        var victim = victimTeam.Characters.Find(character => character.GridX == (luck == 1000 ? 41 : 39));
                        victim.PhysicsBody.SetVelocity(30f, -20f);
                        deadline = Time.realtimeSinceStartupAsDouble + 10;
                        while (victim.IsAlive && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                        result.Assert(!victim.IsAlive && victim.PhysicsBody.IsInWater,
                            "EXT-AI-SNAPSHOT-02 real T" + victimTeam.TeamNumber + " water death retains moving corpse, Luck=" + luck);
                        float corpseY = victim.PhysicsBody.State.Y;
                        long corpseTicks = victim.PhysicsBody.SimulationTickCount;
                        manager.PassTurn();
                        deadline = Time.realtimeSinceStartupAsDouble + 45;
                        while (ai.LastDecisionCandidateCount == 0 && ai.InvalidatedDecisionCount < 5 &&
                            Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                        result.Logs.Add("[CORPSE] luck=" + luck + " invalidations=" + ai.InvalidatedDecisionCount +
                            " completedCandidates=" + ai.LastDecisionCandidateCount + " corpseY=" + corpseY.ToString("R") +
                            "->" + victim.PhysicsBody.State.Y.ToString("R"));
                        result.Assert(ai.LastDecisionCandidateCount >= luck && ai.InvalidatedDecisionCount < 5,
                            "EXT-AI-SNAPSHOT-02 level 12 Luck=" + luck + " completes native inventory search without corpse restart loop");
                        result.Assert(!victim.IsAlive && victim.PhysicsBody.IsActive &&
                            victim.PhysicsBody.SimulationTickCount > corpseTicks && victim.PhysicsBody.State.Y > corpseY,
                            "EXT-AI-SNAPSHOT-02 corpse continues real physics while native AI searches, Luck=" + luck);
                        if (ai.LastDecisionCandidateCount == 0) continue; // Fail promptly on the original defect.
                        deadline = Time.realtimeSinceStartupAsDouble + 20;
                        while ((root.Team2.SelectedCharacter == null || root.Team2.SelectedCharacter.CanThrow &&
                            root.Team2.SelectedCharacter.CanShoot) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                        result.Assert(root.Team2.SelectedCharacter != null && (!root.Team2.SelectedCharacter.CanThrow ||
                            !root.Team2.SelectedCharacter.CanShoot),
                            "EXT-AI-SNAPSHOT-02 native level 12 AI submits a real action after completed search, Luck=" + luck);
                    }
                    finally
                    {
                        controller.ClearLevel(); Object.DestroyImmediate(host);
                        MutinyBoxRegistry.ResetForLevel();
                    }
                    yield return null;
                }
            }
            finally { Time.captureDeltaTime = previousCapture; }
        }

        private static IEnumerable<object> RunAliveAndPresentationChecks(MutinyLevel1VerificationResult result)
        {
            float previousCapture = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            try
            {
                using (var fixture = new Fixture(99999, runtime: true))
                {
                    var otherEnemy = fixture.AddCharacter("OtherEnemy", fixture.EnemyTeam, 240f, 312f, true);
                    otherEnemy.PhysicsBody.SetTerrain(fixture.Terrain, 20, 20);
                    for (int frame = 0; frame < 50; frame++) yield return null;
                    var manager = fixture.Host.AddComponent<MutinyTurnManager>(); manager.Initialize(fixture.Team, fixture.EnemyTeam);
                    manager.TryTakeOverCurrentPlayerTurn(99999, out _);
                    double deadline = Time.realtimeSinceStartupAsDouble + 10;
                    while (fixture.Ai.EvaluatedCandidateCount < 100 && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                    int invalidations = fixture.Ai.InvalidatedDecisionCount;
                    fixture.Enemy.TakeDamage(100f); // Production death; opponent still has OtherEnemy alive.
                    for (int frame = 0; frame < 4; frame++) yield return null;
                    result.Assert(!fixture.Enemy.IsAlive && fixture.EnemyTeam.AliveCount == 1 &&
                        fixture.Ai.InvalidatedDecisionCount > invalidations &&
                        fixture.Ai.LastDecisionInvalidationReason == "character-alive:Enemy:True->False",
                        "EXT-AI-SNAPSHOT-01 real alive-to-dead transition still discards stale search with an explicit reason");
                    fixture.Ai.enabled = false;
                }
                yield return null;
                using (var fixture = new Fixture(9999, runtime: true))
                {
                    for (int frame = 0; frame < 50; frame++) yield return null;
                    var manager = fixture.Host.AddComponent<MutinyTurnManager>(); manager.Initialize(fixture.Team, fixture.EnemyTeam);
                    // The production presentation hook affects only the rendered pose.
                    // LateUpdate applies it and the next Update restores authority.
                    fixture.Actor.PhysicsBody.PresentationPositionOverride = () => MutinyPhysics.PixelToUnity(
                        fixture.Actor.PhysicsBody.State.X + (Time.frameCount % 2 == 0 ? 160f : -160f),
                        fixture.Actor.PhysicsBody.State.Y);
                    manager.TryTakeOverCurrentPlayerTurn(9999, out _);
                    double deadline = Time.realtimeSinceStartupAsDouble + 15;
                    while (fixture.Ai.LastDecisionCandidateCount == 0 && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                    result.Assert(fixture.Ai.LastDecisionCandidateCount >= 9999 && fixture.Ai.InvalidatedDecisionCount == 0,
                        "EXT-AI-SNAPSHOT-01 real LateUpdate presentation/Transform changes do not invalidate authoritative physics search");
                    fixture.Actor.PhysicsBody.PresentationPositionOverride = null;
                    fixture.Ai.enabled = false;
                }
            }
            finally { Time.captureDeltaTime = previousCapture; }
        }

        private static bool SameMove(AIMove a, AIMove b) => a.Character == b.Character && a.MoveType == b.MoveType &&
            a.WeaponType == b.WeaponType && a.Score == b.Score && a.LaunchVelocity == b.LaunchVelocity &&
            a.TargetPosition == b.TargetPosition && a.TargetCharacter == b.TargetCharacter &&
            a.CannonRotationDegrees == b.CannonRotationDegrees && a.SeagullFlightY == b.SeagullFlightY &&
            a.UsesForcedWeaponSupply == b.UsesForcedWeaponSupply;
    }
}
