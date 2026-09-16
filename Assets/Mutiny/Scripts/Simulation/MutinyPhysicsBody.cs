using System;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyPhysicsBody : MonoBehaviour
    {
        [Header("Physics State")]
        public PhysicsBodyState State;

        [Header("Settings")]
        public bool IsActive = true;
        public bool SyncTransform = true;
        public float WaterPixelY = float.PositiveInfinity;
        public bool IsInWater { get; private set; }

        public event Action OnFloorLanded;
        public event Action OnWallHit;
        public event Action OnRest;
        public event Action OnEnterWater;
        public event Action OnBeforeSimulationStep;
        public event Action OnAfterMotionStep;
        public event Action OnWaterMotionAdjusted;
        public event Action OnSimulationStep;

        private float m_TimeAccumulator;
        private string[,] m_CachedTerrain;
        private int m_GridWidth;
        private int m_GridHeight;

        public bool IsAtRest => State.IsAtRest;

        private void Awake()
        {
            EnsureInitializedState();
        }

        private void Start()
        {
            EnsureInitializedState();
            Vector2 px = MutinyPhysics.UnityToPixel(transform.position);
            State.X = px.x;
            State.Y = px.y;

            CacheLevelTerrain();
        }

        private void EnsureInitializedState()
        {
            // Levels can be baked into a scene in edit mode. Older scenes were saved
            // before PhysicsBodyState was serializable, leaving every numeric field at
            // zero in Play Mode (including gravity). Extents are never all zero for a
            // valid Solid, so they provide a safe legacy-state check.
            if (State.LeftExtent != 0f || State.RightExtent != 0f ||
                State.TopExtent != 0f || State.BottomExtent != 0f)
                return;

            Vector2 px = MutinyPhysics.UnityToPixel(transform.position);
            State = PhysicsBodyState.CreateDefault(px.x, px.y);
        }

        public void CacheLevelTerrain()
        {
            var levelRoot = FindAnyObjectByType<MutinyLevelRoot>();
            if (levelRoot != null)
            {
                WaterPixelY = -levelRoot.WaterLevelY * MutinyPhysics.PixelsPerUnit;

                // Find level controller if present to get terrain array
                var controller = FindAnyObjectByType<MutinyLevelController>();
                if (controller != null && controller.LevelXml != null)
                {
                    var levelData = MutinyLevelXmlParser.Parse(controller.LevelXml.text);
                    m_CachedTerrain = levelData.Terrain;
                    m_GridWidth = levelData.Width;
                    m_GridHeight = levelData.Height;
                }
            }
        }

        public void SetTerrain(string[,] terrain, int width, int height)
        {
            m_CachedTerrain = terrain;
            m_GridWidth = width;
            m_GridHeight = height;
        }

        private void Update()
        {
            if (!IsActive)
                return;

            if (m_CachedTerrain == null)
            {
                CacheLevelTerrain();
            }

            m_TimeAccumulator += Time.deltaTime;
            int maxSubSteps = 5; // Prevent spiral of death
            int steps = 0;

            while (m_TimeAccumulator >= MutinyPhysics.TimeStep && steps < maxSubSteps)
            {
                m_TimeAccumulator -= MutinyPhysics.TimeStep;
                steps++;
                AdvanceSimulationTick();
            }

            if (SyncTransform)
            {
                transform.position = MutinyPhysics.PixelToUnity(State.X, State.Y);
            }
        }

        public StepResult AdvanceSimulationTick()
        {
            // Flash weapon advanceMotion overrides rotate before Solid.advanceMotion.
            OnBeforeSimulationStep?.Invoke();

            StepResult result = MutinyPhysics.Step(ref State, m_CachedTerrain, m_GridWidth, m_GridHeight);

            // Solid.contact runs inside advanceMotion in Flash. Character floor
            // correction therefore precedes Character.advance's airborne rotation.
            if (result.HitFloor)
                OnFloorLanded?.Invoke();
            if (result.HitLeftWall || result.HitRightWall)
                OnWallHit?.Invoke();
            if (result.IsAtRest)
                OnRest?.Invoke();

            OnAfterMotionStep?.Invoke();
            EvaluateWaterState();
            OnSimulationStep?.Invoke();
            return result;
        }

        public void EvaluateWaterState()
        {
            if (float.IsInfinity(WaterPixelY))
            {
                var levelRoot = FindAnyObjectByType<MutinyLevelRoot>();
                if (levelRoot != null && levelRoot.WaterLevelY != 0f)
                {
                    WaterPixelY = -levelRoot.WaterLevelY * MutinyPhysics.PixelsPerUnit;
                }
            }

            if (State.Y <= WaterPixelY)
                return;

            if (!IsInWater)
            {
                IsInWater = true;
                OnEnterWater?.Invoke();
            }

            // Flash Character.advance: motion continues below the water after the
            // one-shot crossing event, with drag and a capped downward velocity.
            State.VelocityX *= 0.8f;
            State.VelocityY *= 0.8f;
            if (State.VelocityY > 1.5f)
            {
                State.VelocityY -= 4f;
                if (State.VelocityY < 1.5f)
                    State.VelocityY = 1.5f;
                OnWaterMotionAdjusted?.Invoke();
            }
        }

        public void AddVelocity(float vx, float vy)
        {
            State.VelocityX += vx;
            State.VelocityY += vy;
        }

        public void SetVelocity(float vx, float vy)
        {
            State.VelocityX = vx;
            State.VelocityY = vy;
        }

        public void Twang(Vector2 startPx, Vector2 dragPx, float maxForce = MutinyPhysics.DefaultTwangMaxForce)
        {
            Vector2 launchVel = MutinyPhysics.CalculateTwangVelocity(startPx, dragPx, maxForce);
            State.VelocityX = launchVel.x;
            State.VelocityY = launchVel.y;
        }
    }
}
