using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyDynamite : MutinyWeapon
    {
        [Header("Animation")]
        public Sprite[] LitFrames;
        public Sprite UnlitFrame;
        public float FrameRate = 25f;

        [Header("Dynamite State")]
        public bool IsLit = true;

        private int m_CurrentLitFrame = 0;
        private float m_FrameTimer = 0f;

        protected override void Awake()
        {
            WeaponType = "dynamite";
            Extent = 11f;
            base.Awake();
            LoadSprites();
        }

        private void LoadSprites()
        {
            var lit = new List<Sprite>();
            for (int i = 1; i <= 5; i++)
            {
                Sprite sp = Resources.Load<Sprite>($"Art/Weapons/Dynamite/{i}");
                if (sp != null)
                {
                    lit.Add(sp);
                }
            }
            LitFrames = lit.ToArray();

            UnlitFrame = Resources.Load<Sprite>("Art/Weapons/Dynamite/6");

            if (LitFrames.Length > 0 && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = LitFrames[0];
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);

            IsLit = true;
            // Flash Dynamite: friction = 1.7, extents = 11
            PhysicsBody.State.Friction = 1.7f;
            PhysicsBody.State.LeftExtent = 11f;
            PhysicsBody.State.RightExtent = 11f;
            PhysicsBody.State.TopExtent = 11f;
            PhysicsBody.State.BottomExtent = 11f;
            PhysicsBody.State.Bounce = 0.2f;
        }

        private void Update()
        {
            if (IsFinished)
                return;

            // Flash AS2: if(this.fired) this.rotation += this.velocityX * 2;
            if (IsFired && PhysicsBody != null)
            {
                transform.Rotate(0f, 0f, -PhysicsBody.State.VelocityX * 2f);

                // Water extinguishes fuse (Flash: if(y > water.y) mc.gotoAndStop("unlit"))
                if (PhysicsBody.IsInWater && IsLit)
                {
                    IsLit = false;
                    if (UnlitFrame != null && SpriteRenderer != null)
                    {
                        SpriteRenderer.sprite = UnlitFrame;
                    }
                }

                // Fuse burning animation
                if (IsLit && LitFrames != null && LitFrames.Length > 0)
                {
                    m_FrameTimer += Time.deltaTime;
                    float frameDuration = 1f / FrameRate;
                    while (m_FrameTimer >= frameDuration)
                    {
                        m_FrameTimer -= frameDuration;
                        m_CurrentLitFrame = (m_CurrentLitFrame + 1) % LitFrames.Length;
                        SpriteRenderer.sprite = LitFrames[m_CurrentLitFrame];
                    }
                }

                // Resting check: Dynamite only explodes when it comes to a complete stop!
                // Flash AS2: if(this.velocityX == 0 && Math.abs(this.velocityY) < 0.2)
                if (PhysicsBody.IsAtRest)
                {
                    if (IsLit && !PhysicsBody.IsInWater)
                    {
                        Explode();
                    }
                    else
                    {
                        // Extinguished in water or rests dud
                        Finish();
                        Destroy(gameObject, 0.5f);
                    }
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

            // Flash AS2 exact formula: new Explosion(x, y, 250, 70, owner)
            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyExplosion.Spawn(posPx, 250f, 70f, Owner);

            Destroy(gameObject, 0.1f);
        }
    }
}
