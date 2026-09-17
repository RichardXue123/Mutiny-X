using Mutiny.Diagnostics;
using Mutiny.Presentation;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyVoodooDoll : MutinyWeapon
    {
        public const int OriginalOwnerFlightTicks = 10;
        public const int OriginalTargetWaitTicks = 10;
        public const float OriginalFadeAlphaPerTick = 0.1f;

        [Header("Target Link")]
        public MutinyCharacter TargetCharacter { get; private set; }

        private int m_FramesOnThis = OriginalOwnerFlightTicks;
        private int m_FramesOnTarget = OriginalTargetWaitTicks;
        private Vector2 m_ThrowVelocity;
        private bool m_TargetFocusRequested;
        private bool m_TransferredTargetVelocity;

        public bool HasTarget => TargetCharacter != null && TargetCharacter.IsAlive;
        public bool IsTargetFocusRequested => m_TargetFocusRequested;
        public bool HasTransferredTargetVelocity => m_TransferredTargetVelocity;
        public int FramesOnThis => m_FramesOnThis;
        public int FramesOnTarget => m_FramesOnTarget;

        // VoodooDoll.advance switches TileSystem.panToCharacter to the target
        // after ten doll ticks. The camera consumes this only until the transfer.
        public Transform CameraFocusTarget => m_TargetFocusRequested && !m_TransferredTargetVelocity &&
                                              TargetCharacter != null
            ? TargetCharacter.transform
            : null;

        protected override void Awake()
        {
            WeaponType = "voodooDoll";
            // VoodooDoll does not override Solid's default four 10 px extents.
            Extent = 10f;
            IsDraggable = false;
            IsTwangable = false;
            base.Awake();
            LoadSprite();
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            TargetCharacter = null;
            m_FramesOnThis = OriginalOwnerFlightTicks;
            m_FramesOnTarget = OriginalTargetWaitTicks;
            m_ThrowVelocity = Vector2.zero;
            m_TargetFocusRequested = false;
            m_TransferredTargetVelocity = false;
            IsTwangable = false;

            if (SpriteRenderer != null)
            {
                Color color = SpriteRenderer.color;
                color.a = 1f;
                SpriteRenderer.color = color;
            }

            PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalTick;
        }

        public bool BindTarget(MutinyCharacter target)
        {
            if (IsFired || target == null || !target.IsAlive)
            {
                MutinyDebugLog.Warning("VoodooDoll",
                    $"target binding rejected fired={IsFired} target={(target == null ? "none" : target.name)}", this);
                return false;
            }

            TargetCharacter = target;
            IsTwangable = true;
            MutinyDebugLog.Info("VoodooDoll", $"target bound target={target.name}; twang enabled", this);
            return true;
        }

        public override void Fire(Vector2 velocityPx)
        {
            if (!HasTarget)
            {
                MutinyDebugLog.Warning("VoodooDoll", "fire rejected because no live target is bound", this);
                return;
            }

            if (IsFired)
                return;

            base.Fire(velocityPx);
            if (!IsFired)
                return;

            m_ThrowVelocity = new Vector2(PhysicsBody.State.VelocityX, PhysicsBody.State.VelocityY);
            MutinyDebugLog.Info("VoodooDoll",
                $"fired target={TargetCharacter.name} velocity=({m_ThrowVelocity.x:F1},{m_ThrowVelocity.y:F1})", this);
        }

        public override void Twang(Vector2 startPx, Vector2 dragPx)
        {
            if (!HasTarget)
                return;

            // Weapon.release clamps all conventional thrown weapons to 20 px/tick.
            Vector2 velocity = MutinyPhysics.CalculateTwangVelocity(startPx, dragPx, TwangMaxForce);
            if (velocity.sqrMagnitude > MutinyPhysics.DefaultTwangMaxForce * MutinyPhysics.DefaultTwangMaxForce)
                velocity = velocity.normalized * MutinyPhysics.DefaultTwangMaxForce;
            Fire(velocity);
        }

        public void FireForAi(Vector2 velocityPx)
        {
            bool wasFired = IsFired;
            Fire(velocityPx);
            if (!wasFired && IsFired)
                MutinyAudioManager.Instance?.PlaySFX("voodoo");
        }

        public void AdvanceOriginalTickForVerification(bool assumeCameraFocused = true)
        {
            AdvanceOriginalTick(assumeCameraFocused);
        }

        protected override void Update()
        {
            // VoodooDoll overrides Weapon.advance in AS2. In particular, it does
            // not inherit generic water, rest, bounds, or safety-timeout paths.
        }

        private void AdvanceOriginalTick()
        {
            AdvanceOriginalTick(false);
        }

        private void AdvanceOriginalTick(bool assumeCameraFocused)
        {
            if (!IsFired || IsFinished)
                return;

            if (m_FramesOnThis > 0)
            {
                m_FramesOnThis--;
                if (m_FramesOnThis <= 0)
                {
                    m_TargetFocusRequested = true;
                    MutinyDebugLog.Info("VoodooDoll", $"owner flight complete; camera target={TargetCharacter?.name}", this);
                }
                return;
            }

            if (!m_TransferredTargetVelocity)
            {
                if (!IsTargetCameraFocused(assumeCameraFocused))
                    return;

                if (m_FramesOnTarget > 0)
                {
                    m_FramesOnTarget--;
                    return;
                }

                TransferVelocityToTarget();
                return;
            }

            FadeAndFinish();
        }

        private bool IsTargetCameraFocused(bool assumeCameraFocused)
        {
            if (assumeCameraFocused)
                return true;

            MutinyCameraController cameraController = FindAnyObjectByType<MutinyCameraController>();
            return cameraController == null || cameraController.HasReachedVoodooTarget(TargetCharacter);
        }

        private void TransferVelocityToTarget()
        {
            if (m_TransferredTargetVelocity)
                return;

            m_TransferredTargetVelocity = true;
            if (TargetCharacter != null && TargetCharacter.PhysicsBody != null)
            {
                // AS2 writes the saved release velocity once. It does not deal
                // direct damage or continuously mirror the doll velocity.
                TargetCharacter.PhysicsBody.SetVelocity(m_ThrowVelocity.x, m_ThrowVelocity.y);
                MutinyDebugLog.Info("VoodooDoll",
                    $"target velocity transferred target={TargetCharacter.name} velocity=({m_ThrowVelocity.x:F1},{m_ThrowVelocity.y:F1})", this);
            }
        }

        private void FadeAndFinish()
        {
            if (SpriteRenderer != null)
            {
                Color color = SpriteRenderer.color;
                color.a -= OriginalFadeAlphaPerTick;
                SpriteRenderer.color = color;
                if (color.a > 0f)
                    return;
            }

            Finish();
            Destroy(gameObject, 0.1f);
        }

        public static readonly Vector2 OriginalPivot = new Vector2(9f / 19f, 12f / 26f); // Symbol 1027: origin (9, 14) of 19x26

        private void LoadSprite()
        {
            Texture2D texture = Resources.Load<Texture2D>("Art/Weapons/VoodooDoll/1");
            if (texture != null && SpriteRenderer != null)
            {
                texture.filterMode = FilterMode.Point;
                SpriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                    OriginalPivot, MutinyPhysics.PixelsPerUnit);
            }
            else
            {
                Sprite sprite = Resources.Load<Sprite>("Art/Weapons/VoodooDoll/1");
                if (sprite != null && SpriteRenderer != null)
                    SpriteRenderer.sprite = sprite;
            }
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
                PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
        }
    }
}
