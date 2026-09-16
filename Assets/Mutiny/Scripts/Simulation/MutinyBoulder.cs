using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyBoulder : MutinyWeapon
    {
        private float m_Visibility = 2f;

        protected override void Awake()
        {
            WeaponType = "boulder";
            Extent = 31f; // Large 62px diameter
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/Boulder/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);

            // Flash AS2 exact: weight = 1.5, friction = 0.25
            PhysicsBody.State.Weight = 1.5f;
            PhysicsBody.State.Friction = 0.25f;
            PhysicsBody.OnSimulationStep += AdvanceOriginalTick;
            m_Visibility = 2f;
        }

        public override void Fire(Vector2 velocityPx)
        {
            base.Fire(velocityPx);
            // Boulder.release calls Weapon.release first, then halves both axes.
            PhysicsBody.State.VelocityX *= 0.5f;
            PhysicsBody.State.VelocityY *= 0.5f;
        }

        protected override void Update()
        {
            if (IsFinished)
                return;

            base.Update();
        }

        private void AdvanceOriginalTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            // Water invalidation: boulder quickly sinks and fades away in water
            if (PhysicsBody.IsInWater)
            {
                m_Visibility -= 0.08f;
                if (SpriteRenderer != null)
                {
                    Color color = SpriteRenderer.color;
                    color.a = Mathf.Clamp01(m_Visibility);
                    SpriteRenderer.color = color;
                }

                float waterY = PhysicsBody.WaterPixelY;
                if (m_Visibility <= 0f || (!float.IsInfinity(waterY) && PhysicsBody.State.Y > waterY + 28f))
                {
                    Finish();
                    Destroy(gameObject, 0.2f);
                    return;
                }
            }
            else if (PhysicsBody.State.VelocityX == 0f && Mathf.Abs(PhysicsBody.State.VelocityY) < 0.5f)
            {
                m_Visibility -= 0.1f;
                if (m_Visibility < 1f && SpriteRenderer != null)
                {
                    Color color = SpriteRenderer.color;
                    color.a = Mathf.Clamp01(m_Visibility);
                    SpriteRenderer.color = color;
                }
                if (m_Visibility < 0f)
                {
                    Finish();
                    Destroy(gameObject, 0.2f);
                    return;
                }
            }

            var characters = FindObjectsByType<MutinyCharacter>();
            for (int i = 0; i < characters.Length; i++)
            {
                var ch = characters[i];
                if (ch == null || !ch.IsAlive || ch == Owner || ch.PhysicsBody == null)
                    continue;

                PhysicsBodyState characterState = ch.PhysicsBody.State;
                if (characterState.Y - characterState.TopExtent > PhysicsBody.State.Y + 32f ||
                    characterState.Y + characterState.BottomExtent < PhysicsBody.State.Y - 32f ||
                    Mathf.Abs(characterState.X - PhysicsBody.State.X) > 32f)
                    continue;

                if (characterState.X > PhysicsBody.State.X)
                {
                    characterState.X = PhysicsBody.State.X + 32f;
                    if (PhysicsBody.State.VelocityX > 0f)
                        characterState.VelocityX += PhysicsBody.State.VelocityX;
                }
                else
                {
                    characterState.X = PhysicsBody.State.X - 32f;
                    if (PhysicsBody.State.VelocityX < 0f)
                        characterState.VelocityX += PhysicsBody.State.VelocityX;
                }

                ch.PhysicsBody.State = characterState;
                ch.TakeDamage(Mathf.Abs(PhysicsBody.State.VelocityX) * 1.5f);
            }
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
                PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
        }
    }
}
