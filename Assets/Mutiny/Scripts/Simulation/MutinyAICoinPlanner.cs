using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    // Shared between a virtual eight-shot rollout and a committed live weapon.
    // One Advance is one bounded search quantum, never a whole trajectory.
    internal sealed class MutinyAICoinBudget
    {
        internal const int MaxWorkSteps = 32768;
        internal int Remaining { get; private set; }
        internal MutinyAICoinBudget(int steps = MaxWorkSteps) => Remaining = Mathf.Max(0, steps);
        internal bool Take() { if (Remaining <= 0) return false; Remaining--; return true; }
    }

    internal sealed class MutinyAICoinPlanner
    {
        internal const int MaxRefinements = 4;
        private readonly MutinyAIEffectInput m_Input;
        private readonly int m_Actor, m_Samples, m_Seed;
        private readonly bool m_Reaim;
        private readonly MutinyAICoinBudget m_Budget;
        private readonly MutinyAIEffectRandom m_Random;
        private readonly List<PhysicsBoxObstacle> m_Boxes = new List<PhysicsBoxObstacle>();
        private readonly List<Vector2> m_Guided = new List<Vector2>();
        private readonly List<Candidate> m_Best = new List<Candidate>(MaxRefinements);
        private PhysicsBodyState m_Body;
        private Vector2 m_CurrentVelocity;
        private int m_SampleIndex, m_FlightTicks, m_Refinement;
        private bool m_Flying, m_Complete;
        private MutinyAIEffectWorld m_Trial;
        private float m_BestScore = float.NegativeInfinity;
        internal MutinyAICoinEvaluation Result { get; }
        private struct Candidate { internal Vector2 Velocity, Impact; internal float Score; }

        internal static int SampleCount(float luck, int total = 1, int alive = 1) =>
            Mathf.Clamp(Mathf.FloorToInt(luck * total / Mathf.Max(1, alive)), 4, MutinyAIController.EnhancedMaxWeaponSamples);

        internal MutinyAICoinPlanner(MutinyAIEffectInput input, int actor, int samples, int seed,
            int index, bool reaim, MutinyAICoinBudget budget)
        {
            m_Input = input; m_Actor = actor; m_Samples = Mathf.Clamp(samples, 4, MutinyAIController.EnhancedMaxWeaponSamples);
            m_Seed = unchecked(seed ^ ((index + 1) * 7919)); m_Random = new MutinyAIEffectRandom(m_Seed);
            m_Reaim = reaim; m_Budget = budget;
            var owner = input.Characters[actor].Body;
            Result = new MutinyAICoinEvaluation { Index = index + 1, ActorPosition = new Vector2(owner.X, owner.Y),
                Fallback = true, Status = "analytic-fallback" };
            foreach (var box in input.Boxes) if (!box.Removed) m_Boxes.Add(PhysicsBoxObstacle.FromSnapshot(box.Body));
            var enemies = new List<MutinyAIEffectCharacter>();
            foreach (var c in input.Characters) if (c.Alive && c.Team != input.OwnTeam) enemies.Add(c);
            enemies.Sort((a, b) => Mathf.Abs(a.Body.X - owner.X).CompareTo(Mathf.Abs(b.Body.X - owner.X)));
            Result.EnemyPositions = new Vector2[enemies.Count];
            for (int i = 0; i < enemies.Count; i++) Result.EnemyPositions[i] = new Vector2(enemies[i].Body.X, enemies[i].Body.Y);
            Vector2 origin = new Vector2(owner.X, owner.Y + (reaim ? 5f : -10f));
            // Directed low/high arcs go first even at Luck=0. Random coverage
            // then searches alternative directions around terrain and boxes.
            foreach (var c in enemies)
            {
                var target = new Vector2(c.Body.X, c.Body.Y);
                m_Guided.Add(MutinyAIEffectWorld.AimedVelocity(origin, target, 20f, weight: owner.EffectiveGravityScale));
                m_Guided.Add(MutinyAIEffectWorld.AimedVelocity(origin, target, 20f, 1.5f, owner.EffectiveGravityScale));
            }
            Result.Velocity = m_Guided.Count > 0 ? m_Guided[0] : new Vector2(0f, -10f);
            if (enemies.Count == 0) { m_Complete = true; Result.Status = "no-target-analytic"; }
        }

        internal bool Advance()
        {
            if (m_Complete) return false;
            if (!m_Budget.Take())
            {
                Result.Status = Result.Fallback ? "budget-analytic-fallback" : "budget-best-settled";
                m_Complete = true; return false;
            }
            Result.WorkSteps++;
            if (m_SampleIndex < m_Samples)
            {
                if (!m_Flying)
                {
                    m_CurrentVelocity = m_SampleIndex < m_Guided.Count ? m_Guided[m_SampleIndex] : RandomArc();
                    m_Body = MutinyAIEffectWorld.ProjectileBody("piecesofeight", m_Input.Characters[m_Actor].Body);
                    if (m_Reaim) m_Body.Y = m_Input.Characters[m_Actor].Body.Y + 5f;
                    m_Body.VelocityX = m_CurrentVelocity.x; m_Body.VelocityY = m_CurrentVelocity.y;
                    m_Flying = true; m_FlightTicks = 0;
                }
                StepResult hit = MutinyPhysics.Step(ref m_Body, m_Input.Terrain, m_Input.Width, m_Input.Height, m_Boxes);
                bool contact = hit.HitFloor || hit.HitCeiling || hit.HitLeftWall || hit.HitRightWall;
                if (contact || m_Body.Y >= m_Input.WaterY || ++m_FlightTicks >= 101)
                {
                    var impact = new Vector2(m_Body.X, m_Body.Y);
                    Keep(new Candidate { Velocity = m_CurrentVelocity, Impact = impact,
                        Score = contact ? CoarseScore(impact) : -0.01f });
                    m_Flying = false; m_SampleIndex++; Result.Samples++;
                }
                return true;
            }
            if (m_Refinement >= m_Best.Count) { m_Complete = true; return false; }
            if (m_Trial == null)
            {
                var command = new MutinyAIEffectCommand { Type = AIMoveType.ShootWeapon, Actor = m_Actor,
                    Weapon = "piecesofeight", Velocity = m_Best[m_Refinement].Velocity, CoinLimit = 1, CoinReaim = m_Reaim };
                m_Trial = new MutinyAIEffectWorld(m_Input, command, m_Seed);
                Result.Simulations++;
            }
            if (m_Trial.Advance()) return true;
            var outcome = m_Trial.Outcome;
            MutinyAIEffectScore.Evaluate(outcome, false,
                outcome.EnemyHpBefore > 0f && outcome.EnemyHpAfter <= 0f && outcome.AllyHpAfter > 0f,
                outcome.AllyHpBefore > 0f && outcome.AllyHpAfter <= 0f);
            if (outcome.Settled && outcome.Score > m_BestScore)
            {
                m_BestScore = outcome.Score; Result.Score = outcome.Score; Result.Velocity = m_Best[m_Refinement].Velocity;
                Result.Fallback = false; Result.Status = "settled-single-coin";
                Result.AllyHpBefore = outcome.AllyHpBefore; Result.AllyHpAfter = outcome.AllyHpAfter;
                Result.EnemyHpBefore = outcome.EnemyHpBefore; Result.EnemyHpAfter = outcome.EnemyHpAfter;
            }
            m_Trial = null; m_Refinement++;
            return true;
        }

        private Vector2 RandomArc()
        {
            float angle = (180f + m_Random.Value() * 180f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (5f + m_Random.Value() * 15f);
        }

        private float CoarseScore(Vector2 impact)
        {
            float score = 0f;
            foreach (var c in m_Input.Characters)
            {
                if (!c.Alive) continue;
                if (!MutinyCombatMath.ExplosionHit(new Vector2(c.Body.X, c.Body.Y), impact,
                    MutinyPiecesOfEight.ExplosionSize * 0.5f + 20f, MutinyPiecesOfEight.ExplosionDamage,
                    out float ratio, out _)) continue;
                float damage = Mathf.Min(c.Health, Mathf.Round(MutinyPiecesOfEight.ExplosionDamage * ratio));
                score += c.Team == m_Input.OwnTeam ? -1.15f * damage : damage;
            }
            return score;
        }

        private void Keep(Candidate candidate)
        {
            // Keep a small, spatially/ballistically diverse shortlist, not 128
            // almost-identical expensive simulations of one landing point.
            for (int i = m_Best.Count - 1; i >= 0; i--)
                if (Vector2.Distance(m_Best[i].Impact, candidate.Impact) < 12f &&
                    Vector2.Distance(m_Best[i].Velocity, candidate.Velocity) < 4f)
                {
                    if (m_Best[i].Score >= candidate.Score) return;
                    m_Best.RemoveAt(i);
                }
            int insert = 0; while (insert < m_Best.Count && m_Best[insert].Score >= candidate.Score) insert++;
            m_Best.Insert(insert, candidate);
            if (m_Best.Count > MaxRefinements) m_Best.RemoveAt(MaxRefinements);
        }

        // This is the only live-scene adapter. Searches/worlds below it are
        // value-only and never mutate an object, inventory or Unity RNG.
        internal static MutinyAIEffectInput CaptureLive(MutinyCharacter owner, MutinyTeam team, out int actor)
        {
            var characters = new List<MutinyAIEffectCharacter>();
            var seen = new HashSet<MutinyCharacter>();
            void Add(MutinyCharacter c)
            {
                if (c == null || c.PhysicsBody == null || !seen.Add(c)) return;
                characters.Add(new MutinyAIEffectCharacter { Body = c.PhysicsBody.State,
                    Health = c.Health, Alive = c.IsAlive, Team = c.TeamIndex });
            }
            Add(owner); actor = 0;
            foreach (var t in UnityEngine.Object.FindObjectsByType<MutinyTeam>()) foreach (var c in t.Characters) Add(c);
            var boxes = new List<MutinyAIEffectBox>();
            foreach (var box in MutinyBoxRegistry.GetObstacles())
                boxes.Add(new MutinyAIEffectBox { Body = box.State, Barrel = box.Body != null && box.Body.GetComponent<MutinyGunpowderBarrel>() != null });
            var mines = new List<MutinyAIEffectMine>();
            foreach (var mine in UnityEngine.Object.FindObjectsByType<MutinyMine>())
                if (mine.IsFired && !mine.IsFinished && mine.PhysicsBody != null)
                    mines.Add(new MutinyAIEffectMine { Body = mine.PhysicsBody.State, Ignore = mine.IgnoreTicksRemaining,
                        Countdown = mine.CountdownRemaining, Stored = mine.IsStored, Active = mine.IsActive });
            owner.PhysicsBody.TryGetTerrain(out var terrain, out int width, out int height);
            return new MutinyAIEffectInput(terrain, width, height, owner.PhysicsBody.WaterPixelY, team.TeamNumber,
                characters.ToArray(), boxes.ToArray(), mines.ToArray());
        }
    }
}
