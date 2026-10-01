using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mutiny.Diagnostics;
using Mutiny.Levels;
using Mutiny.Presentation;
using UnityEngine;

namespace Mutiny.Simulation
{
    /// <summary>
    /// Flash PiecesOfEight: one equipped object fires eight consecutive coins in the
    /// same turn. The object returns to its owner after each of the first seven.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MutinyPiecesOfEight : MutinyWeapon
    {
        public const int TotalCoins = 8;
        public const float OriginalExtentPixels = 7f;
        public const float ExplosionSize = 50f;
        public const float ExplosionDamage = 25f;
        public const int AiReaimDelayTicks = 20;
        [Min(0.1f)] public float AiDecisionBudgetMilliseconds = MutinyAIController.DefaultDecisionBudgetMilliseconds;

        private bool m_OverWater = true;
        private int m_AiWaitTicks;
        private float m_AiTickAccumulator;
        private MutinyAICoinPlanner m_EnhancedSearch;
        private MutinyAICoinBudget m_EnhancedBudget;
        internal readonly List<MutinyAICoinEvaluation> EnhancedCoinSearchesForVerification = new List<MutinyAICoinEvaluation>();
        internal int EnhancedSearchSlicesForVerification { get; private set; }

        public int TimesFired { get; private set; }
        public int ShotsRemaining => Mathf.Max(0, TotalCoins - TimesFired);
        public bool IsAwaitingNextCoin => !IsFinished && !IsFired && TimesFired > 0 && TimesFired < TotalCoins;
        public bool CanFireNextCoin => !IsFinished && !IsFired && TimesFired < TotalCoins;

        public static bool HasPlayerAwaitingNextCoin(MutinyTeam team)
        {
            if (team == null || team.IsAiControlled || team.SelectedCharacter == null)
                return false;

            MutinyPiecesOfEight[] sequences = FindObjectsByType<MutinyPiecesOfEight>();
            for (int i = 0; i < sequences.Length; i++)
            {
                MutinyPiecesOfEight sequence = sequences[i];
                if (sequence != null && sequence.IsAwaitingNextCoin &&
                    sequence.Owner == team.SelectedCharacter)
                    return true;
            }
            return false;
        }

        public static readonly Vector2 OriginalPivot = new Vector2(7f / 15f, 8f / 15f); // Symbol 884: origin (7, 7) of 15x15

        protected override void Awake()
        {
            WeaponType = "piecesOfEight";
            Extent = OriginalExtentPixels;
            base.Awake();
            Texture2D texture = Resources.Load<Texture2D>("Art/Weapons/PiecesOfEight/1");
            if (texture != null && SpriteRenderer != null)
            {
                texture.filterMode = FilterMode.Point;
                SpriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                    OriginalPivot, MutinyPhysics.PixelsPerUnit);
            }
            else
            {
                Sprite sprite = Resources.Load<Sprite>("Art/Weapons/PiecesOfEight/1");
                if (sprite != null && SpriteRenderer != null)
                    SpriteRenderer.sprite = sprite;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            PhysicsBody.State.LeftExtent = OriginalExtentPixels;
            PhysicsBody.State.RightExtent = OriginalExtentPixels;
            PhysicsBody.State.TopExtent = OriginalExtentPixels;
            PhysicsBody.State.BottomExtent = OriginalExtentPixels;
            PhysicsBody.State.HitsBoxes = true;
            // Weapon.advance only calls splashCheck. Coin water entry must not use
            // Character-style underwater drag.
            PhysicsBody.ApplyWaterPhysics = false;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalPostMotionTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalPostMotionTick;
            TimesFired = 0;
            m_OverWater = true;
            m_AiWaitTicks = 0;
            m_AiTickAccumulator = 0f;
            m_EnhancedSearch = null;
            m_EnhancedBudget = new MutinyAICoinBudget();
            EnhancedCoinSearchesForVerification.Clear();
            EnhancedSearchSlicesForVerification = 0;
            PhysicsBody.OnBeforeSimulationStep -= PrepareUnfiredOwnerHold;
            PhysicsBody.OnBeforeSimulationStep += PrepareUnfiredOwnerHold;
            PhysicsBody.PresentationPositionOverride = SampleReadyPresentationPosition;
        }

        public override void PrepareForEquip()
        {
            base.PrepareForEquip();
        }

        public override void Fire(Vector2 velocityPx)
        {
            if (!CanFireNextCoin)
            {
                MutinyDebugLog.Warning("PiecesOfEight",
                    $"fire rejected fired={IsFired} finished={IsFinished} times={TimesFired}", this);
                return;
            }

            base.Fire(velocityPx);
            if (!IsFired)
                return;

            m_OverWater = true;
            m_AiWaitTicks = 0;
            m_EnhancedSearch = null;
            PhysicsBody.IsActive = true;
            // Weapon.fire sets track=true in the original. Register the exact
            // reusable coin instance rather than asking the camera to infer it
            // from an unordered scene-wide weapon scan.
            MutinyCameraController.RequestTrackWeapon(this);
            MutinyDebugLog.Info("PiecesOfEight",
                $"coin fired index={TimesFired + 1}/{TotalCoins} velocity=({PhysicsBody.State.VelocityX:F2},{PhysicsBody.State.VelocityY:F2})", this);
        }

        protected override void OnContact(CollisionSide side)
        {
            // PiecesOfEight.contact calls next(true) for every Solid contact,
            // including a box; it does not defer to generic weapon completion.
            if (IsFired && !IsFinished)
                ResolveCoin(explode: true, $"contact:{side}");
        }

        protected override void Update()
        {
            if (IsFinished)
                return;

            if (!IsFired)
            {
                if (Owner == null || !Owner.IsAlive)
                {
                    m_EnhancedSearch = null;
                    Finish();
                    PhysicsBody.IsActive = false;
                    if (SpriteRenderer != null)
                        SpriteRenderer.enabled = false;
                    Destroy(gameObject, 0.1f);
                    return;
                }

                AdvanceAiWait(Time.deltaTime);
            }
        }

        // This invokes the same terminal state transition used by contact/water.
        // It is deliberately limited to simulation verification after a production
        // input launch; the game never calls it from the player path.
        public void ResolveCoinForVerification(bool explode)
        {
            ResolveCoin(explode, explode ? "verification-contact" : "verification-water");
        }

        private void AdvanceOriginalPostMotionTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            // Solid.splashCheck toggles a one-shot splash; PiecesOfEight.advance then
            // calls next(false) as soon as the coin reaches the water surface.
            if (!float.IsInfinity(PhysicsBody.WaterPixelY) &&
                PhysicsBody.State.Y >= PhysicsBody.WaterPixelY)
            {
                if (m_OverWater)
                {
                    MutinyWaterSurface.SpawnSplash(PhysicsBody.State.X, PhysicsBody.WaterPixelY);
                    MutinyAudioManager.Instance?.PlaySFX("splash");
                }
                m_OverWater = false;
                ResolveCoin(explode: false, "water");
            }
        }

