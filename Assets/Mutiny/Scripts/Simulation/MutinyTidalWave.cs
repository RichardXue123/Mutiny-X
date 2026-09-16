using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyTidalWave : MutinyWeapon
    {
        [Header("Wave Motion")]
        public float WaveSpeedPx = 20f;

        protected override void Awake()
        {
            WeaponType = "tidalWave";
            Extent = 24f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/TidalWave/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            PhysicsBody.OnSimulationStep += AdvanceOriginalTick;
        }

        public void StartWave(float startX, float waterPixelY, float direction = 1f)
        {
            PhysicsBody.State.X = -550f;
            PhysicsBody.State.Y = waterPixelY;
            PhysicsBody.State.Weight = 0f;
            PhysicsBody.State.HitsTiles = false;
            PhysicsBody.SetVelocity(WaveSpeedPx * direction, 0f);
            transform.position = MutinyPhysics.PixelToUnity(-550f, waterPixelY);

            IsFired = true;
            IsFinished = false;
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

            if (IsFired && PhysicsBody != null)
            {
                Mutiny.Levels.MutinyLevelRoot root = FindAnyObjectByType<Mutiny.Levels.MutinyLevelRoot>();
                float finishX = (root != null ? root.Width * 32f : 1600f) + 550f;
                if (PhysicsBody.State.X > finishX)
                {
                    Finish();
                    Destroy(gameObject, 0.1f);
                }
            }
        }

        private void AdvanceOriginalTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            var chars = FindObjectsByType<MutinyCharacter>();
            for (int i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                if (ch == null || !ch.IsAlive)
                    continue;

                if (ch.PhysicsBody == null)
                    continue;
                PhysicsBodyState state = ch.PhysicsBody.State;
                if (state.Y >= PhysicsBody.WaterPixelY - 300f &&
                    state.X >= PhysicsBody.State.X - 150f &&
                    state.X <= PhysicsBody.State.X + 150f)
                    ch.TakeDamage(5f);
            }
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
                PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
        }
    }
}
