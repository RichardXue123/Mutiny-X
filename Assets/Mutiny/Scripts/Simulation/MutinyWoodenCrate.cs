using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyWoodenCrate : MutinyWeapon
    {
        protected override void Awake()
        {
            WeaponType = "woodenCrate";
            Extent = 16f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/WoodenCrate/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            PhysicsBody.State.Bounce = 0.2f;
            PhysicsBody.State.Friction = 0.5f;
        }

        protected override void Update()
        {
            if (IsFinished)
                return;

            base.Update();
            if (IsFinished)
                return;

            if (IsFired && PhysicsBody != null && PhysicsBody.IsInWater)
            {
                // Wooden crate sinks and breaks/destroys in water
                float waterY = PhysicsBody.WaterPixelY;
                if (m_WaterTimer >= 0.35f || (!float.IsInfinity(waterY) && PhysicsBody.State.Y > waterY + 20f))
                {
                    DestroyCrate();
                }
            }
        }

        public void DestroyCrate()
        {
            if (IsFinished)
                return;

            Finish();
            Destroy(gameObject, 0.05f);
        }
    }
}

