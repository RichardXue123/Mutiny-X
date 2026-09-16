using System;
using Mutiny.Diagnostics;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class MutinyWeapon : MonoBehaviour
    {
        // Dynamic Character-layer clips are created after all XML characters.
        // Keep them above the largest initial character holder and below water.
        public const int WeaponSortingOrder = 180;

        [Header("Weapon Identity")]
        public string WeaponType = "weapon";
        public MutinyCharacter Owner;

        [Header("State")]
        public bool IsFired = false;
        public bool IsFinished = false;
        public bool IsTwangable = true;
        public bool IsDraggable = false;
        public float TwangMaxForce = MutinyPhysics.DefaultTwangMaxForce;

        [Header("Physics Extents (px)")]
        public float Extent = 9f; // default 9 for cherry bomb

        public MutinyPhysicsBody PhysicsBody { get; protected set; }
        public SpriteRenderer SpriteRenderer { get; protected set; }

        public event Action OnFired;
        public event Action OnFinished;

        protected virtual void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer.sortingOrder = WeaponSortingOrder;

            PhysicsBody = GetComponent<MutinyPhysicsBody>();
            if (PhysicsBody == null)
            {
                PhysicsBody = gameObject.AddComponent<MutinyPhysicsBody>();
            }
        }

        protected float m_LifetimeTimer = 0f;
        protected float m_WaterTimer = 0f;
        protected bool m_HasSpawnedWaterSplash = false;

        public virtual void Initialize(MutinyCharacter owner)
        {
            Owner = owner;
            IsFired = false;
            IsFinished = false;
            m_LifetimeTimer = 0f;
            m_WaterTimer = 0f;
            m_HasSpawnedWaterSplash = false;

            if (Owner != null)
            {
                transform.position = Owner.transform.position;
            }

            Vector2 px = MutinyPhysics.UnityToPixel(transform.position);
            PhysicsBody.State = PhysicsBodyState.CreateDefault(px.x, px.y);
            PhysicsBody.State.LeftExtent = Extent;
            PhysicsBody.State.RightExtent = Extent;
            PhysicsBody.State.TopExtent = Extent;
            PhysicsBody.State.BottomExtent = Extent;
            PhysicsBody.State.Bounce = 0.2f;
            PhysicsBody.State.Friction = 0.3f;
            PhysicsBody.State.HitsTiles = true;

            if (Owner != null && Owner.PhysicsBody != null && !float.IsInfinity(Owner.PhysicsBody.WaterPixelY))
            {
                PhysicsBody.WaterPixelY = Owner.PhysicsBody.WaterPixelY;
            }
            PhysicsBody.CacheLevelTerrain();

            PhysicsBody.OnFloorLanded += () => OnContact(CollisionSide.Floor);
            PhysicsBody.OnCeilingHit += () => OnContact(CollisionSide.Ceiling);
            PhysicsBody.OnWallHit += () => OnContact(CollisionSide.Wall);
            PhysicsBody.OnEnterWater += HandleEnterWater;
            PhysicsBody.OnBeforeSimulationStep -= AdvanceOriginalRotationTick;
            PhysicsBody.OnBeforeSimulationStep += AdvanceOriginalRotationTick;
            MutinyDebugLog.Info("Weapon",
                $"initialized type={WeaponType} owner={(Owner == null ? "none" : Owner.name)} waterY={PhysicsBody.WaterPixelY}", this);
        }

        // Character.equip in the Flash game creates the real weapon before the
        // player starts pulling it.  Keep that instance fixed at the original
        // equipment point until a weapon-specific commit method activates it.
        public virtual void PrepareForEquip()
        {
            if (Owner == null || PhysicsBody == null)
                return;

            float yOffset = string.Equals(WeaponType, "boulder", StringComparison.OrdinalIgnoreCase)
                ? -30f
                : -10f;
            Vector2 ownerPixels = Owner.PhysicsBody != null
                ? new Vector2(Owner.PhysicsBody.State.X, Owner.PhysicsBody.State.Y)
                : MutinyPhysics.UnityToPixel(Owner.transform.position);
            PhysicsBody.State.X = ownerPixels.x;
            PhysicsBody.State.Y = ownerPixels.y + yOffset;
            PhysicsBody.SetVelocity(0f, 0f);
            PhysicsBody.IsActive = false;
            transform.position = MutinyPhysics.PixelToUnity(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyDebugLog.Info("Weapon",
                $"equipped type={WeaponType} owner={Owner.name} pos=({PhysicsBody.State.X:F1},{PhysicsBody.State.Y:F1})", this);
        }

        public virtual void Fire(Vector2 velocityPx)
        {
            if (IsFired)
            {
                MutinyDebugLog.Warning("Weapon", $"duplicate fire ignored type={WeaponType}", this);
                return;
            }

            IsFired = true;
            IsFinished = false;
            PhysicsBody.IsActive = true;
            m_LifetimeTimer = 0f;
            m_WaterTimer = 0f;

            // Clamp max velocity to 20 px/tick (Flash Weapon.release)
            float sqrLen = velocityPx.sqrMagnitude;
            if (sqrLen > TwangMaxForce * TwangMaxForce)
            {
                velocityPx = velocityPx.normalized * TwangMaxForce;
            }

            PhysicsBody.SetVelocity(velocityPx.x, velocityPx.y);
            MutinyDebugLog.Info("Weapon",
                $"fired type={WeaponType} owner={(Owner == null ? "none" : Owner.name)} velocity={velocityPx}", this);

            if (Owner != null)
            {
                Owner.CanShoot = false;
                Owner.CanThrow = false;
            }

            OnFired?.Invoke();

            var turnManager = FindAnyObjectByType<MutinyTurnManager>();
            if (turnManager != null)
            {
                turnManager.NotifyActionStarted();
            }
        }

        public virtual void Twang(Vector2 startPx, Vector2 dragPx)
        {
            Vector2 launchVel = MutinyPhysics.CalculateTwangVelocity(startPx, dragPx, TwangMaxForce);
            Fire(launchVel);
        }

        protected virtual void OnContact(CollisionSide side)
        {
            // Base weapon contact handling (overridden by CherryBomb, Dynamite, etc.)
        }

        // Most weapons rotate their root sprite.  Symbols with a non-rotating
        // overlay can override this to rotate only their original child layer.
        protected virtual Transform RotationTransform => transform;

        protected virtual void HandleEnterWater()
        {
            if (!m_HasSpawnedWaterSplash && PhysicsBody != null)
            {
                m_HasSpawnedWaterSplash = true;
                float waterY = float.IsInfinity(PhysicsBody.WaterPixelY) ? PhysicsBody.State.Y : PhysicsBody.WaterPixelY;
                MutinyWaterSurface.SpawnSplash(PhysicsBody.State.X, waterY);
                Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("splash");
            }

            OnContact(CollisionSide.Water);
            OnWaterSubmerged();
        }

        protected virtual void OnWaterSubmerged()
        {
            // Optional hook for subclasses (e.g. Dynamite extinguishing fuse)
        }

        protected virtual void Update()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            m_LifetimeTimer += Time.deltaTime;

            // 1. Water invalidation check
            if (PhysicsBody.IsInWater)
            {
                m_WaterTimer += Time.deltaTime;
                if (!m_HasSpawnedWaterSplash)
                {
                    HandleEnterWater();
                }

                // Submerged below water surface or sunk for duration -> invalidate
                float waterY = PhysicsBody.WaterPixelY;
                if (m_WaterTimer >= 0.4f || (!float.IsInfinity(waterY) && PhysicsBody.State.Y > waterY + 24f))
                {
                    Finish();
                    Destroy(gameObject, 0.3f);
                    return;
                }
            }

            // 2. Out of bounds check (Flash AS2 parity: y > levelHeight * 32)
            var levelRoot = FindAnyObjectByType<MutinyLevelRoot>();
            float maxLevelY = levelRoot != null && levelRoot.Height > 0
                ? levelRoot.Height * 32f + 64f
                : (float.IsInfinity(PhysicsBody.WaterPixelY) ? 1200f : PhysicsBody.WaterPixelY + 200f);

            if (PhysicsBody.State.Y > maxLevelY || PhysicsBody.State.Y > 2500f ||
                PhysicsBody.State.X < -600f || PhysicsBody.State.X > 3500f)
            {
                MutinyDebugLog.Info("Weapon", $"out of bounds expiration type={WeaponType} pos=({PhysicsBody.State.X:F1},{PhysicsBody.State.Y:F1})", this);
                Finish();
                Destroy(gameObject);
                return;
            }

            // 3. Safety lifetime timeout (prevents hanging indefinitely)
            if (m_LifetimeTimer > 8.0f)
            {
                MutinyDebugLog.Warning("Weapon", $"safety lifetime timeout expired type={WeaponType}", this);
                Finish();
                Destroy(gameObject);
            }
        }

        protected void AdvanceOriginalRotationTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            float delta = MutinyRotationRules.WeaponMotionDelta(
                WeaponType, PhysicsBody.State.VelocityX);
            if (!Mathf.Approximately(delta, 0f))
                RotationTransform?.Rotate(0f, 0f, delta);
        }

        public virtual void Finish()
        {
            if (IsFinished)
                return;

            IsFinished = true;
            MutinyDebugLog.Info("Weapon", $"finished type={WeaponType} name={name}", this);
            OnFinished?.Invoke();
        }
    }

    public enum CollisionSide
    {
        Floor,
        Ceiling,
        Wall,
        Water
    }
}
