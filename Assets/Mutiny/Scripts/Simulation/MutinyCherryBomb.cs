using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyCherryBomb : MutinyWeapon
    {
        [Header("Animation")]
        public Sprite[] AnimationFrames;
        public float FrameRate = 25f;

        private int m_CurrentFrame = 0;
        private float m_FrameTimer = 0f;

        protected override void Awake()
        {
            WeaponType = "cherryBomb";
            Extent = 9f;
            base.Awake();
            LoadSprites();
        }

        private void LoadSprites()
        {
            if (AnimationFrames != null && AnimationFrames.Length > 0)
                return;

            var frames = new List<Sprite>();
            for (int i = 1; i <= 4; i++)
            {
                Sprite sp = Resources.Load<Sprite>($"Art/Weapons/CherryBomb/{i}");
                if (sp != null)
                {
                    frames.Add(sp);
                }
            }

            AnimationFrames = frames.ToArray();
            if (AnimationFrames.Length > 0 && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = AnimationFrames[0];
            }
        }

        protected override void Update()
        {
            if (IsFinished)
                return;

            base.Update();
            if (IsFinished)
                return;

            // Animate frames
            if (AnimationFrames != null && AnimationFrames.Length > 0)
            {
                m_FrameTimer += Time.deltaTime;
                float frameDuration = 1f / FrameRate;
                while (m_FrameTimer >= frameDuration)
                {
                    m_FrameTimer -= frameDuration;
                    m_CurrentFrame = (m_CurrentFrame + 1) % AnimationFrames.Length;
                    SpriteRenderer.sprite = AnimationFrames[m_CurrentFrame];
                }
            }

            // Also check if fell below water level or off screen
            if (IsFired && PhysicsBody != null && PhysicsBody.IsInWater)
            {
                Explode();
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

            // Hide sprite
            if (SpriteRenderer != null)
            {
                SpriteRenderer.enabled = false;
            }

            // Spawn exact Flash explosion: size = 80, maxDamage = 40
            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyExplosion.Spawn(posPx, 80f, 40f, Owner);

            // Destroy weapon gameobject after a short delay so any remaining references can clean up
            Destroy(gameObject, 0.1f);
        }
    }
}
