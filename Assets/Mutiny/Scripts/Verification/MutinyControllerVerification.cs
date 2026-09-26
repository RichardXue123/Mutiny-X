using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mutiny.Levels;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Mutiny.Verification
{
    /// <summary>Virtual hardware drives the real action asset, input Update, IMGUI and Twang entries.</summary>
    public sealed class MutinyControllerVerification : MonoBehaviour
    {
        public static bool Finished { get; private set; }
        public static MutinyLevel1VerificationResult Result { get; private set; }
        private Gamepad m_Pad;
        private MutinyFrontendController m_Frontend;

        public static void Begin()
        {
            Finished = false;
            Result = new MutinyLevel1VerificationResult();
            new GameObject("ControllerVerification").AddComponent<MutinyControllerVerification>();
        }

        private void Start() => StartCoroutine(Verify());

        private IEnumerator Press(GamepadButton button, float duration = 0.1f)
        {
            InputSystem.QueueStateEvent(m_Pad, new GamepadState().WithButton(button));
            yield return new WaitForSecondsRealtime(duration);
            InputSystem.QueueStateEvent(m_Pad, new GamepadState());
            yield return new WaitForSecondsRealtime(0.08f);
        }

        private IEnumerator Verify()
        {
            Application.runInBackground = true;
            // The unattended Editor has no OS foreground focus; deliver the real focus event.
            MutinyInputHub.Instance.SendMessage("OnApplicationFocus", true);
            m_Pad = InputSystem.AddDevice<Gamepad>();
            m_Frontend = new GameObject("ControllerFrontendVerification").AddComponent<MutinyFrontendController>();
            GameObject frontendCamera = new GameObject("ControllerFrontendCamera");
            frontendCamera.transform.SetParent(m_Frontend.transform, false);
            frontendCamera.tag = "MainCamera";
            frontendCamera.AddComponent<UnityEngine.Camera>().orthographic = true;
            m_Frontend.Initialize(null);
            yield return new WaitForSecondsRealtime(0.25f);
            yield return Press(GamepadButton.DpadUp);
            yield return Press(GamepadButton.DpadDown);
            yield return Press(GamepadButton.DpadDown);
            ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../controller-ui.png"));
            yield return Press(GamepadButton.South);
            yield return new WaitForSecondsRealtime(0.8f);
            Result.Assert(m_Frontend.CurrentPage == MutinyFrontendPage.Help,
                "GP-NAV-01 actual IMGUI Dpad focus and A enter Help via its production transition");
            yield return Press(GamepadButton.East);
            yield return new WaitForSecondsRealtime(0.8f);
            Result.Assert(m_Frontend.CurrentPage == MutinyFrontendPage.Title,
                "GP-NAV-01 B activates the actual Help Back button");
            yield return Press(GamepadButton.DpadUp);
            yield return Press(GamepadButton.South, 1.1f);
            Result.Assert(m_Frontend.CurrentPage == MutinyFrontendPage.GameSelect,
                "GP-NAV-01 holding A through multiple OnGUI events and a transition executes only Play");
            yield return Press(GamepadButton.DpadUp);
            yield return Press(GamepadButton.South);
            yield return new WaitForSecondsRealtime(0.8f);
            Result.Assert(m_Frontend.CurrentPage == MutinyFrontendPage.LevelSelect,
                "GP-NAV-01 A activates the real one-player button");
            yield return Press(GamepadButton.East);
            yield return new WaitForSecondsRealtime(0.8f);
            Result.Assert(m_Frontend.CurrentPage == MutinyFrontendPage.GameSelect,
                "GP-NAV-01 B returns LevelSelect to GameSelect");
            yield return Press(GamepadButton.East);
            yield return new WaitForSecondsRealtime(0.8f);
            Result.Assert(m_Frontend.CurrentPage == MutinyFrontendPage.Title,
                "GP-NAV-01 B returns GameSelect to Title");
            DestroyImmediate(m_Frontend.gameObject);
            m_Frontend = null;
            yield return VerifyTurnFocusUI();
            yield return VerifyBattleUI();
            yield return VerifyCannonUI();
            try
            {
                VerifyTurnFocus();
                VerifyBattle();
                VerifyTriggerPower();
                VerifySpecialWeapons();
                VerifyCannon();
                InputSystem.RemoveDevice(m_Pad);
                m_Pad = null;
                Merge(MutinyTurnActionUiVerificationTest.RunCharacterAimOverlay(), "mouse/touch cancel");
                Merge(MutinyTurnActionUiVerificationTest.RunScrollArrows(), "scroll arrows");
                Merge(MutinyTurnActionUiVerificationTest.RunCameraMovement(), "camera movement");
                Merge(MutinyTurnActionUiVerificationTest.RunCannon(), "original Cannon/AI");
            }
            catch (Exception error)
            {
                Result.Assert(false, error.ToString());
            }
            finally
            {
                if (m_Pad != null && m_Pad.added) InputSystem.RemoveDevice(m_Pad);
                Finished = true;
                Debug.Log($"[Mutiny Controller] Verification finished: {Result.PassedAssertions}/{Result.TotalAssertions} passed.");
            }
        }

        private IEnumerator VerifyBattleUI()
        {
            using (var rig = new Rig())
            {
                // Freeze simulation only; Update/OnGUI still drive production input and panel animation.
                rig.Turn.enabled = false;
                foreach (MutinyPhysicsBody body in rig.Turn.GetComponentsInChildren<MutinyPhysicsBody>())
                    body.enabled = false;
                MutinyGameHUD hud = rig.Turn.gameObject.AddComponent<MutinyGameHUD>();
                hud.TurnManager = rig.Turn;
                hud.PlayerInput = rig.Input;
                yield return new WaitForSecondsRealtime(0.1f);
                yield return Press(GamepadButton.DpadRight);
                yield return Press(GamepadButton.South);
                yield return new WaitForSecondsRealtime(0.55f);
                Result.Assert(rig.Input.IsActionMenuOpen, "GP-NAV-01 actual HUD opens after controller character confirmation");
                yield return Press(GamepadButton.DpadUp); // Initialize Throw focus.
                yield return Press(GamepadButton.DpadRight); // Cherry Bomb.
                yield return Press(GamepadButton.South);
                Result.Assert(rig.Input.IsWeaponReady && rig.Input.ActiveWeapon == "cherryBomb",
                    "GP-NAV-01 Dpad and A activate the actual HUD weapon slot");
                yield return Press(GamepadButton.East);
                yield return new WaitForSecondsRealtime(0.55f);
                Result.Assert(rig.Input.IsActionMenuOpen, "GP-BACK-01 actual ready B reopens the action panel");
                bool opened = hud.OpenQuitPrompt();
                Result.Assert(opened && hud.IsQuitPromptShowRequested &&
                    MutinyInputHub.Instance.CurrentContext == "quit" && rig.Input.IsActionMenuOpen,
                    "GP-NAV-01 popup owns the input context immediately when requested");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(hud.IsQuitPromptShowRequested && rig.Input.IsActionMenuOpen,
                    "GP-NAV-01 A during popup opening cannot activate an underlying action");
                yield return Press(GamepadButton.East); // Still fading in; must not click an underlying Back.
                Result.Assert(hud.IsQuitPromptShowRequested && rig.Input.IsActionMenuOpen,
                    "GP-NAV-01 fading popup B cannot activate hidden controls or Cancel Character");
                yield return new WaitForSecondsRealtime(0.55f);
                yield return Press(GamepadButton.East);
                yield return new WaitForSecondsRealtime(0.55f);
                yield return Press(GamepadButton.Start);
                Result.Assert(hud.IsQuitPromptVisible && rig.Input.IsActionMenuOpen,
                    "GP-NAV-01 Start opens the real quit popup without executing an underlying action");
                yield return new WaitForSecondsRealtime(0.55f);
                yield return Press(GamepadButton.East);
                yield return new WaitForSecondsRealtime(0.55f);
                Result.Assert(!hud.IsQuitPromptVisible && rig.Input.IsActionMenuOpen,
                    "GP-BACK-01 popup B activates Continue and retains the action selection");
                yield return Press(GamepadButton.East);
                Result.Assert(!rig.Character.IsSelected && rig.Character.CanThrow &&
                    rig.Input.InteractionState == MutinyPlayerInteractionState.CharacterSelection,
                    "GP-BACK-01 actual panel B activates Cancel Character after the popup closes");
            }
        }

        private IEnumerator VerifyTurnFocusUI()
        {
            using (var rig = new Rig(withTeammate: true))
            {
                rig.Turn.enabled = false;
                foreach (MutinyPhysicsBody body in rig.Turn.GetComponentsInChildren<MutinyPhysicsBody>())
                    body.enabled = false;
                MutinyGameHUD hud = rig.Turn.gameObject.AddComponent<MutinyGameHUD>();
                hud.TurnManager = rig.Turn;
                hud.PlayerInput = rig.Input;
                yield return new WaitForSecondsRealtime(0.1f);
                Result.Assert(rig.Input.ControllerFocusedCharacter == rig.Teammate &&
                    rig.Team.SelectedCharacter == null && !rig.Input.IsActionMenuOpen,
                    "GP-CHAR-02 actual Update focuses the camera's captain without selecting or opening a menu");
                float scale = Mathf.Min(Screen.width / 550f, Screen.height / 400f);
                MutinyInputHub.Instance.SetPointerFromGui(new Vector2(
                    (Screen.width - 550f * scale) * 0.5f + 490f * scale,
                    (Screen.height - 400f * scale) * 0.5f + 18f * scale));
                yield return Press(GamepadButton.South, 0.85f);
                Result.Assert(rig.Team.SelectedCharacter == rig.Teammate && rig.Input.IsActionMenuOpen &&
                    !hud.IsQuitPromptShowRequested,
                    "GP-CHAR-02 first A opens the camera captain's real HUD menu instead of a corner control");
                Result.Assert(rig.Input.ActiveWeapon == null && rig.Teammate.CanThrow && rig.Teammate.CanShoot &&
                    rig.Turn.CurrentPhase == TurnPhase.TurnActive,
                    "GP-CHAR-02 holding the first A through HUD opening does not also choose or submit an action");
            }
        }

        private void VerifyTurnFocus()
        {
            using (var rig = new Rig(withTeammate: true))
            {
                Sample(rig, new GamepadState());
                Result.Assert(rig.Camera.TurnFocusCharacter == rig.Teammate &&
                    rig.Input.ControllerFocusedCharacter == rig.Teammate && rig.Team.SelectedCharacter == null,
                    "GP-CHAR-02 turn focus reuses the actual camera captain rather than the first/nearest teammate");
                for (int i = 0; i < 80; i++) rig.Camera.AdvanceCameraForVerification(0.04f);
                Result.Assert(!rig.Camera.IsPanningToTurnTarget && rig.Camera.TurnFocusCharacter == rig.Teammate,
                    "GP-CHAR-02 the camera retains its turn character after the pan finishes");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(rig.Team.SelectedCharacter == rig.Teammate && rig.Input.IsActionMenuOpen,
                    "GP-CHAR-02 A after camera arrival selects its retained target through production selection");
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState().WithButton(GamepadButton.East));
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState().WithButton(GamepadButton.DpadLeft));
                Sample(rig, new GamepadState());
                Result.Assert(rig.Input.ControllerFocusedCharacter == rig.Character && !rig.Input.IsActionMenuOpen,
                    "GP-CHAR-02 Dpad switches away from automatic focus and neutral frames retain the user's choice");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(rig.Team.SelectedCharacter == rig.Character && rig.Input.IsActionMenuOpen,
                    "GP-CHAR-02 A confirms the Dpad teammate rather than restoring the turn's captain");
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState().WithButton(GamepadButton.East));
                Sample(rig, new GamepadState());
                Hold(rig, new GamepadState { leftStick = Vector2.left }, 0.5f);
                Vector2 freePointer = MutinyInputHub.Instance.PointerPosition;
                Sample(rig, new GamepadState());
                Result.Assert(rig.Input.ControllerFocusedCharacter == null &&
                    Vector2.Distance(freePointer, MutinyInputHub.Instance.PointerPosition) < 0.001f,
                    "GP-CHAR-02 left-stick movement frees the pointer and recentering does not reapply automatic focus");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(!rig.Input.IsActionMenuOpen && !rig.Teammate.IsSelected,
                    "GP-CHAR-02 A at empty space after free-pointer movement cannot implicitly select the captain");
            }
            using (var rig = new Rig(withTeammate: true))
            {
                Mouse mouse = Mouse.current;
                bool createdMouse = mouse == null;
                if (createdMouse) mouse = InputSystem.AddDevice<Mouse>();
                Vector2 previousMouse = mouse.position.ReadValue();
                try
                {
                    Sample(rig, new GamepadState());
                    Sample(rig, new GamepadState().WithButton(GamepadButton.DpadLeft));
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = previousMouse + new Vector2(31f, 17f) });
                    Sample(rig, new GamepadState());
                    Result.Assert(!MutinyInputHub.Instance.IsControllerActive &&
                        rig.Input.ControllerFocusedCharacter == null && rig.Team.SelectedCharacter == null,
                        "GP-CHAR-02 mouse mode clears controller focus and does not implicitly select anyone");
                    Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                    Result.Assert(MutinyInputHub.Instance.IsControllerActive &&
                        rig.Team.SelectedCharacter == rig.Teammate && rig.Input.IsActionMenuOpen,
                        "GP-CHAR-02 the first A that switches from mouse to controller already opens the camera target's menu");
                }
                finally
                {
                    if (createdMouse) InputSystem.RemoveDevice(mouse);
                    else InputSystem.QueueStateEvent(mouse, new MouseState { position = previousMouse });
                }
            }
            using (var rig = new Rig(withTeammate: true))
            {
                Sample(rig, new GamepadState().WithButton(GamepadButton.DpadUp));
                rig.Turn.PassTurn();
                for (int i = 0; i < 40 && rig.Turn.CurrentTeam == rig.Team; i++) rig.Turn.AdvanceSimulationTick();
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(rig.Turn.CurrentTeam != rig.Team && rig.Turn.CurrentTeam.IsAiControlled &&
                    rig.Turn.CurrentTeam.SelectedCharacter == null && rig.Input.ControllerFocusedCharacter == null,
                    "GP-CHAR-02 production pass/settlement switches to AI and A cannot automatically choose an AI character");
                _ = rig.Camera.TurnFocusCharacter;
                rig.Turn.PassTurn();
                for (int i = 0; i < 40 && rig.Turn.CurrentTeam != rig.Team; i++) rig.Turn.AdvanceSimulationTick();
                Sample(rig, new GamepadState());
                Result.Assert(rig.Turn.CurrentTeam == rig.Team && rig.Team.TotalTurnsTaken == 2 &&
                    rig.Camera.TurnFocusCharacter == rig.Character &&
                    rig.Input.ControllerFocusedCharacter == rig.Character && rig.Team.SelectedCharacter == null,
                    "GP-CHAR-02 the next human turn refreshes focus from the camera's new nearest-character target");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(rig.Team.SelectedCharacter == rig.Character && rig.Input.IsActionMenuOpen,
                    "GP-CHAR-02 A on the second human turn selects the new camera target, not the previous captain");
            }
            using (var rig = new Rig(withTeammate: true))
            {
                Sample(rig, new GamepadState());
                rig.Teammate.TakeDamage(100f);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(!rig.Teammate.IsAlive && rig.Team.SelectedCharacter != rig.Teammate &&
                    rig.Input.ControllerFocusedCharacter == null && !rig.Input.IsActionMenuOpen,
                    "GP-CHAR-02 a camera target killed via production damage cannot retain focus or be confirmed");
            }
        }

        private void Sample(Rig rig, GamepadState state, float deltaTime = 0.04f)
        {
            InputSystem.QueueStateEvent(m_Pad, state);
            InputSystem.Update();
            MutinyInputHub.Instance.CaptureInput(deltaTime);
            rig.Input.SendMessage("Update");
        }

        private IEnumerator VerifyCannonUI()
        {
            using (var rig = new Rig())
            {
                rig.Turn.enabled = false;
                foreach (MutinyPhysicsBody body in rig.Turn.GetComponentsInChildren<MutinyPhysicsBody>())
                    body.enabled = false;
                rig.Character.AddWeapon("mine", 1);
                MutinyGameHUD hud = rig.Turn.gameObject.AddComponent<MutinyGameHUD>();
                hud.TurnManager = rig.Turn;
                hud.PlayerInput = rig.Input;
                yield return new WaitForSecondsRealtime(0.1f);
                yield return Press(GamepadButton.DpadRight);
                yield return Press(GamepadButton.South);
                yield return new WaitForSecondsRealtime(0.55f);
                yield return Press(GamepadButton.DpadUp); // Throw.
                yield return Press(GamepadButton.DpadRight); // Cherry Bomb.
                yield return Press(GamepadButton.DpadDown); // Banana.
                yield return Press(GamepadButton.DpadDown); // Mine.
                yield return Press(GamepadButton.DpadRight); // Cannon.
                yield return Press(GamepadButton.South);
                Result.Assert(rig.Input.IsControllerCannonReady && rig.Input.ActiveWeapon == "cannon" &&
                    rig.Input.ArmedCannon != null && rig.Character.GetAmmunition("cannon") == 1,
                    $"GP-CAN-01 real HUD Dpad/A equips Cannon without spending (weapon={rig.Input.ActiveWeapon})");
                InputSystem.QueueStateEvent(m_Pad, new GamepadState
                    { leftStick = new Vector2(-1f, -1f), rightTrigger = 1f });
                yield return new WaitForSecondsRealtime(1f);
                InputSystem.QueueStateEvent(m_Pad, new GamepadState());
                yield return new WaitForSecondsRealtime(0.1f);
                Result.Assert(rig.Input.IsControllerAiming && rig.Input.ArmedCannon.IsDraggingPin &&
                    rig.Input.ArmedCannon.PinX < MutinyCannon.PinRestX,
                    "GP-CAN-02 actual Update holds the visually pulled pin after releasing RT");
                ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath, "../controller-cannon-aim.png"));
                yield return new WaitForSecondsRealtime(0.1f);
                yield return Press(GamepadButton.East);
                Result.Assert(rig.Input.IsControllerCannonReady && rig.Input.ArmedCannon != null,
                    "GP-CAN-04 actual Update B exits aiming while retaining the deployed Cannon");
                yield return Press(GamepadButton.East);
                Result.Assert(rig.Input.IsActionMenuOpen && rig.Character.HasWeapon("cannon"),
                    "GP-CAN-04 ready B returns the actual Cannon selection to the action menu");
            }
        }

        private void Hold(Rig rig, GamepadState state, float seconds, float step = 0.04f)
        {
            for (float remaining = seconds; remaining > 0.0001f; remaining -= step)
                Sample(rig, state, Mathf.Min(step, remaining));
        }

        private void Select(Rig rig, string weapon)
        {
            Sample(rig, new GamepadState());
            Sample(rig, new GamepadState().WithButton(GamepadButton.DpadRight));
            Result.Assert(rig.Team.SelectedCharacter == null && rig.Input.ControllerFocusedCharacter != null,
                $"GP-CHAR-01 controller navigation highlights without selecting a character (active={MutinyInputHub.Instance.IsControllerActive}, context={MutinyInputHub.Instance.CurrentContext}, navigation={MutinyInputHub.Instance.Frame.Navigate}, phase={rig.Turn.CurrentPhase})");
            Sample(rig, new GamepadState());
            Sample(rig, new GamepadState().WithButton(GamepadButton.South));
            Result.Assert(rig.Team.SelectedCharacter != null && rig.Input.IsActionMenuOpen,
                "GP-CHAR-01 controller A uses the production character selection transition");
            if (rig.Team.SelectedCharacter == null) throw new InvalidOperationException("Production controller character selection failed.");
            Result.Assert(weapon == null ? rig.Input.SelectCharacterThrow() : rig.Input.SelectControllerWeapon(weapon),
                "GP-AIM-01 selects the ready action through its production command");
            Sample(rig, new GamepadState());
        }

        private void VerifyBattle()
        {
            using (var rig = new Rig())
            {
                Select(rig, "cherryBomb");
                MutinyGameHUD hud = rig.Turn.gameObject.AddComponent<MutinyGameHUD>();
                hud.TurnManager = rig.Turn; hud.PlayerInput = rig.Input;
                MutinyWeapon equipped = rig.Input.EquippedWeapon;
                int ammo = rig.Character.GetAmmunition("cherryBomb");
                Sample(rig, new GamepadState { leftStick = new Vector2(-1f, -1f), rightTrigger = 1f });
                Result.Assert(rig.Input.IsControllerAiming && !equipped.IsFired,
                    "GP-AIM-01 RT begins aiming without firing");
                Vector3 before = rig.Camera.transform.position;
                Sample(rig, new GamepadState { rightStick = Vector2.right, rightTrigger = 1f });
                rig.Camera.AdvanceCameraForVerification(0.04f);
                Result.Assert(rig.Camera.transform.position.x > before.x && rig.Input.IsControllerAiming,
                    "GP-CAM-01 production camera accepts right-stick pan during controller aim");
                Sample(rig, new GamepadState());
                Result.Assert(rig.Input.IsControllerAiming && !equipped.IsFired &&
                    rig.Character.GetAmmunition("cherryBomb") == ammo,
                    "GP-AIM-01 releasing RT never fires or spends ammunition");
                Sample(rig, new GamepadState { rightTrigger = 1f });
                Sample(rig, new GamepadState { rightTrigger = 1f }.WithButton(GamepadButton.South).WithButton(GamepadButton.East));
                Result.Assert(!rig.Input.IsAiming && !equipped.IsFired && rig.Character.CanShoot &&
                    rig.Input.InteractionState == MutinyPlayerInteractionState.WeaponReady,
                    "GP-BACK-01 simultaneous A/B cancels to ready without spending the action");
                Sample(rig, new GamepadState { rightTrigger = 1f });
                Result.Assert(!rig.Input.IsAiming, "GP-BACK-01 held RT cannot restart canceled aim");
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState { leftStick = new Vector2(-1f, -1f), rightTrigger = 1f });
                Hold(rig, new GamepadState { rightTrigger = 1f }, 2f);
                Sample(rig, new GamepadState { rightStick = Vector2.right, rightTrigger = 1f });
                rig.Camera.AdvanceCameraForVerification(0.04f);
                float scale = Mathf.Min(Screen.width / 550f, Screen.height / 400f);
                MutinyInputHub.Instance.SetPointerFromGui(new Vector2(
                    (Screen.width - 550f * scale) * 0.5f + 490f * scale,
                    (Screen.height - 400f * scale) * 0.5f + 18f * scale));
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(equipped.IsFired && !hud.IsQuitPromptShowRequested,
                    "GP-AIM-01 A commits aim even when the software pointer overlaps a corner control");
                Vector2 velocity = new Vector2(equipped.PhysicsBody.State.VelocityX, equipped.PhysicsBody.State.VelocityY);
                Result.Assert(equipped.IsFired && Mathf.Abs(velocity.magnitude - 20f) < 0.001f &&
                    velocity.x > 0f && velocity.y < 0f && Mathf.Abs(velocity.x + velocity.y) < 0.001f,
                    "GP-AIM-01/GP-CAM-01 left-down pull launches right-up at original 20 force after stick recenter and camera pan");
                Result.Assert(rig.Character.GetAmmunition("cherryBomb") == ammo - 1 &&
                    !rig.Character.CanShoot && !rig.Character.CanThrow && rig.Turn.CurrentPhase == TurnPhase.ActionExecuting,
                    "GP-AIM-01 production Twang spends ammunition once and enters the real executing phase");
                Result.Assert(!rig.Input.TryControllerBack(), "GP-BACK-01 committed action cannot be rolled back");
            }
            using (var rig = new Rig())
            {
                Select(rig, "rumBottle");
                MutinyWeapon equipped = rig.Input.EquippedWeapon;
                Hold(rig, new GamepadState { leftStick = Vector2.right, rightTrigger = 1f }, 2f);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(equipped.IsFired && Mathf.Abs(equipped.PhysicsBody.State.VelocityX + 30f) < 0.001f &&
                    Mathf.Abs(equipped.PhysicsBody.State.VelocityY) < 0.001f,
                    "GP-AIM-01 right pull launches Rum Bottle left at its original 30-force Twang cap");
            }
            using (var rig = new Rig())
            {
                Select(rig, "rumBottle");
                MutinyWeapon equipped = rig.Input.EquippedWeapon;
                Hold(rig, new GamepadState { leftStick = Vector2.left, rightTrigger = 0.54f }, 2f);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(equipped.IsFired && Mathf.Abs(equipped.PhysicsBody.State.VelocityX - 15f) < 0.01f,
                    "GP-AIM-01 left pull launches right at half-power through real Twang");
            }
            using (var rig = new Rig())
            {
                Select(rig, "cherryBomb");
                MutinyWeapon equipped = rig.Input.EquippedWeapon;
                int ammo = rig.Character.GetAmmunition("cherryBomb");
                Sample(rig, new GamepadState { rightTrigger = 1f });
                Sample(rig, new GamepadState { rightTrigger = 1f }.WithButton(GamepadButton.South));
                Result.Assert(!equipped.IsFired && rig.Input.IsWeaponReady &&
                    rig.Character.GetAmmunition("cherryBomb") == ammo,
                    "GP-AIM-01 RT and A without any left-stick pull do not invent a launch direction");
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState { leftStick = Vector2.up, rightTrigger = 1f });
                Hold(rig, new GamepadState { rightTrigger = 1f }, 2f);
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(equipped.IsFired &&
                    Mathf.Abs(equipped.PhysicsBody.State.VelocityX) < 0.001f &&
                    Mathf.Abs(equipped.PhysicsBody.State.VelocityY - 20f) < 0.001f,
                    "GP-AIM-01 upward pull launches straight downward and survives stick recenter");
            }
            using (var rig = new Rig())
            {
                Select(rig, null);
                Sample(rig, new GamepadState { leftTrigger = 1f });
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(!rig.Character.IsSelfThrown && rig.Character.CanThrow &&
                    rig.Turn.CurrentPhase == TurnPhase.TurnActive,
                    "GP-AIM-01 zero-power A does not commit or spend a jump");
                Sample(rig, new GamepadState());
                Hold(rig, new GamepadState { leftStick = Vector2.down, rightTrigger = 1f }, 2f);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(rig.Character.IsSelfThrown && !rig.Character.CanThrow &&
                    rig.Character.PhysicsBody.State.VelocityY < 0f &&
                    rig.Turn.CurrentPhase == TurnPhase.ActionExecuting,
                    "GP-AIM-01 downward pull jumps upward through the production commit");
                Result.Assert(!rig.Input.TryControllerBack(), "GP-BACK-01 B cannot reselect after a committed jump");
            }
            using (var rig = new Rig())
            {
                Select(rig, "cherryBomb");
                MutinyWeapon equipped = rig.Input.EquippedWeapon;
                Sample(rig, new GamepadState { rightTrigger = 1f });
                InputSystem.RemoveDevice(m_Pad);
                Result.Assert(!rig.Input.IsAiming && !equipped.IsFired && rig.Character.CanShoot,
                    "GP-DEVICE-01 gamepad removal cancels without firing");
                m_Pad = InputSystem.AddDevice<Gamepad>();
                Sample(rig, new GamepadState { leftTrigger = 1f }.WithButton(GamepadButton.South));
                Result.Assert(MutinyInputHub.Instance.IsAwaitingNeutral && !equipped.IsFired,
                    "GP-DEVICE-01 reconnect with held LT and A requires neutral input");
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState { rightTrigger = 1f });
                MutinyInputHub.Instance.SendMessage("OnApplicationFocus", false);
                Result.Assert(!rig.Input.IsAiming && !equipped.IsFired,
                    "GP-DEVICE-01 focus loss cancels without manufacturing a release");
                MutinyInputHub.Instance.SendMessage("OnApplicationFocus", true);
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState());
                Result.Assert(rig.Input.TryControllerBack() && rig.Input.IsActionMenuOpen &&
                    rig.Character.CanThrow && rig.Character.CanShoot,
                    "GP-BACK-01 ready Back retains action qualifications");
                Result.Assert(rig.Input.SelectControllerWeapon("cannon") && rig.Input.IsControllerCannonReady &&
                    rig.Character.HasWeapon("cannon"), "GP-SCOPE-01 controller now equips Cannon without spending inventory");
                rig.Input.CancelWeaponSelection();
                Result.Assert(rig.Input.SelectWeapon("cannon") && rig.Input.ArmedCannon != null,
                    "GP-SCOPE-01 existing mouse/touch weapon command still equips Cannon");
                rig.Input.CancelWeaponSelection();
                Result.Assert(rig.Input.TryControllerBack() &&
                    rig.Input.InteractionState == MutinyPlayerInteractionState.CharacterSelection,
                    "GP-BACK-01 action-menu Back returns to character selection when permitted");
            }
            using (var rig = new Rig())
            {
                Mouse mouse = InputSystem.AddDevice<Mouse>();
                try
                {
                    Select(rig, "cherryBomb");
                    Sample(rig, new GamepadState { rightTrigger = 1f });
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(100f, 100f) });
                    Sample(rig, new GamepadState { rightTrigger = 1f });
                    Result.Assert(MutinyInputHub.Instance.IsControllerActive && rig.Input.IsControllerAiming,
                        "GP-DEVICE-01 mouse motion cannot steal an active controller aim");
                    Sample(rig, new GamepadState().WithButton(GamepadButton.East));
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(150f, 100f) });
                    Sample(rig, new GamepadState());
                    Result.Assert(!MutinyInputHub.Instance.IsControllerActive && !rig.Input.IsAiming,
                        "GP-DEVICE-01 real mouse motion switches back after controller cancellation");
                    Vector2 origin = MutinyPhysics.UnityToPixel(rig.Input.EquippedWeapon.transform.position);
                    Result.Assert(rig.Input.TryBeginAimFromPrimaryPointerForVerification(rig.Character, origin),
                        "GP-DEVICE-01 original pointer aim remains usable after switching devices");
                    Sample(rig, new GamepadState { rightTrigger = 1f });
                    Result.Assert(!MutinyInputHub.Instance.IsControllerActive && rig.Input.IsAiming &&
                        !rig.Input.IsControllerAiming, "GP-DEVICE-01 RT cannot steal an active mouse aim");
                    rig.Input.CancelCurrentAim();
                    Sample(rig, new GamepadState { rightStick = Vector2.right });
                    Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
                    try
                    {
                        keyboard.MakeCurrent();
                        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.RightArrow));
                        Sample(rig, new GamepadState());
                        Result.Assert(!MutinyInputHub.Instance.IsControllerActive,
                            $"GP-DEVICE-01 an actual keyboard press switches to the original input mode " +
                            $"(keyCurrent={Keyboard.current == keyboard}, pressed={keyboard.rightArrowKey.isPressed}, " +
                            $"newPress={keyboard.anyKey.wasPressedThisFrame}, neutral={MutinyInputHub.Instance.IsAwaitingNeutral}, " +
                            $"controllerAim={rig.Input.IsControllerAiming}, mouseAim={rig.Input.IsAiming})");
                    }
                    finally { InputSystem.RemoveDevice(keyboard); }
                }
                finally { InputSystem.RemoveDevice(mouse); }
            }
        }

        private static void Merge(MutinyLevel1VerificationResult result, string label)
        {
            Result.TotalAssertions += result.TotalAssertions;
            Result.PassedAssertions += result.PassedAssertions;
            if (!result.Passed)
            {
                Result.Passed = false;
                foreach (string failure in result.Failures) Result.Failures.Add(label + ": " + failure);
            }
        }

        private void PointAt(Rig rig, Vector2 pixelPosition)
        {
            Vector3 screen = rig.Input.GameCamera.WorldToScreenPoint(
                MutinyPhysics.PixelToUnity(pixelPosition.x, pixelPosition.y));
            MutinyInputHub.Instance.SetPointerFromGui(new Vector2(screen.x, Screen.height - screen.y));
            Sample(rig, new GamepadState());
        }

        private void VerifyTriggerPower()
        {
            using (var rig = new Rig())
            {
                Select(rig, "cherryBomb");
                MutinyWeapon weapon = rig.Input.EquippedWeapon;
                Sample(rig, new GamepadState { leftTrigger = 0.04f, rightTrigger = 0.04f });
                Result.Assert(!rig.Input.IsAiming && rig.Input.ControllerPower == 0f,
                    "GP-POWER-01 both trigger deadzones leave the weapon ready");
                Sample(rig, new GamepadState { leftStick = Vector2.left, leftTrigger = 1f });
                Result.Assert(rig.Input.IsControllerAiming && rig.Input.ControllerPower == 0f,
                    "GP-POWER-01 LT alone starts aiming at zero power");
                Hold(rig, new GamepadState { rightTrigger = 1f }, 0.5f);
                Result.Assert(Mathf.Abs(rig.Input.ControllerPower - 0.25f) < 0.001f,
                    "GP-POWER-02 full RT for half a second raises power to 25 percent");
                Hold(rig, new GamepadState(), 0.5f);
                Result.Assert(rig.Input.IsControllerAiming &&
                    Mathf.Abs(rig.Input.ControllerPower - 0.25f) < 0.001f && !weapon.IsFired,
                    "GP-POWER-02 releasing both triggers retains aim and accumulated power");
                Hold(rig, new GamepadState { rightTrigger = 0.54f }, 0.5f);
                Result.Assert(Mathf.Abs(rig.Input.ControllerPower - 0.375f) < 0.001f,
                    "GP-POWER-02 half effective RT pressure increases power at half speed");
                Hold(rig, new GamepadState { leftTrigger = 1f }, 0.25f);
                Result.Assert(Mathf.Abs(rig.Input.ControllerPower - 0.25f) < 0.001f,
                    "GP-POWER-02 full LT reduces accumulated power");
                Hold(rig, new GamepadState { leftTrigger = 1f, rightTrigger = 1f }, 0.5f);
                Result.Assert(Mathf.Abs(rig.Input.ControllerPower - 0.25f) < 0.001f,
                    "GP-POWER-02 equal trigger pressures cancel without leaving aim");
                Hold(rig, new GamepadState { leftTrigger = 0.54f, rightTrigger = 1f }, 0.5f);
                Result.Assert(Mathf.Abs(rig.Input.ControllerPower - 0.375f) < 0.001f,
                    "GP-POWER-02 unequal trigger pressures use their net change rate");
                Hold(rig, new GamepadState { leftTrigger = 1f }, 2f);
                Result.Assert(rig.Input.ControllerPower == 0f && rig.Input.IsControllerAiming,
                    "GP-POWER-02 LT clamps power at zero and retains aim");
                Hold(rig, new GamepadState { rightTrigger = 1f }, 3f);
                Result.Assert(rig.Input.ControllerPower == 1f,
                    "GP-POWER-02 RT clamps power at 100 percent");
                Hold(rig, new GamepadState { leftTrigger = 1f }, 1f);
                PointAt(rig, new Vector2(120f, 193f)); // Pointer on cancel cross must not steal aiming A.
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(weapon.IsFired &&
                    Mathf.Abs(weapon.PhysicsBody.State.VelocityX - 10f) < 0.01f &&
                    rig.Character.GetAmmunition("cherryBomb") == 2 &&
                    rig.Turn.CurrentPhase == TurnPhase.ActionExecuting,
                    "GP-AIM-01 A with released triggers commits retained half power via real Twang");
            }
            foreach (float step in new[] { 0.02f, 0.1f })
            using (var rig = new Rig())
            {
                Select(rig, "rumBottle");
                Hold(rig, new GamepadState { rightTrigger = 1f }, 1f, step);
                Result.Assert(Mathf.Abs(rig.Input.ControllerPower - 0.5f) < 0.001f,
                    $"GP-POWER-02 one second of RT gives 50 percent at {step:F2}s sampling");
            }
            using (var rig = new Rig())
            {
                Select(rig, "cherryBomb");
                Hold(rig, new GamepadState { rightTrigger = 1f }, 0.5f);
                Sample(rig, new GamepadState { leftTrigger = 1f }
                    .WithButton(GamepadButton.East).WithButton(GamepadButton.South));
                Result.Assert(!rig.Input.IsAiming && rig.Input.ControllerPower == 0f &&
                    !rig.Input.EquippedWeapon.IsFired,
                    "GP-POWER-01 B wins over A and clears retained power");
                Hold(rig, new GamepadState { leftTrigger = 1f }, 0.2f);
                Sample(rig, new GamepadState { leftTrigger = 1f, rightTrigger = 1f });
                Sample(rig, new GamepadState { rightTrigger = 1f });
                Result.Assert(!rig.Input.IsAiming,
                    "GP-POWER-01 cancellation requires both triggers to release before reentry");
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState { leftTrigger = 1f, leftStick = Vector2.left });
                Result.Assert(rig.Input.IsControllerAiming && rig.Input.ControllerPower == 0f,
                    "GP-POWER-01 a fresh LT after neutral reenters aim");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(!rig.Input.EquippedWeapon.IsFired && rig.Character.GetAmmunition("cherryBomb") == 3,
                    "GP-AIM-01 zero power with a real pull direction still cannot fire");
            }
        }

        private void VerifySpecialWeapons()
        {
            using (var rig = new Rig())
            {
                Select(rig, "anchor");
                MutinyAnchor anchor = rig.Input.EquippedWeapon as MutinyAnchor;
                PointAt(rig, new Vector2(260f, 100f));
                Result.Assert(rig.Input.SpecialWeaponCursorModeForVerification == "Anchor" &&
                    rig.Input.SpecialWeaponCursorTextureForVerification != null,
                    "GP-SPC-01 Anchor uses the original special cursor on controller");
                Sample(rig, new GamepadState { leftTrigger = 1f, rightTrigger = 1f });
                Result.Assert(!rig.Input.IsAiming && rig.Input.IsControllerPointerWeaponReady &&
                    anchor != null && !anchor.IsFired,
                    "GP-SPC-07 triggers cannot start ordinary aim for a click weapon");
                Vector2 before = MutinyInputHub.Instance.PointerPosition;
                Sample(rig, new GamepadState { leftStick = Vector2.right });
                Vector2 after = MutinyInputHub.Instance.PointerPosition;
                Result.Assert(after.x > before.x && rig.Input.SpecialWeaponCursorModeForVerification == "Anchor",
                    "GP-SPC-01 left stick moves the visible Anchor release cursor");
                int ammo = rig.Character.GetAmmunition("anchor");
                float expectedX = MutinyPhysics.UnityToPixel(rig.Input.GameCamera.ScreenToWorldPoint(
                    new Vector3(after.x, after.y, -rig.Input.GameCamera.transform.position.z))).x;
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(anchor != null && anchor.IsFired &&
                    Mathf.Abs(anchor.PhysicsBody.State.X - expectedX) < 0.1f &&
                    Mathf.Abs(anchor.PhysicsBody.State.Y - MutinyAnchor.DropStartYPixels) < 0.1f &&
                    rig.Character.GetAmmunition("anchor") == ammo - 1,
                    "GP-SPC-02 A drops Anchor at the moved cursor X and original fixed Y");
            }
            using (var rig = new Rig())
            {
                Select(rig, "seagull");
                MutinySeagull gull = rig.Input.EquippedWeapon as MutinySeagull;
                PointAt(rig, new Vector2(260f, 110f));
                Result.Assert(rig.Input.SpecialWeaponCursorModeForVerification == "Seagull",
                    "GP-SPC-01 Seagull shows its original controller cursor");
                int ammo = rig.Character.GetAmmunition("seagull");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(gull != null && gull.IsFired && Mathf.Abs(gull.FlightY - 110f) < 0.2f &&
                    rig.Character.GetAmmunition("seagull") == ammo - 1 &&
                    rig.Turn.CurrentPhase == TurnPhase.ActionExecuting,
                    "GP-SPC-02 A starts Seagull at cursor height and spends once");
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                gull?.AdvanceOriginalTickForVerification();
                Result.Assert(gull != null && gull.ActiveShotCount == 1,
                    "GP-SPC-02 A during flight requests one real Seagull shot");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                gull?.AdvanceOriginalTickForVerification();
                Result.Assert(gull != null && gull.ActiveShotCount == 1,
                    "GP-SPC-02 holding A does not repeat Seagull shots");
            }
            using (var rig = new Rig())
            {
                Select(rig, "tidalWave");
                MutinyTidalWave wave = rig.Input.EquippedWeapon as MutinyTidalWave;
                PointAt(rig, new Vector2(260f, 110f));
                Result.Assert(rig.Input.SpecialWeaponCursorModeForVerification == "TidalWave",
                    "GP-SPC-01 Tidal Wave shows its original controller cursor");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(wave != null && wave.IsFired &&
                    Mathf.Abs(wave.PhysicsBody.State.X - MutinyTidalWave.OriginalStartX) < 0.1f &&
                    Mathf.Abs(wave.PhysicsBody.State.Y + rig.Root.WaterLevelY * MutinyPhysics.PixelsPerUnit) < 0.1f,
                    "GP-SPC-02 Tidal Wave starts at its fixed original edge and waterline");
            }
            foreach (string type in new[] { "woodenCrate", "gunpowderBarrel" })
            using (var rig = new Rig())
            {
                Select(rig, type);
                MutinyWeapon equipped = rig.Input.EquippedWeapon;
                // A real empty map supports placement; the 1x1 UI fixture does not.
                equipped.PhysicsBody.SetTerrain(new string[60, 100], 100, 60);
                int ammo = rig.Character.GetAmmunition(type);
                PointAt(rig, new Vector2(120f, 160f));
                Result.Assert(rig.Input.SpecialWeaponCursorModeForVerification == "Cross",
                    $"GP-SPC-03 {type} shows the original invalid placement cross");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(rig.Character.GetAmmunition(type) == ammo && !equipped.IsFired,
                    $"GP-SPC-03 {type} rejects A on occupied character without spending");
                Sample(rig, new GamepadState());
                PointAt(rig, new Vector2(260f, 110f));
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                bool isCrate = type == "woodenCrate";
                int firstCount = isCrate ? ((MutinyWoodenCrate)equipped).PlacedCount :
                    ((MutinyGunpowderBarrel)equipped).PlacedCount;
                Result.Assert(firstCount == 1 && rig.Character.GetAmmunition(type) == ammo - 1 &&
                    rig.Turn.CurrentPhase == TurnPhase.ActionExecuting,
                    $"GP-SPC-03 {type} first A places once and preserves the executing stage");
                Sample(rig, new GamepadState());
                PointAt(rig, new Vector2(460f, 110f));
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                int secondCount = isCrate ? ((MutinyWoodenCrate)equipped).PlacedCount :
                    ((MutinyGunpowderBarrel)equipped).PlacedCount;
                Result.Assert(secondCount == 2 && rig.Character.GetAmmunition(type) == ammo - 1,
                    $"GP-SPC-03 {type} accepts second A while ActionExecuting without extra ammo " +
                    $"(count={secondCount}, ammo={rig.Character.GetAmmunition(type)}, phase={rig.Turn.CurrentPhase}, " +
                    $"weapon={rig.Input.ActiveWeapon}, state={rig.Input.InteractionState}, " +
                    $"gate={rig.Input.CanProcessCurrentTurnInputForVerification()}, " +
                    $"context={MutinyInputHub.Instance.CurrentContext}, confirm={MutinyInputHub.Instance.Frame.Confirm})");
                if (isCrate)
                {
                    Sample(rig, new GamepadState());
                    PointAt(rig, new Vector2(660f, 110f));
                    Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                    Result.Assert(((MutinyWoodenCrate)equipped).PlacedCount == 3 &&
                        rig.Character.GetAmmunition(type) == ammo - 1,
                        "GP-SPC-03 Wooden Crate accepts the third A without extra ammo");
                }
            }
            using (var rig = new Rig())
            {
                Select(rig, "voodooDoll");
                MutinyVoodooDoll doll = rig.Input.EquippedWeapon as MutinyVoodooDoll;
                PointAt(rig, new Vector2(280f, 160f));
                Result.Assert(rig.Input.SpecialWeaponCursorModeForVerification == "VoodooDoll",
                    "GP-SPC-04 Voodoo target stage shows original doll cursor");
                Sample(rig, new GamepadState { rightTrigger = 1f });
                Result.Assert(!rig.Input.IsAiming, "GP-SPC-04 Voodoo cannot aim before target binding");
                Sample(rig, new GamepadState());
                PointAt(rig, new Vector2(360f, 160f));
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Sample(rig, new GamepadState());
                Result.Assert(doll != null && doll.TargetCharacter == rig.Enemy &&
                    rig.Input.SpecialWeaponCursorModeForVerification == "None",
                    "GP-SPC-04 A binds the enemy and clears the doll cursor");
                Hold(rig, new GamepadState { leftStick = Vector2.left, rightTrigger = 1f }, 2f);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(doll != null && doll.IsFired,
                    "GP-SPC-04 RT, pull, A throws the bound Voodoo Doll");
            }
            using (var rig = new Rig())
            {
                Select(rig, "banana");
                MutinyBanana banana = rig.Input.EquippedWeapon as MutinyBanana;
                Hold(rig, new GamepadState { leftStick = Vector2.left, rightTrigger = 1f }, 2f);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                banana?.AdvanceOriginalTickForVerification();
                Result.Assert(banana != null && (banana.IsFinished || !MutinyBanana.HasPlayerDetonatableBanana(rig.Team)),
                    "GP-SPC-05 a fresh A detonates a flying Banana through the production request");
            }
            using (var rig = new Rig())
            {
                Select(rig, "parachuteBomb");
                MutinyParachuteBomb bomb = rig.Input.EquippedWeapon as MutinyParachuteBomb;
                Hold(rig, new GamepadState { leftStick = Vector2.down, rightTrigger = 1f }, 2f);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                bomb?.PhysicsBody.AdvanceSimulationTick();
                Result.Assert(bomb != null && bomb.IsFired && !bomb.IsFanActive,
                    "GP-SPC-05 launch A cannot immediately activate Parachute fan");
                Sample(rig, new GamepadState());
                bomb?.SendMessage("Update");
                PointAt(rig, new Vector2(0f, 100f));
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                bomb?.PhysicsBody.AdvanceSimulationTick();
                Result.Assert(bomb != null && bomb.IsFanActive &&
                    rig.Input.SpecialWeaponCursorModeForVerification == "ParachuteFan" &&
                    rig.Input.InteractionState == MutinyPlayerInteractionState.WeaponReady,
                    $"GP-SPC-05 fresh held A fans the Parachute Bomb at the software cursor side " +
                    $"(fired={bomb?.IsFired}, active={bomb?.IsFanActive}, ready={bomb?.ControllerFanCanStart}, " +
                    $"cursor={rig.Input.SpecialWeaponCursorModeForVerification}, " +
                    $"held={MutinyInputHub.Instance.IsConfirmHeld}, context={MutinyInputHub.Instance.CurrentContext})");
            }
            using (var rig = new Rig())
            {
                Select(rig, "piecesOfEight");
                MutinyPiecesOfEight coins = rig.Input.EquippedWeapon as MutinyPiecesOfEight;
                int ammo = rig.Character.GetAmmunition("piecesOfEight");
                Hold(rig, new GamepadState { leftStick = Vector2.left, rightTrigger = 1f }, 2f);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(coins != null && coins.IsFired,
                    "GP-SPC-05 first Piece of Eight uses real controller Twang");
                coins?.ResolveCoinForVerification(false);
                Sample(rig, new GamepadState());
                Hold(rig, new GamepadState { leftStick = Vector2.left, rightTrigger = 1f }, 2f);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(coins != null && coins.IsFired && coins.TimesFired == 1 &&
                    rig.Character.GetAmmunition("piecesOfEight") == ammo - 1,
                    "GP-SPC-05 second Piece of Eight works in executing phase without extra ammo");
            }
        }

        private static Vector2 CannonPosition(MutinyCannon cannon) =>
            new Vector2(cannon.PhysicsBody.State.X, cannon.PhysicsBody.State.Y);

        private void VerifyCannon()
        {
            using (var rig = new Rig())
            {
                Select(rig, "cannon");
                MutinyCannon cannon = rig.Input.ArmedCannon;
                cannon.PhysicsBody.SetTerrain(new string[60, 100], 100, 60);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(!cannon.IsFired && !cannon.IsFirePending && rig.Character.HasWeapon("cannon"),
                    "GP-CAN-03 ready A cannot fire before entering Cannon aim");
                Vector2 initial = CannonPosition(cannon);
                Hold(rig, new GamepadState { leftStick = Vector2.right }, 0.12f);
                Vector2 placed = CannonPosition(cannon);
                Result.Assert(placed.x > initial.x && !rig.Input.IsControllerAiming && !cannon.IsDraggingBody,
                    "GP-CAN-01 left stick deploys the Cannon through actual collision motion");
                Sample(rig, new GamepadState { leftTrigger = 1f });
                Result.Assert(rig.Input.IsControllerAiming && cannon.IsDraggingPin &&
                    rig.Input.ControllerPower == 0f,
                    "GP-CAN-02 LT starts Cannon aiming at zero visual power");
                Sample(rig, new GamepadState { leftStick = new Vector2(-1f, -1f) });
                Result.Assert(Vector2.Distance(CannonPosition(cannon), placed) < 0.01f &&
                    cannon.RotationDegrees == 315,
                    "GP-CAN-02 left-down pull aims Cannon right-up without relocating its body");
                Hold(rig, new GamepadState { rightTrigger = 1f }, 1f);
                Result.Assert(Mathf.Abs(rig.Input.ControllerPower - 0.5f) < 0.001f &&
                    Mathf.Abs(cannon.PinX + 30.5f) < 0.01f &&
                    Mathf.Abs(cannon.PinRenderer.transform.localPosition.x * MutinyPhysics.PixelsPerUnit + 30.5f) < 0.01f,
                    "GP-CAN-02 half visual power places the real pin at -30.5 pixels");
                Vector3 cameraBefore = rig.Camera.transform.position;
                Sample(rig, new GamepadState { rightStick = Vector2.right });
                rig.Camera.AdvanceCameraForVerification(0.04f);
                Result.Assert(rig.Camera.transform.position.x > cameraBefore.x &&
                    Vector2.Distance(CannonPosition(cannon), placed) < 0.01f &&
                    Mathf.Abs(rig.Input.ControllerPower - 0.5f) < 0.001f && cannon.RotationDegrees == 315,
                    "GP-CAN-02 right-stick camera pan preserves Cannon placement, direction and visual power");
                Sample(rig, new GamepadState { rightTrigger = 1f }
                    .WithButton(GamepadButton.South).WithButton(GamepadButton.East));
                Result.Assert(rig.Input.IsControllerCannonReady && !cannon.IsDraggingPin &&
                    Mathf.Abs(cannon.PinX - MutinyCannon.PinRestX) < 0.01f &&
                    Vector2.Distance(CannonPosition(cannon), placed) < 0.01f &&
                    rig.Input.ControllerPower == 0f && rig.Character.GetAmmunition("cannon") == 1 &&
                    rig.Turn.CurrentPhase == TurnPhase.TurnActive,
                    "GP-CAN-04 B wins over A, returns to deployment and retains the equipped Cannon position");
                Sample(rig, new GamepadState { rightTrigger = 1f });
                Result.Assert(!rig.Input.IsControllerAiming, "GP-CAN-04 held RT cannot reenter canceled Cannon aim");
                Sample(rig, new GamepadState { leftStick = Vector2.left });
                Result.Assert(CannonPosition(cannon).x < placed.x && rig.Input.IsControllerCannonReady,
                    "GP-CAN-04 left stick can reposition the Cannon again after B");
                Sample(rig, new GamepadState { rightTrigger = 1f });
                Result.Assert(rig.Input.IsControllerAiming, "GP-CAN-04 a fresh RT after neutral reenters Cannon aim");
                MutinyInputHub.Instance.SendMessage("OnApplicationFocus", false);
                Result.Assert(!rig.Input.IsControllerAiming && !cannon.IsDraggingPin && !cannon.IsFirePending &&
                    rig.Character.HasWeapon("cannon"), "GP-CAN-04 focus loss cancels Cannon aim without loading");
                MutinyInputHub.Instance.SendMessage("OnApplicationFocus", true);
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState { leftTrigger = 1f });
                InputSystem.RemoveDevice(m_Pad);
                Result.Assert(!rig.Input.IsControllerAiming && !cannon.IsDraggingPin && !cannon.IsFirePending,
                    "GP-CAN-04 disconnect cancels the real Cannon pin without firing");
                m_Pad = InputSystem.AddDevice<Gamepad>();
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState());
                Sample(rig, new GamepadState().WithButton(GamepadButton.East));
                Result.Assert(rig.Input.IsActionMenuOpen && rig.Character.HasWeapon("cannon"),
                    "GP-CAN-04 a second B from deployment returns to the action menu");
            }
            using (var rig = new Rig())
            {
                Select(rig, "cannon");
                MutinyCannon cannon = rig.Input.ArmedCannon;
                cannon.PhysicsBody.SetTerrain(new string[60, 100], 100, 60);
                Hold(rig, new GamepadState { leftStick = Vector2.up }, 2f);
                Result.Assert(Vector2.Distance(CannonPosition(cannon), cannon.PlacementCenterPixels) <=
                    MutinyCannon.PlacementRadius + 0.1f && cannon.PhysicsBody.State.Y < 0f,
                    "GP-CAN-01 held deployment stick stops at the original upper placement circle");
            }
            using (var rig = new Rig())
            {
                Select(rig, "cannon");
                MutinyCannon cannon = rig.Input.ArmedCannon;
                var terrain = new string[60, 100];
                for (int y = 0; y < 60; y++) terrain[y, 6] = "wall";
                cannon.PhysicsBody.SetTerrain(terrain, 100, 60);
                Hold(rig, new GamepadState { leftStick = Vector2.right }, 0.5f);
                Result.Assert(cannon.PhysicsBody.State.X > 150f && cannon.PhysicsBody.State.X <= 182.1f,
                    $"GP-CAN-01 controller deployment cannot pass a real terrain wall (x={cannon.PhysicsBody.State.X:F2})");
            }
            using (var rig = new Rig(withBoxObstacle: true))
            {
                Result.Assert(rig.BoxObstacle != null && rig.BoxObstacle.IsFired,
                    "GP-CAN-01 fixture creates a real registered Wooden Crate before initializing the test turn");
                Select(rig, "cannon");
                MutinyCannon cannon = rig.Input.ArmedCannon;
                cannon.PhysicsBody.SetTerrain(new string[60, 100], 100, 60);
                Hold(rig, new GamepadState { leftStick = Vector2.right }, 0.2f);
                Result.Assert(cannon.PhysicsBody.State.X > 120f && cannon.PhysicsBody.State.X <= 154.1f,
                    $"GP-CAN-01 controller deployment respects the registered box obstacle (x={cannon.PhysicsBody.State.X:F2})");
            }
            foreach (float chargeSeconds in new[] { 0f, 1f, 2f })
            using (var rig = new Rig())
            {
                Select(rig, "cannon");
                MutinyCannon cannon = rig.Input.ArmedCannon;
                Sample(rig, new GamepadState { leftTrigger = 1f, leftStick = new Vector2(-1f, -1f) });
                Hold(rig, new GamepadState { rightTrigger = 1f }, chargeSeconds);
                float visualPower = rig.Input.ControllerPower;
                Result.Assert(Mathf.Abs(visualPower - chargeSeconds * 0.5f) < 0.001f,
                    $"GP-CAN-02 Cannon visual charge reaches {chargeSeconds * 50f:F0} percent");
                Vector2 placed = CannonPosition(cannon);
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(cannon.IsFirePending && rig.Character.GetAmmunition("cannon") == 0 &&
                    rig.Turn.CurrentPhase == TurnPhase.ActionExecuting && !rig.Turn.CheckAllBodiesAtRest(),
                    $"GP-CAN-03 A at {visualPower:P0} loads the real shot, spends once and blocks settlement");
                // Pin return and spawning use the actual Cannon production tick.
                for (int tick = 0; tick < 2 && !cannon.IsFired; tick++) cannon.AdvanceOriginalTickForVerification();
                Vector2 velocity = cannon.Cannonball != null
                    ? new Vector2(cannon.Cannonball.PhysicsBody.State.VelocityX, cannon.Cannonball.PhysicsBody.State.VelocityY)
                    : Vector2.zero;
                Result.Assert(cannon.IsFired && Mathf.Abs(velocity.magnitude - 30f) < 0.01f &&
                    velocity.x > 0f && velocity.y < 0f && Mathf.Abs(velocity.x + velocity.y) < 0.01f &&
                    Vector2.Distance(CannonPosition(cannon), placed) < 0.01f,
                    $"GP-CAN-03 {visualPower:P0} pin creates a separate right-up 30-force ball without moving the body");
                Sample(rig, new GamepadState().WithButton(GamepadButton.South));
                Result.Assert(rig.Character.GetAmmunition("cannon") == 0 && !rig.Input.TryControllerBack(),
                    "GP-CAN-03 held A cannot double-spend; B cannot undo the committed Cannon");
            }
        }

        private sealed class Rig : IDisposable
        {
            private readonly GameObject m_Host = new GameObject("ControllerBattleVerification");
            private readonly TextAsset m_Xml;
            private readonly HashSet<MutinyWeapon> m_ExistingWeapons = new HashSet<MutinyWeapon>(
                UnityEngine.Object.FindObjectsByType<MutinyWeapon>(FindObjectsSortMode.None));
            private readonly HashSet<MutinyExplosion> m_ExistingExplosions = new HashSet<MutinyExplosion>(
                UnityEngine.Object.FindObjectsByType<MutinyExplosion>(FindObjectsSortMode.None));
            public readonly MutinyTurnManager Turn;
            public readonly MutinyTeam Team;
            public readonly MutinyCharacter Character;
            public readonly MutinyCharacter Enemy;
            public readonly MutinyCharacter Teammate;
            public readonly MutinyLevelRoot Root;
            public readonly MutinyWoodenCrate BoxObstacle;
            public readonly MutinyPlayerInput Input;
            public readonly MutinyCameraController Camera;

            public Rig(bool withBoxObstacle = false, bool withTeammate = false)
            {
                MutinyLevelRoot root = m_Host.AddComponent<MutinyLevelRoot>();
                Root = root;
                root.Width = 100; root.Height = 60; root.WaterLevelY = -100f;
                Team = Child("Team1").AddComponent<MutinyTeam>();
                Team.TeamNumber = 1;
                MutinyTeam enemy = Child("Team2").AddComponent<MutinyTeam>();
                enemy.TeamNumber = 2; enemy.IsAiControlled = true;
                Character = Child("Player").AddComponent<MutinyCharacter>();
                Character.TeamIndex = 1;
                Character.PhysicsBody.State = PhysicsBodyState.CreateDefault(120f, 160f);
                Character.transform.position = MutinyPhysics.PixelToUnity(120f, 160f);
                Character.AddWeapon("cherryBomb", 3); Character.AddWeapon("rumBottle", 2); Character.AddWeapon("cannon", 1);
                foreach (string special in new[] { "anchor", "seagull", "tidalWave", "woodenCrate",
                    "gunpowderBarrel", "voodooDoll", "banana", "parachuteBomb", "piecesOfEight" })
                    Character.AddWeapon(special, 2);
                MutinyCharacter opponent = Child("Enemy").AddComponent<MutinyCharacter>();
                Enemy = opponent;
                opponent.TeamIndex = 2;
                opponent.PhysicsBody.State = PhysicsBodyState.CreateDefault(360f, 160f);
                opponent.transform.position = MutinyPhysics.PixelToUnity(360f, 160f);
                Team.RegisterCharacter(Character); enemy.RegisterCharacter(opponent);
                root.Team1 = Team; root.Team2 = enemy;
                root.AllCharacters.Add(Character); root.AllCharacters.Add(opponent);
                if (withTeammate)
                {
                    Teammate = Child("Captain").AddComponent<MutinyCharacter>();
                    Teammate.TeamIndex = 1;
                    Teammate.CharacterType = "RedCaptain";
                    Teammate.PhysicsBody.State = PhysicsBodyState.CreateDefault(720f, 160f);
                    Teammate.transform.position = MutinyPhysics.PixelToUnity(720f, 160f);
                    Teammate.AddWeapon("cherryBomb", 1);
                    Team.RegisterCharacter(Teammate);
                    root.AllCharacters.Add(Teammate);
                }
                if (withBoxObstacle)
                {
                    // Environment setup precedes Turn.Initialize. Calling place
                    // after it would commit the human test turn before selection.
                    BoxObstacle = MutinyWeaponFactory.SpawnWeapon("woodenCrate", opponent) as MutinyWoodenCrate;
                    BoxObstacle.PhysicsBody.SetTerrain(new string[60, 100], 100, 60);
                    if (!BoxObstacle.TryPlaceAt(new Vector2(180f, 150f)))
                        throw new InvalidOperationException("Production Cannon obstacle fixture could not place its crate.");
                }
                Turn = m_Host.AddComponent<MutinyTurnManager>();
                Turn.Initialize(Team, enemy);
                MutinyLevelController loader = m_Host.AddComponent<MutinyLevelController>();
                loader.enabled = false;
                m_Xml = new TextAsset("<level width=\"1\" height=\"1\" players=\"1\"><row>-</row><bgRow>-</bgRow></level>");
                loader.LevelXml = m_Xml;
                GameObject cameraObject = Child("Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(8f, -6f, -10f);
                UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
                camera.orthographic = true; camera.orthographicSize = 6.25f;
                Camera = cameraObject.AddComponent<MutinyCameraController>();
                Input = m_Host.AddComponent<MutinyPlayerInput>();
                Input.TurnManager = Turn; Input.GameCamera = camera;
                Camera.TurnManager = Turn; Camera.PlayerInput = Input; Camera.SetLevelRootForVerification(root);
                Input.SendMessage("Start");
            }

            private GameObject Child(string name)
            {
                GameObject child = new GameObject(name);
                child.transform.SetParent(m_Host.transform, false);
                return child;
            }

            public void Dispose()
            {
                foreach (MutinyWeapon weapon in UnityEngine.Object.FindObjectsByType<MutinyWeapon>(FindObjectsSortMode.None))
                    if (!m_ExistingWeapons.Contains(weapon)) UnityEngine.Object.DestroyImmediate(weapon.gameObject);
                foreach (MutinyExplosion explosion in UnityEngine.Object.FindObjectsByType<MutinyExplosion>(FindObjectsSortMode.None))
                    if (!m_ExistingExplosions.Contains(explosion)) UnityEngine.Object.DestroyImmediate(explosion.gameObject);
                UnityEngine.Object.DestroyImmediate(m_Host);
                UnityEngine.Object.DestroyImmediate(m_Xml);
            }
        }
    }
}
