using System.Collections.Generic;
using Mutiny.Diagnostics;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyMine : MutinyWeapon
    {
        public const int IgnoreTicks = 10;
        public const int CountdownTicks = 60;
        public const float TriggerRadiusPixels = 60f;
        private static readonly int[] BeepTimes = { 0, 15, 30, 38, 45, 49, 53, 55, 57, 59 };

        private readonly List<Sprite> m_Frames = new();
        private int m_IgnoreTicks;
        private int m_Countdown;
        private int m_NextBeepIndex;
        private bool m_Stored;
        private bool m_Active;
        private bool m_Exploded;

        public bool IsStored => m_Stored;
        public bool IsActive => m_Active;
        public int CountdownRemaining => m_Countdown;
        public bool BlocksTurn => !m_Stored || m_Active;

        public static readonly Vector2 OriginalPivot = new Vector2(18f / 38f, 17f / 35f); // Symbol 1024: origin (18, 18) of 38x35

        protected override void Awake()
        {
            WeaponType = "mine";
            Extent = 14f;
            base.Awake();
            for (int i = 1; i <= 30; i++)
            {
                Texture2D texture = Resources.Load<Texture2D>($"Art/Weapons/Mine/{i}");
                if (texture != null)
                {
                    texture.filterMode = FilterMode.Point;
                    Sprite frame = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        OriginalPivot, MutinyPhysics.PixelsPerUnit);
                    m_Frames.Add(frame);
                }
                else
                {
                    Sprite frame = Resources.Load<Sprite>($"Art/Weapons/Mine/{i}");
                    if (frame != null) m_Frames.Add(frame);
                }
            }
            if (m_Frames.Count > 0) SpriteRenderer.sprite = m_Frames[0];
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            PhysicsBody.State.LeftExtent = PhysicsBody.State.RightExtent = 14f;
            PhysicsBody.State.TopExtent = PhysicsBody.State.BottomExtent = 14f;
            PhysicsBody.State.Friction = 1.5f;
            PhysicsBody.State.HitsBoxes = true;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalTick;
            m_IgnoreTicks = IgnoreTicks;
            m_Countdown = CountdownTicks;
            m_NextBeepIndex = 0;
            m_Stored = m_Active = m_Exploded = false;
        }

        protected override void Update()
        {
            // Mine.advance owns its lifecycle. Do not apply this project's generic
            // Weapon water timeout or eight-second fail-safe to a stored mine.
        }

        public void AdvanceOriginalTickForVerification() => AdvanceOriginalTick();

        private void AdvanceOriginalTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            if (!m_Active)
                CheckForProximity();

            PhysicsBodyState state = PhysicsBody.State;
            if (!m_Stored && state.VelocityX == 0f && Mathf.Abs(state.VelocityY) < 0.2f)
            {
                m_Stored = true;
                MutinyDebugLog.Info("Mine", "stored and armed", this);
            }

            if (!m_Active || m_Exploded)
                return;

            m_Countdown--;
            int elapsed = CountdownTicks - m_Countdown;
            while (m_NextBeepIndex < BeepTimes.Length && elapsed >= BeepTimes[m_NextBeepIndex])
            {
                Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("mine_beep");
                m_NextBeepIndex++;
            }

            if (m_Countdown <= 0)
                Explode();
        }

        private void CheckForProximity()
        {
            if (--m_IgnoreTicks > 0)
                return;

            MutinyTeam[] teams = FindObjectsByType<MutinyTeam>();
            for (int i = 0; i < teams.Length; i++)
            {
                List<MutinyCharacter> characters = teams[i].Characters;
                if (characters == null) continue;
                for (int j = 0; j < characters.Count; j++)
                {
                    MutinyCharacter character = characters[j];
                    if (character == null || !character.IsAlive || character.PhysicsBody == null)
                        continue;
                    PhysicsBodyState target = character.PhysicsBody.State;
                    bool stationary = target.VelocityX == 0f && Mathf.Abs(target.VelocityY) <= 0.2f;
                    if (stationary) continue;
                    float dx = target.X - PhysicsBody.State.X;
                    float dy = target.Y + target.BottomExtent - PhysicsBody.State.Y;
                    if (dx * dx + dy * dy < TriggerRadiusPixels * TriggerRadiusPixels)
                    {
                        m_Active = true;
                        MutinyDebugLog.Info("Mine", $"warn triggered by={character.name}", this);
                        return;
                    }
                }
            }
        }

        public void Explode()
        {
            if (m_Exploded || IsFinished) return;
            m_Exploded = true;
            Vector2 position = new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);
            MutinyExplosion.Spawn(position, 250f, 70f, Owner, playPopOnHit: false);
            Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("pop");
            Finish();
            if (SpriteRenderer != null) SpriteRenderer.enabled = false;
            Destroy(gameObject, 0.1f);
            MutinyDebugLog.Info("Mine", "exploded size=250 damage=70", this);
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null) PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
        }
    }
}
