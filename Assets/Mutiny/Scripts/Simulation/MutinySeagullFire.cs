using Mutiny.Diagnostics;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class MutinySeagullFire : MonoBehaviour
    {
        public const float OriginalExtent = 10f;
        public const float OriginalWeight = 1f;
        public const float ExplosionSize = 50f;
        public const float ExplosionDamage = 50f;

        private MutinySeagull m_Parent;
        private MutinyCharacter m_Owner;
        private MutinyPhysicsBody m_PhysicsBody;
        private bool m_Ended;

        public MutinyPhysicsBody PhysicsBody => m_PhysicsBody;

        public static MutinySeagullFire Spawn(MutinySeagull parent, Vector2 position, float velocityX)
        {
            GameObject shotObject = new GameObject("SeagullFire");
            shotObject.transform.position = MutinyPhysics.PixelToUnity(position.x, position.y);
            MutinySeagullFire shot = shotObject.AddComponent<MutinySeagullFire>();
            shot.Initialize(parent, position, velocityX);
            return shot;
        }

        public static readonly Vector2 OriginalPivot = new Vector2(7f / 13f, 8f / 20f); // Symbol 971: origin (7, 12) of 13x20

        private void Awake()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sortingOrder = Mutiny.Levels.MutinyLevelBuilder.SeagullSortingOrder - 1;
            Texture2D texture = Resources.Load<Texture2D>("Art/Weapons/SeagullFire/1");
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                    OriginalPivot, MutinyPhysics.PixelsPerUnit);
            }
            else
            {
                renderer.sprite = Resources.Load<Sprite>("Art/Weapons/SeagullFire/1");
            }
            m_PhysicsBody = gameObject.AddComponent<MutinyPhysicsBody>();
            m_PhysicsBody.ApplyWaterMotion = false;
            m_PhysicsBody.OnFloorLanded += ExplodeOnContact;
            m_PhysicsBody.OnWallHit += ExplodeOnContact;
            m_PhysicsBody.OnEnterWater += EndInWater;
        }

        private void Initialize(MutinySeagull parent, Vector2 position, float velocityX)
        {
            m_Parent = parent;
            m_Owner = parent != null ? parent.Owner : null;
            if (parent != null && parent.SpriteRenderer != null)
            {
                // Seagull.as shows the shot, then hides and re-shows the bird in
                // the same Character layer, placing the bird above its shot.
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                renderer.sortingLayerID = parent.SpriteRenderer.sortingLayerID;
                renderer.sortingOrder = parent.SpriteRenderer.sortingOrder - 1;
            }
            m_PhysicsBody.State = PhysicsBodyState.CreateDefault(position.x, position.y);
            m_PhysicsBody.State.LeftExtent = OriginalExtent;
            m_PhysicsBody.State.RightExtent = OriginalExtent;
            m_PhysicsBody.State.TopExtent = OriginalExtent;
            m_PhysicsBody.State.BottomExtent = OriginalExtent;
            m_PhysicsBody.State.Weight = OriginalWeight;
            m_PhysicsBody.State.HitsTiles = true;
            // Seagull.as assigns hitsBoxes=true to every spawned fire projectile.
            m_PhysicsBody.State.HitsBoxes = true;
            m_PhysicsBody.SetVelocity(velocityX, 0f);
            // In Flash these children are advanced by Seagull.advance, not by a
            // separate global update. Parent ownership guarantees one step per
            // bird tick and prevents Unity component order from adding a delay or
            // a second step in the spawn frame.
            m_PhysicsBody.IsActive = false;

            if (parent != null && parent.PhysicsBody != null)
            {
                parent.PhysicsBody.TryGetTerrain(out string[,] terrain, out int width, out int height);
                m_PhysicsBody.SetTerrain(terrain, width, height);
                m_PhysicsBody.WaterPixelY = parent.PhysicsBody.WaterPixelY;
            }
        }

        public void AdvanceOriginalTick()
        {
            if (m_Ended || m_PhysicsBody == null)
                return;

            m_PhysicsBody.AdvanceSimulationTick();
            if (!m_Ended && m_PhysicsBody.SyncTransform)
            {
                transform.position = MutinyPhysics.PixelToUnity(
                    m_PhysicsBody.State.X, m_PhysicsBody.State.Y);
            }
        }

        private void ExplodeOnContact()
        {
            if (m_Ended)
                return;

            Vector2 position = new Vector2(m_PhysicsBody.State.X, m_PhysicsBody.State.Y);
            // Seagull.as creates Explosion directly and contains no pop call. The
            // dedicated poop sound was played when this projectile was spawned.
            MutinyExplosion.Spawn(position, ExplosionSize, ExplosionDamage, m_Owner, playPopOnHit: false);
            MutinyDebugLog.Info("Seagull", $"seagullFire impact pos=({position.x:F1},{position.y:F1})", this);
            End();
        }

        private void EndInWater()
        {
            // The shot's custom advance destroys it below water without an explosion.
            MutinyDebugLog.Info("Seagull", "seagullFire entered water without explosion", this);
            End();
        }

        private void Update()
        {
            if (m_Ended || m_PhysicsBody == null)
                return;

            if (!float.IsInfinity(m_PhysicsBody.WaterPixelY) && m_PhysicsBody.State.Y > m_PhysicsBody.WaterPixelY)
                EndInWater();
        }

        private void End()
        {
            if (m_Ended)
                return;

            m_Ended = true;
            m_Parent?.EndShot(this);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (m_PhysicsBody == null)
                return;
            m_PhysicsBody.OnFloorLanded -= ExplodeOnContact;
            m_PhysicsBody.OnWallHit -= ExplodeOnContact;
            m_PhysicsBody.OnEnterWater -= EndInWater;
        }
    }
}
