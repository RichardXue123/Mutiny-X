using System.Collections.Generic;
using Mutiny.Diagnostics;
using Mutiny.Levels;
using Mutiny.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mutiny.Simulation
{
    /// <summary>Flash ParachuteBomb: a 30-force projectile that opens a chute mid-flight.</summary>
    [DisallowMultipleComponent]
    public sealed class MutinyParachuteBomb : MutinyWeapon
    {
        public const float OriginalExtentPixels = 11f;
        public const float OriginalTwangMaxForce = 30f;
        public const float HorizontalDragPerTick = 0.95f;
        public const float FanImpulsePerTick = 0.2f;
        public const int FanSoundIntervalTicks = 12;
        public const float ChuteOpenVelocityThreshold = -10f;
        public const float CeilingClampPixels = -300f;
        public const float ExplosionSize = 160f;
        public const float ExplosionDamage = 50f;

        // DefineSprite 939: closed frame 1, opening label at frame 11, open label
        // at frame 26, and frame 30 calls gotoAndPlay("open"). Indices are zero-based.
        private const int OriginalFrameCount = 30;
        private const int OpeningFrameIndex = 10;
        private const int OpenLoopFrameIndex = 25;

        private readonly List<Sprite> m_Frames = new List<Sprite>(OriginalFrameCount);
        private int m_FramesFromFire;
        private int m_CurrentFrame;
        private bool m_OverWater = true;
        private bool? m_VerificationFanHeld;
        private float m_VerificationMousePixelX;
        private bool m_ChuteOpenedThisTick;

        [Header("Parachute State")]
        public bool ChuteOpen { get; private set; }
        public bool IsFanActive { get; private set; }
        public int FramesFromFire => m_FramesFromFire;
        public int CurrentAnimationFrame => m_CurrentFrame + 1;

        protected override void Awake()
        {
            WeaponType = "parachuteBomb";
            Extent = OriginalExtentPixels;
            TwangMaxForce = OriginalTwangMaxForce;
            base.Awake();
            LoadFrames();
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            PhysicsBody.State.LeftExtent = OriginalExtentPixels;
            PhysicsBody.State.RightExtent = OriginalExtentPixels;
            PhysicsBody.State.TopExtent = OriginalExtentPixels;
            PhysicsBody.State.BottomExtent = OriginalExtentPixels;
            PhysicsBody.State.HitsBoxes = true;

            // Weapon.advance uses splashCheck only. It does not apply the project's
            // Character-style underwater drag, and crossing water is not contact().
            PhysicsBody.ApplyWaterPhysics = false;
            PhysicsBody.OnBeforeSimulationStep -= AdvanceOriginalMotionTick;
            PhysicsBody.OnBeforeSimulationStep += AdvanceOriginalMotionTick;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalPostMotionTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalPostMotionTick;

            ChuteOpen = false;
            IsFanActive = false;
            m_FramesFromFire = 0;
            m_CurrentFrame = 0;
            m_OverWater = true;
            m_VerificationFanHeld = null;
            m_ChuteOpenedThisTick = false;
            ApplyFrame();
        }

        public override void Fire(Vector2 velocityPx)
        {
            base.Fire(velocityPx);
            if (IsFired)
            {
                PhysicsBody.IsActive = true;
                MutinyDebugLog.Info("ParachuteBomb",
                    $"fired velocity=({PhysicsBody.State.VelocityX:F2},{PhysicsBody.State.VelocityY:F2})", this);
            }
        }

        // Runs one actual MutinyPhysicsBody production tick. The optional fan input
        // replaces physical mouse polling only for the verification caller.
        public void AdvanceOriginalTickForVerification(bool fanHeld, float mousePixelX)
        {
            if (PhysicsBody == null || IsFinished)
                return;

            m_VerificationFanHeld = fanHeld;
            m_VerificationMousePixelX = mousePixelX;
            PhysicsBody.AdvanceSimulationTick();
            SyncTransformFromState();
            m_VerificationFanHeld = null;
        }

        protected override void Update()
        {
            // ParachuteBomb inherits Weapon.advance, whose only lifecycle exits are
            // map-bottom and exact rest. The common Unity water timeout/safety timer
            // would incorrectly remove it or turn a splash into an explosion.
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            if ((PhysicsBody.State.VelocityY > 0f &&
                 PhysicsBody.State.Y > ResolveLevelHeightPixels()) ||
                (PhysicsBody.State.VelocityX == 0f && Mathf.Abs(PhysicsBody.State.VelocityY) < 0.2f))
            {
                FinishWithoutExplosion("weapon-advance");
            }
        }

        private void AdvanceOriginalMotionTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            // ParachuteBomb.advanceMotion performs these operations before
            // Solid.advanceMotion adds weight and moves/collides.
            if (PhysicsBody.State.VelocityY > 1f)
            {
                PhysicsBody.State.VelocityY -= 2f;
                if (PhysicsBody.State.VelocityY < 1f)
                    PhysicsBody.State.VelocityY = 1f;
            }
            PhysicsBody.State.VelocityX *= HorizontalDragPerTick;

            if (!ChuteOpen && PhysicsBody.State.VelocityY > ChuteOpenVelocityThreshold)
            {
                ChuteOpen = true;
                m_CurrentFrame = OpeningFrameIndex;
                m_ChuteOpenedThisTick = true;
                ApplyFrame();
                MutinyDebugLog.Info("ParachuteBomb",
                    $"chute opened tick={m_FramesFromFire + 1} vy={PhysicsBody.State.VelocityY:F2}", this);
            }

            m_FramesFromFire++;
            ApplyFanInput();
        }

        private void AdvanceOriginalPostMotionTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            // Weapon.advance calls splashCheck after motion. It only makes a splash
            // and sound when the over-water boolean changes; it never calls contact.
            AdvanceSplashCheck();

            // ParachuteBomb.advance runs after Weapon.advance and emits smoke only
            // while its chute remains closed.
            if (!ChuteOpen)
                MutinyRumBottleSmokeTrail.Spawn(new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y));
            else if (m_ChuteOpenedThisTick)
                m_ChuteOpenedThisTick = false;
            else
                AdvanceChuteAnimation();

            if (PhysicsBody.State.Y < CeilingClampPixels)
            {
                PhysicsBody.State.Y = CeilingClampPixels;
                if (PhysicsBody.State.VelocityY < 0f)
                    PhysicsBody.State.VelocityY = 0f;
                SyncTransformFromState();
                MutinyDebugLog.Info("ParachuteBomb", "clamped at original y=-300 ceiling", this);
            }
        }

        private void ApplyFanInput()
        {
            if (!IsHumanOwned() || !TryGetFanInput(out bool held, out float mousePixelX))
            {
                IsFanActive = false;
                return;
            }

            IsFanActive = held;
            if (!held)
                return;

            // Original direction is intentionally opposite the cursor side:
            // mouse left pushes right, mouse right pushes left.
            if (mousePixelX - PhysicsBody.State.X < 0f)
                PhysicsBody.State.VelocityX += FanImpulsePerTick;
            else
                PhysicsBody.State.VelocityX -= FanImpulsePerTick;

            if (m_FramesFromFire % FanSoundIntervalTicks == 0)
                MutinyAudioManager.Instance?.PlaySFX("fan");
        }

        private bool TryGetFanInput(out bool held, out float mousePixelX)
        {
            if (m_VerificationFanHeld.HasValue)
            {
                held = m_VerificationFanHeld.Value;
                mousePixelX = m_VerificationMousePixelX;
                return true;
            }

            Mouse mouse = Mouse.current;
            Camera camera = Camera.main;
            if (mouse == null || camera == null)
            {
                held = false;
                mousePixelX = 0f;
                return false;
            }

            Vector2 screen = mouse.position.ReadValue();
            Vector3 world = camera.ScreenToWorldPoint(
                new Vector3(screen.x, screen.y, -camera.transform.position.z));
            held = mouse.leftButton.isPressed;
            mousePixelX = MutinyPhysics.UnityToPixel(world).x;
            return true;
        }

        private void AdvanceSplashCheck()
        {
            if (float.IsInfinity(PhysicsBody.WaterPixelY))
                return;

            bool overWater = PhysicsBody.State.Y < PhysicsBody.WaterPixelY;
            if (m_OverWater != overWater)
            {
                MutinyWaterSurface.SpawnSplash(PhysicsBody.State.X, PhysicsBody.WaterPixelY);
                MutinyAudioManager.Instance?.PlaySFX("splash");
                MutinyDebugLog.Info("ParachuteBomb",
                    $"water crossing y={PhysicsBody.State.Y:F1} waterY={PhysicsBody.WaterPixelY:F1}", this);
            }
            m_OverWater = overWater;
        }

        private void AdvanceChuteAnimation()
        {
            if (m_Frames.Count == 0 || SpriteRenderer == null)
                return;

            m_CurrentFrame++;
            if (m_CurrentFrame >= OriginalFrameCount)
                m_CurrentFrame = OpenLoopFrameIndex;
            ApplyFrame();
        }

        private void LoadFrames()
        {
            m_Frames.Clear();
            for (int frame = 1; frame <= OriginalFrameCount; frame++)
            {
                Sprite sprite = Resources.Load<Sprite>($"Art/Weapons/ParachuteBomb/{frame}");
                if (sprite != null)
                    m_Frames.Add(sprite);
            }
            ApplyFrame();
        }

        private void ApplyFrame()
        {
            if (SpriteRenderer != null && m_CurrentFrame >= 0 && m_CurrentFrame < m_Frames.Count)
                SpriteRenderer.sprite = m_Frames[m_CurrentFrame];
        }

        protected override void OnContact(CollisionSide side)
        {
            base.OnContact(side);
            if (IsFired && !IsFinished)
                Explode(side);
        }

        private void Explode(CollisionSide side)
        {
            Vector2 position = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            Finish();
            if (SpriteRenderer != null)
                SpriteRenderer.enabled = false;

            // ParachuteBomb.contact creates the Explosion and plays pop in the same
            // tick, before Explosion.hit reaches its visual damage frame.
            MutinyExplosion.Spawn(position, ExplosionSize, ExplosionDamage, Owner, playPopOnHit: false);
            MutinyAudioManager.Instance?.PlaySFX("pop");
            Destroy(gameObject, 0.1f);
            MutinyDebugLog.Info("ParachuteBomb",
                $"exploded side={side} pos=({position.x:F1},{position.y:F1})", this);
        }

        private void FinishWithoutExplosion(string reason)
        {
            Finish();
            if (SpriteRenderer != null)
                SpriteRenderer.enabled = false;
            Destroy(gameObject, 0.1f);
            MutinyDebugLog.Info("ParachuteBomb", $"finished reason={reason}", this);
        }

        private float ResolveLevelHeightPixels()
        {
            if (PhysicsBody.TryGetTerrain(out _, out _, out int height))
                return height * MutinyPhysics.PixelsPerUnit;

            MutinyLevelRoot root = FindAnyObjectByType<MutinyLevelRoot>();
            return root != null && root.Height > 0 ? root.Height * MutinyPhysics.PixelsPerUnit : 1200f;
        }

        private void SyncTransformFromState()
        {
            transform.position = MutinyPhysics.PixelToUnity(PhysicsBody.State.X, PhysicsBody.State.Y);
        }

        private bool IsHumanOwned()
        {
            MutinyTeam[] teams = FindObjectsByType<MutinyTeam>();
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null && teams[i].Characters.Contains(Owner))
                    return !teams[i].IsAiControlled;
            }
            return false;
        }

        private void OnDestroy()
        {
            if (PhysicsBody == null)
                return;
            PhysicsBody.OnBeforeSimulationStep -= AdvanceOriginalMotionTick;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalPostMotionTick;
        }
    }
}
