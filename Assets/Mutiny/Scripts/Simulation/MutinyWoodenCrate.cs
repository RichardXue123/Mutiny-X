using System.Collections.Generic;
using Mutiny.Diagnostics;
using UnityEngine;

namespace Mutiny.Simulation
{
    /// <summary>
    /// Flash BoxWeapon/WoodenCrate: one selection places three static Solid boxes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MutinyWoodenCrate : MutinyWeapon
    {
        public const float LeftExtentPixels = 16f;
        public const float RightExtentPixels = 15f;
        public const int OriginalPlacementCount = 3;

        private static readonly List<MutinyWoodenCrate> PlacedCrates = new();
        private readonly List<Sprite> m_Frames = new();
        private MutinyWoodenCrate m_NextBox;
        private MutinyWoodenCrate m_ParentBox;
        private int m_CreateMore = OriginalPlacementCount - 1;
        private bool m_IsRegistered;
        private bool m_IsExploding;
        private float m_AnimationAccumulator;
        private int m_AnimationFrame;

        public MutinyWoodenCrate NextBox => m_NextBox;
        public bool HasPlacedAny => IsFired;
        public bool HasPendingPlacement => !IsFinished && FindPendingBox() != null;
        public bool IsExploding => m_IsExploding;
        public int PlacedCount => CountPlaced(this);

        protected override void Awake()
        {
            WeaponType = "woodenCrate";
            Extent = LeftExtentPixels;
            base.Awake();
            LoadFrames();
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            // BoxWeapon constructor: left/top=16, right/bottom=15; Solid defaults
            // remain weight=1, bounce=.2, friction=.3.
            PhysicsBody.State.LeftExtent = LeftExtentPixels;
            PhysicsBody.State.TopExtent = LeftExtentPixels;
            PhysicsBody.State.RightExtent = RightExtentPixels;
            PhysicsBody.State.BottomExtent = RightExtentPixels;
            PhysicsBody.State.Weight = MutinyPhysics.Gravity;
            PhysicsBody.State.Bounce = 0.2f;
            PhysicsBody.State.Friction = 0.3f;
            PhysicsBody.State.HitsTiles = true;
            PhysicsBody.State.HitsBoxes = true;
            PhysicsBody.IsActive = false;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalTick;

            m_CreateMore = OriginalPlacementCount - 1;
            m_NextBox = null;
            m_IsRegistered = false;
            m_IsExploding = false;
            m_AnimationAccumulator = 0f;
            m_AnimationFrame = 0;
            SetVisible(true);
        }

        /// <summary>Routes a stage click to the first unplaced BoxWeapon child.</summary>
        public bool TryPlaceAt(Vector2 pixelPosition)
        {
            MutinyWoodenCrate pending = FindPendingBox();
            if (pending == null)
                return false;
            if (!pending.CanPlace(pixelPosition))
            {
                MutinyDebugLog.Info("WoodenCrate", $"placement rejected pos=({pixelPosition.x:F1},{pixelPosition.y:F1}) placed={PlacedCount}", this);
                return false;
            }

            bool firstPlacement = !IsFired;
            pending.Place(pixelPosition);
            if (firstPlacement)
                FindAnyObjectByType<MutinyTurnManager>()?.NotifyActionStarted();
            return true;
        }

        /// <summary>Original BoxWeapon.canPlace, retaining raw pixel coordinates.</summary>
        public bool CanPlace(Vector2 pixelPosition)
        {
            if (PhysicsBody == null || !PhysicsBody.TryGetTerrain(out string[,] terrain, out int width, out int height))
                return false;
            return CanPlace(pixelPosition, terrain, width, height);
        }

        public bool CanPlace(Vector2 pixelPosition, string[,] terrain, int gridWidth, int gridHeight)
        {
            if (terrain == null || gridWidth <= 0 || gridHeight <= 0)
                return false;

            float px = pixelPosition.x;
            float py = pixelPosition.y;
            int left = Mathf.FloorToInt((px - LeftExtentPixels) / 32f);
            int right = Mathf.FloorToInt((px + LeftExtentPixels) / 32f);
            int top = Mathf.FloorToInt((py - LeftExtentPixels) / 32f);
            int bottom = Mathf.FloorToInt((py + LeftExtentPixels) / 32f);
            for (int x = left; x <= right; x++)
                for (int y = top; y <= bottom; y++)
                    if (IsSolidTile(terrain, x, y, gridWidth, gridHeight))
                        return false;

            // The original retains the first support surface as a vertical boundary.
            float obstructionY = (bottom + 1) * 32f;
            for (int y = bottom; y < gridHeight; y++)
            {
                bool foundSolid = false;
                for (int x = left; x <= right; x++)
                {
                    if (IsSolidTile(terrain, x, y, gridWidth, gridHeight))
                    {
                        foundSolid = true;
                        break;
                    }
                }
                if (foundSolid)
                {
                    obstructionY = (y + 1) * 32f;
                    break;
                }
            }

            for (int i = PlacedCrates.Count - 1; i >= 0; i--)
            {
                MutinyWoodenCrate box = PlacedCrates[i];
                if (box == null || !box.m_IsRegistered || box.PhysicsBody == null)
                    continue;
                PhysicsBodyState other = box.PhysicsBody.State;
                if (other.X - other.LeftExtent <= px + LeftExtentPixels &&
                    other.X + other.RightExtent >= px - RightExtentPixels &&
                    other.Y + other.BottomExtent >= py - RightExtentPixels)
                {
                    if (other.Y - other.TopExtent < obstructionY)
                        obstructionY = other.Y - other.TopExtent;
                    if (other.Y - other.TopExtent <= py + LeftExtentPixels)
                        return false;
                }
            }

            MutinyTreasureChest[] chests = FindObjectsByType<MutinyTreasureChest>();
            for (int i = 0; i < chests.Length; i++)
            {
                if (chests[i] == null)
                    continue;
                Vector2 chestPx = new Vector2(chests[i].PixelX, chests[i].PixelY);
                if (chestPx.x - 16f <= px + LeftExtentPixels &&
                    chestPx.x + 16f >= px - RightExtentPixels &&
                    chestPx.y + 16f >= py - LeftExtentPixels)
                    return false;
            }

            MutinyCharacter[] characters = FindObjectsByType<MutinyCharacter>();
            for (int i = 0; i < characters.Length; i++)
            {
                MutinyCharacter character = characters[i];
                if (character == null || !character.IsAlive || character.PhysicsBody == null)
                    continue;
                PhysicsBodyState body = character.PhysicsBody.State;
                if (body.X - body.LeftExtent <= px + LeftExtentPixels &&
                    body.X + body.RightExtent >= px - RightExtentPixels &&
                    body.Y + body.BottomExtent >= py - RightExtentPixels &&
                    body.Y - body.TopExtent <= obstructionY)
                    return false;
            }
            return true;
        }

        /// <summary>Called by MutinyExplosion after its original AABB-radius check.</summary>
        public void Explode()
        {
            if (m_IsExploding)
                return;
            UnregisterPlacedCrate();
            m_IsExploding = true;
            if (PhysicsBody != null)
                PhysicsBody.IsActive = false;
            // `explode` is a timeline label. Frame 1 is the static crate; the later
            // frames are retained as the destruction sequence pending label timing.
            m_AnimationFrame = Mathf.Min(1, m_Frames.Count - 1);
            ApplyFrame();
            MutinyDebugLog.Info("WoodenCrate", $"exploded pos={PhysicsBody?.State.X:F1},{PhysicsBody?.State.Y:F1}", this);
        }

        public static List<PhysicsBoxObstacle> GetPhysicsObstacles(MutinyPhysicsBody requester)
        {
            var obstacles = new List<PhysicsBoxObstacle>(PlacedCrates.Count);
            for (int i = 0; i < PlacedCrates.Count; i++)
            {
                MutinyWoodenCrate crate = PlacedCrates[i];
                if (crate != null && crate.m_IsRegistered && crate.PhysicsBody != null)
                    obstacles.Add(new PhysicsBoxObstacle(crate.PhysicsBody, crate.PhysicsBody.State));
            }
            return obstacles;
        }

        protected override void Update()
        {
            if (m_IsExploding)
                AdvanceExplosionAnimation();
        }

        private void OnDestroy() => UnregisterPlacedCrate();

        private MutinyWoodenCrate FindPendingBox()
        {
            if (!IsFired)
                return this;
            return m_NextBox != null ? m_NextBox.FindPendingBox() : null;
        }

        private void Place(Vector2 pixelPosition)
        {
            transform.position = MutinyPhysics.PixelToUnity(pixelPosition.x, pixelPosition.y);
            PhysicsBody.State.X = pixelPosition.x;
            PhysicsBody.State.Y = pixelPosition.y;
            PhysicsBody.State.VelocityX = 0f;
            PhysicsBody.State.VelocityY = 0f;
            PhysicsBody.IsActive = true;
            IsFired = true;
            SetVisible(true);
            if (!m_IsRegistered)
            {
                PlacedCrates.Add(this);
                m_IsRegistered = true;
            }
            if (Owner != null)
            {
                Owner.CanShoot = false;
                Owner.CanThrow = false;
            }

            if (m_CreateMore > 0)
            {
                m_NextBox = MutinyWeaponFactory.SpawnWeapon("woodenCrate", Owner) as MutinyWoodenCrate;
                if (m_NextBox != null)
                {
                    m_NextBox.m_CreateMore = m_CreateMore - 1;
                    m_NextBox.m_ParentBox = this;
                    if (PhysicsBody.TryGetTerrain(out string[,] terrain, out int width, out int height))
                        m_NextBox.PhysicsBody.SetTerrain(terrain, width, height);
                    m_NextBox.SetVisible(false);
                }
            }
            else
            {
                Finish();
            }
            MutinyDebugLog.Info("WoodenCrate", $"placed index={PlacedCount}/{OriginalPlacementCount} pos=({pixelPosition.x:F1},{pixelPosition.y:F1}) next={(m_NextBox != null)}", this);
        }

        // BoxWeapon.advance performs this parent-completion propagation as part of
        // the original fixed advance loop, after advancing a child box.
        private void AdvanceOriginalTick()
        {
            if (m_NextBox != null && m_NextBox.IsFinished && !IsFinished)
            {
                Finish();
                MutinyDebugLog.Info("WoodenCrate", $"placement sequence finished count={PlacedCount}", this);
            }

            // An exploded parent stops its own Solid motion. Its live child still
            // drives the same BoxWeapon completion chain on the 25 Hz tick.
            if (m_ParentBox != null)
                m_ParentBox.AdvanceOriginalTick();
        }

        public static readonly Vector2 OriginalPivot = new Vector2(30.65f / 62f, 29.35f / 64f); // Symbol 965: origin (30.65, 34.65) of 62x64

        private void LoadFrames()
        {
            m_Frames.Clear();
            for (int i = 1; i <= 18; i++)
            {
                Texture2D texture = Resources.Load<Texture2D>($"Art/Weapons/WoodenCrate/{i}");
                if (texture != null)
                {
                    texture.filterMode = FilterMode.Point;
                    Sprite frame = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        OriginalPivot, MutinyPhysics.PixelsPerUnit);
                    m_Frames.Add(frame);
                }
                else
                {
                    Sprite frame = Resources.Load<Sprite>($"Art/Weapons/WoodenCrate/{i}");
                    if (frame != null)
                        m_Frames.Add(frame);
                }
            }
            ApplyFrame();
        }