        private void ResolveCoin(bool explode, string reason)
        {
            if (!IsFired || IsFinished)
                return;

            Vector2 position = PhysicsBody != null
                ? new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y)
                : MutinyPhysics.UnityToPixel(transform.position);

            if (explode)
            {
                // PiecesOfEight.next(true) creates 50/25 then plays pop immediately;
                // Explosion.hit must not replay it two visual frames later.
                MutinyExplosion.Spawn(position, ExplosionSize, ExplosionDamage, Owner, playPopOnHit: false);
                MutinyAudioManager.Instance?.PlaySFX("pop");
            }

            TimesFired++;
            if (TimesFired < TotalCoins && Owner != null && Owner.IsAlive)
            {
                IsFired = false;
                PhysicsBody.IsActive = false;
                PhysicsBody.SetVelocity(0f, 0f);
                // PiecesOfEight.advance returns the coin to (owner.x,
                // owner.y + 5) at the start of the following simulation tick.
                // Preserve the impact position for the remainder of this tick.
                PhysicsBody.IsActive = true;
                Owner.WeaponLocked = true;
                // Original PiecesOfEight.next clears weapon tracking and assigns
                // TileSystem.panToCharacter to the owner after coins 1..7.
                MutinyCameraController.ReleaseWeaponTracking(this);
                MutinyCameraController.RequestPanToCharacter(Owner);
                if (TryGetOwnerTeam(out MutinyTeam team) && team.IsAiControlled)
                    m_AiWaitTicks = AiReaimDelayTicks;

                MutinyDebugLog.Info("PiecesOfEight",
                    $"coin resolved index={TimesFired}/{TotalCoins} reason={reason} awaitingNext=true aiWait={m_AiWaitTicks}", this);
                return;
            }

