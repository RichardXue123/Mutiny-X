using System.Collections.Generic;
using Mutiny.Diagnostics;
using UnityEngine;

namespace Mutiny.Simulation
{
    /// <summary>Flash Anchor: a placeable, vertically falling 48/96/0 Solid.</summary>
    [DisallowMultipleComponent]
    public sealed class MutinyAnchor : MutinyWeapon
    {
        // Anchor.advance does not call advanceMotion until place() has committed.
        public override bool AdvancesMotionWhileReady => false;
        // Anchor constructor omits show(); place() creates the visible clip.
        public override bool IsBodyVisibleWhileReady => false;

        public const float DropStartYPixels = -200f;
        public const float DropSpeedPixelsPerTick = 40f;
        public const float CrushDamage = 60f;
        public const int HoldTicks = 30;
        public const int FadeTicks = 10;

        private readonly List<Sprite> m_Frames = new();
        private bool m_HitBottom;
        private bool m_AnimationPlaying;
        private int m_HoldTicksRemaining;
        private int m_FadeTicksRemaining;
        private int m_AnimationFrame;
        private float m_TickAccumulator;

        public bool HasHitBottom => m_HitBottom;
        public int HoldTicksRemaining => m_HoldTicksRemaining;
        public int FadeTicksRemaining => m_FadeTicksRemaining;
        public int CurrentAnimationFrame => m_AnimationFrame + 1;

        protected override void Awake()
        {
            WeaponType = "anchor";
            Extent = 48f;
            base.Awake();
            LoadFrames();
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            // Anchor constructor: left/right=48, top=96, bottom=0, hitsBoxes=true.
            PhysicsBody.State.LeftExtent = 48f;
            PhysicsBody.State.RightExtent = 48f;
            PhysicsBody.State.TopExtent = 96f;
            PhysicsBody.State.BottomExtent = 0f;
            PhysicsBody.State.Weight = 0f;
            PhysicsBody.State.Bounce = 0.2f; // inherited Solid value; contact clears vy.
            PhysicsBody.State.Friction = 0.3f;
            PhysicsBody.State.HitsTiles = true;
            PhysicsBody.State.HitsBoxes = true;
            PhysicsBody.ApplyWaterPhysics = false;
            PhysicsBody.IsActive = false;
            // Anchor.advance calls Solid.advanceMotion directly and does not call
            // Weapon.advance. Its fall has no generic flying-weapon rotation.
            PhysicsBody.OnBeforeSimulationStep -= AdvanceOriginalRotationTick;
            PhysicsBody.OnFloorLanded -= HitFloor;
            PhysicsBody.OnFloorLanded += HitFloor;
            ResetVisualState();
        }

        /// <summary>Anchor.place(x,y) then its y=-200 override. Click y is ignored.</summary>
        public bool DropAt(float targetPixelX)
        {
            if (IsFired || IsFinished || PhysicsBody == null)
            {
                MutinyDebugLog.Warning("Anchor", "duplicate drop ignored", this);
                return false;
            }

            PhysicsBody.State.X = targetPixelX;
            PhysicsBody.State.Y = DropStartYPixels;
            PhysicsBody.SetVelocity(0f, DropSpeedPixelsPerTick);
            // This weapon advances its own Solid step in AdvanceOriginalTick.
            // Leaving the shared body inactive prevents a script-order-dependent
            // second or delayed step.
            PhysicsBody.IsActive = false;
            transform.position = MutinyPhysics.PixelToUnity(targetPixelX, DropStartYPixels);
            IsFired = true;
            IsFinished = false;
            m_HitBottom = false;
            m_AnimationPlaying = false;
            m_HoldTicksRemaining = HoldTicks;
            m_FadeTicksRemaining = FadeTicks;
            m_TickAccumulator = 0f;
            ResetVisualState();

            if (Owner != null)
            {
                Owner.CanThrow = false;
                Owner.CanShoot = false;
            }

            MutinyDebugLog.Info("Anchor", $"dropped x={targetPixelX:F1} y={DropStartYPixels:F1}", this);
            return true;
        }

        /// <summary>Anchor.aiPerform: place, then preserve the original 20 tick wait.</summary>
        public bool DropForAi(float targetPixelX, int waitTicks = 20)
        {
            if (!DropAt(targetPixelX))
                return false;
            m_HoldTicksRemaining = HoldTicks;
            m_FadeTicksRemaining = FadeTicks;
            m_AiWaitTicks = Mathf.Max(0, waitTicks);
            return true;
        }

        private int m_AiWaitTicks;

        protected override void Update()
        {
            // Anchor.advance deliberately does not call Weapon.advance, avoiding
            // generic splash/out-of-bounds/safety-timeout behavior.
            if (!IsFired || IsFinished)
                return;

            m_TickAccumulator += Time.deltaTime;
            while (m_TickAccumulator >= MutinyPhysics.TimeStep)
            {
                m_TickAccumulator -= MutinyPhysics.TimeStep;
                AdvanceOriginalTick();
            }
        }

