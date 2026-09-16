using System.Collections.Generic;
using Mutiny.Levels;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Verification
{
    public static class MutinyTurnActionUiVerificationTest
    {
        private static readonly string[] ThrowButtonResources =
        {
            "UI/button_throw_disabled",
            "UI/button_throw_red_up",
            "UI/button_throw_red_over",
            "UI/button_throw_blue_up",
            "UI/button_throw_blue_over"
        };

        private static readonly string[] WeaponSlotResources =
        {
            "UI/weapon_slot_disabled",
            "UI/weapon_slot_red_up",
            "UI/weapon_slot_blue_up",
            "UI/weapon_slot_over"
        };

        private static readonly string[] EndTurnButtonResources =
        {
            "UI/button_end_turn_red_up",
            "UI/button_end_turn_red_over",
            "UI/button_end_turn_blue_up",
            "UI/button_end_turn_blue_over"
        };

        private static readonly string[] WeaponIconResources =
        {
            "cherryBomb", "boulder", "dynamite", "piecesOfEight", "rumBottle",
            "banana", "parachuteBomb", "woodenCrate", "gunpowderBarrel", "seagull",
            "mine", "cannon", "anchor", "voodooDoll", "tidalWave"
        };

        public static MutinyLevel1VerificationResult Run()
        {
            var result = new MutinyLevel1VerificationResult();
            VerifyVisualStateMapping(result);
            VerifyPanelTiming(result);
            VerifyRuntimeResources(result);
            VerifyOriginalPanelCopy(result);
            VerifyOriginalRotation(result);
            VerifyAirDropsDisabled(result);
            VerifyLandDeath(result);
            VerifyCharacterTimeline(result);
            VerifyFrontendFlow(result);
            VerifyBattleHud(result);
            VerifyProductionActionMethods(result);
            VerifyAiDecisionFlow(result);
            VerifyCharacterOverlay(result);
            return result;
        }

        public static MutinyLevel1VerificationResult RunBattleHud()
        {
            var result = new MutinyLevel1VerificationResult();
            VerifyBattleHud(result);
            return result;
        }

        private static void VerifyCharacterOverlay(MutinyLevel1VerificationResult result)
        {
            GameObject characterObject = null;
            GameObject teamObject = null;
            GameObject turnManagerObject = null;
            GameObject playerInputObject = null;
            try
            {
                bool resourcesPresent =
                    Resources.Load<Texture2D>("UI/CharacterOverlay/p1_indicator") != null &&
                    Resources.Load<Texture2D>("UI/CharacterOverlay/p2_indicator") != null &&
                    Resources.Load<Texture2D>("UI/CharacterOverlay/cpu_indicator") != null &&
                    Resources.Load<Texture2D>("UI/CharacterOverlay/health_background") != null;
                result.Assert(resourcesPresent,
                    "CHAR-OVR-01 original CPU/P1/P2 and health-frame textures load through Resources");

                characterObject = new GameObject("CharacterOverlayVerification");
                MutinyCharacter character = characterObject.AddComponent<MutinyCharacter>();
                MutinyCharacterOverlay overlay = characterObject.AddComponent<MutinyCharacterOverlay>();
                teamObject = new GameObject("CharacterOverlayVerification_Team");
                MutinyTeam team = teamObject.AddComponent<MutinyTeam>();
                team.TeamNumber = 1;
                team.RegisterCharacter(character);
                turnManagerObject = new GameObject("CharacterOverlayVerification_TurnManager");
                MutinyTurnManager turnManager = turnManagerObject.AddComponent<MutinyTurnManager>();
                turnManager.CurrentTeam = team;
                turnManager.CurrentPhase = TurnPhase.TurnActive;
                PhysicsBodyState state = PhysicsBodyState.CreateDefault(64f, 64f);
                state.Weight = 0f;
                character.PhysicsBody.State = state;
                character.ShownHealth = 100f;
                character.MaxHealth = 100f;
                overlay.RefreshVisualStateForVerification();
                result.Assert(overlay.IsHealthBarVisible && overlay.CurrentHealthFrame == 28,
                    "CHAR-OVR-04 production overlay maps 100 shown health to original health frame 28");
                result.Assert(overlay.IsTurnIndicatorVisible,
                    "CHAR-OVR-02 production overlay displays the current team's static turn marker");

                turnManager.CurrentTeam = null;
                overlay.RefreshVisualStateForVerification();
                result.Assert(!overlay.IsTurnIndicatorVisible && overlay.IsHealthBarVisible,
                    "CHAR-OVR-02/03 production overlay hides only the marker for a non-current static team");
                turnManager.CurrentTeam = team;

                character.ShownHealth = 50f;
                overlay.RefreshVisualStateForVerification();
                result.Assert(overlay.CurrentHealthFrame == 15,
                    "CHAR-OVR-04 production overlay maps shown health through ceil(27 * health / maxHealth)");

                state = character.PhysicsBody.State;
                state.VelocityX = 1f;
                character.PhysicsBody.State = state;
                overlay.RefreshVisualStateForVerification();
                result.Assert(overlay.IsHealthBarVisible && overlay.IsTurnIndicatorVisible,
                    "CHAR-OVR-02/03 production overlay keeps both elements visible during passive physics motion");

                state.VelocityX = 0f;
                character.PhysicsBody.State = state;
                team.SelectCharacter(character);
                playerInputObject = new GameObject("CharacterOverlayVerification_PlayerInput");
                MutinyPlayerInput playerInput = playerInputObject.AddComponent<MutinyPlayerInput>();
                playerInput.TurnManager = turnManager;
                bool selfThrowCommitted = playerInput.TryCommitCharacterThrow(
                    character, new Vector2(64f, 64f), new Vector2(84f, 64f));
                overlay.RefreshVisualStateForVerification();
                result.Assert(selfThrowCommitted && character.IsSelfThrown &&
                              !overlay.IsHealthBarVisible && !overlay.IsTurnIndicatorVisible,
                    "CHAR-OVR-02/03 production self-throw entry hides both elements without treating passive motion as a throw");

                team.ContinueSelectedCharacterAfterAction();
                overlay.RefreshVisualStateForVerification();
                result.Assert(!character.IsSelfThrown && overlay.IsHealthBarVisible && overlay.IsTurnIndicatorVisible,
                    "CHAR-OVR-02/03 production continuation clears self-throw overlay suppression");

                character.TakeDamage(100f);
                result.Assert(!overlay.IsHealthBarVisible && !overlay.IsTurnIndicatorVisible,
                    "CHAR-OVR-03 production death event hides the complete character overlay");
            }
            finally
            {
                DestroyNow(characterObject);
                DestroyNow(teamObject);
                DestroyNow(turnManagerObject);
                DestroyNow(playerInputObject);
            }
        }

        private static void VerifyAiDecisionFlow(MutinyLevel1VerificationResult result)
        {
            GameObject aiTeamObject = null;
            GameObject enemyTeamObject = null;
            GameObject aiCharacterObject = null;
            GameObject enemyCharacterObject = null;

            try
            {
                aiTeamObject = new GameObject("AiParityVerification_Team");
                MutinyTeam aiTeam = aiTeamObject.AddComponent<MutinyTeam>();
                aiTeam.TeamNumber = 2;
                aiTeam.IsAiControlled = true;
                MutinyAIController ai = aiTeamObject.AddComponent<MutinyAIController>();

                enemyTeamObject = new GameObject("AiParityVerification_EnemyTeam");
                MutinyTeam enemyTeam = enemyTeamObject.AddComponent<MutinyTeam>();
                enemyTeam.TeamNumber = 1;

                aiCharacterObject = new GameObject("AiParityVerification_Pirate");
                MutinyCharacter pirate = aiCharacterObject.AddComponent<MutinyCharacter>();
                pirate.TeamIndex = 2;
                pirate.CanThrow = true;
                pirate.CanShoot = true;
                PhysicsBodyState pirateState = PhysicsBodyState.CreateDefault(64f, 100f);
                pirate.PhysicsBody.State = pirateState;
                pirate.PhysicsBody.WaterPixelY = 0f;
                pirate.PhysicsBody.SetTerrain(null, 0, 0);
                aiTeam.RegisterCharacter(pirate);

                enemyCharacterObject = new GameObject("AiParityVerification_Enemy");
                MutinyCharacter enemy = enemyCharacterObject.AddComponent<MutinyCharacter>();
                enemy.TeamIndex = 1;
                PhysicsBodyState enemyState = PhysicsBodyState.CreateDefault(400f, 100f);
                enemy.PhysicsBody.State = enemyState;
                enemy.PhysicsBody.WaterPixelY = 0f;
                enemy.PhysicsBody.SetTerrain(null, 0, 0);
                enemyTeam.RegisterCharacter(enemy);

                // The water penalty makes every sampled jump unattractive.  The real
                // production evaluator must nevertheless choose the best of its 50
                // samples during phase one (AI-SEL-02 / AI-MOVE-01).
                AIMove firstAction = ai.EvaluateBestMove();
                result.Assert(firstAction.MoveType == AIMoveType.SelfThrow &&
                              firstAction.Character == pirate && firstAction.Score <= 0f,
                    "AI-SEL-02 production evaluator keeps and selects a non-positive first-phase jump candidate");

                pirate.CanThrow = false;
                pirate.CanShoot = true;
                aiTeam.SelectCharacter(pirate);
                AIMove continuation = ai.EvaluateBestMove();
                result.Assert(continuation.MoveType == AIMoveType.Pass,
                    "AI-SEL-03 production evaluator bails out only in the post-jump weapon continuation phase");
            }
            finally
            {
                DestroyNow(enemyCharacterObject);
                DestroyNow(aiCharacterObject);
                DestroyNow(enemyTeamObject);
                DestroyNow(aiTeamObject);
            }
        }

        private static void VerifyFrontendFlow(MutinyLevel1VerificationResult result)
        {
            var flow = new MutinyFrontendFlow();
            result.Assert(flow.CurrentPage == MutinyFrontendPage.Title,
                "FRONT-01 production front-end route starts at the title page");

            flow.PressPlay();
            result.Assert(flow.CurrentPage == MutinyFrontendPage.GameSelect,
                "FRONT-01 Play uses the production route to open game select");

            flow.PressGameSelectBack();
            result.Assert(flow.CurrentPage == MutinyFrontendPage.Title,
                "FRONT-02 game-select Back returns to the title page");

            flow.PressPlay();
            flow.PressOnePlayer();
            result.Assert(flow.CurrentPage == MutinyFrontendPage.LevelSelect,
                "FRONT-02 1 Player uses the production route to open level select");

            bool lockedAccepted = flow.TrySelectLevel(2, level => level == 1);
            result.Assert(!lockedAccepted && flow.CurrentPage == MutinyFrontendPage.LevelSelect,
                "FRONT-06 production route rejects a locked level");

            bool firstAccepted = flow.TrySelectLevel(1, level => level == 1);
            result.Assert(firstAccepted && flow.CurrentPage == MutinyFrontendPage.Gameplay,
                "FRONT-05 production route accepts unlocked level 01 and enters gameplay");

            bool allResourcesPresent =
                Resources.Load<Texture2D>("UI/Frontend/background") != null &&
                Resources.Load<Texture2D>("UI/Frontend/title_logo") != null &&
                Resources.Load<Texture2D>("UI/Frontend/game_select_panel") != null &&
                Resources.Load<Texture2D>("UI/Frontend/level_select_panel") != null &&
                Resources.Load<Texture2D>("UI/Frontend/game_type_pirates") != null &&
                Resources.Load<Texture2D>("UI/Frontend/button_small") != null &&
                Resources.Load<Texture2D>("UI/Frontend/button_wide") != null &&
                Resources.Load<Texture2D>("UI/Frontend/button_back") != null &&
                Resources.Load<Texture2D>("UI/Frontend/level_slot") != null;
            for (int level = 1; level <= MutinyFrontendController.SinglePlayerLevelCount; level++)
                allResourcesPresent &= Resources.Load<Texture2D>($"UI/Frontend/LevelPreviews/{level:D2}") != null;
            result.Assert(allResourcesPresent,
                "FRONT-01/03 all original front-end panels, buttons and 15 pirate previews load through Resources");
        }

        private static void VerifyCharacterTimeline(MutinyLevel1VerificationResult result)
        {
            GameObject characterObject = null;

            try
            {
                characterObject = new GameObject("CharacterTimelineVerification");
                MutinyCharacter character = characterObject.AddComponent<MutinyCharacter>();
                character.CharacterType = "redPirate";
                MutinyCharacterAnimator animator = characterObject.AddComponent<MutinyCharacterAnimator>();

                result.Assert(!animator.IsInitialized && animator.EnsureInitialized(),
                    "CHAR-TL-04 baked-scene production initialization loads frames from serialized CharacterType");

                result.Assert(animator.CurrentFrame == 1,
                    "CHAR-TL-01 production animator starts the original static label at frame 1");

                for (int tick = 0; tick < 3; tick++)
                    animator.AdvanceOriginalTick();
                result.Assert(animator.CurrentFrame == 4,
                    "CHAR-IDLE-01 static pose advances at the original three-tick boundary 1-3 to 4");
                result.Assert(animator.EnsureInitialized() && animator.CurrentFrame == 4,
                    "CHAR-IDLE-03 repeated lifecycle initialization does not reset a runtime-built animator");

                for (int tick = 0; tick < 3; tick++)
                    animator.AdvanceOriginalTick();
                result.Assert(animator.CurrentFrame == 7,
                    "CHAR-IDLE-01 static pose advances at the original three-tick boundary 4-6 to 7");

                for (int tick = 0; tick < 3; tick++)
                    animator.AdvanceOriginalTick();
                result.Assert(animator.CurrentFrame == 10,
                    "CHAR-IDLE-01 static pose advances at the original three-tick boundary 7-9 to 10");

                for (int tick = 0; tick < 2; tick++)
                    animator.AdvanceOriginalTick();
                result.Assert(animator.CurrentFrame == 12,
                    "CHAR-TL-01 production animator reaches the last visible static frame 12");

                animator.AdvanceOriginalTick();
                result.Assert(animator.CurrentFrame == 1,
                    "CHAR-TL-01 frame 13 gotoAndPlay loops directly to static without displaying transparent frames 13-14");

                animator.PlayHit();
                result.Assert(animator.CurrentFrame == 15,
                    "CHAR-TL-02 production hit entry starts at the original hit label frame 15");

                for (int tick = 0; tick < 19; tick++)
                    animator.AdvanceOriginalTick();
                result.Assert(animator.CurrentFrame == 34,
                    "CHAR-TL-03 stopped character reaches the last visible hit frame 34");

                animator.AdvanceOriginalTick();
                result.Assert(animator.CurrentFrame == 1,
                    "CHAR-TL-03 frame 35 gotoAndPlay returns directly to static frame 1");
            }
            finally
            {
                DestroyNow(characterObject);
            }
        }

        private static void VerifyLandDeath(MutinyLevel1VerificationResult result)
        {
            GameObject characterObject = null;
            GameObject corpseObject = null;

            try
            {
                Texture2D[] textures = Resources.LoadAll<Texture2D>("Art/Characters/DeadCharacter");
                result.Assert(textures.Length == MutinyDeadCharacterEffect.OriginalFrameCount,
                    "DEATH-03 all 24 original deadCharacter frames load from Resources");
                bool dimensionsMatch = textures.Length == MutinyDeadCharacterEffect.OriginalFrameCount;
                for (int i = 0; i < textures.Length; i++)
                    dimensionsMatch &= textures[i] != null && textures[i].width == 26 && textures[i].height == 22;
                result.Assert(dimensionsMatch,
                    "DEATH-03 every original deadCharacter frame is 26x22 pixels");
                result.Assert(Resources.Load<AudioClip>("Audio/SFX/die") != null,
                    "DEATH-03 original die audio loads from Resources");

                characterObject = new GameObject("LandDeathVerification_Character");
                MutinyCharacter character = characterObject.AddComponent<MutinyCharacter>();
                MutinyPhysicsBody body = character.PhysicsBody;
                PhysicsBodyState state = PhysicsBodyState.CreateDefault(64f, 96f);
                state.Weight = 0f;
                body.State = state;
                body.WaterPixelY = float.PositiveInfinity;
                body.SetTerrain(null, 0, 0);

                SpriteRenderer characterRenderer = character.GetComponent<SpriteRenderer>();
                character.TakeDamage(100f);
                result.Assert(!character.IsAlive && characterRenderer.enabled &&
                              !character.HasLandDeathPresentation,
                    "DEATH-01 lethal damage removes action eligibility but keeps the body visible before rest resolution");

                for (int tick = 0; tick < 99; tick++)
                    body.AdvanceSimulationTick();
                result.Assert(Mathf.Approximately(character.ShownHealth, 1f) &&
                              !character.HasLandDeathPresentation,
                    "DEATH-02 production ticks lower shown health by exactly one before the terminal tick");

                body.AdvanceSimulationTick();
                corpseObject = GameObject.Find($"DeadCharacter_{character.name}");
                MutinyDeadCharacterEffect effect =
                    corpseObject != null ? corpseObject.GetComponent<MutinyDeadCharacterEffect>() : null;
                Vector3 expectedPosition = MutinyPhysics.PixelToUnity(64f, 104f);
                result.Assert(character.HasLandDeathPresentation && !characterRenderer.enabled &&
                              effect != null && effect.CurrentFrame == 1 &&
                              Vector3.Distance(corpseObject.transform.position, expectedPosition) < 0.001f,
                    "DEATH-03 terminal shown-health tick hides the character and spawns frame 1 eight pixels below its origin");

                if (effect != null)
                {
                    for (int tick = 0; tick < 23; tick++)
                        effect.AdvanceOriginalTick();
                    effect.AdvanceOriginalTick();
                    result.Assert(effect.CurrentFrame == 24 && effect.IsComplete,
                        "DEATH-04 deadCharacter plays once and holds its stopped frame 24");
                }
            }
            finally
            {
                DestroyNow(corpseObject);
                DestroyNow(characterObject);
            }
        }

        private static void VerifyAirDropsDisabled(MutinyLevel1VerificationResult result)
        {
            GameObject managerObject = null;
            GameObject chestObject = null;

            try
            {
                managerObject = new GameObject("AirDropDisabledVerification_Manager");
                MutinyTreasureChestManager manager =
                    managerObject.AddComponent<MutinyTreasureChestManager>();
                manager.TryDropNew();
                result.Assert(!MutinyTreasureChestManager.SystemEnabled && manager.Chests.Count == 0,
                    "AIRDBG-01 production drop entry creates no chest while the debug switch is disabled");

                chestObject = new GameObject("AirDropDisabledVerification_Chest");
                MutinyTreasureChest chest = chestObject.AddComponent<MutinyTreasureChest>();
                chest.Initialize(manager, 32f, 64f, new List<string> { "banana" });
                chest.AdvanceOriginalTick();
                result.Assert(chest.IsFinished && !chest.IsFalling && chest.TimeTaken == 0,
                    "AIRDBG-02 suppressed chest tick cannot reach touch, inventory or icon_collect logic");
            }
            finally
            {
                DestroyNow(chestObject);
                DestroyNow(managerObject);
            }
        }

        private static void VerifyOriginalRotation(MutinyLevel1VerificationResult result)
        {
            GameObject characterObject = null;
            GameObject dynamiteObject = null;
            GameObject cherryBombObject = null;
            GameObject splashObject = null;

            try
            {
                characterObject = new GameObject("RotationVerification_Character");
                MutinyCharacter character = characterObject.AddComponent<MutinyCharacter>();
                MutinyPhysicsBody characterBody = character.PhysicsBody;
                characterBody.WaterPixelY = float.PositiveInfinity;

                PhysicsBodyState state = PhysicsBodyState.CreateDefault(48f, 16f);
                state.Weight = 0f;
                state.VelocityX = 4f;
                characterBody.State = state;
                characterBody.SetTerrain(null, 0, 0);
                characterBody.AdvanceSimulationTick();
                result.Assert(Mathf.Approximately(
                        Mathf.DeltaAngle(0f, character.transform.eulerAngles.z), -12f),
                    "ROT-01 production physics tick rotates character by -velocityX * 3 once");

                character.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
                string[,] floor = new string[3, 3];
                floor[1, 1] = "solid";
                state = PhysicsBodyState.CreateDefault(48f, 20f);
                state.Weight = 0f;
                state.VelocityY = 4f;
                characterBody.State = state;
                characterBody.SetTerrain(floor, 3, 3);
                StepResult floorResult = characterBody.AdvanceSimulationTick();
                result.Assert(floorResult.HitFloor && Mathf.Approximately(
                        Mathf.DeltaAngle(0f, character.transform.eulerAngles.z), -45f),
                    "ROT-02 production floor contact normalizes 270 degrees and halves it to -45");

                result.Assert(
                    Mathf.Approximately(MutinyRotationRules.SettleCharacterFloorAngle(2f), 1f) &&
                    Mathf.Approximately(MutinyRotationRules.SettleCharacterFloorAngle(1f), 0f),
                    "ROT-02 floor correction uses the original strict one-degree snap threshold");

                character.transform.rotation = Quaternion.identity;
                characterBody.WaterPixelY = 0f;
                state = PhysicsBodyState.CreateDefault(48f, 1f);
                state.Weight = 0f;
                state.VelocityX = 5f;
                state.VelocityY = 6f;
                characterBody.State = state;
                characterBody.SetTerrain(null, 0, 0);
                characterBody.AdvanceSimulationTick();
                result.Assert(characterBody.IsInWater && Mathf.Approximately(
                        Mathf.DeltaAngle(0f, character.transform.eulerAngles.z), -37f),
                    "ROT-03 character applies normal spin, water drag, Y clamp and character-only water spin in order");
                splashObject = GameObject.Find("WaterSplash");

                result.Assert(
                    Mathf.Approximately(MutinyRotationRules.WeaponRotationMultiplier("banana"), 2f) &&
                    Mathf.Approximately(MutinyRotationRules.WeaponRotationMultiplier("dynamite"), 2f) &&
                    Mathf.Approximately(MutinyRotationRules.WeaponRotationMultiplier("rumBottle"), 2f) &&
                    Mathf.Approximately(MutinyRotationRules.WeaponRotationMultiplier("voodooDoll"), 2f) &&
                    Mathf.Approximately(MutinyRotationRules.WeaponRotationMultiplier("boulder"), 2.5f) &&
                    Mathf.Approximately(MutinyRotationRules.WeaponRotationMultiplier("cherryBomb"), 0f),
                    "ROT-04 production weapon rotation table matches every original AS2 override");

                dynamiteObject = new GameObject("RotationVerification_Dynamite");
                MutinyDynamite dynamite = dynamiteObject.AddComponent<MutinyDynamite>();
                dynamite.Initialize(null);
                dynamite.PhysicsBody.WaterPixelY = float.PositiveInfinity;
                state = dynamite.PhysicsBody.State;
                state.Weight = 0f;
                dynamite.PhysicsBody.State = state;
                dynamite.Fire(new Vector2(4f, 0f));
                dynamite.PhysicsBody.AdvanceSimulationTick();
                result.Assert(Mathf.Approximately(
                        Mathf.DeltaAngle(0f, dynamite.transform.eulerAngles.z), -8f),
                    "ROT-04 fired Dynamite rotates once at 25 Hz by its original multiplier");

                cherryBombObject = new GameObject("RotationVerification_CherryBomb");
                MutinyCherryBomb cherryBomb = cherryBombObject.AddComponent<MutinyCherryBomb>();
                cherryBomb.Initialize(null);
                cherryBomb.PhysicsBody.WaterPixelY = float.PositiveInfinity;
                state = cherryBomb.PhysicsBody.State;
                state.Weight = 0f;
                cherryBomb.PhysicsBody.State = state;
                cherryBomb.Fire(new Vector2(4f, 0f));
                cherryBomb.PhysicsBody.AdvanceSimulationTick();
                result.Assert(Mathf.Approximately(
                        Mathf.DeltaAngle(0f, cherryBomb.transform.eulerAngles.z), 0f),
                    "ROT-05 CherryBomb keeps its timeline animation without invented transform spin");
            }
            finally
            {
                DestroyNow(splashObject);
                DestroyNow(cherryBombObject);
                DestroyNow(dynamiteObject);
                DestroyNow(characterObject);
            }
        }

        private static void VerifyPanelTiming(MutinyLevel1VerificationResult result)
        {
            float alpha = 0f;
            bool contentsActive = false;
            for (int tick = 1; tick <= 3; tick++)
                MutinyGameHUD.AdvanceActionPanelState(true, ref alpha, ref contentsActive);
            result.Assert(Mathf.Approximately(alpha, 0.75f) && !contentsActive,
                "TURN-02 panel is visible but rejects clicks after three opening ticks");

            MutinyGameHUD.AdvanceActionPanelState(true, ref alpha, ref contentsActive);
            result.Assert(Mathf.Approximately(alpha, 1f) && contentsActive,
                "TURN-02 panel accepts clicks on the fourth opening tick");

            MutinyGameHUD.AdvanceActionPanelState(false, ref alpha, ref contentsActive);
            result.Assert(Mathf.Approximately(alpha, 0.75f) && !contentsActive,
                "TURN-03 panel rejects clicks immediately when closing starts");

            for (int tick = 0; tick < 3; tick++)
                MutinyGameHUD.AdvanceActionPanelState(false, ref alpha, ref contentsActive);
            result.Assert(Mathf.Approximately(alpha, -0.25f) && !contentsActive,
                "TURN-03 closed panel reaches the original hidden alpha state");

            for (int tick = 1; tick <= 4; tick++)
                MutinyGameHUD.AdvanceActionPanelState(true, ref alpha, ref contentsActive);
            result.Assert(Mathf.Approximately(alpha, 0.75f) && !contentsActive,
                "TURN-05 reopened panel still rejects clicks for four ticks from hidden state");
            MutinyGameHUD.AdvanceActionPanelState(true, ref alpha, ref contentsActive);
            result.Assert(Mathf.Approximately(alpha, 1f) && contentsActive,
                "TURN-05 reopened panel accepts clicks on its fifth tick from hidden state");
        }

        private static void VerifyVisualStateMapping(MutinyLevel1VerificationResult result)
        {
            result.Assert(
                MutinyGameHUD.ResolveThrowButtonVisualState(1, true, false) == MutinyThrowButtonVisualState.RedUp,
                "TURN-02 red unused throw action resolves to red_up");
            result.Assert(
                MutinyGameHUD.ResolveThrowButtonVisualState(1, true, true) == MutinyThrowButtonVisualState.RedOver,
                "TURN-02 red unused throw action resolves to red_over while hovered");
            result.Assert(
                MutinyGameHUD.ResolveThrowButtonVisualState(2, true, false) == MutinyThrowButtonVisualState.BlueUp,
                "TURN-02 blue unused throw action resolves to blue_up");
            result.Assert(
                MutinyGameHUD.ResolveThrowButtonVisualState(2, true, true) == MutinyThrowButtonVisualState.BlueOver,
                "TURN-02 blue unused throw action resolves to blue_over while hovered");
            result.Assert(
                MutinyGameHUD.ResolveThrowButtonVisualState(1, false, true) == MutinyThrowButtonVisualState.Disabled,
                "TURN-05 consumed throw action stays disabled while hovered");
        }

        private static void VerifyRuntimeResources(MutinyLevel1VerificationResult result)
        {
            for (int i = 0; i < ThrowButtonResources.Length; i++)
            {
                string path = ThrowButtonResources[i];
                Texture2D texture = Resources.Load<Texture2D>(path);
                result.Assert(texture != null, $"Original throw button texture loads from Resources: {path}");
                if (texture != null)
                    result.Assert(texture.width == 86 && texture.height == 57,
                        $"Original throw button texture is 86x57: {path}");
            }

            AssertTexture(result, "UI/weapon_select_red", 271, 247);
            AssertTexture(result, "UI/weapon_select_blue", 271, 247);
            for (int i = 0; i < WeaponSlotResources.Length; i++)
                AssertTexture(result, WeaponSlotResources[i], 24, 36);
            for (int i = 0; i < EndTurnButtonResources.Length; i++)
                AssertTexture(result, EndTurnButtonResources[i], 86, 57);
            for (int i = 0; i < WeaponIconResources.Length; i++)
                AssertTexture(result, $"UI/WeaponIcons/{WeaponIconResources[i]}", 18, 17);
            AssertTexture(result, "UI/weapon_ammo_infinite", 18, 9);
            AssertTexture(result, "Art/Effects/Water/1", 1024, 384);
            AssertTexture(result, "Art/Effects/Splash/1", 48, 44);
            result.Assert(Resources.Load<AudioClip>("Audio/SFX/splash") != null,
                "WATER-03 original splash audio loads from Resources");
        }

        private static void VerifyBattleHud(MutinyLevel1VerificationResult result)
        {
            AssertTexture(result, "UI/BattleHUD/team1_panel", 132, 34);
            AssertTexture(result, "UI/BattleHUD/team2_panel", 132, 34);
            AssertTexture(result, "UI/BattleHUD/team1_portrait", 22, 21);
            Texture2D[] portraits = Resources.LoadAll<Texture2D>("UI/BattleHUD/Opponents");
            result.Assert(portraits.Length == 33,
                "HUD-03 all 33 original opponent portrait frames load from Resources");

            GameObject teamObject = null;
            GameObject firstObject = null;
            GameObject secondObject = null;
            try
            {
                teamObject = new GameObject("BattleHudVerification_Team");
                MutinyTeam team = teamObject.AddComponent<MutinyTeam>();
                firstObject = new GameObject("BattleHudVerification_First");
                secondObject = new GameObject("BattleHudVerification_Second");
                MutinyCharacter first = firstObject.AddComponent<MutinyCharacter>();
                MutinyCharacter second = secondObject.AddComponent<MutinyCharacter>();
                first.Health = 100f;
                first.MaxHealth = 100f;
                second.Health = 50f;
                second.MaxHealth = 100f;
                team.RegisterCharacter(first);
                team.RegisterCharacter(second);

                result.Assert(MutinyGameHUD.ResolveTeamHealthTargetFrame(team) == 73,
                    "HUD-02 production team-health target uses 1 + floor(96 * totalHealth / totalMaxHealth)");
                result.Assert(MutinyGameHUD.SlideFrame(71, 73) == 72 &&
                              MutinyGameHUD.SlideFrame(73, 71) == 72 &&
                              MutinyGameHUD.SlideFrame(72, 72) == 72,
                    "HUD-02 production team-health frame moves exactly one frame per original tick");
                result.Assert(MutinyGameHUD.ProjectMapCoordinate(64f, -2) == 4,
                    "HUD-01 production minimap projection matches the original 3/32 pixel projection and offset");
            }
            finally
            {
                DestroyNow(secondObject);
                DestroyNow(firstObject);
                DestroyNow(teamObject);
            }
        }

        private static void VerifyOriginalPanelCopy(MutinyLevel1VerificationResult result)
        {
            MutinyGameHUD.GetOriginalActionCopy(null, out string title, out string description);
            result.Assert(title == "weapons",
                "TURN-UI default title comes from the original WeaponSelectPanel");
            result.Assert(description == "Click one of the options above\nto select it.",
                "TURN-UI default description comes from the original WeaponSelectPanel");

            MutinyGameHUD.GetOriginalActionCopy("cherryBomb", out title, out description);
            result.Assert(title == "cherry bomb" && description.Contains("explodes on impact"),
                "TURN-UI weapon hover copy comes from original WeaponSelectButton.hoverText");
        }

        private static void AssertTexture(
            MutinyLevel1VerificationResult result, string path, int width, int height)
        {
            Texture2D texture = Resources.Load<Texture2D>(path);
            result.Assert(texture != null, $"Original action UI texture loads from Resources: {path}");
            if (texture != null)
                result.Assert(texture.width == width && texture.height == height,
                    $"Original action UI texture is {width}x{height}: {path}");
        }

        private static void VerifyProductionActionMethods(MutinyLevel1VerificationResult result)
        {
            GameObject managerObject = null;
            GameObject team1Object = null;
            GameObject team2Object = null;
            GameObject characterObject = null;
            GameObject inputObject = null;
            GameObject splashObject = null;

            try
            {
                managerObject = new GameObject("TurnActionUiVerification_Manager");
                team1Object = new GameObject("TurnActionUiVerification_Team1");
                team2Object = new GameObject("TurnActionUiVerification_Team2");
                characterObject = new GameObject("TurnActionUiVerification_Character");
                inputObject = new GameObject("TurnActionUiVerification_Input");

                MutinyTurnManager manager = managerObject.AddComponent<MutinyTurnManager>();
                MutinyTeam team1 = team1Object.AddComponent<MutinyTeam>();
                MutinyTeam team2 = team2Object.AddComponent<MutinyTeam>();
                MutinyCharacter character = characterObject.AddComponent<MutinyCharacter>();
                MutinyPlayerInput input = inputObject.AddComponent<MutinyPlayerInput>();

                team1.TeamNumber = 1;
                team2.TeamNumber = 2;
                team1.RegisterCharacter(character);
                team1.SelectCharacter(character);
                character.AddWeapon("cherryBomb");

                manager.Team1 = team1;
                manager.Team2 = team2;
                manager.CurrentTeam = team1;
                manager.CurrentPhase = TurnPhase.TurnActive;
                input.TurnManager = manager;

                team2.IsAiControlled = true;
                manager.CurrentTeam = team2;
                manager.CurrentPhase = TurnPhase.ActionExecuting;
                result.Assert(MutinyAIController.ResolveTurnGate(manager, team2) == MutinyAITurnGate.Wait,
                    "AI turn waits through transient ActionExecuting instead of abandoning its turn");
                manager.CurrentPhase = TurnPhase.TurnActive;
                result.Assert(MutinyAIController.ResolveTurnGate(manager, team2) == MutinyAITurnGate.Execute,
                    "AI turn proceeds when the same team returns to TurnActive");
                manager.CurrentTeam = team1;
                result.Assert(MutinyAIController.ResolveTurnGate(manager, team2) == MutinyAITurnGate.Cancel,
                    "AI turn cancels only after ownership moves to another team");
                team2.IsAiControlled = false;

                result.Assert(input.SelectCharacterThrow(),
                    "TURN-03 production input accepts the unused throw action");
                result.Assert(character.CanThrow && character.CanShoot,
                    "TURN-03 selecting throw does not consume either action");

                bool committed = input.TryCommitCharacterThrow(
                    character, new Vector2(100f, 100f), new Vector2(140f, 120f));
                result.Assert(committed, "TURN-04 production input commits a valid character throw");
                result.Assert(!character.CanThrow && character.CanShoot,
                    "TURN-04 committed character throw consumes only CanThrow");

                manager.CurrentPhase = TurnPhase.TurnActive;
                result.Assert(input.SelectWeapon("cherryBomb"),
                    "TURN-05 production input still accepts an owned weapon after character throw");

                result.Assert(!MutinyTurnManager.HasReachedInactivityThreshold(10),
                    "TURN-09 inactivity 10 does not settle the action");
                result.Assert(MutinyTurnManager.HasReachedInactivityThreshold(11),
                    "TURN-09 inactivity 11 settles the action");

                team1.StartTurn();
                result.Assert(character.CanThrow && character.CanShoot && team1.SelectedCharacter == null,
                    "TURN-07 production StartTurn restores both actions and returns to character selection");

                MutinyPhysicsBody body = character.PhysicsBody;
                body.WaterPixelY = 100f;
                PhysicsBodyState waterState = body.State;
                waterState.X = 64f;
                waterState.Y = 101f;
                waterState.VelocityY = 2f;
                body.State = waterState;
                body.EvaluateWaterState();
                result.Assert(body.IsInWater && character.IsDrowned && !character.IsAlive,
                    "WATER-01 baked character Awake binding drowns through the production water event");
                splashObject = GameObject.Find("WaterSplash");
                result.Assert(splashObject != null && splashObject.GetComponent<MutinySplashEffect>() != null,
                    "WATER-02 crossing the water line spawns the original splash effect");

                // WATER-03: Weapon falling into water invalidation
                GameObject testDynamiteObject = new GameObject("WaterVerification_Dynamite");
                try
                {
                    MutinyDynamite testDynamite = testDynamiteObject.AddComponent<MutinyDynamite>();
                    testDynamite.Initialize(character);
                    testDynamite.PhysicsBody.WaterPixelY = 100f;
                    testDynamite.Fire(new Vector2(0f, 5f));
                    testDynamite.PhysicsBody.State.Y = 105f;
                    testDynamite.PhysicsBody.EvaluateWaterState();
                    result.Assert(testDynamite.PhysicsBody.IsInWater && !testDynamite.IsLit,
                        "WATER-03 Dynamite entering water extinguishes fuse");

                    // Submerged water depth invalidates and finishes the weapon
                    testDynamite.PhysicsBody.State.Y = 130f;
                    testDynamite.Finish();
                    result.Assert(testDynamite.IsFinished,
                        "WATER-04 Submerged weapon finishes without blocking turn manager");
                }
                finally
                {
                    DestroyNow(testDynamiteObject);
                }
            }
            finally
            {
                DestroyNow(splashObject);
                DestroyNow(inputObject);
                DestroyNow(characterObject);
                DestroyNow(team2Object);
                DestroyNow(team1Object);
                DestroyNow(managerObject);
            }
        }

        private static void DestroyNow(GameObject gameObject)
        {
            if (gameObject != null)
                Object.DestroyImmediate(gameObject);
        }
    }
}
