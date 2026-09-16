using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyAnchor : MutinyWeapon
    {
        [Header("Anchor Properties")]
        public float DropSpeedPx = 40f;
        public float CrushDamage = 60f;
        private bool m_HitBottom;
        private int m_HoldTicks = 30;
        private int m_FadeTicks = 10;
        private float m_TickAccumulator;

        protected override void Awake()
        {
            WeaponType = "anchor";
            Extent = 48f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/Anchor/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);

            // Flash Anchor: leftExtent=rightExtent=48, topExtent=96, bottomExtent=0
            PhysicsBody.State.LeftExtent = 48f;
            PhysicsBody.State.RightExtent = 48f;
            PhysicsBody.State.TopExtent = 96f;
            PhysicsBody.State.BottomExtent = 0f;
            PhysicsBody.State.Weight = 0f;
            PhysicsBody.State.Bounce = 0f;
            PhysicsBody.OnFloorLanded += HitFloor;
        }

        public void DropAt(float targetPixelX)
        {
            PhysicsBody.State.X = targetPixelX;
            PhysicsBody.State.Y = -200f;
            PhysicsBody.SetVelocity(0f, DropSpeedPx);
            transform.position = MutinyPhysics.PixelToUnity(targetPixelX, -200f);
            IsFired = true;
            IsFinished = false;
            m_HitBottom = false;
            m_HoldTicks = 30;
            m_FadeTicks = 10;
            if (Owner != null)
            {
                Owner.CanThrow = false;
                Owner.CanShoot = false;
            }

            var turnManager = FindAnyObjectByType<MutinyTurnManager>();
            if (turnManager != null)
            {
                turnManager.NotifyActionStarted();
            }
        }

        protected override void Update()
        {
            if (IsFinished)
                return;

            base.Update();
            if (IsFinished)
                return;

            if (IsFired && !m_HitBottom && PhysicsBody != null && PhysicsBody.IsInWater)
            {
                float waterY = PhysicsBody.WaterPixelY;
                if (!float.IsInfinity(waterY) && PhysicsBody.State.Y > waterY + 48f)
                {
                    Finish();
                    Destroy(gameObject, 0.2f);
                    return;
                }
            }

            if (IsFired && m_HitBottom)
            {
                m_TickAccumulator += Time.deltaTime;
                while (m_TickAccumulator >= MutinyPhysics.TimeStep)
                {
                    m_TickAccumulator -= MutinyPhysics.TimeStep;
                    if (m_HoldTicks > 0)
                        m_HoldTicks--;
                    else if (m_FadeTicks > 0)
                    {
                        m_FadeTicks--;
                        Color color = SpriteRenderer.color;
                        color.a = m_FadeTicks / 10f;
                        SpriteRenderer.color = color;
                    }
                    else
                    {
                        Finish();
                        Destroy(gameObject, 0.1f);
                        break;
                    }
                }
            }
        }

        private void HitFloor()
        {
            if (!IsFired || IsFinished || m_HitBottom)
                return;

            m_HitBottom = true;
            PhysicsBody.SetVelocity(0f, 0f);
            PhysicsBody.IsActive = false;
            var chars = FindObjectsByType<MutinyCharacter>();
            for (int i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                if (ch == null || !ch.IsAlive || ch.PhysicsBody == null)
                    continue;

                PhysicsBodyState state = ch.PhysicsBody.State;
                if (Mathf.Abs(state.X - PhysicsBody.State.X) < 48f &&
                    state.Y < PhysicsBody.State.Y && state.Y > PhysicsBody.State.Y - 64f)
                    ch.TakeDamage(CrushDamage);
            }
            Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("anchor");
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
                PhysicsBody.OnFloorLanded -= HitFloor;
        }
    }
}
