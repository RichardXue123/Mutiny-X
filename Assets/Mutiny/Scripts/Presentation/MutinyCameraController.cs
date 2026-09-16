using Mutiny.Levels;
using Mutiny.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mutiny.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class MutinyCameraController : MonoBehaviour
    {
        private const float OriginalHorizontalPixels = 550f;
        private const float OriginalVerticalPixels = 400f;
        private const float OriginalEdgePixels = 40f;
        private const float OriginalMaxScrollPixelsPerTick = 10f;
        private const float OriginalScrollAccelerationPixelsPerTick = 1f;
        private const float OriginalTrackingPixelsPerTick = 30f;
        private const float OriginalTrackingVerticalOffsetPixels = 50f;

        public MutinyTurnManager TurnManager;
        public MutinyPlayerInput PlayerInput;

        private Camera m_Camera;
        private MutinyLevelRoot m_LevelRoot;
        private MutinyTeam m_PreviousTeam;
        private Transform m_TurnPanTarget;
        private Vector2 m_EdgeVelocityPixelsPerSecond;

        private void Awake()
        {
            m_Camera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            EnsureReferences();
            if (m_Camera == null || m_LevelRoot == null || TurnManager == null)
                return;

            if (TurnManager.CurrentTeam != m_PreviousTeam)
            {
                m_PreviousTeam = TurnManager.CurrentTeam;
                MutinyCharacter panCharacter = FindTurnPanCharacter(m_PreviousTeam);
                m_TurnPanTarget = panCharacter != null ? panCharacter.transform : null;
            }

            MutinyTreasureChest fallingChest = FindFallingChest();
            if (fallingChest != null)
            {
                m_EdgeVelocityPixelsPerSecond = Vector2.zero;
                PanTowards(fallingChest.transform.position, 50f, 0f);
                return;
            }

            Transform actionTarget = FindActionTarget();
            if (actionTarget != null)
            {
                m_EdgeVelocityPixelsPerSecond = Vector2.zero;
                PanTowards(actionTarget.position, OriginalTrackingPixelsPerTick);
                return;
            }

            if (m_TurnPanTarget != null)
            {
                if (PanTowards(m_TurnPanTarget.position, OriginalTrackingPixelsPerTick))
                    m_TurnPanTarget = null;
                return;
            }

            AdvanceEdgeScrolling();
        }

        private void EnsureReferences()
        {
            if (m_Camera == null)
                m_Camera = GetComponent<Camera>();
            if (TurnManager == null)
                TurnManager = FindAnyObjectByType<MutinyTurnManager>();
            if (PlayerInput == null)
                PlayerInput = FindAnyObjectByType<MutinyPlayerInput>();
            if (m_LevelRoot == null)
                m_LevelRoot = FindAnyObjectByType<MutinyLevelRoot>();
        }

        private Transform FindActionTarget()
        {
            if (TurnManager.CurrentPhase != TurnPhase.ActionExecuting &&
                TurnManager.CurrentPhase != TurnPhase.Settling)
                return null;

            MutinyWeapon[] weapons = FindObjectsByType<MutinyWeapon>();
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] is MutinyVoodooDoll doll && doll.CameraFocusTarget != null)
                    return doll.CameraFocusTarget;
                if (weapons[i] != null && weapons[i].IsFired && !weapons[i].IsFinished)
                    return weapons[i].transform;
            }

            MutinyCharacter selected = TurnManager.CurrentTeam != null
                ? TurnManager.CurrentTeam.SelectedCharacter
                : null;
            if (selected != null && selected.IsAlive && selected.PhysicsBody != null &&
                !selected.PhysicsBody.IsAtRest)
                return selected.transform;

            return null;
        }

        public bool HasReachedVoodooTarget(MutinyCharacter target)
        {
            if (target == null || m_Camera == null || m_LevelRoot == null)
                return true;

            Vector3 desired = target.transform.position;
            desired.y += OriginalTrackingVerticalOffsetPixels / MutinyPhysics.PixelsPerUnit;
            desired.z = transform.position.z;
            desired = ClampPosition(desired);
            return Vector2.Distance(transform.position, desired) < 0.01f;
        }

        private MutinyCharacter FindTurnPanCharacter(MutinyTeam team)
        {
            if (team == null || team.Characters == null)
                return null;

            if (team.TotalTurnsTaken == 1)
            {
                MutinyCharacter captain = team.GetCaptain();
                if (captain != null)
                    return captain;
            }

            MutinyCharacter nearest = null;
            float nearestDistance = float.PositiveInfinity;
            Vector2 visualCentre = transform.position;
            for (int i = 0; i < team.Characters.Count; i++)
            {
                MutinyCharacter character = team.Characters[i];
                if (character == null || !character.IsAlive)
                    continue;

                float distance = Vector2.SqrMagnitude((Vector2)character.transform.position - visualCentre);
                if (distance < nearestDistance)
                {
                    nearest = character;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void AdvanceEdgeScrolling()
        {
            bool canScroll = TurnManager.CurrentPhase == TurnPhase.TurnActive &&
                             TurnManager.CurrentTeam != null &&
                             !TurnManager.CurrentTeam.IsAiControlled &&
                             PlayerInput != null &&
                             !PlayerInput.IsActionMenuOpen &&
                             !PlayerInput.IsAiming;

            Vector2 direction = Vector2.zero;
            Mouse mouse = Mouse.current;
            if (canScroll && mouse != null)
            {
                Vector2 position = mouse.position.ReadValue();
                float horizontalEdge = Screen.width * (OriginalEdgePixels / OriginalHorizontalPixels);
                float verticalEdge = Screen.height * (OriginalEdgePixels / OriginalVerticalPixels);

                if (position.x < horizontalEdge)
                    direction.x = -1f;
                else if (position.x > Screen.width - horizontalEdge)
                    direction.x = 1f;

                if (position.y < verticalEdge)
                    direction.y = -1f;
                else if (position.y > Screen.height - verticalEdge)
                    direction.y = 1f;
            }

            float maximumSpeed = OriginalMaxScrollPixelsPerTick / MutinyPhysics.TimeStep;
            float acceleration = OriginalScrollAccelerationPixelsPerTick /
                                 (MutinyPhysics.TimeStep * MutinyPhysics.TimeStep);
            m_EdgeVelocityPixelsPerSecond = Vector2.MoveTowards(
                m_EdgeVelocityPixelsPerSecond,
                direction * maximumSpeed,
                acceleration * Time.deltaTime);

            Vector2 movementPixels = m_EdgeVelocityPixelsPerSecond * Time.deltaTime;
            Vector3 positionWorld = transform.position;
            positionWorld.x += movementPixels.x / MutinyPhysics.PixelsPerUnit;
            positionWorld.y += movementPixels.y / MutinyPhysics.PixelsPerUnit;
            SetClampedPosition(positionWorld);
        }

        private MutinyTreasureChest FindFallingChest()
        {
            if (!MutinyTreasureChestManager.SystemEnabled)
                return null;

            MutinyTreasureChest[] chests = FindObjectsByType<MutinyTreasureChest>();
            for (int i = 0; i < chests.Length; i++)
            {
                if (chests[i] != null && chests[i].IsFalling && chests[i].TimeTaken < 100)
                    return chests[i];
            }
            return null;
        }

        private bool PanTowards(Vector3 targetWorld, float pixelsPerTick, float verticalOffsetPixels = OriginalTrackingVerticalOffsetPixels)
        {
            Vector3 desired = targetWorld;
            desired.y += verticalOffsetPixels / MutinyPhysics.PixelsPerUnit;
            desired.z = transform.position.z;
            desired = ClampPosition(desired);

            float speedWorldPerSecond = pixelsPerTick /
                                        (MutinyPhysics.PixelsPerUnit * MutinyPhysics.TimeStep);
            Vector3 next = Vector3.MoveTowards(transform.position, desired, speedWorldPerSecond * Time.deltaTime);
            SetClampedPosition(next);
            return Vector2.Distance(transform.position, desired) < 0.01f;
        }

        private void SetClampedPosition(Vector3 position)
        {
            transform.position = ClampPosition(position);
        }

        private Vector3 ClampPosition(Vector3 position)
        {
            float minX = 0f;
            float maxX = Mathf.Max(0f, m_LevelRoot.Width);
            float minY = -Mathf.Max(0f, m_LevelRoot.Height);
            float maxY = 0f;

            // Flash clamps cameraY to water.y - 320. With a 400 px viewport this
            // keeps the camera centre at least 120 px above the water line.
            float waterLimitedMinY = m_LevelRoot.WaterLevelY +
                                     120f / MutinyPhysics.PixelsPerUnit;
            minY = Mathf.Max(minY, waterLimitedMinY);

            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);
            position.z = transform.position.z;
            return position;
        }
    }
}
