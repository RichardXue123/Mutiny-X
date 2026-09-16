using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyVoodooDoll : MutinyWeapon
    {
        [Header("Target Link")]
        public MutinyCharacter TargetCharacter;

        protected override void Awake()
        {
            WeaponType = "voodooDoll";
            Extent = 10f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/VoodooDoll/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public void BindTarget(MutinyCharacter target)
        {
            TargetCharacter = target;
        }

        protected override void Update()
        {
            if (IsFinished)
                return;

            base.Update();
            if (IsFinished)
                return;

            if (IsFired && PhysicsBody != null)
            {
                // Transfer velocity / damage to target
                if (TargetCharacter != null && TargetCharacter.IsAlive && TargetCharacter.PhysicsBody != null)
                {
                    TargetCharacter.PhysicsBody.State.VelocityX = PhysicsBody.State.VelocityX;
                    TargetCharacter.PhysicsBody.State.VelocityY = PhysicsBody.State.VelocityY;
                }

                if (PhysicsBody.IsAtRest || PhysicsBody.IsInWater)
                {
                    if (PhysicsBody.IsInWater && TargetCharacter != null && TargetCharacter.IsAlive)
                    {
                        TargetCharacter.Drown();
                    }
                    Finish();
                    Destroy(gameObject, 0.2f);
                }
            }
        }
    }
}

