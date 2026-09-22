using Mutiny.Diagnostics;
using Mutiny.Levels;
using Mutiny.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mutiny.Presentation
{
    public enum MutinyPlayerInteractionState
    {
        CharacterSelection,
        ActionMenu,
        WeaponReady,
        Aiming
    }

    [DisallowMultipleComponent]
    public sealed class MutinyPlayerInput : MonoBehaviour
    {
        [Header("References")]
        public MutinyTurnManager TurnManager;
        public MutinyTrajectoryRenderer TrajectoryRenderer;
        public Camera GameCamera;

        [Header("Original Mouse Settings (pixels)")]
        public float CharacterSelectionRadiusPixels = 30f;
        public float DragSelectionRadiusPixels = 30f;
        public float MinDragDistancePixels = 5f;

        public string ActiveWeapon { get; private set; }
        public MutinyPlayerInteractionState InteractionState { get; private set; } =
            MutinyPlayerInteractionState.CharacterSelection;
        public bool IsActionMenuOpen => InteractionState == MutinyPlayerInteractionState.ActionMenu;
        public bool IsAiming => InteractionState == MutinyPlayerInteractionState.Aiming ||
                                (m_ArmedCannon != null &&
                                 (m_ArmedCannon.IsDraggingBody || m_ArmedCannon.IsDraggingPin));
        public bool IsWeaponReady => InteractionState == MutinyPlayerInteractionState.WeaponReady &&
                                     EquippedWeapon != null;
        public bool IsAwaitingBoxPlacement => HasPendingBoxPlacement();
        public MutinyWeapon EquippedWeapon => m_EquippedWeapon != null && !m_EquippedWeapon.IsFired
            ? m_EquippedWeapon
            : null;
        public MutinyVoodooDoll ArmedVoodooDoll => m_ArmedVoodooDoll != null && !m_ArmedVoodooDoll.IsFired
            ? m_ArmedVoodooDoll
            : null;
        public MutinyWoodenCrate ArmedWoodenCrate => m_ArmedWoodenCrate != null &&
                                                       m_ArmedWoodenCrate.HasPendingPlacement
            ? m_ArmedWoodenCrate
            : null;
        public MutinyGunpowderBarrel ArmedGunpowderBarrel => m_ArmedGunpowderBarrel != null &&
                                                               m_ArmedGunpowderBarrel.HasPendingPlacement
            ? m_ArmedGunpowderBarrel
            : null;
        public MutinyAnchor ArmedAnchor => m_ArmedAnchor != null && !m_ArmedAnchor.IsFired
            ? m_ArmedAnchor
            : null;
        public MutinyCannon ArmedCannon => m_ArmedCannon != null && !m_ArmedCannon.IsFinished
            ? m_ArmedCannon
            : null;
        public MutinyPiecesOfEight ArmedPiecesOfEight => m_ArmedPiecesOfEight != null &&
                                                          !m_ArmedPiecesOfEight.IsFinished
            ? m_ArmedPiecesOfEight
            : null;

        // Controller.dragging == character in the Flash game.  Aiming a weapon
        // drags its equipped weapon instead, so it deliberately does not hide the
        // character's triangle or health bar.
        public bool IsCharacterThrowDragInProgress(MutinyCharacter character)
        {
            return character != null &&
                   InteractionState == MutinyPlayerInteractionState.Aiming &&
                   string.IsNullOrEmpty(ActiveWeapon) &&
                   TurnManager != null &&
                   TurnManager.CurrentTeam != null &&
                   TurnManager.CurrentTeam.SelectedCharacter == character;
        }

        private Vector3 m_AimOrigin;
        private string[,] m_CachedTerrain;
        private int m_GridWidth;
        private int m_GridHeight;
        private MutinyTeam m_ObservedTeam;
        private bool m_WasTurnActive;
        private MutinyCharacter m_HoveredCharacter;
        private MutinyVoodooDoll m_ArmedVoodooDoll;
        private MutinyWoodenCrate m_ArmedWoodenCrate;
        private MutinyAnchor m_ArmedAnchor;
        private MutinyGunpowderBarrel m_ArmedGunpowderBarrel;
        private MutinyCannon m_ArmedCannon;
        private MutinyWeapon m_EquippedWeapon;
        private MutinySpecialWeaponCursor m_SpecialWeaponCursor;
        private const int NoActiveTouchId = int.MinValue;
        private int m_ActiveTouchId = NoActiveTouchId;
        private Vector2 m_LastTouchPosition;
        // Unlike ordinary projectiles, PiecesOfEight remains equipped and is reused
        // for coin 2..8 while the turn stays in ActionExecuting.
        private MutinyPiecesOfEight m_ArmedPiecesOfEight;

        private void Start()
        {
            if (TurnManager == null)
                TurnManager = FindAnyObjectByType<MutinyTurnManager>();

            if (TrajectoryRenderer == null)
            {
                var trObj = new GameObject("TrajectoryRenderer");
                trObj.transform.SetParent(transform, false);
                trObj.AddComponent<LineRenderer>();
                TrajectoryRenderer = trObj.AddComponent<MutinyTrajectoryRenderer>();
            }

            if (GameCamera == null)
                GameCamera = Camera.main;

            if (GameCamera != null)
            {
                MutinyCameraController cameraController = GameCamera.GetComponent<MutinyCameraController>();
                if (cameraController == null)
                    cameraController = GameCamera.gameObject.AddComponent<MutinyCameraController>();
                cameraController.TurnManager = TurnManager;
                cameraController.PlayerInput = this;
            }

            EnsureSpecialWeaponCursor();

            CacheTerrain();
            ResetForCurrentTurn();
        }

        public void CacheTerrain()
        {
            var controller = FindAnyObjectByType<MutinyLevelController>();
            if (controller != null && controller.LevelXml != null)
            {
                var levelData = MutinyLevelXmlParser.Parse(controller.LevelXml.text);
                m_CachedTerrain = levelData.Terrain;
                m_GridWidth = levelData.Width;
                m_GridHeight = levelData.Height;
            }
        }

        private void Update()
        {
            // popup is above the game root in the Flash timeline. While it is
            // visible (including the four-tick fade) its buttons consume mouse
            // input; character selection and aiming must not see the same click.
            MutinyGameHUD hud = FindAnyObjectByType<MutinyGameHUD>();
            if (hud != null && hud.IsQuitPromptVisible)
            {
                ClearSpecialWeaponCursor();
                ClearHoveredCharacter();
                if (InteractionState == MutinyPlayerInteractionState.Aiming)
                    HideTrajectory();
                return;
            }

            if (!CanProcessCurrentTurnInput())
            {
                ClearSpecialWeaponCursor();
                ClearHoveredCharacter();
                m_WasTurnActive = false;
                if (InteractionState == MutinyPlayerInteractionState.Aiming)
                    HideTrajectory();
                return;
            }

            MutinyTeam currentTeam = TurnManager.CurrentTeam;
            bool placingBoxWeapon = HasPendingBoxPlacement();
            bool awaitingPiecesOfEight = IsAwaitingPiecesOfEight();
            if (currentTeam != m_ObservedTeam)
                ResetForCurrentTurn();
            else if (!m_WasTurnActive &&
                     currentTeam != null &&
                     !currentTeam.IsAiControlled &&
                     currentTeam.SelectedCharacter != null &&
                     currentTeam.SelectedCharacter.IsAlive &&
                     InteractionState == MutinyPlayerInteractionState.WeaponReady &&
                     !placingBoxWeapon && !awaitingPiecesOfEight)
            {
                // A character throw leaves the weapon action available. Reopen the
                // action menu when the board has settled.
                ClearEquippedWeapon();
                InteractionState = MutinyPlayerInteractionState.ActionMenu;
                ActiveWeapon = null;
            }

            m_WasTurnActive = true;

            if (currentTeam == null || currentTeam.IsAiControlled)
            {
                ClearSpecialWeaponCursor();
                ClearHoveredCharacter();
                return;
            }

            if (!TryReadPointer(out PointerFrameState pointer))
            {
                ClearSpecialWeaponCursor();
                ClearHoveredCharacter();
                return;
            }

            if (pointer.CanceledThisFrame)
            {
                HandlePointerCancellation();
                return;
            }

            Vector3 mouseWorld = GetMouseWorldPosition(pointer.Position);

            // Global weapon triggers in flight: Seagull and Banana take priority
            // over any character selection or action states.
            if (pointer.PressedThisFrame)
            {
                if (MutinySeagull.TryRequestPlayerShot(currentTeam))
                    return;

                if (MutinyBanana.TryRequestPlayerDetonation(currentTeam))
                    return;
            }

            UpdateHoveredCharacter(currentTeam, mouseWorld);

            // A new turn begins without SelectedCharacter.  Character-selection
            // input must therefore run before any code that requires one.
            if (InteractionState == MutinyPlayerInteractionState.CharacterSelection)
            {
                ClearSpecialWeaponCursor();
                if (pointer.PressedThisFrame)
                    TrySelectCharacter(currentTeam, mouseWorld);
                return;
            }

            MutinyCharacter selectedCharacter = currentTeam.SelectedCharacter;
            if (selectedCharacter == null || !selectedCharacter.IsAlive)
            {
                ClearSpecialWeaponCursor();
                ReturnToCharacterSelection();
                return;
            }

            UpdateSpecialWeaponCursor(selectedCharacter, pointer.Position, pointer.IsPressed);

            // CancelWeaponButton.onPress is consumed before TileSystem begins a
            // drag.
            if (pointer.PressedThisFrame &&
                TryCancelWeaponFromOverlay(selectedCharacter, mouseWorld))
                return;

            if (pointer.SecondaryPressedThisFrame &&
                TryCancelAimFromSecondaryPointer())
            {
                return;
            }

            if (InteractionState == MutinyPlayerInteractionState.ActionMenu)
                return;

            if (InteractionState == MutinyPlayerInteractionState.WeaponReady &&
                ShouldHandleWeaponReadyPrimaryInput(
                    pointer.PressedThisFrame, pointer.IsPressed))
            {
                if (TryHandleCannonInput(pointer, selectedCharacter, mouseWorld))
                    return;

                if (TrySelectVoodooTarget(currentTeam, selectedCharacter, mouseWorld))
                    return;

                if (TryActivateClickWeapon(selectedCharacter, mouseWorld))
                    return;

                Vector3 readyOrigin = GetReadyActionOrigin(selectedCharacter);
                float distancePixels = PixelDistance(mouseWorld, readyOrigin);
                if (distancePixels <= DragSelectionRadiusPixels && CanAim(selectedCharacter))
                {
                    InteractionState = MutinyPlayerInteractionState.Aiming;
                    m_AimOrigin = readyOrigin;
                    m_EquippedWeapon?.SetAimingState(true);
                    if (string.IsNullOrEmpty(ActiveWeapon))
                        MutinyMine.NotifyCharacterBeganSelfThrowAim(selectedCharacter);
                    MutinyDebugLog.Info("Input",
                        $"aim started character={selectedCharacter.name} weapon={ActiveWeapon ?? "character"} origin={MutinyPhysics.UnityToPixel(m_AimOrigin)}", this);
                }
            }

            if (InteractionState == MutinyPlayerInteractionState.WeaponReady &&
                ActiveWeapon != null && ActiveWeapon.Equals("cannon", System.StringComparison.OrdinalIgnoreCase) &&
                TryHandleCannonInput(pointer, selectedCharacter, mouseWorld))
                return;

            if (InteractionState != MutinyPlayerInteractionState.Aiming)
                return;

            if (pointer.IsPressed)
                ShowTrajectory(mouseWorld);

            if (pointer.ReleasedThisFrame)
            {
                float dragDistancePixels = PixelDistance(mouseWorld, m_AimOrigin);
                if (dragDistancePixels >= MinDragDistancePixels)
                {
                    Launch(selectedCharacter, mouseWorld);
                    HideTrajectory();
                    return;
                }

                CancelCurrentAim();
            }
        }

        // Banana.advanceMotion checks Controller.tileSystem.mouseButtonDown after
        // Weapon.fire has spent CanShoot. Its next click therefore must remain
        // routable while this team is in ActionExecuting, unlike ordinary input.
        private bool CanProcessCurrentTurnInput()
        {
            if (TurnManager == null)
                return false;

            bool placingBoxWeapon = HasPendingBoxPlacement();
            bool awaitingPiecesOfEight = IsAwaitingPiecesOfEight();
            bool awaitingBananaDetonation = MutinyBanana.HasPlayerDetonatableBanana(TurnManager.CurrentTeam);
            bool awaitingSeagullShot = MutinySeagull.HasPlayerActiveFlight(TurnManager.CurrentTeam);
            bool awaitingParachuteFan = MutinyParachuteBomb.HasPlayerActiveFlight(TurnManager.CurrentTeam);
            return TurnManager.CurrentPhase == TurnPhase.TurnActive || placingBoxWeapon ||
                   awaitingPiecesOfEight || awaitingBananaDetonation || awaitingSeagullShot ||
                   awaitingParachuteFan;
        }

        internal readonly struct PointerFrameState
        {
            public readonly Vector2 Position;
            public readonly bool PressedThisFrame;
            public readonly bool IsPressed;
            public readonly bool ReleasedThisFrame;
            public readonly bool CanceledThisFrame;
            public readonly bool SecondaryPressedThisFrame;

            public PointerFrameState(
                Vector2 position,
                bool pressedThisFrame,
                bool isPressed,
                bool releasedThisFrame,
                bool canceledThisFrame,
                bool secondaryPressedThisFrame = false)
            {
                Position = position;
                PressedThisFrame = pressedThisFrame;
                IsPressed = isPressed;
                ReleasedThisFrame = releasedThisFrame;
                CanceledThisFrame = canceledThisFrame;
                SecondaryPressedThisFrame = secondaryPressedThisFrame;
            }
        }

        private bool TryReadPointer(out PointerFrameState pointer)
        {
            if (Application.isMobilePlatform)
                return TryReadTouchPointer(Touchscreen.current, out pointer);

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                pointer = default;
                return false;
            }

            pointer = new PointerFrameState(
                mouse.position.ReadValue(),
                mouse.leftButton.wasPressedThisFrame,
                mouse.leftButton.isPressed,
                mouse.leftButton.wasReleasedThisFrame,
                false,
                mouse.rightButton.wasPressedThisFrame);
            return true;
        }

        private bool TryReadTouchPointer(Touchscreen touchscreen, out PointerFrameState pointer)
        {
            if (touchscreen == null)
            {
                pointer = default;
                return false;
            }

            if (m_ActiveTouchId == NoActiveTouchId)
            {
                for (int i = 0; i < touchscreen.touches.Count; i++)
                {
                    var candidate = touchscreen.touches[i];
                    if (!ShouldAcquireTouch(m_ActiveTouchId, candidate.press.wasPressedThisFrame))
                        continue;

                    m_ActiveTouchId = candidate.touchId.ReadValue();
                    break;
                }
            }

            if (m_ActiveTouchId == NoActiveTouchId)
            {
                pointer = default;
                return false;
            }

            for (int i = 0; i < touchscreen.touches.Count; i++)
            {
                var touch = touchscreen.touches[i];
                if (touch.touchId.ReadValue() != m_ActiveTouchId)
                    continue;

                Vector2 position = touch.position.ReadValue();
                m_LastTouchPosition = position;
                UnityEngine.InputSystem.TouchPhase phase = touch.phase.ReadValue();
                pointer = CreateTouchPointerState(
                    position,
                    phase,
                    touch.press.wasPressedThisFrame,
                    touch.press.isPressed,
                    touch.press.wasReleasedThisFrame);

                if (touch.press.wasReleasedThisFrame || pointer.CanceledThisFrame)
                    m_ActiveTouchId = NoActiveTouchId;
                return true;
            }

            // A platform focus change can remove a touch slot without delivering
            // a normal Ended event. Convert that loss into cancellation so an aim
            // or cannon drag can never remain latched or accidentally fire.
            pointer = new PointerFrameState(
                m_LastTouchPosition, false, false, false, true);
            m_ActiveTouchId = NoActiveTouchId;
            return true;
        }

        private static bool ShouldAcquireTouch(int activeTouchId, bool wasPressedThisFrame)
        {
            return activeTouchId == NoActiveTouchId && wasPressedThisFrame;
        }

        internal static bool ShouldAcquireTouchForVerification(
            bool alreadyHasActiveTouch, bool wasPressedThisFrame)
        {
            return ShouldAcquireTouch(
                alreadyHasActiveTouch ? 1 : NoActiveTouchId,
                wasPressedThisFrame);
        }

        internal static PointerFrameState TouchPhaseToPointerStateForVerification(
            UnityEngine.InputSystem.TouchPhase phase)
        {
            bool pressed = phase == UnityEngine.InputSystem.TouchPhase.Began;
            bool held = pressed || phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                        phase == UnityEngine.InputSystem.TouchPhase.Stationary;
            bool released = phase == UnityEngine.InputSystem.TouchPhase.Ended;
            return CreateTouchPointerState(Vector2.zero, phase, pressed, held, released);
        }

        private static PointerFrameState CreateTouchPointerState(
            Vector2 position,
            UnityEngine.InputSystem.TouchPhase phase,
            bool pressedThisFrame,
            bool isPressed,
            bool releasedThisFrame)
        {
            bool canceled = phase == UnityEngine.InputSystem.TouchPhase.Canceled;
            return new PointerFrameState(
                position,
                pressedThisFrame,
                isPressed,
                releasedThisFrame && !canceled,
                canceled);
        }

        private void HandlePointerCancellation()
        {
            m_ArmedCannon?.CancelPointer();
            if (InteractionState == MutinyPlayerInteractionState.Aiming)
                CancelCurrentAim();
            HideTrajectory();
            ClearSpecialWeaponCursor();
            ClearHoveredCharacter();
        }

        private void UpdateSpecialWeaponCursor(
            MutinyCharacter selectedCharacter, Vector2 mousePosition, bool leftButtonHeld)
        {
            EnsureSpecialWeaponCursor();

            MutinySpecialWeaponCursor.Mode mode = MutinySpecialWeaponCursor.Mode.None;
            float rotationDegrees = 0f;
            bool animateFan = false;
            MutinyParachuteBomb activeBomb = MutinyParachuteBomb.FindPlayerActiveFlight(TurnManager?.CurrentTeam);
            if (activeBomb != null)
            {
                mode = MutinySpecialWeaponCursor.Mode.ParachuteFan;
                float mousePixelX = GetMouseWorldPixelX(mousePosition);
                rotationDegrees = mousePixelX >= activeBomb.PhysicsBody.State.X ? -90f : 90f;
                animateFan = leftButtonHeld;
            }
            else if (selectedCharacter != null && InteractionState == MutinyPlayerInteractionState.WeaponReady)
            {
                if (string.Equals(ActiveWeapon, "seagull", System.StringComparison.OrdinalIgnoreCase) ||
                    (m_EquippedWeapon is MutinySeagull && !m_EquippedWeapon.IsFired))
                {
                    mode = MutinySpecialWeaponCursor.Mode.Seagull;
                }
                else if (string.Equals(ActiveWeapon, "tidalWave", System.StringComparison.OrdinalIgnoreCase) ||
                         (m_EquippedWeapon is MutinyTidalWave && !m_EquippedWeapon.IsFired))
                {
                    mode = MutinySpecialWeaponCursor.Mode.TidalWave;
                }
                else
                {
                    mode = ResolveBoxPlacementCursorMode(
                        MutinyPhysics.UnityToPixel(GetMouseWorldPosition(mousePosition)));
                }
            }

            m_SpecialWeaponCursor.SetMode(mode, mousePosition, rotationDegrees, animateFan);
        }

        private float GetMouseWorldPixelX(Vector2 mousePosition)
        {
            return MutinyPhysics.UnityToPixel(GetMouseWorldPosition(mousePosition)).x;
        }

        private void EnsureSpecialWeaponCursor()
        {
            if (m_SpecialWeaponCursor != null)
                return;

            var cursorObject = new GameObject("SpecialWeaponCursor");
            cursorObject.transform.SetParent(transform, false);
            m_SpecialWeaponCursor = cursorObject.AddComponent<MutinySpecialWeaponCursor>();
        }

        internal void UpdateSpecialWeaponCursorForVerification(
            MutinyCharacter selectedCharacter, Vector2 mousePosition, bool leftButtonHeld)
        {
            UpdateSpecialWeaponCursor(selectedCharacter, mousePosition, leftButtonHeld);
        }

        internal void UpdateBoxPlacementCursorForVerification(Vector2 pixelPosition)
        {
            EnsureSpecialWeaponCursor();
            m_SpecialWeaponCursor.SetMode(ResolveBoxPlacementCursorMode(pixelPosition), Vector2.zero);
        }

        internal string SpecialWeaponCursorModeForVerification =>
            m_SpecialWeaponCursor != null ? m_SpecialWeaponCursor.CurrentMode.ToString() : "None";

        internal float SpecialWeaponCursorRotationForVerification =>
            m_SpecialWeaponCursor != null ? m_SpecialWeaponCursor.RotationDegrees : 0f;

        internal bool SpecialWeaponCursorAnimatingForVerification =>
            m_SpecialWeaponCursor != null && m_SpecialWeaponCursor.IsFanAnimating;

        internal int SpecialWeaponCursorFanFrameForVerification =>
            m_SpecialWeaponCursor != null ? m_SpecialWeaponCursor.CurrentFanFrame : 0;

        internal void AdvanceSpecialWeaponCursorFanFrameForVerification()
        {
            m_SpecialWeaponCursor?.AdvanceFanFrameForVerification();
        }

        private MutinySpecialWeaponCursor.Mode ResolveBoxPlacementCursorMode(Vector2 pixelPosition)
        {
            if (m_ArmedWoodenCrate != null && m_ArmedWoodenCrate.HasPendingPlacement)
            {
                return m_ArmedWoodenCrate.CanPlaceNext(pixelPosition)
                    ? MutinySpecialWeaponCursor.Mode.WoodenCrate
                    : MutinySpecialWeaponCursor.Mode.Cross;
            }
            if (m_ArmedGunpowderBarrel != null && m_ArmedGunpowderBarrel.HasPendingPlacement)
            {
                return m_ArmedGunpowderBarrel.CanPlaceNext(pixelPosition)
                    ? MutinySpecialWeaponCursor.Mode.GunpowderBarrel
                    : MutinySpecialWeaponCursor.Mode.Cross;
            }
            return MutinySpecialWeaponCursor.Mode.None;
        }

        private bool HasPendingBoxPlacement()
        {
            return (m_ArmedWoodenCrate != null && m_ArmedWoodenCrate.HasPendingPlacement) ||
                   (m_ArmedGunpowderBarrel != null && m_ArmedGunpowderBarrel.HasPendingPlacement);
        }

        private void ClearSpecialWeaponCursor()
        {
            if (m_SpecialWeaponCursor != null)
                m_SpecialWeaponCursor.Clear();
        }

        private void OnDisable()
        {
            m_ActiveTouchId = NoActiveTouchId;
            HandlePointerCancellation();
            ClearSpecialWeaponCursor();
        }

        // The verification entry invokes exactly the Update phase gate; it avoids
        // only physical Mouse.current acquisition.
        internal bool CanProcessCurrentTurnInputForVerification()
        {
            return CanProcessCurrentTurnInput();
        }

        public bool SelectWeapon(string weaponType)
        {
            MutinyCharacter character = GetHumanSelectedCharacter();
            if (character == null || !character.CanShoot || character.WeaponLocked || !character.HasWeapon(weaponType))
            {
                MutinyDebugLog.Warning("Input",
                    $"weapon selection rejected weapon={weaponType} character={(character == null ? "none" : character.name)}", this);
                return false;
            }

            ClearEquippedWeapon();
            ActiveWeapon = weaponType;
            InteractionState = MutinyPlayerInteractionState.WeaponReady;
            m_EquippedWeapon = MutinyWeaponFactory.SpawnWeapon(weaponType, character);
            if (m_EquippedWeapon == null)
            {
                ActiveWeapon = null;
                InteractionState = MutinyPlayerInteractionState.ActionMenu;
                MutinyDebugLog.Warning("Input",
                    $"weapon selection rejected because production instance could not be created weapon={weaponType}", this);
                return false;
            }

            m_ArmedPiecesOfEight = m_EquippedWeapon as MutinyPiecesOfEight;
            m_ArmedVoodooDoll = m_EquippedWeapon as MutinyVoodooDoll;
            m_ArmedWoodenCrate = m_EquippedWeapon as MutinyWoodenCrate;
            m_ArmedGunpowderBarrel = m_EquippedWeapon as MutinyGunpowderBarrel;
            m_ArmedAnchor = m_EquippedWeapon as MutinyAnchor;
            m_ArmedCannon = m_EquippedWeapon as MutinyCannon;
            HideTrajectory();
            MutinyDebugLog.Info("Input",
                $"weapon ready character={character.name} weapon={weaponType} instance={m_EquippedWeapon.name} pos=({m_EquippedWeapon.PhysicsBody.State.X:F1},{m_EquippedWeapon.PhysicsBody.State.Y:F1})", this);
            return true;
        }

        public bool SelectCharacterThrow()
        {
            MutinyCharacter character = GetHumanSelectedCharacter();
            if (character == null || !character.CanThrow)
            {
                MutinyDebugLog.Warning("Input",
                    $"character throw selection rejected character={(character == null ? "none" : character.name)}", this);
                return false;
            }

            ActiveWeapon = null;
            InteractionState = MutinyPlayerInteractionState.WeaponReady;
            HideTrajectory();
            MutinyDebugLog.Info("Input", $"character throw selected character={character.name}", this);
            return true;
        }

        public void CancelWeaponSelection()
        {
            HideTrajectory();
            ClearEquippedWeapon();
            ActiveWeapon = null;

            MutinyCharacter character = GetHumanSelectedCharacter();
            InteractionState = character != null
                ? MutinyPlayerInteractionState.ActionMenu
                : MutinyPlayerInteractionState.CharacterSelection;
            MutinyDebugLog.Info("Input", $"weapon selection cancelled nextState={InteractionState}", this);
        }

        public bool ShouldShowCancelWeapon(MutinyCharacter character)
        {
            MutinyCharacter selected = GetHumanSelectedCharacter();
            bool hasCancelableReadyAction =
                (EquippedWeapon != null && EquippedWeapon.Owner == character) ||
                (string.IsNullOrEmpty(ActiveWeapon) && character != null && character.CanThrow);
            return character != null && character == selected && character.IsAlive &&
                   !character.WeaponLocked &&
                   InteractionState == MutinyPlayerInteractionState.WeaponReady &&
                   hasCancelableReadyAction;
        }

        public bool TryCancelWeaponFromOverlay(MutinyCharacter character, Vector3 mouseWorld)
        {
            if (!ShouldShowCancelWeapon(character) || character.PhysicsBody == null)
                return false;

            Vector2 mousePixels = MutinyPhysics.UnityToPixel(mouseWorld);
            float centerX = character.PhysicsBody.State.X;
            float centerY = character.PhysicsBody.State.Y + 33f;
            if (Mathf.Abs(mousePixels.x - centerX) > 10f || Mathf.Abs(mousePixels.y - centerY) > 10f)
                return false;

            MutinyDebugLog.Info("Input",
                $"cancel weapon button pressed character={character.name} weapon={ActiveWeapon}", this);
            CancelWeaponSelection();
            return true;
        }

        public void CancelCurrentAim()
        {
            if (InteractionState != MutinyPlayerInteractionState.Aiming)
                return;

            HideTrajectory();
            InteractionState = MutinyPlayerInteractionState.WeaponReady;
            m_EquippedWeapon?.SetAimingState(false);
            MutinyCharacter character = GetHumanSelectedCharacter();
            if (character != null)
                m_AimOrigin = GetReadyActionOrigin(character);
            MutinyDebugLog.Info("Input",
                $"current aim cancelled; weapon retained weapon={ActiveWeapon} instance={(m_EquippedWeapon == null ? "none" : m_EquippedWeapon.name)}", this);
        }

        private bool TryCancelAimFromSecondaryPointer()
        {
            if (InteractionState != MutinyPlayerInteractionState.Aiming)
                return false;

            CancelCurrentAim();
            return true;
        }

        internal bool TryCancelAimFromSecondaryPointerForVerification()
        {
            return TryCancelAimFromSecondaryPointer();
        }

        internal bool TryCancelWeaponFromOverlayForVerification(MutinyCharacter character, Vector2 pixelPosition)
        {
            return TryCancelWeaponFromOverlay(character,
                MutinyPhysics.PixelToUnity(pixelPosition.x, pixelPosition.y));
        }

        internal void BeginAimForVerification(MutinyCharacter character)
        {
            if (character == GetHumanSelectedCharacter() && CanAim(character))
            {
                m_AimOrigin = GetReadyActionOrigin(character);
                InteractionState = MutinyPlayerInteractionState.Aiming;
                m_EquippedWeapon?.SetAimingState(true);
                if (string.IsNullOrEmpty(ActiveWeapon))
                    MutinyMine.NotifyCharacterBeganSelfThrowAim(character);
            }
        }

        public void ReturnToCharacterSelection()
        {
            MutinyCharacter selected = GetHumanSelectedCharacter();
            if (selected != null && !selected.CanThrow)
                return;

            HideTrajectory();
            ClearEquippedWeapon();
            ActiveWeapon = null;
            if (selected != null)
                selected.IsSelected = false;
            InteractionState = MutinyPlayerInteractionState.CharacterSelection;
        }

        public void EndTurn()
        {
            MutinyCharacter character = GetHumanSelectedCharacter();
            if (character == null)
            {
                MutinyDebugLog.Warning("Input", "end turn rejected because no human character is selected", this);
                return;
            }

            HideTrajectory();
            ClearHoveredCharacter();
            ClearEquippedWeapon();
            ActiveWeapon = null;
            character.CanThrow = false;
            character.CanShoot = false;
            InteractionState = MutinyPlayerInteractionState.WeaponReady;
            MutinyDebugLog.Info("Input", $"end turn character={character.name}", this);
            TurnManager.NotifyActionStarted();
        }

        private void TrySelectCharacter(MutinyTeam team, Vector3 mouseWorld)
        {
            MutinyCharacter clicked = FindCharacterNearPosition(mouseWorld, team, CharacterSelectionRadiusPixels);
            if (clicked == null)
                return;

            if (team.SelectedCharacter != null && !team.SelectedCharacter.CanThrow)
                return;

            team.SelectCharacter(clicked);
            ClearHoveredCharacter();
            ActiveWeapon = null;
            InteractionState = MutinyPlayerInteractionState.ActionMenu;
            MutinyDebugLog.Info("Input", $"character selected team=T{team.TeamNumber} character={clicked.name}", this);
        }

        // Uses the same production selection method while bypassing only the
        // physical Mouse.current read required by Update.
        internal bool TrySelectCharacterForVerification(MutinyTeam team, Vector2 pixelPosition)
        {
            if (team == null)
                return false;

            MutinyCharacter before = team.SelectedCharacter;
            TrySelectCharacter(team, MutinyPhysics.PixelToUnity(pixelPosition.x, pixelPosition.y));
            return team.SelectedCharacter != null && team.SelectedCharacter != before &&
                   InteractionState == MutinyPlayerInteractionState.ActionMenu;
        }

        private bool CanAim(MutinyCharacter character)
        {
            if (string.IsNullOrEmpty(ActiveWeapon))
                return character.CanThrow;

            if (ActiveWeapon.Equals("piecesOfEight", System.StringComparison.OrdinalIgnoreCase))
                return m_ArmedPiecesOfEight != null && m_ArmedPiecesOfEight.Owner == character &&
                       m_ArmedPiecesOfEight.CanFireNextCoin;

            if (ActiveWeapon.Equals("voodooDoll", System.StringComparison.OrdinalIgnoreCase))
                return character.CanShoot && character.HasWeapon(ActiveWeapon) &&
                       m_ArmedVoodooDoll != null && m_ArmedVoodooDoll.HasTarget;

            return character.CanShoot && character.HasWeapon(ActiveWeapon);
        }

        private bool TrySelectVoodooTarget(MutinyTeam team, MutinyCharacter owner, Vector3 mouseWorld)
        {
            if (!IsVoodooTargetSelection() || team == null || owner == null)
                return false;

            MutinyTeam enemyTeam = FindOpposingTeam(team);
            MutinyCharacter target = FindCharacterNearPosition(mouseWorld, enemyTeam, CharacterSelectionRadiusPixels);
            if (target == null)
                return false;

            if (!m_ArmedVoodooDoll.BindTarget(target))
                return true;

            ClearHoveredCharacter();
            MutinyAudioManager.Instance?.PlaySFX("voodoo");
            MutinyDebugLog.Info("Input", $"voodoo target selected owner={owner.name} target={target.name}", this);
            return true;
        }

        internal bool TrySelectVoodooTargetForVerification(
            MutinyTeam team, MutinyCharacter owner, Vector3 mouseWorld)
        {
            return TrySelectVoodooTarget(team, owner, mouseWorld);
        }

        private bool TryHandleCannonInput(
            PointerFrameState pointer, MutinyCharacter character, Vector3 mouseWorld)
        {
            if (ActiveWeapon == null || !ActiveWeapon.Equals("cannon", System.StringComparison.OrdinalIgnoreCase) ||
                m_ArmedCannon == null || m_ArmedCannon.Owner != character || m_ArmedCannon.IsFinished)
                return false;

            Vector2 mousePixels = MutinyPhysics.UnityToPixel(mouseWorld);
            if (pointer.PressedThisFrame)
            {
                // The source tests pin first, then the 20px cannon-body circle.
                if (!m_ArmedCannon.TryBeginPinDrag(mousePixels))
                    m_ArmedCannon.TryBeginBodyDrag(mousePixels);
                return true;
            }

            if (pointer.IsPressed)
            {
                if (m_ArmedCannon.IsDraggingPin)
                    m_ArmedCannon.DragPinTo(mousePixels);
                else if (m_ArmedCannon.IsDraggingBody)
                    m_ArmedCannon.DragBodyTo(mousePixels);
                return true;
            }

            if (pointer.ReleasedThisFrame)
            {
                bool committed = m_ArmedCannon.ReleasePointer(TurnManager);
                if (committed)
                {
                    // Character.weaponExpired removes Cannon only after its visual
                    // lifecycle completes.  Unity inventory stores counts, so spend
                    // this selected weapon exactly once when the pin commits.
                    character.ConsumeWeapon("cannon");
                    MutinyDebugLog.Info("Input", $"cannon pin committed character={character.name}", this);
                }
                return true;
            }

            return true; // A selected Cannon owns board input while it is placed.
        }

        private bool TryActivateClickWeapon(MutinyCharacter character, Vector3 mouseWorld)
        {
            if (character == null || string.IsNullOrEmpty(ActiveWeapon))
                return false;

            if (ActiveWeapon.Equals("woodenCrate", System.StringComparison.OrdinalIgnoreCase))
            {
                if (m_ArmedWoodenCrate == null || m_ArmedWoodenCrate.Owner != character)
                    return false;

                bool firstPlacement = !m_ArmedWoodenCrate.HasPlacedAny;
                bool placed = m_ArmedWoodenCrate.TryPlaceAt(MutinyPhysics.UnityToPixel(mouseWorld));
                if (!placed)
                    return true; // A placeable weapon consumes this stage click even when rejected.

                if (firstPlacement)
                {
                    character.ConsumeWeapon(ActiveWeapon);
                    MutinyDebugLog.Info("Input", $"wooden crate sequence committed character={character.name}", this);
                }

                if (!m_ArmedWoodenCrate.HasPendingPlacement)
                {
                    ActiveWeapon = null;
                    m_EquippedWeapon = null;
                    m_ArmedWoodenCrate = null;
                    MutinyDebugLog.Info("Input", "wooden crate third placement committed", this);
                }
                InteractionState = MutinyPlayerInteractionState.WeaponReady;
                return true;
            }

            if (ActiveWeapon.Equals("gunpowderBarrel", System.StringComparison.OrdinalIgnoreCase))
            {
                if (m_ArmedGunpowderBarrel == null || m_ArmedGunpowderBarrel.Owner != character)
                    return false;
                bool first = !m_ArmedGunpowderBarrel.HasPlacedAny;
                if (!m_ArmedGunpowderBarrel.TryPlaceAt(MutinyPhysics.UnityToPixel(mouseWorld)))
                    return true;
                if (first)
                {
                    // TryPlaceAt commits the production turn exactly once, matching
                    // BoxWeapon.place. Input owns only inventory consumption.
                    character.ConsumeWeapon(ActiveWeapon);
                    MutinyDebugLog.Info("Input", $"gunpowder barrel sequence committed character={character.name}", this);
                }
                if (!m_ArmedGunpowderBarrel.HasPendingPlacement)
                {
                    ActiveWeapon = null;
                    m_EquippedWeapon = null;
                    m_ArmedGunpowderBarrel = null;
                    MutinyDebugLog.Info("Input", "gunpowder barrel second placement committed", this);
                }
                InteractionState = MutinyPlayerInteractionState.WeaponReady;
                return true;
            }

            if (ActiveWeapon.Equals("anchor", System.StringComparison.OrdinalIgnoreCase))
            {
                if (m_ArmedAnchor == null || m_ArmedAnchor.Owner != character)
                    return false;

                if (!m_ArmedAnchor.DropAt(MutinyPhysics.UnityToPixel(mouseWorld).x))
                    return true;

                character.ConsumeWeapon(ActiveWeapon);
                TurnManager?.NotifyActionStarted();
                MutinyDebugLog.Info("Input", $"anchor drop committed character={character.name} x={m_ArmedAnchor.PhysicsBody.State.X:F1}", this);
                ActiveWeapon = null;
                m_EquippedWeapon = null;
                m_ArmedAnchor = null;
                InteractionState = MutinyPlayerInteractionState.WeaponReady;
                return true;
            }

            if (!character.CanShoot || !character.HasWeapon(ActiveWeapon))
                return false;

            if (ActiveWeapon.Equals("seagull", System.StringComparison.OrdinalIgnoreCase))
            {
                if (m_EquippedWeapon is MutinySeagull seagull && seagull.Owner == character)
                {
                    // Seagull.place(-300, content._ymouse): this first press picks
                    // the route height; later presses are consumed by the live bird.
                    seagull.PlaceAtFlightHeight(MutinyPhysics.UnityToPixel(mouseWorld).y);
                    character.ConsumeWeapon(ActiveWeapon);
                    character.CanShoot = false;
                    character.CanThrow = false;
                    TurnManager.NotifyActionStarted();
                    m_EquippedWeapon = null;
                    ActiveWeapon = null;
                InteractionState = MutinyPlayerInteractionState.WeaponReady;
                    MutinyDebugLog.Info("Input", $"seagull path placed y={seagull.FlightY:F1}", this);
                    return true;
                }
            }
            else if (ActiveWeapon.Equals("tidalWave", System.StringComparison.OrdinalIgnoreCase))
            {
                if (m_EquippedWeapon is MutinyTidalWave tidalWave && tidalWave.Owner == character)
                {
                    MutinyLevelRoot root = FindAnyObjectByType<MutinyLevelRoot>();
                    float waterPixelY = root != null
                        ? -root.WaterLevelY * MutinyPhysics.PixelsPerUnit
                        : 448f;
                    tidalWave.StartWave(-550f, waterPixelY);
                    character.ConsumeWeapon(ActiveWeapon);
                    character.CanShoot = false;
                    character.CanThrow = false;
                    TurnManager.NotifyActionStarted();
                    m_EquippedWeapon = null;
                    ActiveWeapon = null;
                InteractionState = MutinyPlayerInteractionState.WeaponReady;
                    return true;
                }
            }

            return false;
        }

        // Verification invokes the same click-weapon production route as Update;
        // only physical pointer acquisition is omitted.
        internal bool TryActivateClickWeaponForVerification(MutinyCharacter character, Vector2 pixelPosition)
        {
            return TryActivateClickWeapon(character, MutinyPhysics.PixelToUnity(pixelPosition.x, pixelPosition.y));
        }

        internal static bool ShouldHandleWeaponReadyPrimaryInput(
            bool wasPressedThisFrame, bool isPressed)
        {
            // BoxWeapon placement is a click request, not a held-button request.
            // An invalid press remains ignored until the button is released and
            // a new press edge is received at another position.
            return wasPressedThisFrame && isPressed;
        }

        // Verification invokes the same generic release path as Update; only the
        // physical pointer press/hold/release events are omitted.
        internal bool TryLaunchWeaponForVerification(
            MutinyCharacter character, Vector2 startPixels, Vector2 releasePixels)
        {
            if (character == null || character != GetHumanSelectedCharacter() ||
                string.IsNullOrEmpty(ActiveWeapon) || !CanLaunchActiveWeapon(character))
                return false;

            m_AimOrigin = MutinyPhysics.PixelToUnity(startPixels.x, startPixels.y);
            Launch(character, MutinyPhysics.PixelToUnity(releasePixels.x, releasePixels.y));
            return true;
        }

        private void ShowTrajectory(Vector3 mouseWorld)
        {
            if (m_CachedTerrain == null)
                CacheTerrain();

            if (TrajectoryRenderer == null || m_CachedTerrain == null)
                return;

            Vector2 startPixels = MutinyPhysics.UnityToPixel(m_AimOrigin);
            Vector2 dragPixels = MutinyPhysics.UnityToPixel(mouseWorld);
            TrajectoryRenderer.ShowTrajectory(
                startPixels,
                dragPixels,
                m_CachedTerrain,
                m_GridWidth,
                m_GridHeight,
                MutinyWeaponFactory.GetTwangMaxForce(ActiveWeapon),
                MutinyWeaponFactory.GetPredictionWeight(ActiveWeapon),
                ActiveWeapon);
        }

        private void Launch(MutinyCharacter character, Vector3 releaseWorldPosition)
        {
            Vector2 startPixels = MutinyPhysics.UnityToPixel(m_AimOrigin);
            Vector2 dragPixels = MutinyPhysics.UnityToPixel(releaseWorldPosition);

            if (!string.IsNullOrEmpty(ActiveWeapon) &&
                ActiveWeapon.Equals("piecesOfEight", System.StringComparison.OrdinalIgnoreCase) &&
                m_ArmedPiecesOfEight != null && m_ArmedPiecesOfEight.Owner == character &&
                m_ArmedPiecesOfEight.CanFireNextCoin)
            {
                bool firstCoin = m_ArmedPiecesOfEight.TimesFired == 0;
                m_ArmedPiecesOfEight.SetAimingState(false);
                m_ArmedPiecesOfEight.Twang(startPixels, dragPixels);
                if (m_ArmedPiecesOfEight.IsFired && firstCoin)
                    character.ConsumeWeapon(ActiveWeapon);
                MutinyDebugLog.Info("Input",
                    $"piecesOfEight launched character={character.name} coin={m_ArmedPiecesOfEight.TimesFired + 1}/{MutinyPiecesOfEight.TotalCoins} first={firstCoin}", this);
                InteractionState = MutinyPlayerInteractionState.WeaponReady;
                return;
            }

            if (!string.IsNullOrEmpty(ActiveWeapon) && character.HasWeapon(ActiveWeapon) && character.CanShoot)
            {
                if (ActiveWeapon.Equals("voodooDoll", System.StringComparison.OrdinalIgnoreCase))
                {
                    MutinyVoodooDoll doll = m_ArmedVoodooDoll;
                    if (doll != null && doll.HasTarget)
                    {
                        doll.SetAimingState(false);
                        doll.Twang(startPixels, dragPixels);
                        if (doll.IsFired)
                        {
                            character.ConsumeWeapon(ActiveWeapon);
                            MutinyDebugLog.Info("Input",
                                $"voodoo launched character={character.name} target={doll.TargetCharacter.name} start={startPixels} drag={dragPixels}", this);
                            m_EquippedWeapon = null;
                            m_ArmedVoodooDoll = null;
                            ActiveWeapon = null;
                        }
                    }
                InteractionState = MutinyPlayerInteractionState.WeaponReady;
                    return;
                }

                string launchedWeaponType = ActiveWeapon;
                MutinyWeapon weapon = m_EquippedWeapon;
                if (weapon == null || weapon.Owner != character || weapon.IsFired)
                {
                    MutinyDebugLog.Warning("Input",
                        $"launch rejected because equipped instance is missing weapon={launchedWeaponType}", this);
                    InteractionState = MutinyPlayerInteractionState.WeaponReady;
                    return;
                }

                weapon.SetAimingState(false);
                weapon.Twang(startPixels, dragPixels);
                if (!weapon.IsFired)
                    return;
                character.ConsumeWeapon(launchedWeaponType);
                // Mine.as/Weapon.as have no launch sound. Its first original sound
                // is mine_beep when proximity activates the countdown.
                if (!launchedWeaponType.Equals("mine", System.StringComparison.OrdinalIgnoreCase))
                    MutinyAudioManager.Instance?.PlaySFX("click");
                MutinyDebugLog.Info("Input",
                    $"equipped weapon launched character={character.name} weapon={launchedWeaponType} instance={weapon.name} start={startPixels} drag={dragPixels}", this);
                m_EquippedWeapon = null;
                ActiveWeapon = null;
                character.CanShoot = false;
                character.CanThrow = false;
            }
            else if (string.IsNullOrEmpty(ActiveWeapon) && character.CanThrow)
            {
                TryCommitCharacterThrow(character, startPixels, dragPixels);
            }

            InteractionState = MutinyPlayerInteractionState.WeaponReady;
        }

        internal bool TryCommitCharacterThrow(
            MutinyCharacter character, Vector2 startPixels, Vector2 dragPixels)
        {
            if (character == null || character != GetHumanSelectedCharacter() || !character.CanThrow)
                return false;

            MutinyPhysicsBody body = character.PhysicsBody;
            if (body == null)
            {
                Debug.LogError($"[MutinyPlayerInput] {character.name} has no physics body.", character);
                InteractionState = MutinyPlayerInteractionState.WeaponReady;
                return false;
            }

            body.Twang(startPixels, dragPixels);
            character.MarkSelfThrown("player input");
            character.CanThrow = false;
            MutinyDebugLog.Info("Input",
                $"character throw committed character={character.name} start={startPixels} drag={dragPixels}; original Character.twang has no direct SFX", this);
            TurnManager.NotifyActionStarted();
            InteractionState = MutinyPlayerInteractionState.WeaponReady;
            return true;
        }

        private void ResetForCurrentTurn()
        {
            HideTrajectory();
            ClearEquippedWeapon();
            m_ObservedTeam = TurnManager != null ? TurnManager.CurrentTeam : null;
            m_WasTurnActive = TurnManager != null && TurnManager.CurrentPhase == TurnPhase.TurnActive;
            ClearHoveredCharacter();
            if (m_ObservedTeam != null && !m_ObservedTeam.IsAiControlled && m_ObservedTeam.SelectedCharacter != null)
                m_ObservedTeam.SelectedCharacter.IsSelected = false;
            ActiveWeapon = null;
            InteractionState = MutinyPlayerInteractionState.CharacterSelection;
            MutinyDebugLog.Info("Input",
                $"reset for team={(m_ObservedTeam == null ? "none" : $"T{m_ObservedTeam.TeamNumber}")} ai={m_ObservedTeam?.IsAiControlled}", this);
        }

        private MutinyCharacter GetHumanSelectedCharacter()
        {
            if (TurnManager == null ||
                (TurnManager.CurrentPhase != TurnPhase.TurnActive && !IsAwaitingPiecesOfEight()))
                return null;

            MutinyTeam team = TurnManager.CurrentTeam;
            if (team == null || team.IsAiControlled)
                return null;

            MutinyCharacter character = team.SelectedCharacter;
            return character != null && character.IsAlive ? character : null;
        }

        private void HideTrajectory()
        {
            if (TrajectoryRenderer != null)
                TrajectoryRenderer.HideTrajectory();
        }

        private Vector3 GetReadyActionOrigin(MutinyCharacter character)
        {
            MutinyWeapon equipped = EquippedWeapon;
            if (!string.IsNullOrEmpty(ActiveWeapon) && equipped != null && equipped.PhysicsBody != null)
            {
                return MutinyPhysics.PixelToUnity(
                    equipped.PhysicsBody.State.X, equipped.PhysicsBody.State.Y);
            }
            return character != null ? character.transform.position : Vector3.zero;
        }

        private void UpdateHoveredCharacter(MutinyTeam team, Vector3 mouseWorld)
        {
            MutinyCharacter hovered = null;
            if (InteractionState == MutinyPlayerInteractionState.CharacterSelection)
                hovered = FindCharacterNearPosition(mouseWorld, team, CharacterSelectionRadiusPixels);
            else if (IsVoodooTargetSelection())
                hovered = FindCharacterNearPosition(mouseWorld, FindOpposingTeam(team), CharacterSelectionRadiusPixels);

            if (hovered == m_HoveredCharacter)
                return;

            ClearHoveredCharacter();
            m_HoveredCharacter = hovered;
            if (m_HoveredCharacter != null)
                m_HoveredCharacter.IsHovered = true;
        }

        private void ClearHoveredCharacter()
        {
            if (m_HoveredCharacter != null)
                m_HoveredCharacter.IsHovered = false;
            m_HoveredCharacter = null;
        }

        private MutinyCharacter FindCharacterNearPosition(Vector3 position, MutinyTeam team, float radiusPixels)
        {
            if (team == null || team.Characters == null)
                return null;

            MutinyCharacter closest = null;
            float closestDistance = radiusPixels;

            for (int i = 0; i < team.Characters.Count; i++)
            {
                MutinyCharacter character = team.Characters[i];
                if (character == null || !character.IsAlive)
                    continue;

                float distance = PixelDistance(position, character.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = character;
                }
            }

            return closest;
        }

        private bool IsAwaitingPiecesOfEight()
        {
            return m_ArmedPiecesOfEight != null && m_ArmedPiecesOfEight.IsAwaitingNextCoin &&
                   TurnManager != null && TurnManager.CurrentTeam != null &&
                   TurnManager.CurrentTeam.SelectedCharacter == m_ArmedPiecesOfEight.Owner;
        }

        private bool IsPiecesOfEightSequenceLocked()
        {
            return m_ArmedPiecesOfEight != null && m_ArmedPiecesOfEight.IsAwaitingNextCoin;
        }

        private bool CanLaunchActiveWeapon(MutinyCharacter character)
        {
            if (ActiveWeapon != null && ActiveWeapon.Equals("piecesOfEight", System.StringComparison.OrdinalIgnoreCase))
            {
                return m_ArmedPiecesOfEight != null && m_ArmedPiecesOfEight.Owner == character &&
                       m_ArmedPiecesOfEight.CanFireNextCoin;
            }
            return character.CanShoot && character.HasWeapon(ActiveWeapon);
        }

        private bool IsVoodooTargetSelection()
        {
            return InteractionState == MutinyPlayerInteractionState.WeaponReady &&
                   ActiveWeapon != null &&
                   ActiveWeapon.Equals("voodooDoll", System.StringComparison.OrdinalIgnoreCase) &&
                   m_ArmedVoodooDoll != null && !m_ArmedVoodooDoll.IsFired &&
                   !m_ArmedVoodooDoll.HasTarget;
        }

        private void ClearEquippedWeapon()
        {
            MutinyWeapon equipped = m_EquippedWeapon;
            if (equipped == null)
                equipped = m_ArmedPiecesOfEight != null ? m_ArmedPiecesOfEight :
                    m_ArmedVoodooDoll != null ? m_ArmedVoodooDoll :
                    m_ArmedWoodenCrate != null ? m_ArmedWoodenCrate :
                    m_ArmedGunpowderBarrel != null ? m_ArmedGunpowderBarrel :
                    m_ArmedAnchor != null ? m_ArmedAnchor : m_ArmedCannon;

            bool committed = equipped != null && equipped.IsFired;
            if (equipped is MutinyWoodenCrate crate)
                committed |= crate.HasPlacedAny;
            if (equipped is MutinyGunpowderBarrel barrel)
                committed |= barrel.HasPlacedAny;
            if (equipped is MutinyPiecesOfEight pieces)
                committed |= pieces.TimesFired > 0;

            if (equipped != null && !committed)
            {
                MutinyDebugLog.Info("Input",
                    $"unequipped pending weapon={equipped.WeaponType} instance={equipped.name}", this);
                if (Application.isPlaying)
                    Destroy(equipped.gameObject);
                else
                    DestroyImmediate(equipped.gameObject);
            }

            m_EquippedWeapon = null;
            m_ArmedPiecesOfEight = null;
            m_ArmedVoodooDoll = null;
            m_ArmedWoodenCrate = null;
            m_ArmedGunpowderBarrel = null;
            m_ArmedAnchor = null;
            m_ArmedCannon = null;
        }

        private MutinyTeam FindOpposingTeam(MutinyTeam team)
        {
            if (team == null)
                return null;

            if (TurnManager != null)
            {
                if (TurnManager.Team1 == team && TurnManager.Team2 != null)
                    return TurnManager.Team2;
                if (TurnManager.Team2 == team && TurnManager.Team1 != null)
                    return TurnManager.Team1;
            }

            MutinyTeam[] teams = FindObjectsByType<MutinyTeam>();
            for (int i = 0; i < teams.Length; i++)
            {
                if (teams[i] != null && teams[i] != team && teams[i].TeamNumber != team.TeamNumber)
                    return teams[i];
            }
            return null;
        }

        private static float PixelDistance(Vector3 first, Vector3 second)
        {
            return Vector2.Distance(MutinyPhysics.UnityToPixel(first), MutinyPhysics.UnityToPixel(second));
        }

        private Vector3 GetMouseWorldPosition(Vector2 screenPosition)
        {
            if (GameCamera == null)
                GameCamera = Camera.main;

            if (GameCamera == null)
                return Vector3.zero;

            Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, 0f);
            screenPoint.z = -GameCamera.transform.position.z;
            Vector3 world = GameCamera.ScreenToWorldPoint(screenPoint);
            world.z = 0f;
            return world;
        }
    }
}