        private void AdvanceExplosionAnimation()
        {
            m_AnimationAccumulator += Time.deltaTime;
            while (m_AnimationAccumulator >= MutinyPhysics.TimeStep)
            {
                m_AnimationAccumulator -= MutinyPhysics.TimeStep;
                m_AnimationFrame++;
                if (m_AnimationFrame >= m_Frames.Count)
                {
                    // BoxWeapon.explode only changes the movie-clip timeline; it
                    // deliberately leaves the chain alive so later crates can still
                    // be placed. The completed final box may now be destroyed.
                    SetVisible(false);
                    if (IsFinished)
                        Destroy(gameObject);
                    return;
                }
                ApplyFrame();
            }
        }

        private void ApplyFrame()
        {
            if (SpriteRenderer != null && m_AnimationFrame >= 0 && m_AnimationFrame < m_Frames.Count)
                SpriteRenderer.sprite = m_Frames[m_AnimationFrame];
        }

        private void SetVisible(bool visible)
        {
            if (SpriteRenderer != null)
                SpriteRenderer.enabled = visible;
        }

        private void UnregisterPlacedCrate()
        {
            if (!m_IsRegistered)
                return;
            PlacedCrates.Remove(this);
            m_IsRegistered = false;
        }

        private static int CountPlaced(MutinyWoodenCrate box)
        {
            if (box == null)
                return 0;
            return (box.IsFired ? 1 : 0) + CountPlaced(box.m_NextBox);
        }

        private static bool IsSolidTile(string[,] terrain, int col, int row, int width, int height)
        {
            if (col < 0 || col >= width || row < 0 || row >= height)
                return false;
            string tile = terrain[row, col];
            return !string.IsNullOrEmpty(tile) && tile != "-" &&
                   tile.IndexOf("ripple", System.StringComparison.OrdinalIgnoreCase) < 0;
        }
    }
}
