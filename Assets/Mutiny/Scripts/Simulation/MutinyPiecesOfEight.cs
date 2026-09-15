using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyPiecesOfEight : MutinyWeapon
    {
        [Header("Triple Shot")]
        public int ShotsRemaining = 3;

        protected override void Awake()
        {
            WeaponType = "piecesOfEight";
            Extent = 7f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/PiecesOfEight/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
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

            // Flash AS2 exact: new Explosion(x, y, 50, 25, owner)
            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyExplosion.Spawn(posPx, 50f, 25f, Owner);

            ShotsRemaining--;
            if (ShotsRemaining > 0 && Owner != null && Owner.IsAlive)
            {
                // Re-enable owner shooting for the next piece of eight
                Owner.CanShoot = true;
            }

            Destroy(gameObject, 0.1f);
        }
    }
}

