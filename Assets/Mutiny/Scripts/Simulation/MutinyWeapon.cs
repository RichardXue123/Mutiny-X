using System;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class MutinyWeapon : MonoBehaviour
    {
        public const int WeaponSortingOrder = 22;

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

        public virtual void Initialize(MutinyCharacter owner)
        {
            Owner = owner;
            IsFired = false;
            IsFinished = false;

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

            PhysicsBody.OnFloorLanded += () => OnContact(CollisionSide.Floor);
            PhysicsBody.OnWallHit += () => OnContact(CollisionSide.Wall);
            PhysicsBody.OnEnterWater += () => OnContact(CollisionSide.Water);
        }

        public virtual void Fire(Vector2 velocityPx)
        {
            if (IsFired)
                return;

            IsFired = true;
            IsFinished = false;

            // Clamp max velocity to 20 px/tick (Flash Weapon.release)
            float sqrLen = velocityPx.sqrMagnitude;
            if (sqrLen > TwangMaxForce * TwangMaxForce)
            {
                velocityPx = velocityPx.normalized * TwangMaxForce;
            }

            PhysicsBody.SetVelocity(velocityPx.x, velocityPx.y);

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

        protected virtual void Finish()
        {
            if (IsFinished)
                return;

            IsFinished = true;
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
