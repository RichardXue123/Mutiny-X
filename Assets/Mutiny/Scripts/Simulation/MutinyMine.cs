using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyMine : MutinyWeapon
    {
        [Header("Mine Proximity")]
        public float TriggerRadiusPx = 60f;
        public int CountdownTicks = 60; // 60 ticks = 2.4s
        public bool IsArmed = false;

        private int m_CurrentCountdown;

        protected override void Awake()
        {
            WeaponType = "mine";
            Extent = 14f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/Mine/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);

            // Flash AS2 Mine: friction = 1.5
            PhysicsBody.State.Friction = 1.5f;
            IsArmed = false;
            m_CurrentCountdown = CountdownTicks;
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
                if (PhysicsBody.IsInWater)
                {
                    // Water neutralizes mine
                    Finish();
                    Destroy(gameObject, 0.5f);
                    return;
                }

                // Arms when comes to rest
                if (!IsArmed && PhysicsBody.IsAtRest)
                {
                    IsArmed = true;
                }

                if (IsArmed)
                {
                    m_CurrentCountdown--;
                    CheckProximity();

                    if (m_CurrentCountdown <= 0)
                    {
                        Explode();
                    }
                }
            }
        }

        private void CheckProximity()
        {
            var chars = FindObjectsByType<MutinyCharacter>();
            for (int i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                if (ch == null || !ch.IsAlive)
                    continue;

                float distPx = Vector2.Distance(
                    new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y),
                    new Vector2(ch.PhysicsBody.State.X, ch.PhysicsBody.State.Y)
                );

                if (distPx <= TriggerRadiusPx)
                {
                    Explode();
                    return;
                }
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

            // Flash AS2 exact: new Explosion(x, y, 250, 70, owner)
            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyExplosion.Spawn(posPx, 250f, 70f, Owner);

            Destroy(gameObject, 0.1f);
        }
    }
}

