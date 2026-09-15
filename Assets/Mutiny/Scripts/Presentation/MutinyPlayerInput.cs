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
        WeaponArmed,
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
        public bool IsAiming => InteractionState == MutinyPlayerInteractionState.Aiming;

        private Vector3 m_AimOrigin;
        private string[,] m_CachedTerrain;
        private int m_GridWidth;
        private int m_GridHeight;
        private MutinyTeam m_ObservedTeam;
        private bool m_WasTurnActive;
        private MutinyCharacter m_HoveredCharacter;

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
            if (TurnManager == null || TurnManager.CurrentPhase != TurnPhase.TurnActive)
            {
                ClearHoveredCharacter();
                m_WasTurnActive = false;
                if (InteractionState == MutinyPlayerInteractionState.Aiming)
                    HideTrajectory();
                return;
            }

            MutinyTeam currentTeam = TurnManager.CurrentTeam;
            if (currentTeam != m_ObservedTeam)
                ResetForCurrentTurn();
            else if (!m_WasTurnActive &&
                     currentTeam != null &&
                     !currentTeam.IsAiControlled &&
                     currentTeam.SelectedCharacter != null &&
                     currentTeam.SelectedCharacter.IsAlive &&
                     InteractionState == MutinyPlayerInteractionState.WeaponArmed)
            {
                // A character throw leaves the weapon action available. Reopen the
                // action menu when the board has settled.
                InteractionState = MutinyPlayerInteractionState.ActionMenu;
                ActiveWeapon = null;
            }

            m_WasTurnActive = true;

            if (currentTeam == null || currentTeam.IsAiControlled)
            {
                ClearHoveredCharacter();
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            Vector3 mouseWorld = GetMouseWorldPosition(mouse.position.ReadValue());
            UpdateHoveredCharacter(currentTeam, mouseWorld);

            if (mouse.rightButton.wasPressedThisFrame &&
                (InteractionState == MutinyPlayerInteractionState.WeaponArmed ||
                 InteractionState == MutinyPlayerInteractionState.Aiming))
            {
                CancelWeaponSelection();
                return;
            }

            if (InteractionState == MutinyPlayerInteractionState.CharacterSelection)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                    TrySelectCharacter(currentTeam, mouseWorld);
                return;
            }

            MutinyCharacter selectedCharacter = currentTeam.SelectedCharacter;
            if (selectedCharacter == null || !selectedCharacter.IsAlive)
            {
                ReturnToCharacterSelection();
                return;
            }

            if (InteractionState == MutinyPlayerInteractionState.ActionMenu)
                return;

            if (InteractionState == MutinyPlayerInteractionState.WeaponArmed &&
                mouse.leftButton.wasPressedThisFrame)
            {
                if (TryActivateClickWeapon(selectedCharacter, mouseWorld))
                    return;

                float distancePixels = PixelDistance(mouseWorld, selectedCharacter.transform.position);
                if (distancePixels <= DragSelectionRadiusPixels && CanAim(selectedCharacter))
                {
                    InteractionState = MutinyPlayerInteractionState.Aiming;
                    m_AimOrigin = selectedCharacter.transform.position;
                }
            }

            if (InteractionState != MutinyPlayerInteractionState.Aiming)
                return;

            if (mouse.leftButton.isPressed)
                ShowTrajectory(mouseWorld);

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                float dragDistancePixels = PixelDistance(mouseWorld, m_AimOrigin);
                if (dragDistancePixels >= MinDragDistancePixels)
                {
                    Launch(selectedCharacter, mouseWorld);
                    HideTrajectory();
                    return;
                }

                HideTrajectory();
                InteractionState = MutinyPlayerInteractionState.WeaponArmed;
            }
        }

        public bool SelectWeapon(string weaponType)
        {
            MutinyCharacter character = GetHumanSelectedCharacter();
            if (character == null || !character.CanShoot || !character.HasWeapon(weaponType))
                return false;

            ActiveWeapon = weaponType;
            InteractionState = MutinyPlayerInteractionState.WeaponArmed;
            HideTrajectory();
            return true;
        }

        public bool SelectCharacterThrow()
        {
            MutinyCharacter character = GetHumanSelectedCharacter();
            if (character == null || !character.CanThrow)
                return false;

            ActiveWeapon = null;
            InteractionState = MutinyPlayerInteractionState.WeaponArmed;
            HideTrajectory();
            return true;
        }

        public void CancelWeaponSelection()
        {
            HideTrajectory();
            ActiveWeapon = null;

            MutinyCharacter character = GetHumanSelectedCharacter();
            InteractionState = character != null
                ? MutinyPlayerInteractionState.ActionMenu
                : MutinyPlayerInteractionState.CharacterSelection;
        }

        public void ReturnToCharacterSelection()
        {
            MutinyCharacter selected = GetHumanSelectedCharacter();
            if (selected != null && !selected.CanThrow)
                return;

            HideTrajectory();
            ActiveWeapon = null;
            if (selected != null)
                selected.IsSelected = false;
            InteractionState = MutinyPlayerInteractionState.CharacterSelection;
        }

        public void EndTurn()
        {
            MutinyCharacter character = GetHumanSelectedCharacter();
            if (character == null)
                return;

            HideTrajectory();
            ClearHoveredCharacter();
            ActiveWeapon = null;
            character.CanThrow = false;
            character.CanShoot = false;
            InteractionState = MutinyPlayerInteractionState.WeaponArmed;
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
        }

        private bool CanAim(MutinyCharacter character)
        {
            if (string.IsNullOrEmpty(ActiveWeapon))
                return character.CanThrow;

            return character.CanShoot && character.HasWeapon(ActiveWeapon);
        }

        private bool TryActivateClickWeapon(MutinyCharacter character, Vector3 mouseWorld)
        {
            if (character == null || string.IsNullOrEmpty(ActiveWeapon) ||
                !character.CanShoot || !character.HasWeapon(ActiveWeapon))
                return false;

            if (ActiveWeapon.Equals("anchor", System.StringComparison.OrdinalIgnoreCase))
            {
                MutinyWeapon weapon = MutinyWeaponFactory.SpawnWeapon(ActiveWeapon, character);
                if (weapon is MutinyAnchor anchor)
                {
                    anchor.DropAt(MutinyPhysics.UnityToPixel(mouseWorld).x);
                    character.ConsumeWeapon(ActiveWeapon);
                    character.CanShoot = false;
                    character.CanThrow = false;
                    TurnManager.NotifyActionStarted();
                    ActiveWeapon = null;
                    InteractionState = MutinyPlayerInteractionState.WeaponArmed;
                    return true;
                }
            }
            else if (ActiveWeapon.Equals("tidalWave", System.StringComparison.OrdinalIgnoreCase))
            {
                MutinyWeapon weapon = MutinyWeaponFactory.SpawnWeapon(ActiveWeapon, character);
                if (weapon is MutinyTidalWave tidalWave)
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
                    ActiveWeapon = null;
                    InteractionState = MutinyPlayerInteractionState.WeaponArmed;
                    return true;
                }
            }

            return false;
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
                MutinyWeaponFactory.GetPredictionWeight(ActiveWeapon));
        }

        private void Launch(MutinyCharacter character, Vector3 releaseWorldPosition)
        {
            Vector2 startPixels = MutinyPhysics.UnityToPixel(m_AimOrigin);
            Vector2 dragPixels = MutinyPhysics.UnityToPixel(releaseWorldPosition);

            if (!string.IsNullOrEmpty(ActiveWeapon) && character.HasWeapon(ActiveWeapon) && character.CanShoot)
            {
                MutinyWeaponFactory.SpawnAndLaunch(ActiveWeapon, character, startPixels, dragPixels);
                character.CanShoot = false;
                character.CanThrow = false;
                TurnManager.NotifyActionStarted();
            }
            else if (string.IsNullOrEmpty(ActiveWeapon) && character.CanThrow)
            {
                MutinyPhysicsBody body = character.PhysicsBody;
                if (body == null)
                {
                    Debug.LogError($"[MutinyPlayerInput] {character.name} has no physics body.", character);
                    InteractionState = MutinyPlayerInteractionState.WeaponArmed;
                    return;
                }

                body.Twang(startPixels, dragPixels);
                character.CanThrow = false;
                TurnManager.NotifyActionStarted();
                MutinyAudioManager.Instance?.PlaySFX("click");
            }

            InteractionState = MutinyPlayerInteractionState.WeaponArmed;
        }

        private void ResetForCurrentTurn()
        {
            HideTrajectory();
            m_ObservedTeam = TurnManager != null ? TurnManager.CurrentTeam : null;
            m_WasTurnActive = TurnManager != null && TurnManager.CurrentPhase == TurnPhase.TurnActive;
            ClearHoveredCharacter();
            if (m_ObservedTeam != null && !m_ObservedTeam.IsAiControlled && m_ObservedTeam.SelectedCharacter != null)
                m_ObservedTeam.SelectedCharacter.IsSelected = false;
            ActiveWeapon = null;
            InteractionState = MutinyPlayerInteractionState.CharacterSelection;
        }

        private MutinyCharacter GetHumanSelectedCharacter()
        {
            if (TurnManager == null || TurnManager.CurrentPhase != TurnPhase.TurnActive)
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

        private void UpdateHoveredCharacter(MutinyTeam team, Vector3 mouseWorld)
        {
            MutinyCharacter hovered = InteractionState == MutinyPlayerInteractionState.CharacterSelection
                ? FindCharacterNearPosition(mouseWorld, team, CharacterSelectionRadiusPixels)
                : null;

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
