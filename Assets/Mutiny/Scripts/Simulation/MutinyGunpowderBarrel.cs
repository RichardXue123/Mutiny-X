using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyGunpowderBarrel : MutinyWeapon
    {
        public bool HasExploded = false;

        protected override void Awake()
        {
            WeaponType = "gunpowderBarrel";
            Extent = 16f; // Standard 32x32 crate block
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/GunpowderBarrel/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            PhysicsBody.State.Bounce = 0.2f;
            PhysicsBody.State.Friction = 0.3f;
        }

        public void Explode()
        {
            if (HasExploded)
                return;

            HasExploded = true;
            Finish();

            if (SpriteRenderer != null)
            {
                SpriteRenderer.enabled = false;
            }

            // Flash AS2 exact: new Explosion(this.x, this.y, 150, 30, null)
            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyExplosion.Spawn(posPx, 150f, 30f, Owner);

            Destroy(gameObject, 0.1f);
        }
    }
}

