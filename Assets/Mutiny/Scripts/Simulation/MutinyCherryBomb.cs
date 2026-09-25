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

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            // CherryBomb.as constructor: hitsBoxes = true.
            PhysicsBody.State.HitsBoxes = true;
            PhysicsBody.OnSimulationStep -= EmitOriginalSmokeTrail;
            PhysicsBody.OnSimulationStep += EmitOriginalSmokeTrail;
            PhysicsBody.OnSimulationStep -= AdvanceInheritedFinishTick;
            PhysicsBody.OnSimulationStep += AdvanceInheritedFinishTick;
        }

        public static readonly Vector2 OriginalPivot = new Vector2(10f / 20f, 10f / 32f); // Symbol 844: origin (10, 22) of 20x32

        private void LoadSprites()
        {
            if (AnimationFrames != null && AnimationFrames.Length > 0)
                return;

            var frames = new List<Sprite>();
            for (int i = 1; i <= 4; i++)
            {
                Texture2D texture = Resources.Load<Texture2D>($"Art/Weapons/CherryBomb/{i}");
                if (texture != null)
                {
                    texture.filterMode = FilterMode.Point;
                    Sprite sp = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        OriginalPivot, MutinyPhysics.PixelsPerUnit);
                    frames.Add(sp);
                }
                else
                {
                    Sprite sp = Resources.Load<Sprite>($"Art/Weapons/CherryBomb/{i}");
                    if (sp != null)
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

        private void EmitOriginalSmokeTrail()
        {
            // CherryBomb.advance emits one trail every non-simulation tick while
            // the clip exists, including its equipped/ready state.
            if (!IsFinished && PhysicsBody != null)
                MutinyRumBottleSmokeTrail.Spawn(PhysicsBody.CurrentStepStartPositionPixels);
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
            {
                PhysicsBody.OnSimulationStep -= EmitOriginalSmokeTrail;
                PhysicsBody.OnSimulationStep -= AdvanceInheritedFinishTick;
            }
        }
    }
}
