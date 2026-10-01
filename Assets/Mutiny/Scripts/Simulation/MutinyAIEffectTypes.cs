using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    // Committed controls are immutable and travel with the weapon, not the GM
    // flag. Accessors never expose the planner's mutable buffers.
    public sealed class MutinyAIActionPlan
    {
        private readonly Vector2[] m_Coins, m_Boxes;
        public int FanDirection { get; }
        public int SimulationSeed { get; }
        public int CoinCount => m_Coins.Length;
        // Predicted velocities are diagnostics; enhanced continuations latch a
        // policy and replan against the live board instead of replaying them.
        public int GreedyCoinSamples { get; }
        public int BoxCount => m_Boxes.Length;
        public Vector2 CoinVelocity(int index) => m_Coins[index];
        public Vector2[] CopyBoxPositions() => (Vector2[])m_Boxes.Clone();
        internal MutinyAIActionPlan(int fan, Vector2[] coins = null, Vector2[] boxes = null, int simulationSeed = 0,
            int greedyCoinSamples = 0)
        {
            FanDirection = Mathf.Clamp(fan, -1, 1);
            SimulationSeed = simulationSeed;
            GreedyCoinSamples = greedyCoinSamples;
            m_Coins = coins != null ? (Vector2[])coins.Clone() : Array.Empty<Vector2>();
            m_Boxes = boxes != null ? (Vector2[])boxes.Clone() : Array.Empty<Vector2>();
        }
    }

    internal struct MutinyAIEffectCharacter
    { public PhysicsBodyState Body; public float Health; public int Team; public bool Alive; }
    internal struct MutinyAIEffectBox
    { public PhysicsBodyState Body; public bool Barrel, Removed; }
    internal struct MutinyAIEffectMine
    { public PhysicsBodyState Body; public int Ignore, Countdown; public bool Stored, Active, Removed; }

    // No GameObjects, components, delegates into the live scene, or Unity RNG.
    internal sealed class MutinyAIEffectInput
    {
        public readonly string[,] Terrain;
        public readonly int Width, Height, OwnTeam;
        public readonly float WaterY;
        public readonly MutinyAIEffectCharacter[] Characters;
        public readonly MutinyAIEffectBox[] Boxes;
        public readonly MutinyAIEffectMine[] Mines;
        public readonly Vector2[] Chests;
        public MutinyAIEffectInput(string[,] terrain, int width, int height, float waterY, int ownTeam,
            MutinyAIEffectCharacter[] characters, MutinyAIEffectBox[] boxes,
            MutinyAIEffectMine[] mines = null, Vector2[] chests = null)
        {
            Terrain = terrain != null ? (string[,])terrain.Clone() : null;
            Width = width; Height = height; WaterY = waterY; OwnTeam = ownTeam;
            Characters = (MutinyAIEffectCharacter[])characters.Clone();
            Boxes = (MutinyAIEffectBox[])boxes.Clone();
            Mines = mines != null ? (MutinyAIEffectMine[])mines.Clone() : Array.Empty<MutinyAIEffectMine>();
            Chests = chests != null ? (Vector2[])chests.Clone() : Array.Empty<Vector2>();
        }
    }

    internal sealed class MutinyAIEffectCommand
    {
        public AIMoveType Type;
        public int Actor, TargetActor = -1, FanDirection;
        public string Weapon;
        public Vector2 Velocity, Target;
        public float FlightY;
        public float[] ShotXs;
        public Vector2[] BoxPossibilities;
        public int GreedyCoinSamples;
        // Single-coin refinement must never recursively start another eight-shot search.
        public int CoinLimit = MutinyPiecesOfEight.TotalCoins;
        public bool CoinReaim;
    }

    [Serializable]
    public sealed class MutinyAICoinEvaluation
    {
        public int Index, Samples, Simulations, WorkSteps;
        public bool Fallback;
        public Vector2 Velocity, ActorPosition;
        public Vector2[] EnemyPositions;
        public float Score, AllyHpBefore, AllyHpAfter, EnemyHpBefore, EnemyHpAfter;
        public string Status;
    }

    [Serializable]
    public sealed class MutinyAIEffectEvaluation
    {
        public string Character, TargetCharacter, Weapon, Action, Status;
        public int Ticks, FanDirection, SimulationSeed, TargetActor, CoinsFired, BoxesPlaced, Explosions, FlameSegments;
        public float AllyHpBefore, AllyHpAfter, EnemyHpBefore, EnemyHpAfter;
        public int AlliesLost, EnemiesLost;
        public float DamageScore, KillScore, TerminalScore, ResourceScore, PositionScore, FollowUpScore, Score;
        public Vector2 Velocity, Target, FinalActorPosition;
        public float FlightY;
        public float[] ShotXs;
        public Vector2[] CoinVelocities, BoxPositions;
        public List<MutinyAICoinEvaluation> CoinSearches;
        public bool Settled;
    }

    [Serializable]
    public sealed class MutinyAIEnhancedDecision
    {
        public const string Algorithm = "effects-v1";
        public string AlgorithmId = Algorithm;
        public string Limitations = "fixed-tick-order; rum-kick-deterministic-sample; voodoo-camera-wait-approx; no-next-enemy-turn";
        public int Seed, CoarseCandidates, JumpSamples, JumpFollowUps, Simulations, Truncated, BudgetSkipped;
        public int CoinSamples, CoinSimulations, CoinWorkSteps, CoinFallbacks;
        public float WinnerScore;
        public List<MutinyAIEffectEvaluation> Evaluations = new List<MutinyAIEffectEvaluation>();
    }

    internal static class MutinyAIEffectScore
    {
        internal static void Evaluate(MutinyAIEffectEvaluation outcome, bool finiteWeapon, bool ownWon, bool ownLost,
            float positionScore = 0f)
        {
            outcome.DamageScore = (outcome.EnemyHpBefore - outcome.EnemyHpAfter) -
                1.15f * (outcome.AllyHpBefore - outcome.AllyHpAfter);
            outcome.KillScore = 10f * outcome.EnemiesLost - 15f * outcome.AlliesLost;
            outcome.TerminalScore = (ownWon ? 500f : 0f) - (ownLost ? 500f : 0f);
            outcome.ResourceScore = finiteWeapon ? -1f : 0f;
            outcome.PositionScore = positionScore;
            outcome.Score = outcome.Settled ? outcome.DamageScore + outcome.KillScore + outcome.TerminalScore +
                outcome.ResourceScore + positionScore : 0f;
        }
    }

    // Independent deterministic stream for model-only uncertainty. Never touches
    // UnityEngine.Random.state, including when worlds interleave across frames.
    internal sealed class MutinyAIEffectRandom
    {
        private uint m_State;
        internal MutinyAIEffectRandom(int seed) => m_State = (uint)seed == 0 ? 1u : (uint)seed;
        internal float Value()
        {
            m_State ^= m_State << 13; m_State ^= m_State >> 17; m_State ^= m_State << 5;
            return (m_State >> 8) * (1f / 16777216f);
        }
    }
}
