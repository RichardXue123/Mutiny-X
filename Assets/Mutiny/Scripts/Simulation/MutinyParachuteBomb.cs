using UnityEngine;
using UnityEngine.InputSystem;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyParachuteBomb : MutinyWeapon
    {
        [Header("Parachute State")]
        public bool ChuteOpen = false;
        private int m_FramesFromFire;

        protected override void Awake()
        {
            WeaponType = "parachuteBomb";
            Extent = 11f;
            TwangMaxForce = 30f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/ParachuteBomb/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            ChuteOpen = false;
            m_FramesFromFire = 0;
            PhysicsBody.OnSimulationStep += AdvanceOriginalTick;
        }

        private void Update()
        {
            if (IsFinished)
                return;

            if (IsFired && PhysicsBody != null && PhysicsBody.IsInWater)
                Explode();
        }

        private void AdvanceOriginalTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            m_FramesFromFire++;
            if (PhysicsBody.State.VelocityY > 1f)
            {
                PhysicsBody.State.VelocityY -= 2f;
                if (PhysicsBody.State.VelocityY < 1f)
                    PhysicsBody.State.VelocityY = 1f;
            }
            PhysicsBody.State.VelocityX *= 0.95f;

            if (!ChuteOpen && PhysicsBody.State.VelocityY > -10f)
                ChuteOpen = true;

            if (IsHumanOwned() && Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                Camera camera = Camera.main;
                if (camera != null)
                {
                    Vector2 screen = Mouse.current.position.ReadValue();
                    Vector3 screenPoint = new Vector3(screen.x, screen.y, -camera.transform.position.z);
                    float mousePixelX = MutinyPhysics.UnityToPixel(camera.ScreenToWorldPoint(screenPoint)).x;
                    if (mousePixelX - PhysicsBody.State.X < 0f)
                        PhysicsBody.State.VelocityX += 0.2f;
                    else
                        PhysicsBody.State.VelocityX -= 0.2f;
                }

                if (m_FramesFromFire % 12 == 0)
                    Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("fan");
            }
        }

        private bool IsHumanOwned()
        {
            MutinyTurnManager manager = FindAnyObjectByType<MutinyTurnManager>();
            if (manager == null || Owner == null)
                return false;
            if (manager.Team1 != null && manager.Team1.Characters.Contains(Owner))
                return !manager.Team1.IsAiControlled;
            if (manager.Team2 != null && manager.Team2.Characters.Contains(Owner))
                return !manager.Team2.IsAiControlled;
            return false;
        }

        protected override void OnContact(CollisionSide side)
        {
            base.OnContact(side);

            if (IsFired && !IsFinished)
            {
                Explode();
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

            // Flash AS2 exact: new Explosion(x, y, 160, 50, owner)
            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyExplosion.Spawn(posPx, 160f, 50f, Owner);

            Destroy(gameObject, 0.1f);
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
                PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
        }
    }
}
