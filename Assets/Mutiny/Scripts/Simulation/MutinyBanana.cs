using UnityEngine;
using UnityEngine.InputSystem;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyBanana : MutinyWeapon
    {
        private int m_FramesSinceFire;

        protected override void Awake()
        {
            WeaponType = "banana";
            Extent = 7f;
            TwangMaxForce = 30f;
            base.Awake();
            LoadSprite();
        }

        private void LoadSprite()
        {
            Sprite sp = Resources.Load<Sprite>("Art/Weapons/Banana/1");
            if (sp != null && SpriteRenderer != null)
            {
                SpriteRenderer.sprite = sp;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);

            // Flash AS2 exact: bounce = 0.8, friction = 0.5
            PhysicsBody.State.Bounce = 0.8f;
            PhysicsBody.State.Friction = 0.5f;
            m_FramesSinceFire = 0;
        }

        private void Update()
        {
            if (IsFinished)
                return;

            if (IsFired && PhysicsBody != null)
            {
                m_FramesSinceFire++;

                // Spin during flight
                transform.Rotate(0f, 0f, -PhysicsBody.State.VelocityX * 2f);

                bool shouldExplode = PhysicsBody.State.VelocityX == 0f &&
                                     Mathf.Abs(PhysicsBody.State.VelocityY) < 0.5f;

                MutinyTurnManager turnManager = FindAnyObjectByType<MutinyTurnManager>();
                bool humanOwned = turnManager != null && turnManager.Team1 != null &&
                                  turnManager.Team1.Characters.Contains(Owner) &&
                                  !turnManager.Team1.IsAiControlled;

                // The original clears the launch mouse press in fire(), then lets the
                // player detonate the moving banana with the next left click.
                if (!shouldExplode && humanOwned && m_FramesSinceFire > 1 &&
                    Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    shouldExplode = true;
                }

                // Original AI detonates once a character is within 20 px.
                if (!shouldExplode && !humanOwned)
                {
                    MutinyCharacter[] characters = FindObjectsByType<MutinyCharacter>();
                    Vector2 bananaPosition = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
                    for (int i = 0; i < characters.Length; i++)
                    {
                        MutinyCharacter character = characters[i];
                        if (character == null || !character.IsAlive)
                            continue;

                        Vector2 characterPosition = new Vector2(
                            character.PhysicsBody.State.X,
                            character.PhysicsBody.State.Y);
                        if ((characterPosition - bananaPosition).sqrMagnitude < 400f)
                        {
                            shouldExplode = true;
                            break;
                        }
                    }
                }

                if (shouldExplode)
                {
                    Explode();
                }
            }
        }

        protected override void OnContact(CollisionSide side)
        {
            base.OnContact(side);

            if (!IsFired || IsFinished)
                return;

            Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("banana_bounce");
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

            // Flash AS2 exact formula: new Explosion(x, y, 160, 80, owner)
            Vector2 posPx = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyExplosion.Spawn(posPx, 160f, 80f, Owner);

            Destroy(gameObject, 0.1f);
        }
    }
}
