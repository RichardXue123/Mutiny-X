using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyDynamite : MutinyWeapon
    {
        public const int OriginalLitVisibleFrameCount = 4;
        public const int OriginalUnlitFrame = 6;

        [Header("Animation")]
        public Sprite[] LitFrames;
        public Sprite UnlitFrame;

        [Header("Dynamite State")]
        public bool IsLit = true;
        public int CurrentAnimationFrame => IsLit ? m_CurrentLitFrame + 1 : OriginalUnlitFrame;

        private int m_CurrentLitFrame = 0;

        protected override void Awake()
        {
            WeaponType = "dynamite";
            Extent = 11f;
            base.Awake();
            LoadSprites();
        }

        public static readonly Vector2 OriginalPivot = new Vector2(5f / 22f, 12f / 27f); // Symbol 881: origin (5, 15) of 22x27

        private void LoadSprites()
        {
            var lit = new List<Sprite>();
            // Frame 5 only runs gotoAndPlay("lit"). Flash executes that action
            // before presenting another image, so the visible loop is 1..4.
            for (int i = 1; i <= OriginalLitVisibleFrameCount; i++)
            {
                Texture2D texture = Resources.Load<Texture2D>($"Art/Weapons/Dynamite/{i}");
                if (texture != null)
                {
                    texture.filterMode = FilterMode.Point;
                    Sprite sp = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        OriginalPivot, MutinyPhysics.PixelsPerUnit);
                    lit.Add(sp);
                }
                else
                {
                    Sprite sp = Resources.Load<Sprite>($"Art/Weapons/Dynamite/{i}");
                    if (sp != null)
                        lit.Add(sp);
                }
            }
            LitFrames = lit.ToArray();

            Texture2D unlitTexture = Resources.Load<Texture2D>($"Art/Weapons/Dynamite/{OriginalUnlitFrame}");
            if (unlitTexture != null)
            {
                unlitTexture.filterMode = FilterMode.Point;
                UnlitFrame = Sprite.Create(unlitTexture, new Rect(0f, 0f, unlitTexture.width, unlitTexture.height),
                    OriginalPivot, MutinyPhysics.PixelsPerUnit);
            }
            else
            {
                UnlitFrame = Resources.Load<Sprite>($"Art/Weapons/Dynamite/{OriginalUnlitFrame}");
            }

            if (LitFrames.Length > 0 && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = LitFrames[0];
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);

            IsLit = true;
            m_CurrentLitFrame = 0;
            if (LitFrames != null && LitFrames.Length > 0 && SpriteRenderer != null)
                SpriteRenderer.sprite = LitFrames[0];
            // Flash Dynamite: friction = 1.7, extents = 11
            PhysicsBody.State.Friction = 1.7f;
            PhysicsBody.State.LeftExtent = 11f;
            PhysicsBody.State.RightExtent = 11f;
            PhysicsBody.State.TopExtent = 11f;
            PhysicsBody.State.BottomExtent = 11f;
            PhysicsBody.State.Bounce = 0.2f;
            // Dynamite.as constructor: hitsBoxes = true.
            PhysicsBody.State.HitsBoxes = true;
            PhysicsBody.OnSimulationStep -= EmitOriginalSmokeTrail;
            PhysicsBody.OnSimulationStep += EmitOriginalSmokeTrail;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalPresentationTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalPresentationTick;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalDetonationTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalDetonationTick;
            PhysicsBody.OnSimulationStep -= AdvanceInheritedFinishTick;
            PhysicsBody.OnSimulationStep += AdvanceInheritedFinishTick;
        }

        protected override void OnWaterSubmerged()
        {
            base.OnWaterSubmerged();
            IsLit = false;
            if (UnlitFrame != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = UnlitFrame;
            }
        }

        private void AdvanceOriginalDetonationTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            // Dynamite.advanceMotion detonates at rest before Weapon.advance's
            // inherited finish check. The unlit frame is visual only, not a dud.
            PhysicsBodyState state = PhysicsBody.State;
            if (state.VelocityX == 0f && Mathf.Abs(state.VelocityY) < 0.2f)
                Explode();
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

        private void EmitOriginalSmokeTrail()
        {
            // Dynamite.advance has no fired/isLit condition around the trail.
            // The unlit water frame still follows the same !finished gate.
            if (!IsFinished && PhysicsBody != null)
                MutinyRumBottleSmokeTrail.Spawn(PhysicsBody.CurrentStepStartPositionPixels);
        }

        private void AdvanceOriginalPresentationTick()
        {
            // The symbol starts playing at construction time, before Weapon.fire.
            // Drive it from the same 25 Hz production tick used by the original
            // game so equipped/ready dynamite keeps its burning-fuse animation.
            if (IsFinished || !IsLit || LitFrames == null || LitFrames.Length == 0 || SpriteRenderer == null)
                return;

            m_CurrentLitFrame = (m_CurrentLitFrame + 1) % LitFrames.Length;
            SpriteRenderer.sprite = LitFrames[m_CurrentLitFrame];
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
            {
                PhysicsBody.OnSimulationStep -= EmitOriginalSmokeTrail;
                PhysicsBody.OnSimulationStep -= AdvanceOriginalPresentationTick;
                PhysicsBody.OnSimulationStep -= AdvanceOriginalDetonationTick;
                PhysicsBody.OnSimulationStep -= AdvanceInheritedFinishTick;
            }
        }
    }
}
