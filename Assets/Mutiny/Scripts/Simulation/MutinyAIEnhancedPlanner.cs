using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Mutiny.Simulation
{
    public sealed partial class MutinyAIController
    {
        public const int EnhancedMaxWeaponSamples = 128;
        public const int EnhancedMaxFullSimulations = 256;
        public MutinyAIEnhancedDecision LastEnhancedDecision { get; private set; }
        internal int EnhancedSimulationsInProgressForVerification => m_ActiveWork?.Enhanced?.Summary.Simulations ?? 0;

        private sealed class EnhancedCandidateGroup
        {
            internal readonly List<AIMove> Best = new List<AIMove>(2);
            internal readonly List<AIMove> Aimed = new List<AIMove>(2);
            internal AIMove Diverse;
            internal bool HasDiverse;
            internal void Add(AIMove move)
            {
                if (!HasDiverse && Best.Count > 0 && Vector2.Distance(move.LaunchVelocity, Best[0].LaunchVelocity) > 8f)
                { Diverse = move; HasDiverse = true; }
                int index = 0; while (index < Best.Count && Best[index].Score >= move.Score) index++;
                Best.Insert(index, move); if (Best.Count > 2) Best.RemoveAt(2);
            }
            internal bool TryGet(int round, out AIMove move)
            {
                if (round < Best.Count) { move = Best[round]; return true; }
                if (round == 2 && HasDiverse) { move = Diverse; return true; }
                if (round >= 3 && round - 3 < Aimed.Count) { move = Aimed[round - 3]; return true; }
                move = default; return false;
            }
        }

        private sealed class EnhancedDecisionState
        {
            internal bool Collecting = true;
            internal MutinyAIEffectInput Input;
            internal readonly Dictionary<MutinyCharacter, int> ActorIds = new Dictionary<MutinyCharacter, int>();
            internal readonly Dictionary<string, EnhancedCandidateGroup> Groups = new Dictionary<string, EnhancedCandidateGroup>();
            internal readonly List<string> GroupOrder = new List<string>();
            internal readonly Dictionary<MutinyMine, MutinyAIEffectMine> Mines = new Dictionary<MutinyMine, MutinyAIEffectMine>();
            internal readonly Dictionary<MutinyCharacter, HashSet<string>> InfiniteSupply = new Dictionary<MutinyCharacter, HashSet<string>>();
            internal readonly MutinyAIEnhancedDecision Summary = new MutinyAIEnhancedDecision();
            internal EnhancedCandidateGroup Group(AIMove move)
            {
                string key = ActorIds[move.Character] + ":" + (move.MoveType == AIMoveType.SelfThrow ? "jump" : move.WeaponType.ToLowerInvariant());
                if (!Groups.TryGetValue(key, out var group))
                { group = new EnhancedCandidateGroup(); Groups.Add(key, group); GroupOrder.Add(key); }
                return group;
            }
        }

        private EnhancedDecisionState CaptureEnhancedInput(DecisionWork work)
        {
            var enhanced = new EnhancedDecisionState(); var characters = new List<MutinyAIEffectCharacter>();
            void Add(MutinyCharacter character)
            {
                if (enhanced.ActorIds.ContainsKey(character)) return;
                var saved = StateOf(character, work); enhanced.ActorIds.Add(character, characters.Count);
                enhanced.InfiniteSupply.Add(character, new HashSet<string>(character.InfiniteWeapons, StringComparer.OrdinalIgnoreCase));
                characters.Add(new MutinyAIEffectCharacter { Body = saved.Body, Health = saved.Health, Team = saved.TeamIndex, Alive = saved.Alive });
            }
            foreach (var character in work.Allies) Add(character);
            foreach (var character in work.Enemies) Add(character);
            var boxes = new List<MutinyAIEffectBox>();
            foreach (var obstacle in work.Snapshot.Boxes)
                boxes.Add(new MutinyAIEffectBox { Body = obstacle.State,
                    Barrel = obstacle.Body != null && obstacle.Body.GetComponent<MutinyGunpowderBarrel>() != null });
            var mines = new List<MutinyAIEffectMine>();
            foreach (var mine in UnityEngine.Object.FindObjectsByType<MutinyMine>())
            {
                if (!mine.IsFired || mine.IsFinished || mine.PhysicsBody == null) continue;
                var saved = CaptureMine(mine); enhanced.Mines.Add(mine, saved); mines.Add(saved);
            }
            var chests = new List<Vector2>();
            foreach (var chest in work.Snapshot.Chests) if (!chest.Finished) chests.Add(new Vector2(chest.X, chest.FloorY));
            enhanced.Input = new MutinyAIEffectInput(work.Terrain, work.GridW, work.GridH, work.WaterY, m_Team.TeamNumber,
                characters.ToArray(), boxes.ToArray(), mines.ToArray(), chests.ToArray());
            enhanced.Summary.Seed = m_Random.Trace.Seed;
            return enhanced;
        }

        private static MutinyAIEffectMine CaptureMine(MutinyMine mine) => new MutinyAIEffectMine
        {
            Body = mine.PhysicsBody.State, Ignore = mine.IgnoreTicksRemaining,
            Countdown = mine.CountdownRemaining, Active = mine.IsActive, Stored = mine.IsStored
        };

        private static bool EnhancedInputsAreCurrent(DecisionWork work, out string reason)
        {
            reason = null; if (work.Enhanced == null) return true;
            foreach (var entry in work.Enhanced.InfiniteSupply)
                if (entry.Key != null && !entry.Value.SetEquals(entry.Key.InfiniteWeapons))
                { reason = "enhanced-infinite-supply"; return false; }
            int count = 0;
            foreach (var mine in UnityEngine.Object.FindObjectsByType<MutinyMine>())
            {
                if (!mine.IsFired || mine.IsFinished || mine.PhysicsBody == null) continue;
                count++;
                if (!work.Enhanced.Mines.TryGetValue(mine, out var saved)) { reason = "enhanced-mine-added"; return false; }
                var current = CaptureMine(mine);
                if (!SamePhysics(saved.Body, current.Body) || saved.Ignore != current.Ignore ||
                    saved.Countdown != current.Countdown || saved.Active != current.Active || saved.Stored != current.Stored)
                { reason = "enhanced-mine-state"; return false; }
            }
            if (count != work.Enhanced.Mines.Count) { reason = "enhanced-mine-removed"; return false; }
            return true;
        }

        private IEnumerator EvaluateEnhancedDecisionSteps(DecisionWork work)
        {
            work.Enhanced = CaptureEnhancedInput(work);
            IEnumerator coarse = EvaluateLegacyDecisionSteps(work);
            try { while (coarse.MoveNext()) yield return null; }
            finally { (coarse as IDisposable)?.Dispose(); }
            foreach (object step in AddEnhancedAimedCandidates(work)) yield return null;
            var state = work.Enhanced;
            state.Summary.CoarseCandidates = work.CandidateCount;
            state.Collecting = false; work.Best = CreatePassMove();
            work.ActionOrder.Clear(); work.BestByAction.Clear();
            // Round robin: each actor/weapon group gets a refinement before any
            // group consumes all of its remaining quota. Fan variants count too.
            for (int round = 0; round < 5; round++)
                foreach (string key in state.GroupOrder)
                {
                    if (!state.Groups[key].TryGet(round, out AIMove candidate)) continue;
                    int[] fans = string.Equals(candidate.WeaponType, "parachuteBomb", StringComparison.OrdinalIgnoreCase)
                        ? new[] { 0, -1, 1 } : new[] { 0 };
                    foreach (int fan in fans)
                    {
                        if (state.Summary.Simulations >= EnhancedMaxFullSimulations) { state.Summary.BudgetSkipped++; continue; }
                        var command = new MutinyAIEffectCommand
                        {
                            Type = candidate.MoveType, Actor = state.ActorIds[candidate.Character],
                            TargetActor = candidate.TargetCharacter != null ? state.ActorIds[candidate.TargetCharacter] : -1,
                            Weapon = candidate.WeaponType?.ToLowerInvariant(), Velocity = candidate.LaunchVelocity,
                            Target = candidate.TargetPosition, FlightY = candidate.SeagullFlightY,
                            ShotXs = candidate.SeagullShotXs, BoxPossibilities = candidate.BoxPossibilities, FanDirection = fan
                        };
                        // Existing trace stores draws as float. Keep integer seeds
                        // exactly representable so replay cannot change rum kicks.
                        var world = new MutinyAIEffectWorld(state.Input, command, NextRandomInt(1, 1 << 24));
                        while (world.Advance()) yield return null;
                        var outcome = world.Outcome;
                        outcome.Character = candidate.Character.name; outcome.Weapon = candidate.WeaponType; outcome.Action = candidate.MoveType.ToString();
                        outcome.TargetCharacter = candidate.TargetCharacter?.name;
                        float position = candidate.MoveType == AIMoveType.SelfThrow && world.CharacterAt(command.Actor).Alive
                            ? EnhancedPositionScore(state.Input, command.Actor, outcome.FinalActorPosition) : 0f;
                        candidate.UsesForcedWeaponSupply = candidate.MoveType == AIMoveType.ShootWeapon && work.ForcedWeaponType != null &&
                            string.Equals(candidate.WeaponType, work.ForcedWeaponType, StringComparison.OrdinalIgnoreCase);
                        var supply = state.InfiniteSupply[candidate.Character];
                        bool infinite = supply.Contains(candidate.WeaponType ?? "") ||
                            string.Equals(candidate.WeaponType, "cannon", StringComparison.OrdinalIgnoreCase) && supply.Contains("cannonball") ||
                            string.Equals(candidate.WeaponType, "cannonball", StringComparison.OrdinalIgnoreCase) && supply.Contains("cannon");
                        bool finite = candidate.MoveType == AIMoveType.ShootWeapon && !candidate.UsesForcedWeaponSupply && !infinite;
                        MutinyAIEffectScore.Evaluate(outcome, finite, outcome.EnemyHpBefore > 0f && outcome.EnemyHpAfter <= 0f && outcome.AllyHpAfter > 0f,
                            outcome.AllyHpBefore > 0f && outcome.AllyHpAfter <= 0f, position);
                        state.Summary.Simulations++; state.Summary.Evaluations.Add(outcome);
                        if (!outcome.Settled) { state.Summary.Truncated++; continue; }
                        candidate.Score = outcome.Score; candidate.ActionPlan = world.Plan;
                        ConsiderCandidate(ref work.Best, candidate, ref work.CandidateCount, work);
                        yield return null;
                    }
                }
            state.Summary.WinnerScore = work.Best.Character != null && work.Best.Score > 0f ? work.Best.Score : 0f;
        }

        private IEnumerable<object> AddEnhancedAimedCandidates(DecisionWork work)
        {
            foreach (var actor in work.Allies)
            {
                var saved = StateOf(actor, work);
                if (!saved.Alive || !saved.CanShoot || (work.ContinuationCharacter != null && actor != work.ContinuationCharacter)) continue;
                foreach (string weapon in WeaponChoices(actor, work))
                {
                    if (!HasEffectiveWeapon(actor, weapon, work)) continue;
                    string kind = weapon.ToLowerInvariant();
                    var targets = new List<MutinyCharacter>();
                    foreach (var enemy in work.Enemies) if (StateOf(enemy, work).Alive) targets.Add(enemy);
                    targets.Sort((a, b) => Vector2.Distance(PositionOf(actor, work), PositionOf(a, work)).CompareTo(
                        Vector2.Distance(PositionOf(actor, work), PositionOf(b, work))));
                    if (kind == "woodencrate" || kind == "gunpowderbarrel")
                    {
                        var positions = new List<Vector2>();
                        for (int col = 0; col < work.GridW && positions.Count < 3; col++)
                        {
                            for (int row = 2; row < work.GridH; row++)
                            {
                                if (!MutinySweepingFlame.IsSolidTile(work.Terrain, work.GridW, work.GridH, col, row)) continue;
                                var point = new Vector2(col * 32f + 16f, row * 32f - 64f);
                                if (MutinyBoxPlacementRules.CanPlace(point, work.Terrain, work.GridW, work.GridH, work.Snapshot.Boxes,
                                    work.Snapshot.ChestPositions, work.Snapshot.LivingBodies)) positions.Add(point);
                                break;
                            }
                            yield return null;
                        }
                        if (positions.Count >= 3)
                        {
                            var boxMove = new AIMove { Character = actor, MoveType = AIMoveType.ShootWeapon, WeaponType = weapon,
                                BoxPossibilities = positions.ToArray() };
                            var group = work.Enhanced.Group(boxMove);
                            group.Aimed.Add(boxMove);
                            if (group.Best.Count == 0) ConsiderCandidate(ref work.Best, boxMove, ref work.CandidateCount, work);
                        }
                        continue;
                    }
                    for (int i = 0; i < Mathf.Min(2, targets.Count); i++)
                    {
                        var target = PositionOf(targets[i], work);
                        var move = new AIMove { Character = actor, MoveType = AIMoveType.ShootWeapon, WeaponType = weapon, TargetPosition = target };
                        if (MutinyWeaponFactoryCanFire(weapon))
                            move.LaunchVelocity = MutinyAIEffectWorld.AimedVelocity(PositionOf(actor, work) + MutinyWeapon.GetOriginalEquipOffsetPixels(weapon),
                                target, MutinyWeaponFactory.GetTwangMaxForce(weapon), i == 0 ? 1f : 1.35f,
                                MutinyAIEffectWorld.ProjectileBody(kind, saved.Body).Weight);
                        else if (kind == "anchor") { }
                        else if (kind == "voodoodoll") { move.TargetCharacter = targets[i]; move.LaunchVelocity = new Vector2(i == 0 ? 18f : -18f, -5f); }
                        else if (kind == "cannon")
                        {
                            move.TargetPosition = PositionOf(actor, work) + new Vector2(0f, MutinyCannon.PlacementOffsetY);
                            move.LaunchVelocity = (target - move.TargetPosition).normalized * MutinyCannon.FireStrength;
                            move.CannonRotationDegrees = Mathf.RoundToInt(Mathf.Atan2(move.LaunchVelocity.y, move.LaunchVelocity.x) * Mathf.Rad2Deg);
                        }
                        else if (kind == "seagull")
                        {
                            move.SeagullFlightY = target.y - 100f;
                            move.SeagullShotXs = new[] { Mathf.Clamp(target.x - 100f, 0f, work.GridW * 32f - 1f),
                                Mathf.Clamp(target.x - 70f, 0f, work.GridW * 32f - 1f) };
                        }
                        else continue;
                        var group = work.Enhanced.Group(move);
                        group.Aimed.Add(move);
                        if (group.Best.Count == 0) ConsiderCandidate(ref work.Best, move, ref work.CandidateCount, work);
                        yield return null;
                    }
                }
            }
        }

        private static float EnhancedPositionScore(MutinyAIEffectInput input, int actor, Vector2 landing)
        {
            var start = new Vector2(input.Characters[actor].Body.X, input.Characters[actor].Body.Y);
            float before = float.PositiveInfinity, after = float.PositiveInfinity;
            foreach (var c in input.Characters)
                if (c.Alive && c.Team != input.OwnTeam)
                {
                    var target = new Vector2(c.Body.X, c.Body.Y);
                    before = Mathf.Min(before, Vector2.Distance(start, target)); after = Mathf.Min(after, Vector2.Distance(landing, target));
                }
            float score = float.IsInfinity(before) ? 0f : Mathf.Clamp((before - after) / 64f, -2f, 2f);
            foreach (var chest in input.Chests)
                if (Vector2.Distance(landing, chest) < 32f && Vector2.Distance(start, chest) >= 32f) { score += 3f; break; }
            return score;
        }

        private void FinishEnhancedDiagnostics(DecisionWork work)
        {
            LastEnhancedDecision = work.Enhanced?.Summary;
            if (LastEnhancedDecision == null || string.IsNullOrEmpty(work.TracePath)) return;
            // Bounded sidecar; serialize on the main thread, disk I/O off-thread.
            string json = JsonUtility.ToJson(LastEnhancedDecision), path = work.TracePath + ".enhanced.json";
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try { File.WriteAllText(path, json); }
                catch (Exception error) { Debug.LogWarning("[Mutiny:AI-Enhanced] cannot save sidecar: " + error.Message); }
            });
        }

        private static void AppendEnhancedLog(StringBuilder line, DecisionWork work, AIMove move)
        {
            if (work.Enhanced == null) return;
            var summary = work.Enhanced.Summary;
            line.Append(" coarseCandidates=").Append(summary.CoarseCandidates).Append(" fullSimulations=").Append(summary.Simulations)
                .Append(" truncated=").Append(summary.Truncated).Append(" budgetSkipped=").Append(summary.BudgetSkipped)
                .Append(" modelLimits=").Append(summary.Limitations);
            MutinyAIEffectEvaluation winner = null;
            foreach (var evaluation in summary.Evaluations)
                if (evaluation.Settled && evaluation.Character == move.Character?.name && evaluation.Weapon == move.WeaponType &&
                    evaluation.Action == move.MoveType.ToString() && evaluation.Score == move.Score &&
                    evaluation.SimulationSeed == (move.ActionPlan?.SimulationSeed ?? 0))
                { winner = evaluation; break; }
            if (winner == null) return;
            line.Append(" allyHp=").Append(Number(winner.AllyHpBefore)).Append("->").Append(Number(winner.AllyHpAfter))
                .Append(" enemyHp=").Append(Number(winner.EnemyHpBefore)).Append("->").Append(Number(winner.EnemyHpAfter))
                .Append(" scoreParts=[damage:").Append(Number(winner.DamageScore)).Append(",kill:").Append(Number(winner.KillScore))
                .Append(",terminal:").Append(Number(winner.TerminalScore)).Append(",resource:").Append(Number(winner.ResourceScore))
                .Append(",position:").Append(Number(winner.PositionScore)).Append(']')
                .Append(" modelStatus=").Append(winner.Status).Append(" modelTicks=").Append(winner.Ticks)
                .Append(" simulationSeed=").Append(winner.SimulationSeed)
                .Append(" fanDirection=").Append(winner.FanDirection).Append(" coinPlan=").Append(Points(winner.CoinVelocities));
        }
    }
}
