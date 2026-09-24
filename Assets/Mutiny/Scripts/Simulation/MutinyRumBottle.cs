using System.Collections.Generic;
using Mutiny.Diagnostics;
using Mutiny.Presentation;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyRumBottle : MutinyWeapon
    {
        private const int OriginalFrameCount = 12;
        private readonly List<Sprite> m_Frames = new List<Sprite>(OriginalFrameCount);
        private int m_CurrentFrame;

        protected override void Awake()
        {
            WeaponType = "rumBottle";
            Extent = 14f;
            TwangMaxForce = 30f;
            base.Awake();
            LoadSprite();
        }

        public static readonly Vector2 OriginalPivot = new Vector2(9f / 18f, 15f / 48f); // Symbol 918: origin (9, 33) of 18x48

        private void LoadSprite()
        {
            for (int frame = 1; frame <= OriginalFrameCount; frame++)
            {
                Texture2D texture = Resources.Load<Texture2D>($"Art/Weapons/RumBottle/{frame}");
                if (texture != null)
                {
                    texture.filterMode = FilterMode.Point;
                    Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        OriginalPivot, MutinyPhysics.PixelsPerUnit);
                    m_Frames.Add(sprite);
                }
                else
                {
                    Sprite sprite = Resources.Load<Sprite>($"Art/Weapons/RumBottle/{frame}");
                    if (sprite != null)
                        m_Frames.Add(sprite);
                }
            }

            if (m_Frames.Count > 0 && SpriteRenderer != null)
                SpriteRenderer.sprite = m_Frames[0];
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            // RumBottle.as constructor: hitsBoxes = true.
            PhysicsBody.State.HitsBoxes = true;
            m_CurrentFrame = 0;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalPresentationTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalPresentationTick;
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnContact(CollisionSide side)
        {
            base.OnContact(side);

            if (IsFired && !IsFinished)
            {
                Explode(side);
            }
        }

        public void Explode()
        {
            Explode(CollisionSide.Wall);
        }

        private void Explode(CollisionSide side)
        {
            if (IsFinished)
                return;

            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            bool createFlames = side == CollisionSide.Floor;
            string[,] terrain = null;
            int terrainWidth = 0;
            int terrainHeight = 0;
            if (createFlames)
                PhysicsBody.TryGetTerrain(out terrain, out terrainWidth, out terrainHeight);

            Finish();

            if (SpriteRenderer != null)
            {
                SpriteRenderer.enabled = false;
            }

            // RumBottle.contact: new Explosion(x, y, 80, 25, owner); playSound("pop").
            // The original bottle plays pop immediately in RumBottle.contact;
            // Explosion.hit happens two timeline frames later and must not duplicate it.
            MutinyExplosion.Spawn(posPx, 80f, 25f, Owner, playPopOnHit: false);
            MutinyAudioManager.Instance?.PlaySFX("pop");

            if (createFlames && terrain != null)
            {
                Vector2 flameOrigin = FindOriginalFlameOrigin(posPx, terrain, terrainWidth, terrainHeight);
                MutinySweepingFlame.Spawn(flameOrigin, true, terrain, terrainWidth, terrainHeight);
                MutinySweepingFlame.Spawn(flameOrigin, false, terrain, terrainWidth, terrainHeight);
                MutinyDebugLog.Info("RumBottle",
                    $"floor impact pos=({posPx.x:F1},{posPx.y:F1}) flameOrigin=({flameOrigin.x:F1},{flameOrigin.y:F1})", this);
            }
            else
            {
                MutinyDebugLog.Info("RumBottle",
                    $"impact side={side} pos=({posPx.x:F1},{posPx.y:F1}) flames=false", this);
            }

            Destroy(gameObject, 0.1f);
        }

        private void AdvanceOriginalPresentationTick()
        {
            if (IsFinished)
                return;

            if (m_Frames.Count > 0 && SpriteRenderer != null)
            {
                m_CurrentFrame = (m_CurrentFrame + 1) % m_Frames.Count;
                SpriteRenderer.sprite = m_Frames[m_CurrentFrame];
            }

            // RumBottle.advance creates one cannonSmokeTrail Debris each 25 Hz
            // non-simulation advance. The source has no fired condition, so the
            // equipped/ready bottle also emits its fuse trail.
            if (!IsFinished)
                MutinyRumBottleSmokeTrail.Spawn(new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y));
        }

        private static Vector2 FindOriginalFlameOrigin(
            Vector2 impact, string[,] terrain, int width, int height)
        {
            int column = Mathf.FloorToInt(impact.x / 32f);
            int row = Mathf.FloorToInt(impact.y / 32f) + 1;

            while (row > 0 && MutinySweepingFlame.IsSolidTile(terrain, width, height, column, row - 1))
                row--;

            return new Vector2(column * 32f, row * 32f);
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
                PhysicsBody.OnSimulationStep -= AdvanceOriginalPresentationTick;
        }
    }
}
