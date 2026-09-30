using System;
using System.Collections;
using System.Collections.Generic;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutiny.Verification
{
    public static class MutinyAiEnhancedVerificationTest
    {
        public static IEnumerator Run(MutinyLevel1VerificationResult result)
        {
            var gm = MutinyGMManager.Instance;
            bool mode = MutinyAIController.EnhancementEnabled;
            int forced = MutinyAIController.ForcedWeaponId;
            var random = UnityEngine.Random.state;
            try
            {
                gm.ExecuteCommand("aiforceusewaepon 0"); gm.ExecuteCommand("aienhance 1");
                VerifyScoreRules(result);
                VerifyJumpRules(result);
                var effectWeapons = new List<string>(MutinyAiStrategyVerificationTest.Weapons) { "cannonball" };
                foreach (string weapon in effectWeapons)
                {
                    Debug.Log("[AI-ENHANCED-STAGE] production outcome " + weapon);
                    VerifyProductionOutcome(result, gm, weapon);
                    yield return null;
                }
                foreach (string scenario in new[] { "barrel-chain", "existing-mine-jump", "existing-mine-still", "fan-left-switch", "rum-gap", "rum-wall" })
                {
                    Debug.Log("[AI-ENHANCED-STAGE] environment " + scenario);
                    VerifyProductionOutcome(result, gm, scenario.StartsWith("rum-") ? "rumBottle" :
                        scenario == "fan-left-switch" ? "parachuteBomb" : "cherryBomb", scenario);
                    yield return null;
                }
                foreach (int luck in new[] { 0, 100, 99999 })
                {
                    Debug.Log("[AI-ENHANCED-STAGE] budgeted planner luck=" + luck);
                    IEnumerator steps = VerifyPlanner(result, gm, luck);
                    while (steps.MoveNext()) yield return steps.Current;
                }
                IEnumerator invalidation = VerifyMineInvalidation(result, gm);
                while (invalidation.MoveNext()) yield return invalidation.Current;
                VerifyPassAndHorizon(result, gm);
            }
            finally
            {
                gm.ExecuteCommand(mode ? "aienhance 1" : "aienhance 0");
                MutinyAIController.TrySetForcedWeaponId(forced); UnityEngine.Random.state = random;
            }
            // Existing lifecycle/cancellation tests still use production GM and
            // real coroutines. Only their authorized enhanced expectations changed.
            gm.ExecuteCommand("aienhance 0");
            IEnumerator boundary = MutinyAiStrategyVerificationTest.Run(result);
            while (boundary.MoveNext()) yield return boundary.Current;
            foreach (int luck in new[] { 100, 9999 })
            {
                IEnumerator level = VerifyLevel12(result, gm, luck);
                while (level.MoveNext()) yield return level.Current;
            }
        }

        private static void VerifyScoreRules(MutinyLevel1VerificationResult result)
        {
            var outcome = new MutinyAIEffectEvaluation { Settled = true, AllyHpBefore = 100, AllyHpAfter = 70,
                EnemyHpBefore = 100, EnemyHpAfter = 40 };
            MutinyAIEffectScore.Evaluate(outcome, false, false, false);
            result.Assert(Mathf.Approximately(outcome.Score, 25.5f), "EXT-AI-FX-09 actual HP loss with stronger friendly penalty");
            outcome.EnemiesLost = 1; outcome.AlliesLost = 1;
            MutinyAIEffectScore.Evaluate(outcome, true, false, false);
            result.Assert(Mathf.Approximately(outcome.Score, 19.5f), "EXT-AI-FX-09 kill/death/resource score components");
            outcome.Settled = false; MutinyAIEffectScore.Evaluate(outcome, false, true, false);
            result.Assert(outcome.Score == 0f && !outcome.Settled, "EXT-AI-FX-09 unfinished outcome not assigned partial success");
        }

        private static void VerifyJumpRules(MutinyLevel1VerificationResult result)
        {
            result.Assert(MutinyAIController.EnhancedJumpSampleCount(0) == 50 &&
                MutinyAIController.EnhancedJumpSampleCount(49) == 50 &&
                MutinyAIController.EnhancedJumpSampleCount(50) == 50 &&
                MutinyAIController.EnhancedJumpSampleCount(100) == 100 &&
                MutinyAIController.EnhancedJumpSampleCount(99999) == 99999,
                "EXT-AI-JUMP-01 enhanced jump samples use max(50, luck) without a high-luck cap");
            using (var fixture = new MutinyAiResponsiveSearchVerificationTest.Fixture(50))
            {
                var input = Input(fixture);
                float overlapping = MutinyAIController.EnhancedPositionScore(input, 0, new Vector2(176f, 312f));
                float separated = MutinyAIController.EnhancedPositionScore(input, 0, new Vector2(16f, 312f));
                result.Assert(overlapping < -100f && separated > overlapping,
                    "EXT-AI-JUMP-02 overlapping landing cannot win through a speculative next shot");
            }
            using (var fixture = new MutinyAiResponsiveSearchVerificationTest.Fixture(50))
            {
                fixture.Enemy.PhysicsBody.State.X = 592f;
                fixture.Actor.WeaponInventory.Clear(); fixture.Actor.AddWeapon("woodenCrate", 5);
                AIMove move = fixture.Ai.EvaluateBestMove();
                var summary = fixture.Ai.LastEnhancedDecision;
                result.Assert(summary != null && summary.JumpSamples == 50 && move.MoveType == AIMoveType.SelfThrow &&
                    move.Score > 0f && Vector2.Distance(move.TargetPosition, new Vector2(592f, 312f)) < 496f &&
                    Vector2.Distance(move.TargetPosition, new Vector2(592f, 312f)) >= 90f,
                    "EXT-AI-JUMP-03 when a weapon cannot deal immediate damage the enhanced AI approaches safely");
                fixture.Actor.AddWeapon("cherryBomb", 5);
                fixture.Ai.EvaluateBestMove();
                summary = fixture.Ai.LastEnhancedDecision;
                result.Assert(summary != null && summary.JumpFollowUps > 0 &&
                    summary.Simulations <= MutinyAIController.EnhancedMaxFullSimulations,
                    "EXT-AI-JUMP-04 jump candidates simulate a real following shot within the shared effect budget");
            }
        }

        private static MutinyAIEffectInput Input(MutinyAiResponsiveSearchVerificationTest.Fixture fixture)
        {
            var characters = new[] { Character(fixture.Actor), Character(fixture.Enemy) };
            var boxes = new List<MutinyAIEffectBox>();
            foreach (var obstacle in MutinyBoxRegistry.GetObstacles()) boxes.Add(new MutinyAIEffectBox {
                Body = obstacle.State, Barrel = obstacle.Body.GetComponent<MutinyGunpowderBarrel>() != null });
            var mines = new List<MutinyAIEffectMine>();
            foreach (var mine in Object.FindObjectsByType<MutinyMine>())
                if (mine.IsFired && !mine.IsFinished) mines.Add(new MutinyAIEffectMine { Body = mine.PhysicsBody.State,
                    Ignore = mine.IgnoreTicksRemaining, Countdown = mine.CountdownRemaining, Active = mine.IsActive, Stored = mine.IsStored });
            return new MutinyAIEffectInput(fixture.Terrain, 20, 20, 608f, 1, characters, boxes.ToArray(), mines.ToArray());
        }
        private static MutinyAIEffectCharacter Character(MutinyCharacter character) => new MutinyAIEffectCharacter {
            Body = character.PhysicsBody.State, Health = character.Health, Alive = character.IsAlive, Team = character.TeamIndex };

        private static void VerifyProductionOutcome(MutinyLevel1VerificationResult result, MutinyGMManager gm, string weapon, string scenario = null)
        {
            // An isolated scene clock is advanced explicitly; no private fired,
            // finished, active, damage, or countdown flags are rewritten.
            var beforeObjects = new HashSet<GameObject>(Object.FindObjectsByType<GameObject>());
            TextAsset scenarioXml = null;
            try
            {
                using (var fixture = new MutinyAiResponsiveSearchVerificationTest.Fixture(7, runtime: true))
                {
                    fixture.Actor.Health = fixture.Actor.MaxHealth = fixture.Actor.ShownHealth = 1000f;
                    fixture.Enemy.Health = fixture.Enemy.MaxHealth = fixture.Enemy.ShownHealth = 1000f;
                    fixture.Actor.WeaponInventory.Clear(); fixture.Actor.AddWeapon(weapon, 5);
                    var manager = fixture.Host.AddComponent<MutinyTurnManager>(); manager.Initialize(fixture.Team, fixture.EnemyTeam);
                    result.Assert(gm.ExecuteCommand("aitakeover 0"), "EXT-AI-FX-04 production GM supplies real AI ownership " + weapon);
                    fixture.Ai.enabled = false; manager.enabled = false;
                    if (weapon == "voodooDoll") fixture.Enemy.PhysicsBody.State.X = 624f;
                    if (weapon == "rumBottle") fixture.Enemy.PhysicsBody.State.X = 400f;
                    MutinyMine existingMine = null;
                    if (scenario == "barrel-chain")
                    {
                        var barrel = (MutinyGunpowderBarrel)MutinyWeaponFactory.SpawnWeapon("gunpowderBarrel", fixture.Enemy);
                        result.Assert(barrel.TryPlaceAt(new Vector2(240, 288)), "EXT-AI-FX-04 fixture barrel uses real placement entry");
                    }
                    if (scenario == "rum-gap") fixture.Terrain[10, 10] = null;
                    if (scenario == "rum-wall") fixture.Terrain[8, 4] = fixture.Terrain[9, 4] = "ground";
                    if (scenario == "rum-gap" || scenario == "rum-wall")
                    {
                        var xml = new System.Text.StringBuilder("<level width=\"20\" height=\"20\" players=\"1\">");
                        for (int row = 0; row < 20; row++)
                        {
                            xml.Append("<row>");
                            for (int col = 0; col < 20; col++)
                            { if (col > 0) xml.Append(','); xml.Append(fixture.Terrain[row, col] ?? "-").Append(":1"); }
                            xml.Append("</row>");
                        }
                        for (int row = 0; row < 20; row++) xml.Append("<bgRow>-:20</bgRow>");
                        scenarioXml = new TextAsset(xml.Append("</level>").ToString());
                        // Real spawned weapons cache their map from the XML parser,
                        // not from the fixture character's private terrain array.
                        fixture.Controller.LevelXml = scenarioXml;
                    }
                    if (scenario == "existing-mine-jump" || scenario == "existing-mine-still")
                    {
                        existingMine = (MutinyMine)MutinyWeaponFactory.SpawnAndFire("mine", fixture.Enemy, Vector2.zero, false);
                        existingMine.PhysicsBody.State.X = 110; existingMine.PhysicsBody.State.Y = 306;
                        for (int i = 0; i < 12; i++) existingMine.PhysicsBody.AdvanceSimulationTick();
                        result.Assert(existingMine.IsStored && !existingMine.IsActive && existingMine.IgnoreTicksRemaining == 0,
                            "EXT-AI-FX-07 existing mine naturally arms without activating near stationary character");
                    }
                    var target = new Vector2(fixture.Enemy.PhysicsBody.State.X, fixture.Enemy.PhysicsBody.State.Y);
                    var command = new MutinyAIEffectCommand { Actor = 0, TargetActor = 1, Type = AIMoveType.ShootWeapon,
                        Weapon = weapon.ToLowerInvariant(), Target = target,
                        Velocity = MutinyAIEffectWorld.AimedVelocity(new Vector2(96, 312) + MutinyWeapon.GetOriginalEquipOffsetPixels(weapon), target,
                            MutinyWeaponFactory.GetTwangMaxForce(weapon)), FlightY = 212f,
                        ShotXs = new[] { 20f, 50f }, BoxPossibilities = new[] { new Vector2(320, 240), new Vector2(384, 240), new Vector2(448, 240) } };
                    if (weapon == "parachuteBomb") command.FanDirection = 1;
                    if (weapon == "rumBottle") command.Velocity = new Vector2(10, -10);
                    if (weapon == "boulder") command.Velocity = new Vector2(10, -3);
                    if (weapon == "voodooDoll") command.Velocity = new Vector2(20, -5);
                    if (weapon == "cannon") { command.Target = new Vector2(96, 212); command.Velocity = (target - command.Target).normalized * 30f; }
                    if (weapon == "cannonball") command.Velocity = (target - new Vector2(96, 302)).normalized * 20f;
                    if (scenario == "barrel-chain") command.Velocity = new Vector2(10, -5);
                    if (scenario == "existing-mine-jump") { command.Type = AIMoveType.SelfThrow; command.Velocity = new Vector2(5, -2); }
                    if (scenario == "existing-mine-still") command.Type = AIMoveType.Pass;
                    if (scenario == "fan-left-switch") command.FanDirection = -1;
                    var input = Input(fixture);
                    int objectCount = Object.FindObjectsByType<GameObject>().Length;
                    var rng = UnityEngine.Random.state;
                    var world = new MutinyAIEffectWorld(input, command, 173);
                    while (world.Advance()) { }
                    result.Assert(world.Outcome.Settled, "EXT-AI-FX-08 full effect reaches settlement " + weapon + " " + world.Outcome.Status);
                    result.Assert(Object.FindObjectsByType<GameObject>().Length == objectCount && fixture.Actor.Health == 1000f && fixture.Enemy.Health == 1000f &&
                        JsonUtility.ToJson(rng) == JsonUtility.ToJson(UnityEngine.Random.state), "EXT-AI-FX-02 trial has no live objects/HP/RNG mutation " + weapon);
                    if (!world.Outcome.Settled) return;
                    var move = new AIMove { Character = fixture.Actor, MoveType = command.Type, WeaponType = weapon,
                        LaunchVelocity = command.Velocity, TargetPosition = command.Target, TargetCharacter = fixture.Enemy,
                        SeagullFlightY = command.FlightY, SeagullShotXs = command.ShotXs, BoxPossibilities = command.BoxPossibilities,
                        CannonRotationDegrees = Mathf.RoundToInt(Mathf.Atan2(command.Velocity.y, command.Velocity.x) * Mathf.Rad2Deg),
                        StrategyContext = new MutinyAIStrategyContext(MutinyAIStrategyMode.Enhanced, MutinyAIController.StrategyConfigurationVersion,
                            "enhanced-effects-v1", "effects-v1", false), ActionPlan = world.Plan };
                    fixture.Ai.ExecuteMoveForVerification(move, manager);
                    if (scenario == "fan-left-switch")
                    {
                        gm.ExecuteCommand("aienhance 0");
                        var bomb = Object.FindAnyObjectByType<MutinyParachuteBomb>();
                        result.Assert(bomb != null && bomb.AiActionPlan.FanDirection == -1,
                            "EXT-AI-FX-06 committed fan controls remain latched after GM disable");
                    }
                    var explosions = new Dictionary<MutinyExplosion, int>();
                    var flamesSeen = new HashSet<MutinySweepingFlame>();
                    int ticks = Mathf.Min(2048, world.Outcome.Ticks + 30);
                    for (int tick = 1; tick <= ticks; tick++) AdvanceProductionTick(fixture, explosions, tick, flamesSeen);
                    if (weapon == "rumBottle" && scenario != "rum-wall")
                        result.Assert((fixture.Actor.Health < 1000) == (world.Outcome.AllyHpAfter < 1000) &&
                            (fixture.Enemy.Health < 1000) == (world.Outcome.EnemyHpAfter < 1000),
                            "EXT-AI-FX-05 real/model flame damage reach only; random final HP not claimed equal " + scenario);
                    else result.Assert(Mathf.Abs(fixture.Actor.Health - world.Outcome.AllyHpAfter) <= 5f &&
                        Mathf.Abs(fixture.Enemy.Health - world.Outcome.EnemyHpAfter) <= 5f,
                        "EXT-AI-FX-04 production/model HP " + weapon + " actual=" + fixture.Actor.Health + "/" + fixture.Enemy.Health +
                        " model=" + world.Outcome.AllyHpAfter + "/" + world.Outcome.EnemyHpAfter + " tolerance=5");
                    if (weapon == "rumBottle") result.Logs.Add("[MODEL-LIMIT] Rum random knockback actual=" + fixture.Actor.Health + "/" + fixture.Enemy.Health +
                        " model=" + world.Outcome.AllyHpAfter + "/" + world.Outcome.EnemyHpAfter + "; no ally HP parity claim");
                    result.Assert(fixture.Actor.WeaponInventory[weapon] == (command.Type == AIMoveType.ShootWeapon ? 4 : 5),
                        "EXT-AI-FX-06 real output inventory debit matches action " + weapon + "/" + scenario);
                    if (scenario == "barrel-chain") result.Assert(world.Outcome.Explosions >= 2 && MutinyBoxRegistry.Count == 0,
                        "EXT-AI-FX-04 barrel chain explodes and synchronously unregisters live obstacle");
                    if (scenario == "existing-mine-jump") result.Assert(existingMine.IsFinished && world.Outcome.Explosions == 1 && fixture.Actor.Health < 1000,
                        "EXT-AI-FX-07 real jump activates previously stored mine and models delayed damage");
                    if (scenario == "existing-mine-still") result.Assert(!existingMine.IsActive && world.Outcome.Explosions == 0 && fixture.Actor.Health == 1000,
                        "EXT-AI-FX-07 stationary stored mine cannot create fictional current-turn damage");
                    if (weapon == "rumBottle" && scenario == null) result.Assert(world.Outcome.FlameSegments > 10 && flamesSeen.Count > 10 &&
                        world.Outcome.EnemyHpAfter < 1000f && fixture.Enemy.Health < 1000f,
                        "EXT-AI-FX-05 model and production include spreading platform flame damage");
                    if (scenario == "rum-gap") result.Assert(world.Outcome.FlameSegments > 10 && flamesSeen.Count > 10 &&
                        world.Outcome.EnemyHpAfter == 1000 && fixture.Enemy.Health == 1000,
                        "EXT-AI-FX-05 model and real fire stop at platform gap before far enemy");
                    if (scenario == "rum-wall") result.Assert(world.Outcome.FlameSegments == 0 && flamesSeen.Count == 0,
                        "EXT-AI-FX-05 real wall contact creates no model or production sweeping flames");
                    if (weapon == "piecesOfEight") result.Assert(world.Outcome.CoinsFired == 8 && move.ActionPlan.CoinCount == 8,
                        "EXT-AI-FX-06 all eight coin launches modeled and submitted");
                    if (weapon == "woodenCrate" || weapon == "gunpowderBarrel") result.Assert(MutinyBoxRegistry.Count ==
                        (weapon == "woodenCrate" ? 3 : 2), "EXT-AI-FX-08 fixed plan produces full real box sequence " + weapon);
                    if (weapon == "voodooDoll") result.Assert(!fixture.Enemy.IsAlive && world.Outcome.EnemyHpAfter == 0f,
                        "EXT-AI-FX-08 voodoo transfer scores real subsequent drowning, not direct doll damage");
                }
            }
            finally
            {
                // Only objects born in this isolated fixture; original singleton
                // objects are preserved. Includes caster-less chain/flame/debris.
                foreach (var go in Object.FindObjectsByType<GameObject>())
                    if (go != null && !beforeObjects.Contains(go)) Object.DestroyImmediate(go);
                MutinyBoxRegistry.ResetForLevel();
                if (scenarioXml != null) Object.DestroyImmediate(scenarioXml);
                gm.ExecuteCommand("aienhance 1");
            }
        }

        private static void AdvanceProductionTick(MutinyAiResponsiveSearchVerificationTest.Fixture fixture,
            Dictionary<MutinyExplosion, int> explosions, int tick, HashSet<MutinySweepingFlame> flamesSeen)
        {
            fixture.Actor.PhysicsBody.enabled = false; fixture.Enemy.PhysicsBody.enabled = false;
            fixture.Actor.PhysicsBody.AdvanceSimulationTick();
            fixture.Enemy.PhysicsBody.AdvanceSimulationTick();
            var weapons = Object.FindObjectsByType<MutinyWeapon>();
            foreach (var weapon in weapons)
            {
                if (weapon.Owner != fixture.Actor && weapon.Owner != fixture.Enemy) continue;
                weapon.enabled = false; weapon.PhysicsBody.enabled = false;
                if (weapon is MutinyPiecesOfEight coins) coins.AdvanceAiWaitForVerification(MutinyPhysics.TimeStep);
                if (weapon is MutinyWoodenCrate crate && crate.IsAiPlacementActive) crate.AdvanceAiPlacementTickForVerification();
                if (weapon is MutinyGunpowderBarrel barrel && barrel.IsAiPlacementActive) barrel.AdvancePlacementTickForVerification();
                if (weapon is MutinyAnchor anchor) anchor.AdvanceOriginalTickForVerification();
                else if (weapon is MutinyCannon cannon) cannon.AdvanceOriginalTickForVerification();
                else if (weapon.PhysicsBody.IsActive && (!weapon.IsFinished || weapon is MutinyWoodenCrate || weapon is MutinyGunpowderBarrel))
                    weapon.PhysicsBody.AdvanceSimulationTick();
            }
            foreach (var explosion in Object.FindObjectsByType<MutinyExplosion>())
            {
                explosion.enabled = false;
                if (!explosions.ContainsKey(explosion)) explosions.Add(explosion, tick);
                if (tick - explosions[explosion] < 2) continue;
                explosion.ApplyHit(); explosions.Remove(explosion); Object.DestroyImmediate(explosion.gameObject);
            }
            foreach (var explosion in Object.FindObjectsByType<MutinyExplosion>())
                if (!explosions.ContainsKey(explosion)) { explosion.enabled = false; explosions.Add(explosion, tick); }
            foreach (var flame in Object.FindObjectsByType<MutinySweepingFlame>())
            {
                flamesSeen.Add(flame);
                flame.enabled = false; flame.AdvanceOriginalTickForVerification();
                if (flame.CurrentFrame >= MutinySweepingFlame.OriginalFrameCount) Object.DestroyImmediate(flame.gameObject);
            }
        }

        private static IEnumerator VerifyPlanner(MutinyLevel1VerificationResult result, MutinyGMManager gm, int luck)
        {
            gm.ExecuteCommand("aienhance 1");
            using (var fixture = new MutinyAiResponsiveSearchVerificationTest.Fixture(luck))
            {
                fixture.Actor.WeaponInventory.Clear(); foreach (string weapon in MutinyAiStrategyVerificationTest.Weapons) fixture.Actor.AddWeapon(weapon, 5);
                fixture.Actor.CanThrow = false;
                int objects = Object.FindObjectsByType<GameObject>().Length;
                string randomBefore = JsonUtility.ToJson(UnityEngine.Random.state);
                AIMove move = default;
                double start = Time.realtimeSinceStartupAsDouble; int firstFrame = Time.frameCount;
                IEnumerator search = fixture.Ai.EvaluateBestMoveBudgetedForVerification(value => move = value);
                while (search.MoveNext()) yield return search.Current;
                var summary = fixture.Ai.LastEnhancedDecision;
                result.Logs.Add("[PERF-ENHANCED] luck=" + luck + " frames=" + (Time.frameCount - firstFrame) +
                    " elapsedSeconds=" + (Time.realtimeSinceStartupAsDouble - start).ToString("R") + " slices=" + fixture.Ai.SearchSliceCount +
                    " maxSliceMs=" + fixture.Ai.MaximumSearchSliceMilliseconds.ToString("R") +
                    " coarse=" + (summary?.CoarseCandidates ?? 0) + " full=" + (summary?.Simulations ?? 0));
                result.Assert(summary != null && summary.AlgorithmId == "effects-v1" && !move.StrategyContext.UsesFallback,
                    "EXT-AI-FX-01 actual budget pump returns enhanced effects route luck=" + luck);
                if (summary == null) yield break;
                result.Assert(summary.Simulations > 0 && summary.Simulations <= MutinyAIController.EnhancedMaxFullSimulations &&
                    summary.CoarseCandidates < 2200, "EXT-AI-FX-03 bounded full/coarse search luck=" + luck);
                var weapons = new HashSet<string>(); foreach (var evaluation in summary.Evaluations) weapons.Add(evaluation.Weapon);
                foreach (string weapon in MutinyAiStrategyVerificationTest.Weapons)
                    result.Assert(weapons.Contains(weapon), "EXT-AI-FX-03 every owned weapon enters full refinement " + luck + "/" + weapon);
                result.Assert(fixture.Ai.SearchSliceCount > 1 && fixture.Ai.MaximumSearchSliceMilliseconds < 100,
                    "EXT-AI-FX-03 search really yields between frames luck=" + luck + " maxSliceMs=" + fixture.Ai.MaximumSearchSliceMilliseconds);
                result.Assert(fixture.Actor.Health == 100 && fixture.Enemy.Health == 100 && fixture.Actor.WeaponInventory["cherryBomb"] == 5 &&
                    Object.FindObjectsByType<GameObject>().Length == objects && randomBefore == JsonUtility.ToJson(UnityEngine.Random.state),
                    "EXT-AI-FX-02 entire enhanced planner leaves real HP/inventory/objects/RNG intact luck=" + luck);
                float max = 0; foreach (var evaluation in summary.Evaluations) if (evaluation.Settled) max = Mathf.Max(max, evaluation.Score);
                result.Assert(Mathf.Approximately(move.Score, max) && (max > 0 || move.MoveType == AIMoveType.Pass),
                    "EXT-AI-FX-09 actual winner is maximum positive settled HP outcome or Pass");
                if (luck == 100)
                {
                    string first = JsonUtility.ToJson(summary); var trace = fixture.Ai.LastDecisionTrace;
                    fixture.Ai.SetReplayTraceForVerification(trace); fixture.Ai.EvaluateBestMove();
                    result.Assert(first == JsonUtility.ToJson(fixture.Ai.LastEnhancedDecision), "EXT-AI-FX-10 enhanced full-effect replay is deterministic");
                }
            }
            yield return null;
        }

        private static void VerifyPassAndHorizon(MutinyLevel1VerificationResult result, MutinyGMManager gm)
        {
            using (var fixture = new MutinyAiResponsiveSearchVerificationTest.Fixture(100))
            {
                fixture.Actor.WeaponInventory.Clear();
                var manager = fixture.Host.AddComponent<MutinyTurnManager>(); manager.Initialize(fixture.Team, fixture.EnemyTeam);
                manager.enabled = false;
                result.Assert(fixture.Actor.HasWeapon("cannonball"), "EXT-AI-FX-08 real StartTurn grants cannonball to unarmed actor");
                AIMove fallback = fixture.Ai.EvaluateBestMove();
                result.Assert(fallback.MoveType == AIMoveType.ShootWeapon && fallback.WeaponType == "cannonball" && fallback.Score > 0,
                    "EXT-AI-FX-08 actual planner can select and fully simulate automatic fallback cannonball");
            }
            using (var fixture = new MutinyAiResponsiveSearchVerificationTest.Fixture(0))
            {
                fixture.Actor.CanThrow = false; fixture.Actor.WeaponInventory.Clear(); fixture.Actor.AddWeapon("woodenCrate", 5);
                AIMove move = fixture.Ai.EvaluateBestMove();
                result.Assert(move.MoveType == AIMoveType.Pass && move.Score == 0, "EXT-AI-FX-09 zero HP gain finite box may legally Pass");
                var world = new MutinyAIEffectWorld(Input(fixture), new MutinyAIEffectCommand {
                    Type = AIMoveType.ShootWeapon, Actor = 0, Weapon = "parachutebomb", Velocity = new Vector2(20, -20) }, 1, maxTicks: 1);
                while (world.Advance()) { }
                result.Assert(!world.Outcome.Settled && world.Outcome.Status == "horizon-exceeded", "EXT-AI-FX-04 truncated flight cannot be called complete");
            }
        }

        private static IEnumerator VerifyMineInvalidation(MutinyLevel1VerificationResult result, MutinyGMManager gm)
        {
            gm.ExecuteCommand("aienhance 1");
            using (var fixture = new MutinyAiResponsiveSearchVerificationTest.Fixture(99999, runtime: true))
            {
                fixture.Actor.WeaponInventory.Clear(); foreach (string weapon in MutinyAiStrategyVerificationTest.Weapons) fixture.Actor.AddWeapon(weapon, 5);
                // This case exercises mutation during full refinement; the jump
                // budget itself is covered above without delaying that checkpoint.
                fixture.Actor.CanThrow = false;
                fixture.Ai.DecisionBudgetMilliseconds = 0.1f; fixture.Ai.SaveDecisionTrace = true;
                var mine = (MutinyMine)MutinyWeaponFactory.SpawnAndFire("mine", fixture.Enemy, Vector2.zero, false);
                mine.PhysicsBody.State.X = 110; mine.PhysicsBody.State.Y = 306;
                for (int tick = 0; tick < 30; tick++)
                {
                    fixture.Actor.PhysicsBody.AdvanceSimulationTick(); fixture.Enemy.PhysicsBody.AdvanceSimulationTick();
                    mine.PhysicsBody.AdvanceSimulationTick();
                }
                var manager = fixture.Host.AddComponent<MutinyTurnManager>(); manager.Initialize(fixture.Team, fixture.EnemyTeam);
                result.Assert(gm.ExecuteCommand("aitakeover 99999"), "EXT-AI-FX-02 start genuine GM enhanced search for mine invalidation");
                double deadline = Time.realtimeSinceStartupAsDouble + 15;
                while (fixture.Ai.EnhancedSimulationsInProgressForVerification < 1 && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                result.Assert(fixture.Ai.EnhancedSimulationsInProgressForVerification > 0,
                    "EXT-AI-FX-02 mutation is applied in actual full refinement, not only coarse sampling");
                int canceled = fixture.Ai.InvalidatedDecisionCount;
                string path = fixture.Ai.CurrentDecisionTracePath;
                result.Assert(MutinyMine.NotifyCharacterBeganSelfThrowAim(fixture.Actor) && mine.IsActive,
                    "EXT-AI-FX-02 real Mine aim-notification transition activates previously frozen mine");
                deadline = Time.realtimeSinceStartupAsDouble + 10;
                while (fixture.Ai.InvalidatedDecisionCount == canceled && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                result.Assert(mine.IsFired && fixture.Ai.InvalidatedDecisionCount > canceled &&
                    fixture.Ai.LastDecisionInvalidationReason == "enhanced-mine-state" && !fixture.Ai.LastCommittedStrategy.IsBound,
                    "EXT-AI-FX-02 real mine countdown activation cancels frozen enhanced result before submission; count=" +
                    canceled + "->" + fixture.Ai.InvalidatedDecisionCount + " reason=" + fixture.Ai.LastDecisionInvalidationReason +
                    " committed=" + fixture.Ai.LastCommittedStrategy.IsBound);
                result.Assert(path != null && !System.IO.File.Exists(path) && !System.IO.File.Exists(path + ".enhanced.json") &&
                    fixture.Actor.WeaponInventory["cherryBomb"] == 5,
                    "EXT-AI-FX-10 canceled full simulation does not publish final trace/sidecar or consume inventory");
            }
            yield return null;
        }

        private static IEnumerator VerifyLevel12(MutinyLevel1VerificationResult result, MutinyGMManager gm, int luck)
        {
            Debug.Log("[AI-ENHANCED-STAGE] real level 12 luck=" + luck);
            var before = new HashSet<GameObject>(Object.FindObjectsByType<GameObject>());
            bool mode = MutinyAIController.EnhancementEnabled;
            var host = new GameObject("EnhancedLevel12Fixture");
            var controller = host.AddComponent<Mutiny.Levels.MutinyLevelController>(); controller.enabled = false;
            try
            {
                gm.ExecuteCommand("aienhance 1"); gm.ExecuteCommand("aiforceusewaepon 0");
                bool loaded = controller.TryLoadLevel(12);
                result.Assert(loaded, "EXT-AI-FX-03 load real level 12 XML through production level builder luck=" + luck);
                if (!loaded) yield break;
                var root = controller.CurrentLevel;
                var ai = root.Team2.GetComponent<MutinyAIController>(); ai.SaveDecisionTrace = false;
                var manager = root.GetComponent<MutinyTurnManager>();
                result.Assert(gm.ExecuteCommand("aisetluck " + luck), "EXT-AI-FX-03 real level 12 GM luck override=" + luck);
                double readyBy = Time.realtimeSinceStartupAsDouble + 5;
                while (manager.CurrentPhase == TurnPhase.NotStarted && Time.realtimeSinceStartupAsDouble < readyBy) yield return null;
                result.Assert(manager.CurrentPhase == TurnPhase.TurnActive && manager.CurrentTeam == root.Team1,
                    "EXT-AI-FX-03 real level manager Start establishes initial human turn before Pass");
                manager.PassTurn();
                double start = Time.realtimeSinceStartupAsDouble, deadline = start + (luck > 100 ? 120 : 30);
                while (!ai.LastCommittedStrategy.IsBound && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                result.Assert(ai.LastCommittedStrategy.IsBound && ai.LastCommittedStrategy.Mode == MutinyAIStrategyMode.Enhanced &&
                    ai.LastEnhancedDecision != null && ai.LastEnhancedDecision.Simulations <= MutinyAIController.EnhancedMaxFullSimulations,
                    "EXT-AI-FX-03 real level 12 enemy completes bounded simulation and submits normal action luck=" + luck);
                result.Assert(ai.SearchSliceCount > 1 && ai.MaximumSearchSliceMilliseconds < 100,
                    "EXT-AI-FX-03 real level 12 search runs over actual rendered frames luck=" + luck);
                result.Logs.Add("[PERF-LEVEL12] luck=" + luck + " elapsedWithTurnSettlement=" +
                    (Time.realtimeSinceStartupAsDouble - start).ToString("R") + " slices=" + ai.SearchSliceCount +
                    " maxSliceMs=" + ai.MaximumSearchSliceMilliseconds.ToString("R") + " invalidations=" + ai.InvalidatedDecisionCount +
                    " coarse=" + (ai.LastEnhancedDecision?.CoarseCandidates ?? 0) + " full=" + (ai.LastEnhancedDecision?.Simulations ?? 0));
            }
            finally
            {
                foreach (var go in Object.FindObjectsByType<GameObject>()) if (go != null && !before.Contains(go)) Object.DestroyImmediate(go);
                MutinyBoxRegistry.ResetForLevel(); gm.ExecuteCommand(mode ? "aienhance 1" : "aienhance 0");
            }
            yield return null;
        }
    }
}