        public void AdvanceOriginalTickForVerification()
        {
            AdvanceOriginalTick();
        }

        private void AdvanceOriginalTick()
        {
            if (m_AiWaitTicks > 0)
            {
                m_AiWaitTicks--;
                return;
            }

            if (!m_HitBottom)
            {
                // Anchor.advance sets this and then calls Solid.advanceMotion in
                // the same original 25 Hz tick.
                if (PhysicsBody != null)
                {
                    PhysicsBody.SetVelocity(0f, DropSpeedPixelsPerTick);
                    PhysicsBody.AdvanceSimulationTick();
                    transform.position = MutinyPhysics.PixelToUnity(
                        PhysicsBody.State.X, PhysicsBody.State.Y);
                }
                return;
            }

            AdvanceImpactTimelineTick();
        }

        private void HitFloor()
        {
            if (!IsFired || IsFinished || m_HitBottom)
                return;

            m_HitBottom = true;
            PhysicsBody.SetVelocity(0f, 0f);
            PhysicsBody.IsActive = false;
            m_AnimationPlaying = true; // MovieClip.play from stopped frame 1.
            m_AnimationFrame = 0;
            ApplyFrame();

            MutinyCharacter[] characters = FindObjectsByType<MutinyCharacter>();
            for (int i = 0; i < characters.Length; i++)
            {
                MutinyCharacter character = characters[i];
                if (character == null || character.PhysicsBody == null)
                    continue;

                PhysicsBodyState state = character.PhysicsBody.State;
                // Exact Anchor.contact strict boundaries. TakeDamage itself safely
                // ignores already-dead characters, matching the observable result.
                if (Mathf.Abs(state.X - PhysicsBody.State.X) < 48f &&
                    state.Y < PhysicsBody.State.Y && state.Y > PhysicsBody.State.Y - 64f)
                    character.TakeDamage(CrushDamage);
            }

            Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("anchor");
            MutinyDebugLog.Info("Anchor", $"floor hit x={PhysicsBody.State.X:F1} y={PhysicsBody.State.Y:F1}", this);

            // Anchor.advance reaches the hitBottom hold block in the same tick as
            // contact, decrementing 30 to 29 immediately.
            AdvanceImpactTimelineTick(advanceAnimation: false);
        }

        private void AdvanceImpactTimelineTick(bool advanceAnimation = true)
        {
            if (advanceAnimation && m_AnimationPlaying && m_AnimationFrame < m_Frames.Count - 1)
            {
                m_AnimationFrame++;
                ApplyFrame();
            }

            if (m_HoldTicksRemaining > 0)
            {
                m_HoldTicksRemaining--;
                return;
            }

            if (m_FadeTicksRemaining > 0)
            {
                m_FadeTicksRemaining--;
                ApplyWhiteOut(m_FadeTicksRemaining / (float)FadeTicks);
                return;
            }

            if (SpriteRenderer != null)
                SpriteRenderer.enabled = false;
            Finish();
            Destroy(gameObject, 0.1f);
            MutinyDebugLog.Info("Anchor", "finished after hold and white-out", this);
        }

        public static readonly Vector2 OriginalAnchorPivot = new Vector2(52f / 104f, 2f / 100f); // Symbol 1003: origin (52, 98) of 104x100

        private void LoadFrames()
        {
            m_Frames.Clear();
            for (int i = 1; i <= 12; i++)
            {
                Texture2D texture = Resources.Load<Texture2D>($"Art/Weapons/Anchor/{i}");
                if (texture != null)
                {
                    texture.filterMode = FilterMode.Point;
                    Sprite frame = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        OriginalAnchorPivot, MutinyPhysics.PixelsPerUnit);
                    m_Frames.Add(frame);
                }
                else
                {
                    Sprite frame = Resources.Load<Sprite>($"Art/Weapons/Anchor/{i}");
                    if (frame != null)
                        m_Frames.Add(frame);
                }
            }
            ApplyFrame();
        }

        private void ResetVisualState()
        {
            m_AnimationFrame = 0;
            if (SpriteRenderer != null)
            {
                SpriteRenderer.enabled = true;
                SpriteRenderer.color = Color.white;
            }
            ApplyFrame();
        }

        private void ApplyFrame()
        {
            if (SpriteRenderer != null && m_AnimationFrame >= 0 && m_AnimationFrame < m_Frames.Count)
                SpriteRenderer.sprite = m_Frames[m_AnimationFrame];
        }

        private void ApplyWhiteOut(float visibility)
        {
            if (SpriteRenderer == null)
                return;

            // Global.whiteOut is additive for the brightening half. SpriteRenderer's
            // default material has no additive color-transform term; retain the exact
            // alpha half and a white tint until a parity material is introduced.
            if (visibility <= 0.5f)
                SpriteRenderer.color = new Color(1f, 1f, 1f, visibility * 2f);
            else
                SpriteRenderer.color = Color.white;
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
                PhysicsBody.OnFloorLanded -= HitFloor;
        }
    }
}
