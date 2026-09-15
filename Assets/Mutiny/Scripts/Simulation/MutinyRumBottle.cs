using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyRumBottle : MutinyWeapon
    {
        protected override void Awake()
        {
            WeaponType = "rumBottle";
            Extent = 14f;
            TwangMaxForce = 30f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/RumBottle/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        private void Update()
        {
            if (IsFinished)
                return;

            if (IsFired && PhysicsBody != null)
            {
                transform.Rotate(0f, 0f, -PhysicsBody.State.VelocityX * 3f);
                if (PhysicsBody.IsInWater)
                {
                    Explode();
                }
            }
        }

        protected override void OnContact(CollisionSide side)
        {
            base.OnContact(side);

            if (IsFired && !IsFinished)
            {
                Explode();
            }
        }

        public void Explode()
        {
            if (IsFinished)
                return;

            Finish();

            if (SpriteRenderer != null)
            {
                SpriteRenderer.enabled = false;
            }

            // Flash AS2 exact: new Explosion(x, y, 80, 25, owner)
            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyExplosion.Spawn(posPx, 80f, 25f, Owner);

            Destroy(gameObject, 0.1f);
        }
    }
}

