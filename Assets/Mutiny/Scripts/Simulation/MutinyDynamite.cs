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

        public static readonly Vector2 OriginalPivot = new Vector2(5f / 22f, 12f / 27f); // Symbol 881: origin (5, 15) of 22x27

        private void LoadSprites()
        {
            var lit = new List<Sprite>();
            for (int i = 1; i <= 5; i++)
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

            Texture2D unlitTexture = Resources.Load<Texture2D>("Art/Weapons/Dynamite/6");
            if (unlitTexture != null)
            {
                unlitTexture.filterMode = FilterMode.Point;
                UnlitFrame = Sprite.Create(unlitTexture, new Rect(0f, 0f, unlitTexture.width, unlitTexture.height),
                    OriginalPivot, MutinyPhysics.PixelsPerUnit);
            }
            else
            {
                UnlitFrame = Resources.Load<Sprite>("Art/Weapons/Dynamite/6");
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
            // Flash Dynamite: friction = 1.7, extents = 11
            PhysicsBody.State.Friction = 1.7f;
            PhysicsBody.State.LeftExtent = 11f;
            PhysicsBody.State.RightExtent = 11f;
            PhysicsBody.State.TopExtent = 11f;
            PhysicsBody.State.BottomExtent = 11f;
            PhysicsBody.State.Bounce = 0.2f;
            // Dynamite.as constructor: hitsBoxes = true.
            PhysicsBody.State.HitsBoxes = true;
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

        protected override void Update()
        {
            if (IsFinished)
                return;

            base.Update();

            if (IsFired && PhysicsBody != null)
            {
                // Water extinguishes fuse (Flash: if(y > water.y) mc.gotoAndStop("unlit"))
                if (PhysicsBody.IsInWater)
                {
                    if (IsLit)
                    {
                        IsLit = false;
                        if (UnlitFrame != null && SpriteRenderer != null)
                        {
                            SpriteRenderer.sprite = UnlitFrame;
                        }
                    }

                    // Once submerged in water, dynamite is a dud and expires after brief sinking
                    float waterY = PhysicsBody.WaterPixelY;
                    if (m_WaterTimer >= 0.35f || (!float.IsInfinity(waterY) && PhysicsBody.State.Y > waterY + 16f))
                    {
                        Finish();
                        Destroy(gameObject, 0.4f);
                        return;
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
