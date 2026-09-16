using System.Collections.Generic;
using Mutiny.Diagnostics;
using Mutiny.Presentation;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinySeagull : MutinyWeapon
    {
        public const float OriginalFlightStartX = -300f;
        public const float OriginalFlightSpeed = 10f;
        public const float OriginalExitPadding = 275f;
        public const float OriginalShotXOffset = -10f;
        private const int OriginalFrameCount = 14;
        private const int FlyingFrameCount = 8;
        private const int ShotFrameStart = 10;
        private const int ShotFrameEnd = 14;

        private readonly List<Sprite> m_Frames = new List<Sprite>(OriginalFrameCount);
        private readonly List<MutinySeagullFire> m_Shots = new List<MutinySeagullFire>();
        private readonly Queue<float> m_AiShotXs = new Queue<float>();
        private int m_FlyingFrame;
        private int m_ShotFrame;

        public int ActiveShotCount => m_Shots.Count;
        public float FlightY => PhysicsBody == null ? 0f : PhysicsBody.State.Y;

        protected override void Awake()
        {
            WeaponType = "seagull";
            // Seagull does not override Solid's four default 10 px extents.
            Extent = 10f;
            IsDraggable = false;
            base.Awake();
            LoadSprites();
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            PhysicsBody.State.Weight = 0f;
            PhysicsBody.State.HitsTiles = false;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalTick;
            m_AiShotXs.Clear();
            m_Shots.Clear();
            m_FlyingFrame = 0;
            m_ShotFrame = 0;
        }

        public void PlaceAtFlightHeight(float flightY)
        {
            BeginFlight(flightY, null);
        }

        public void PlaceForAi(float flightY, IReadOnlyList<float> shotXs)
        {
            BeginFlight(flightY, shotXs);
        }

        // Retained for existing scene scripts. The original does not have a
        // single target-drop coordinate; every later click is a separate shot.
        public void CallAirstrike(float startX, float flightY, float unusedDropX)
        {
            BeginFlight(flightY, null, startX);
        }

        public static bool TryRequestPlayerShot(MutinyTeam inputTeam)
        {
            if (inputTeam == null || inputTeam.IsAiControlled)
                return false;

            MutinySeagull[] seagulls = Object.FindObjectsByType<MutinySeagull>();
            for (int i = 0; i < seagulls.Length; i++)
            {
                MutinySeagull seagull = seagulls[i];
                if (seagull == null || !seagull.IsFired || seagull.IsFinished || seagull.Owner == null ||
                    !inputTeam.Characters.Contains(seagull.Owner))
                    continue;

                return seagull.RequestShot("player-click");
            }
            return false;
        }

        public bool RequestShotForVerification()
        {
            return RequestShot("verification");
        }

        public void AdvanceOriginalTickForVerification()
        {
            AdvanceOriginalTick();
        }

        public void EndShot(MutinySeagullFire shot)
        {
            if (shot != null)
                m_Shots.Remove(shot);
            MutinyDebugLog.Info("Seagull", $"shot ended activeShots={m_Shots.Count}", this);
        }

        protected override void Update()
        {
            // The common projectile timeout/rest rules are not applicable. The
            // original keeps the bird alive until it clears levelWidth + 275 and
            // every seagullFire child has ended.
        }

        private void BeginFlight(float flightY, IReadOnlyList<float> shotXs, float startX = OriginalFlightStartX)
        {
            if (IsFired)
            {
                MutinyDebugLog.Warning("Seagull", "duplicate path placement ignored", this);
                return;
            }

            PhysicsBody.State.X = startX;
            PhysicsBody.State.Y = flightY;
            PhysicsBody.State.Weight = 0f;
            PhysicsBody.State.HitsTiles = false;
            PhysicsBody.IsActive = true;
            PhysicsBody.SetVelocity(OriginalFlightSpeed, 0f);
            transform.position = MutinyPhysics.PixelToUnity(startX, flightY);
            IsFired = true;
            IsFinished = false;
            m_AiShotXs.Clear();
            if (shotXs != null)
            {
                for (int i = 0; i < shotXs.Count; i++)
                    m_AiShotXs.Enqueue(shotXs[i]);
            }

            if (Owner != null)
            {
                Owner.CanShoot = false;
                Owner.CanThrow = false;
            }

            MutinyDebugLog.Info("Seagull",
                $"flight placed start=({startX:F0},{flightY:F0}) aiShots={m_AiShotXs.Count}", this);
        }

        private void AdvanceOriginalTick()
        {
            if (!IsFired || IsFinished || PhysicsBody == null)
                return;

            AdvanceAnimation();
            if (IsAiOwner() && m_AiShotXs.Count > 0 && PhysicsBody.State.X >= m_AiShotXs.Peek())
            {
                m_AiShotXs.Dequeue();
                RequestShot("ai-plan");
            }

            if (PhysicsBody.State.X > ResolveLevelWidthPixels() + OriginalExitPadding && m_Shots.Count == 0)
            {
                Finish();
                MutinyDebugLog.Info("Seagull", "flight exited after all shots ended", this);
                Destroy(gameObject, 0.1f);
            }
        }

        private bool RequestShot(string source)
        {
            if (!IsFired || IsFinished || PhysicsBody == null || PhysicsBody.State.X >= ResolveLevelWidthPixels())
            {
                MutinyDebugLog.Info("Seagull", $"shot rejected source={source}", this);
                return false;
            }

            Vector2 position = new Vector2(PhysicsBody.State.X + OriginalShotXOffset, PhysicsBody.State.Y);
            MutinySeagullFire shot = MutinySeagullFire.Spawn(this, position, PhysicsBody.State.VelocityX);
            m_Shots.Add(shot);
            m_ShotFrame = ShotFrameStart - 1;
            MutinyAudioManager.Instance?.PlaySFX($"poop{Random.Range(1, 4)}");
            MutinyDebugLog.Info("Seagull",
                $"shot created source={source} pos=({position.x:F1},{position.y:F1}) activeShots={m_Shots.Count}", this);
            return true;
        }

        private void AdvanceAnimation()
        {
            if (m_Frames.Count == 0 || SpriteRenderer == null)
                return;

            if (m_ShotFrame >= ShotFrameStart - 1)
            {
                m_ShotFrame++;
                if (m_ShotFrame > ShotFrameEnd)
                    m_ShotFrame = 0;
                else
                    SpriteRenderer.sprite = m_Frames[m_ShotFrame - 1];
                return;
            }

            m_FlyingFrame = (m_FlyingFrame + 1) % FlyingFrameCount;
            SpriteRenderer.sprite = m_Frames[m_FlyingFrame];
        }

        private float ResolveLevelWidthPixels()
        {
            if (PhysicsBody.TryGetTerrain(out _, out int width, out _))
                return width * MutinyPhysics.PixelsPerUnit;

            Mutiny.Levels.MutinyLevelRoot root = FindAnyObjectByType<Mutiny.Levels.MutinyLevelRoot>();
            return root != null && root.Width > 0 ? root.Width * MutinyPhysics.PixelsPerUnit : 1600f;
        }

        private bool IsAiOwner()
        {
            MutinyTeam[] teams = Object.FindObjectsByType<MutinyTeam>();
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null && teams[i].Characters.Contains(Owner))
                    return teams[i].IsAiControlled;
            }
            return false;
        }

        private void LoadSprites()
        {
            for (int frame = 1; frame <= OriginalFrameCount; frame++)
            {
                Sprite sprite = Resources.Load<Sprite>($"Art/Weapons/Seagull/{frame}");
                if (sprite != null)
                    m_Frames.Add(sprite);
            }
            if (m_Frames.Count > 0 && SpriteRenderer != null)
                SpriteRenderer.sprite = m_Frames[0];
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
                PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
        }
    }
}
