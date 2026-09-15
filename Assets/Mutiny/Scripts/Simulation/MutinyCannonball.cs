using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyCannonball : MutinyWeapon
    {
        protected override void Awake()
        {
            WeaponType = "cannonball";
            Extent = 9f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/Cannonball/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            PhysicsBody.State.Weight = 0f;
        }

        private void Update()
        {
            if (IsFinished)
                return;

            if (IsFired && PhysicsBody != null && PhysicsBody.IsInWater)
            {
                Finish();
                Destroy(gameObject, 0.2f);
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

            // Flash AS2 exact: new Explosion(this.x, this.y, 100, 50, this.owner)
            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyExplosion.Spawn(posPx, 100f, 50f, Owner);

            Destroy(gameObject, 0.1f);
        }
    }
}