            Finish();
            MutinyCameraController.ReleaseWeaponTracking(this);
            if (PhysicsBody != null)
                PhysicsBody.IsActive = false;
            if (SpriteRenderer != null)
                SpriteRenderer.enabled = false;
            Destroy(gameObject, 0.1f);
            MutinyDebugLog.Info("PiecesOfEight",
                $"sequence finished index={TimesFired}/{TotalCoins} reason={reason}", this);
        }

        private void HoldAtOwner()
        {
            if (Owner == null || !Owner.IsAlive || PhysicsBody == null)
                return;

            float x = Owner.PhysicsBody != null ? Owner.PhysicsBody.State.X : MutinyPhysics.UnityToPixel(Owner.transform.position).x;
            float y = Owner.PhysicsBody != null ? Owner.PhysicsBody.State.Y + 5f : MutinyPhysics.UnityToPixel(Owner.transform.position).y + 5f;
            PhysicsBody.State.X = x;
            PhysicsBody.State.Y = y;
            PhysicsBody.State.VelocityX = 0f;
            PhysicsBody.State.VelocityY = 0f;
            transform.position = MutinyPhysics.PixelToUnity(x, y);
        }

        private void PrepareUnfiredOwnerHold()
        {
            // Original PiecesOfEight.advance performs this reset only when the
            // coin is not the current Controller.twanging object, then calls
            // Weapon.advance so gravity is still applied in the same tick.
            if (!IsFired && !IsFinished && !IsBeingAimed)
                HoldAtOwner();
        }

        private Vector3? SampleReadyPresentationPosition()
        {
            if (IsFired || IsFinished || IsBeingAimed || Owner == null || Owner.PhysicsBody == null)
                return null;

            // Flash presents only the pose after its owner reset and gravity
            // step. Interpolating that repeated +5 -> +6 step makes the coin
            // fall one pixel and jump back every 25 Hz tick. Keep its settled
            // authoritative pose, but inherit the owner's smooth display delta.
            Vector3 coinPosition = MutinyPhysics.PixelToUnity(PhysicsBody.State.X, PhysicsBody.State.Y);
            PhysicsBodyState ownerState = Owner.PhysicsBody.State;
            Vector3 ownerAuthoritative = MutinyPhysics.PixelToUnity(ownerState.X, ownerState.Y);
            return coinPosition + Owner.PhysicsBody.PresentationPosition - ownerAuthoritative;
        }

        internal void AdvanceAiWaitForVerification(float deltaTime) => AdvanceAiWait(deltaTime);

        private void AdvanceAiWait(float deltaTime)
        {
            if (!IsAwaitingNextCoin || Owner == null || !Owner.IsAlive)
                return;

            if (m_EnhancedSearch != null)
            {
                PumpEnhancedSearch();
                return;
            }
            if (m_AiWaitTicks <= 0) return;

            m_AiTickAccumulator += deltaTime;
            while (m_AiTickAccumulator >= MutinyPhysics.TimeStep && m_AiWaitTicks > 0)
            {
                m_AiTickAccumulator -= MutinyPhysics.TimeStep;
                m_AiWaitTicks--;
            }

            if (m_AiWaitTicks == 0)
                FireAiBestCandidate();
        }

        private void FireAiBestCandidate()
        {
            if (!TryGetOwnerTeam(out MutinyTeam ownTeam) || !ownTeam.IsAiControlled || Owner == null || !Owner.IsAlive)
                return;

            if (AiActionPlan != null && AiActionPlan.GreedyCoinSamples > 0)
            {
                var input = MutinyAICoinPlanner.CaptureLive(Owner, ownTeam, out int actor);
                m_EnhancedSearch = new MutinyAICoinPlanner(input, actor, AiActionPlan.GreedyCoinSamples,
                    AiActionPlan.SimulationSeed, TimesFired, true, m_EnhancedBudget);
                PumpEnhancedSearch();
                return;
            }

            if (AiActionPlan != null && TimesFired < AiActionPlan.CoinCount)
            {
                // Compatibility for explicit fixed plans. Production enhanced
                // decisions use the greedy policy above, not this array replay.
                HoldAtOwner();
                Vector2 velocity = AiActionPlan.CoinVelocity(TimesFired);
                MutinyDebugLog.Info("PiecesOfEight", $"AI planned continuation index={TimesFired + 1} velocity={velocity}", this);
                Fire(velocity);
                return;
            }

            MutinyTeam enemyTeam = FindOpponent(ownTeam);
            Vector2 bestVelocity = Vector2.zero;
            float bestScore = float.NegativeInfinity;
            bool found = false;
            for (int i = 0; i < 10; i++)
            {
                int degrees = 180 + UnityEngine.Random.Range(0, 180);
                float force = UnityEngine.Random.Range(5f, TwangMaxForce);
                float radians = degrees * Mathf.Deg2Rad;
                Vector2 velocity = new Vector2(Mathf.Cos(radians) * force, Mathf.Sin(radians) * force);
                Vector2 landing = SimulateLanding(velocity);
                float score = ScoreAiLanding(landing, ownTeam, enemyTeam);
                if (!found || score > bestScore)
                {
                    found = true;
                    bestScore = score;
                    bestVelocity = velocity;
                }
            }

            if (found)
            {
                MutinyDebugLog.Info("PiecesOfEight",
                    $"AI re-aimed after {AiReaimDelayTicks} ticks index={TimesFired + 1}/{TotalCoins} score={bestScore:F3} velocity={bestVelocity}", this);
                Fire(bestVelocity);
            }
        }

        private void PumpEnhancedSearch()
        {
            if (m_EnhancedSearch == null) return;
            EnhancedSearchSlicesForVerification++;
            long started = Stopwatch.GetTimestamp();
            float budget = float.IsNaN(AiDecisionBudgetMilliseconds) || float.IsInfinity(AiDecisionBudgetMilliseconds)
                ? MutinyAIController.DefaultDecisionBudgetMilliseconds : Mathf.Max(0.1f, AiDecisionBudgetMilliseconds);
            do
            {
                if (!m_EnhancedSearch.Advance())
                {
                    var result = m_EnhancedSearch.Result;
                    EnhancedCoinSearchesForVerification.Add(result);
                    m_EnhancedSearch = null;
                    if (!IsAwaitingNextCoin || Owner == null || !Owner.IsAlive) return;
                    HoldAtOwner();
                    if (MutinyAIController.ActionLogEnabled)
                        UnityEngine.Debug.Log($"[Mutiny:AI-Action] action=CoinContinuation mode={AiStrategyContext.ModeId} " +
                            $"strategy={AiStrategyContext.StrategyId} algorithm={AiStrategyContext.AlgorithmId} " +
                            $"fallback={AiStrategyContext.UsesFallback} strategyVersion={AiStrategyContext.ConfigurationVersion} weapon=piecesOfEight " +
                            $"coin={result.Index}/{TotalCoins} velocity=({result.Velocity.x:F3},{result.Velocity.y:F3}) " +
                            $"score={(result.Fallback ? "NA" : result.Score.ToString("F3", System.Globalization.CultureInfo.InvariantCulture))} " +
                            $"scoreScope=single-coin allyHp={result.AllyHpBefore:F0}->{result.AllyHpAfter:F0} " +
                            $"enemyHp={result.EnemyHpBefore:F0}->{result.EnemyHpAfter:F0} samples={result.Samples} " +
                            $"trials={result.Simulations} workSteps={result.WorkSteps} status={result.Status} " +
                            $"policySamples={AiActionPlan.GreedyCoinSamples} seed={AiActionPlan.SimulationSeed}", this);
                    Fire(result.Velocity);
                    return;
                }
            }
            while ((Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency <
                budget);
        }

        private Vector2 SimulateLanding(Vector2 velocity)
        {
            PhysicsBodyState state = PhysicsBodyState.CreateDefault(
                Owner.PhysicsBody != null ? Owner.PhysicsBody.State.X : PhysicsBody.State.X,
                Owner.PhysicsBody != null ? Owner.PhysicsBody.State.Y + 5f : PhysicsBody.State.Y);
            state.LeftExtent = state.RightExtent = state.TopExtent = state.BottomExtent = OriginalExtentPixels;
            state.HitsBoxes = true;
            state.VelocityX = velocity.x;
            state.VelocityY = velocity.y;
            PhysicsBody.TryGetTerrain(out string[,] terrain, out int width, out int height);
            state.GravityScale = PhysicsBody.State.EffectiveGravityScale;
            float waterY = PhysicsBody.WaterPixelY;
            for (int tick = 0; tick < 101; tick++)
            {
                StepResult step = MutinyPhysics.Step(ref state, terrain, width, height);
                if (step.HitFloor || step.HitCeiling || step.HitLeftWall || step.HitRightWall || state.Y >= waterY)
                    break;
            }
            return new Vector2(state.X, state.Y);
        }

        private static float ScoreAiLanding(Vector2 landing, MutinyTeam ownTeam, MutinyTeam enemyTeam)
        {
            float score = 0f;
            if (enemyTeam != null)
            {
                for (int i = 0; i < enemyTeam.Characters.Count; i++)
                    score += ScoreOne(enemyTeam.Characters[i], landing, 1f);
            }
            if (ownTeam != null)
            {
                for (int i = 0; i < ownTeam.Characters.Count; i++)
                    score += ScoreOne(ownTeam.Characters[i], landing, -1f, 1.5f);
            }
            return score;
        }

        private static float ScoreOne(MutinyCharacter character, Vector2 landing, float sign, float baseScore = 1f)
        {
            if (character == null || !character.IsAlive || character.PhysicsBody == null)
                return 0f;
            float distance = Vector2.Distance(landing, new Vector2(character.PhysicsBody.State.X, character.PhysicsBody.State.Y));
            return distance < 70f ? sign * (baseScore - distance / 70f) : 0f;
        }

        private bool TryGetOwnerTeam(out MutinyTeam team)
        {
            MutinyTeam[] teams = FindObjectsByType<MutinyTeam>();
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null && teams[i].Characters.Contains(Owner))
                {
                    team = teams[i];
                    return true;
                }
            }
            team = null;
            return false;
        }

        private static MutinyTeam FindOpponent(MutinyTeam ownTeam)
        {
            MutinyTeam[] teams = FindObjectsByType<MutinyTeam>();
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null && teams[i] != ownTeam && teams[i].TeamNumber != ownTeam.TeamNumber)
                    return teams[i];
            }
            return null;
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
            {
                PhysicsBody.OnSimulationStep -= AdvanceOriginalPostMotionTick;
                PhysicsBody.OnBeforeSimulationStep -= PrepareUnfiredOwnerHold;
                PhysicsBody.PresentationPositionOverride = null;
            }
        }
    }
}
