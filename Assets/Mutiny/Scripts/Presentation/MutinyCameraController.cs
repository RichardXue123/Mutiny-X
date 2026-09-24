using Mutiny.Levels;
using Mutiny.Simulation;
using Mutiny.Diagnostics;
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

        private MutinySpeechController m_Speech;

        private Camera m_Camera;
        private MutinyLevelRoot m_LevelRoot;
        private MutinyTeam m_PreviousTeam;
        private Transform m_TurnPanTarget;
        private MutinyWeapon m_TrackedWeapon;
        private Vector2 m_EdgeVelocityPixelsPerSecond;
        private bool m_AirDropCameraWasLocked;

        private void Awake()
        {
            m_Camera = GetComponent<Camera>();
            ApplyViewportLetterbox();
        }

        private void OnEnable()
        {
            ApplyViewportLetterbox();
        }

        private void Update()
        {
            ApplyViewportLetterbox();
        }

        public bool IsTrackingAirDrop => FindFallingChest() != null;
        public bool IsPanningToTurnTarget => m_TurnPanTarget != null;

        public void PanToTarget(Transform target)
        {
            m_TurnPanTarget = target;
        }

        public void PanToCharacter(MutinyCharacter character)
        {
            // PiecesOfEight.next sets track=false before assigning
            // panToCharacter. Keep that ordering explicit so a resolved coin can
            // never win FindActionTarget while the camera is returning.
            m_TrackedWeapon = null;
            m_TurnPanTarget = character != null ? character.transform : null;
        }

        public void TrackWeapon(MutinyWeapon weapon)
        {
            m_TrackedWeapon = weapon;
            if (weapon != null)
                m_TurnPanTarget = null;
        }

        public static void RequestTrackWeapon(MutinyWeapon weapon)
        {
            MutinyCameraController[] controllers = FindObjectsByType<MutinyCameraController>();
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i] != null)
                    controllers[i].TrackWeapon(weapon);
            }
        }

        public static void ReleaseWeaponTracking(MutinyWeapon weapon)
        {
            MutinyCameraController[] controllers = FindObjectsByType<MutinyCameraController>();
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i] != null && controllers[i].m_TrackedWeapon == weapon)
                    controllers[i].m_TrackedWeapon = null;
            }
        }

        public static void RequestPanToCharacter(MutinyCharacter character)
        {
            MutinyCameraController[] controllers = FindObjectsByType<MutinyCameraController>();
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i] != null)
                    controllers[i].PanToCharacter(character);
            }
        }

        private void LateUpdate()
        {
            ApplyViewportLetterbox();
            EnsureReferences();
            if (m_Camera == null || m_LevelRoot == null || TurnManager == null)
                return;

            if (TurnManager.CurrentTeam != m_PreviousTeam)
            {
                m_PreviousTeam = TurnManager.CurrentTeam;
                MutinyCharacter panCharacter = FindTurnPanCharacter(m_PreviousTeam);
                m_TurnPanTarget = panCharacter != null ? panCharacter.transform : null;
            }

            // TileSystem.advanceScrolling returns before every automatic camera
            // branch while Controller.dragging is set (human player dragging).
            if (PlayerInput != null && PlayerInput.IsAiming)
                return;

            if (m_Speech == null)
                m_Speech = TurnManager.GetComponent<MutinySpeechController>();
            // Original TileSystem checks the speech target ahead of chest, weapon,
            // turn and manual scrolling. The bubble position is captured once.
            if (m_Speech != null && m_Speech.HasActiveBubble)
            {
                m_EdgeVelocityPixelsPerSecond = Vector2.zero;
                PanTowards(m_Speech.BubbleWorldPosition, 50f, 0f);
                return;
            }

            // TileSystem.advanceScrolling returns before the falling-chest branch
            // while currentTeam.aiFinished is false. Unity evaluates synchronously,
            // but exposes that interval so camera priority remains source-authentic.
            MutinyAIController activeAi = TurnManager.CurrentTeam != null
                ? TurnManager.CurrentTeam.GetComponent<MutinyAIController>()
                : null;
            if (ShouldPauseForAiThinking(
                    TurnManager.CurrentTeam != null && TurnManager.CurrentTeam.IsAiControlled,
                    activeAi != null && activeAi.IsEvaluatingCandidates))
            {
                m_EdgeVelocityPixelsPerSecond = Vector2.zero;
                return;
            }

            MutinyTreasureChest fallingChest = FindFallingChest();
            if (fallingChest != null)
            {
                if (!m_AirDropCameraWasLocked)
                {
                    m_AirDropCameraWasLocked = true;
                    MutinyDebugLog.Info("Camera",
                        $"airdrop tracking started chest={fallingChest.name} timeTaken={fallingChest.TimeTaken} speed=50px/tick", this);
                }
                m_EdgeVelocityPixelsPerSecond = Vector2.zero;
                PanTowards(fallingChest.transform.position, 50f, 0f);
                return;
            }
            if (m_AirDropCameraWasLocked)
            {
                m_AirDropCameraWasLocked = false;
                MutinyDebugLog.Info("Camera",
                    "airdrop tracking released; normal action/edge camera controls resumed", this);
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

        internal static bool ShouldPauseForAiThinking(bool isAiControlled, bool isEvaluatingCandidates)
            => isAiControlled && isEvaluatingCandidates;

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

            // BoxWeapon.place calls Weapon.place (track=true) and then clears
            // track again in the same call. While a human waits for the next box,
            // the original therefore has no action target and falls straight
            // through to ordinary edge/key scrolling. Resolve this before any
            // stale explicit target from the preceding action can take priority.
            if (IsAwaitingPlayerBoxPlacement())
                return null;

            // Weapon.track in the Flash original belongs to the currently
            // equipped weapon, not to whichever fired object happens to be found
            // first in the scene. Pieces of Eight explicitly refreshes this on
            // every launch and clears it on every intermediate resolution.
            if (m_TrackedWeapon != null)
            {
                // VoodooDoll.advance permanently clears track when it hands the
                // camera to its target. Never let a stale explicit reference
                // override that source-authentic transition.
                if (m_TrackedWeapon is MutinyVoodooDoll trackedDoll &&
                    trackedDoll.IsTargetFocusRequested)
                {
                    m_TrackedWeapon = null;
                }
                else
                {
                    if (m_TrackedWeapon.IsFired && !m_TrackedWeapon.IsFinished &&
                        !(m_TrackedWeapon is MutinyMine trackedMine && trackedMine.IsStored))
                        return m_TrackedWeapon.transform;
                    m_TrackedWeapon = null;
                }
            }

            // During the gap between coins, track=false and panToCharacter owns
            // the camera. Once that pan completes there must be no action target,
            // even if the owner is still settling from unrelated motion.
            if (MutinyPiecesOfEight.HasPlayerAwaitingNextCoin(TurnManager.CurrentTeam))
                return null;

            MutinyWeapon[] weapons = FindObjectsByType<MutinyWeapon>();
            for (int i = 0; i < weapons.Length; i++)
            {
                // After ten ticks the original doll sets track=false for the rest
                // of its lifetime. panToCharacter owns the one-time move to the
                // victim; once that move clears, neither the fading doll nor the
                // moving victim is an automatic action target.
                if (weapons[i] is MutinyVoodooDoll doll && doll.IsTargetFocusRequested)
                    continue;
                // Cannon.update sets trackX/trackY from its child cannonball in the
                // Flash game. Keep the placed cannon stationary and follow that
                // separate projectile directly in Unity.
                if (weapons[i] is MutinyCannon cannon && cannon.CameraFocusTarget != null)
                    return cannon.CameraFocusTarget;
                // Mine.advanceMotion marks a settled mine finished in Flash.
                // It stays armed on the map, but it no longer owns the camera.
                if (weapons[i] is MutinyMine mine && mine.IsStored)
                    continue;
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

        internal Transform FindActionTargetForVerification() => FindActionTarget();
        internal MutinyWeapon TrackedWeaponForVerification => m_TrackedWeapon;
        internal void AdvanceCameraForVerification()
        {
            EnsureReferences();
            if (m_TurnPanTarget != null &&
                PanTowards(m_TurnPanTarget.position, OriginalTrackingPixelsPerTick))
                m_TurnPanTarget = null;
        }
        internal bool CanAcceptManualScrollingForVerification()
        {
            return m_TurnPanTarget == null && FindActionTarget() == null && CanUseManualScrolling();
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

        public bool IsPanningToCharacter(MutinyCharacter character)
        {
            return character != null && m_TurnPanTarget == character.transform;
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
            bool canScroll = CanUseManualScrolling() &&
                             TurnManager.CurrentTeam != null &&
                             !TurnManager.CurrentTeam.IsAiControlled &&
                             PlayerInput != null &&
                             !PlayerInput.IsActionMenuOpen &&
                             !PlayerInput.IsAiming;

            Vector2 direction = Vector2.zero;
            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            if (canScroll)
            {
                bool scrollLeft = keyboard != null && (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed);
                bool scrollRight = keyboard != null && (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed);
                bool scrollDown = keyboard != null && (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed);
                bool scrollUp = keyboard != null && (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed);

                // Mobile builds expose pointer state for touch-driven UI, but
                // there is no persistent hover cursor. Treating that pointer as
                // a desktop mouse makes its idle/default position (commonly
                // 0,0) continuously trigger the left and bottom edge zones.
                if (ShouldUseMouseEdgeScrolling(Application.isMobilePlatform, mouse != null))
                {
                    Vector2 position = mouse.position.ReadValue();
                    Rect pixelRect = m_Camera != null ? m_Camera.pixelRect : new Rect(0f, 0f, Screen.width, Screen.height);
                    float scale = Mathf.Min(Screen.width / OriginalHorizontalPixels, Screen.height / OriginalVerticalPixels);
                    float edgePixels = OriginalEdgePixels * scale;
                    CalculateMouseEdgeScroll(position, pixelRect, Screen.width, Screen.height, edgePixels,
                        ref scrollLeft, ref scrollRight, ref scrollDown, ref scrollUp);
                }

                if (scrollLeft)
                    direction.x = -1f;
                if (scrollRight)
                    direction.x = 1f;
                if (scrollDown)
                    direction.y = -1f;
                if (scrollUp)
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

        private bool CanUseManualScrolling()
        {
            if (TurnManager == null)
                return false;

            // The original camera has no blanket ActionExecuting lock. Once a
            // Pieces of Eight coin resolves, track=false; after panToCharacter
            // reaches the owner, ordinary edge/key scrolling is available while
            // the same weapon waits for the next throw.
            return TurnManager.CurrentPhase == TurnPhase.TurnActive ||
                   MutinyPiecesOfEight.HasPlayerAwaitingNextCoin(TurnManager.CurrentTeam) ||
                   MutinyVoodooDoll.HasPlayerCameraHandoff(TurnManager.CurrentTeam) ||
                   IsAwaitingPlayerBoxPlacement();
        }

        private bool IsAwaitingPlayerBoxPlacement()
        {
            return TurnManager != null && TurnManager.CurrentTeam != null &&
                   !TurnManager.CurrentTeam.IsAiControlled &&
                   PlayerInput != null && PlayerInput.IsAwaitingBoxPlacement;
        }

        internal bool CanUseManualScrollingForVerification() => CanUseManualScrolling();

        internal static bool ShouldUseMouseEdgeScrolling(bool isMobilePlatform, bool hasMouse)
        {
            return hasMouse && !isMobilePlatform;
        }

        internal static void CalculateMouseEdgeScroll(
            Vector2 mousePosition,
            Rect viewportPixelRect,
            float screenWidth,
            float screenHeight,
            float edgePixels,
            ref bool scrollLeft,
            ref bool scrollRight,
            ref bool scrollDown,
            ref bool scrollUp)
        {
            if (mousePosition.x < 0f || mousePosition.x > screenWidth ||
                mousePosition.y < 0f || mousePosition.y > screenHeight)
                return;

            scrollLeft |= mousePosition.x < viewportPixelRect.xMin + edgePixels;
            scrollRight |= mousePosition.x > viewportPixelRect.xMax - edgePixels;
            scrollDown |= mousePosition.y < viewportPixelRect.yMin + edgePixels;
            scrollUp |= mousePosition.y > viewportPixelRect.yMax - edgePixels;
        }

        public bool CanStartMobileTouchPan(bool allowWhileAiming = false)
        {
            EnsureReferences();
            return Application.isMobilePlatform &&
                   m_Camera != null && m_LevelRoot != null && TurnManager != null &&
                   TurnManager.CurrentTeam != null && !TurnManager.CurrentTeam.IsAiControlled &&
                   PlayerInput != null && !PlayerInput.IsActionMenuOpen &&
                   IsAimingCompatibleWithMobilePan(PlayerInput.IsAiming, allowWhileAiming) &&
                   m_TurnPanTarget == null && FindActionTarget() == null && CanUseManualScrolling();
        }

        internal static bool IsAimingCompatibleWithMobilePan(
            bool isAiming, bool allowWhileAiming)
        {
            return !isAiming || allowWhileAiming;
        }

        public bool PanByMobileTouchDelta(Vector2 screenDelta, bool allowWhileAiming = false)
        {
            if (!CanStartMobileTouchPan(allowWhileAiming))
                return false;

            float viewportWidth = m_Camera != null ? m_Camera.pixelRect.width : Screen.width;
            float viewportHeight = m_Camera != null ? m_Camera.pixelRect.height : Screen.height;

            Vector2 worldDelta = ScreenDeltaToWorldDelta(
                screenDelta, viewportWidth, viewportHeight,
                m_Camera.orthographicSize, m_Camera.aspect);
            m_EdgeVelocityPixelsPerSecond = Vector2.zero;
            Vector3 next = transform.position;
            next.x -= worldDelta.x;
            next.y -= worldDelta.y;
            SetClampedPosition(next);
            return true;
        }

        internal static Vector2 ScreenDeltaToWorldDelta(
            Vector2 screenDelta, float screenWidth, float screenHeight,
            float orthographicSize, float aspect)
        {
            if (screenWidth <= 0f || screenHeight <= 0f)
                return Vector2.zero;

            float visibleHeight = orthographicSize * 2f;
            float visibleWidth = visibleHeight * aspect;
            return new Vector2(
                screenDelta.x * visibleWidth / screenWidth,
                screenDelta.y * visibleHeight / screenHeight);
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

        public static Rect CalculateViewportRect(int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0 || screenHeight <= 0)
                return new Rect(0f, 0f, 1f, 1f);

            float targetAspect = OriginalHorizontalPixels / OriginalVerticalPixels;
            float windowAspect = (float)screenWidth / screenHeight;

            if (Mathf.Abs(windowAspect - targetAspect) < 0.001f)
                return new Rect(0f, 0f, 1f, 1f);

            if (windowAspect > targetAspect)
            {
                float insetW = targetAspect / windowAspect;
                float insetX = (1f - insetW) * 0.5f;
                return new Rect(insetX, 0f, insetW, 1f);
            }
            else
            {
                float insetH = windowAspect / targetAspect;
                float insetY = (1f - insetH) * 0.5f;
                return new Rect(0f, insetY, 1f, insetH);
            }
        }

        public void ApplyViewportLetterbox()
        {
            if (m_Camera == null)
                m_Camera = GetComponent<Camera>();

            if (m_Camera == null)
                return;

            Rect targetRect = CalculateViewportRect(Screen.width, Screen.height);
            if (m_Camera.rect != targetRect)
            {
                m_Camera.rect = targetRect;
            }
        }

        public static void DrawLetterboxBars()
        {
            float scale = Mathf.Min(Screen.width / OriginalHorizontalPixels, Screen.height / OriginalVerticalPixels);
            float canvasW = OriginalHorizontalPixels * scale;
            float canvasH = OriginalVerticalPixels * scale;
            float left = (Screen.width - canvasW) * 0.5f;
            float top = (Screen.height - canvasH) * 0.5f;

            Color oldColor = GUI.color;
            GUI.color = Color.black;

            if (left > 0.5f)
            {
                float rightX = left + canvasW;
                float rightW = Screen.width - rightX;
                GUI.DrawTexture(new Rect(0f, 0f, left, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(rightX, 0f, rightW, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
            }

            if (top > 0.5f)
            {
                float bottomY = top + canvasH;
                float bottomH = Screen.height - bottomY;
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, top), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(new Rect(0f, bottomY, Screen.width, bottomH), Texture2D.whiteTexture, ScaleMode.StretchToFill);
            }

            GUI.color = oldColor;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                GUI.depth = 1000;
                DrawLetterboxBars();
            }
        }
    }
}
