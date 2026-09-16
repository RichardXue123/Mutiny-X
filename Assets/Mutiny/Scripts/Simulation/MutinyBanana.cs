using Mutiny.Diagnostics;
using Mutiny.Presentation;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyBanana : MutinyWeapon
    {
        public const float ExtentPixels = 7f;
        public const float Bounce = 0.8f;
        public const float Friction = 0.5f;
        public const float AiImmediateDetonationDistanceSquared = 400f;
        public const float AiRecedingDetonationLimitSquared = 2500f;

        // Banana.as initializes this to Infinity and never assigns it in this SWF.
        // Keep the comparison in the production flow rather than inventing a
        // distance-history behaviour the original did not execute.
        private float m_LastNearestDistanceSquared = float.PositiveInfinity;
        private bool m_PlayerDetonationRequested;

        protected override void Awake()
        {
            WeaponType = "banana";
            Extent = ExtentPixels;
            TwangMaxForce = 30f;
            base.Awake();
            Sprite sprite = Resources.Load<Sprite>("Art/Weapons/Banana/1");
            if (sprite != null && SpriteRenderer != null)
                SpriteRenderer.sprite = sprite;
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            PhysicsBody.State.Bounce = Bounce;
            PhysicsBody.State.Friction = Friction;
            m_LastNearestDistanceSquared = float.PositiveInfinity;
            m_PlayerDetonationRequested = false;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalTick;
        }

        public override void Twang(Vector2 startPx, Vector2 dragPx)
        {
            // Banana exposes a 30-force pull gauge, then Weapon.release clamps the
            // committed throw to the shared 20 px/tick cap.
            Vector2 launchVelocity = MutinyPhysics.CalculateTwangVelocity(startPx, dragPx, TwangMaxForce);
            if (launchVelocity.sqrMagnitude > MutinyPhysics.DefaultTwangMaxForce * MutinyPhysics.DefaultTwangMaxForce)
                launchVelocity = launchVelocity.normalized * MutinyPhysics.DefaultTwangMaxForce;
            Fire(launchVelocity);
        }

        public static bool TryRequestPlayerDetonation(MutinyTeam inputTeam)
        {
            MutinyBanana banana = FindPlayerDetonatableBanana(inputTeam);
            if (banana == null)
                return false;

            banana.m_PlayerDetonationRequested = true;
            MutinyDebugLog.Info("Banana", $"player detonation requested owner={banana.Owner.name}", banana);
            return true;
        }

        /// <summary>
        /// Tests whether a human team's already-thrown banana owns the next global
        /// tile-system click. This query is intentionally side-effect free so the
        /// input phase gate can admit the click before it requests detonation.
        /// </summary>
        public static bool HasPlayerDetonatableBanana(MutinyTeam inputTeam)
        {
            return FindPlayerDetonatableBanana(inputTeam) != null;
        }

        public bool RequestDetonationForVerification()
        {
            if (!IsFired || IsFinished || IsAiOwner())
                return false;

            m_PlayerDetonationRequested = true;
            return true;
        }

        public void AdvanceOriginalTickForVerification()
        {
            AdvanceOriginalTick();
        }

        protected override void OnContact(CollisionSide side)
        {
            base.OnContact(side);
            if (IsFired && !IsFinished)
            {
                // Banana.contact plays this for floor, wall and ceiling collisions.
                MutinyAudioManager.Instance?.PlaySFX("banana_bounce");
                MutinyDebugLog.Info("Banana", $"bounce side={side}", this);
            }
        }

        public void Explode()
        {
            if (IsFinished)
                return;

            Finish();
            if (SpriteRenderer != null)
                SpriteRenderer.enabled = false;

            Vector2 position = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            // Banana.advanceMotion creates the explosion and plays pop in the same
            // tick; Explosion.hit must not play a second pop two animation frames on.
            MutinyExplosion.Spawn(position, 160f, 80f, Owner, playPopOnHit: false);
            MutinyAudioManager.Instance?.PlaySFX("pop");
            MutinyDebugLog.Info("Banana", $"exploded pos=({position.x:F1},{position.y:F1})", this);
            Destroy(gameObject, 0.1f);
        }

        private void AdvanceOriginalTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            PhysicsBodyState state = PhysicsBody.State;
            bool shouldExplode = state.VelocityX == 0f && Mathf.Abs(state.VelocityY) < 0.5f;
            string reason = shouldExplode ? "at-rest" : null;

            if (!shouldExplode)
            {
                if (IsAiOwner())
                {
                    float nearestDistanceSquared = FindOriginalNearestCharacterDistanceSquared(state);
                    if ((nearestDistanceSquared > m_LastNearestDistanceSquared &&
                         nearestDistanceSquared < AiRecedingDetonationLimitSquared) ||
                        nearestDistanceSquared < AiImmediateDetonationDistanceSquared)
                    {
                        shouldExplode = true;
                        reason = nearestDistanceSquared < AiImmediateDetonationDistanceSquared
                            ? "ai-near-character"
                            : "ai-receding";
                    }
                }
                else if (m_PlayerDetonationRequested)
                {
                    shouldExplode = true;
                    reason = "player-click";
                }
            }

            m_PlayerDetonationRequested = false;
            if (shouldExplode)
            {
                MutinyDebugLog.Info("Banana", $"detonation condition={reason}", this);
                Explode();
            }
        }

        private bool IsAiOwner()
        {
            if (Owner == null)
                return false;

            MutinyTeam[] teams = Object.FindObjectsByType<MutinyTeam>();
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null && teams[i].Characters.Contains(Owner))
                    return teams[i].IsAiControlled;
            }
            return false;
        }

        private static float FindOriginalNearestCharacterDistanceSquared(PhysicsBodyState bananaState)
        {
            float nearest = float.PositiveInfinity;
            MutinyCharacter[] characters = Object.FindObjectsByType<MutinyCharacter>();
            for (int i = 0; i < characters.Length; i++)
            {
                MutinyCharacter character = characters[i];
                if (character == null || character.PhysicsBody == null)
                    continue;

                PhysicsBodyState state = character.PhysicsBody.State;
                float dx = state.X - bananaState.X;
                float dy = state.Y - bananaState.Y;
                float distanceSquared = dx * dx + dy * dy;
                if (distanceSquared < nearest)
                    nearest = distanceSquared;
            }
            return nearest;
        }

        private static MutinyBanana FindPlayerDetonatableBanana(MutinyTeam inputTeam)
        {
            if (inputTeam == null || inputTeam.IsAiControlled)
                return null;

            MutinyBanana[] bananas = Object.FindObjectsByType<MutinyBanana>();
            for (int i = 0; i < bananas.Length; i++)
            {
                MutinyBanana banana = bananas[i];
                if (banana != null && banana.IsFired && !banana.IsFinished && banana.Owner != null &&
                    inputTeam.Characters.Contains(banana.Owner))
                    return banana;
            }
            return null;
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
                PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
        }
    }
}
