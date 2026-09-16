using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinySeagull : MutinyWeapon
    {
        [Header("Flight Settings")]
        public float FlightSpeedPx = 10f;
        public float TargetDropX = 0f;
        public bool HasDropped = false;

        protected override void Awake()
        {
            WeaponType = "seagull";
            Extent = 12f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/Seagull/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public void CallAirstrike(float startX, float flightY, float dropX)
        {
            PhysicsBody.State.X = startX;
            PhysicsBody.State.Y = flightY;
            PhysicsBody.State.Weight = 0f; // horizontal flight, no gravity
            PhysicsBody.State.HitsTiles = false;
            PhysicsBody.SetVelocity(FlightSpeedPx, 0f);
            transform.position = MutinyPhysics.PixelToUnity(startX, flightY);

            TargetDropX = dropX;
            HasDropped = false;
            IsFired = true;
            IsFinished = false;

            var turnManager = FindAnyObjectByType<MutinyTurnManager>();
            if (turnManager != null)
            {
                turnManager.NotifyActionStarted();
            }
        }

        protected override void Update()
        {
            if (IsFinished)
                return;

            base.Update();
            if (IsFinished)
                return;

            if (IsFired && PhysicsBody != null)
            {
                if (!HasDropped && PhysicsBody.State.X >= TargetDropX)
                {
                    HasDropped = true;
                    DropBomb();
                }

                // Off-screen finish
                if (PhysicsBody.State.X > 2000f)
                {
                    Finish();
                    Destroy(gameObject, 0.1f);
                }
            }
        }

        private void DropBomb()
        {
            // Flash AS2 exact: new Explosion(this.x, this.y, 50, 50, this.owner)
            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y + 16f);
            MutinyExplosion.Spawn(posPx, 50f, 50f, Owner);
        }
    }
}
