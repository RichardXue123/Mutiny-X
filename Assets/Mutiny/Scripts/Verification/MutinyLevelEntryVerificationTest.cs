using System;
using System.Collections.Generic;
using Mutiny.Levels;
using Mutiny.Persistence;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mutiny.Verification
{
    /// <summary>Isolated Play Mode checks through the GM parser and production front-end/loader.</summary>
    public static class MutinyLevelEntryVerificationTest
    {
        public static MutinyLevel1VerificationResult Run()
        {
            var result = new MutinyLevel1VerificationResult();
            if (Object.FindAnyObjectByType<MutinyLevelController>() != null ||
                Object.FindAnyObjectByType<MutinyFrontendController>() != null)
            {
                result.Assert(false, "GM-15 requires an empty verification scene, preserving the user's live board");
                return result;
            }

            int unlockedBefore = MutinySaveSystem.HighestUnlockedLevel;
            string highScoresBefore = string.Join(",", MutinySaveSystem.GetTopCompletedScores());
            var host = new GameObject("LevelEntryVerification_Controller");
            var frontendHost = new GameObject("LevelEntryVerification_Frontend");
            var cameraHost = new GameObject("LevelEntryVerification_Camera");
            MutinyLevelController controller = host.AddComponent<MutinyLevelController>();
            MutinyFrontendController frontend = frontendHost.AddComponent<MutinyFrontendController>();
            cameraHost.AddComponent<Camera>().orthographic = true;
            MutinyCameraController camera = cameraHost.AddComponent<MutinyCameraController>();
            MutinyGMManager gm = MutinyGMManager.Instance;
            GameObject gmHost = null;
            if (gm == null)
            {
                gmHost = new GameObject("LevelEntryVerification_GM");
                gm = gmHost.AddComponent<MutinyGMManager>();
            }
            try
            {
                frontend.Initialize(controller);
                result.Assert(frontend.CurrentPage == MutinyFrontendPage.Title &&
                    controller.CurrentLevel == null, "GM-15 starts from the real title front-end");
                foreach (string token in new[] { "16", "1_16", "level_1_16", "2_01", "LEVEL_2_18" })
                    result.Assert(MutinyLevelId.TryParse(token, out _), $"EXT-LVL-ID-01 accepts identity {token}");

                result.Assert(gm.ExecuteCommand("  EnTeRlEvEl   16  ") &&
                    frontend.CurrentPage == MutinyFrontendPage.Gameplay &&
                    controller.CurrentLevelId.Equals(new MutinyLevelId(MutinyGameMode.SinglePlayer, 16)) &&
                    controller.LevelXml.name == "level_1_16" &&
                    controller.CurrentLevel.LevelName == MutinyLevelXmlParser.Parse(controller.LevelXml.text).Name &&
                    controller.CurrentLevel.Team2.IsAiControlled &&
                    camera.TurnManager == controller.CurrentLevel.GetComponent<MutinyTurnManager>() &&
                    gm.RecentSuccessfulCommands[0] == "EnTeRlEvEl   16",
                    "GM-15 title-to-temporary-16 loads the actual resource, AI, camera, page and history");
                MutinyAudioManager audio = MutinyAudioManager.Instance;
                result.Assert(audio.MusicSource.clip ==
                    Resources.Load<AudioClip>("Audio/Music/game_music"),
                    "GM-15 gameplay entry resolves combat music, including its logical target while muted");

                MutinyLevelRoot oldRoot = controller.CurrentLevel;
                result.Assert(gm.RunRecentCommand(0) && controller.CurrentLevel != oldRoot &&
                    !oldRoot.gameObject.activeSelf && controller.CurrentLevelIndex == 16,
                    "GM-15 history replay rebuilds the same identity and immediately retires the old root");
                DestroyNow(oldRoot.gameObject);
                oldRoot = controller.CurrentLevel;
                controller.RestartCurrentLevel();
                result.Assert(controller.CurrentLevel != oldRoot && controller.LevelXml.name == "level_1_16" &&
                    controller.ActiveGameMode == MutinyGameMode.SinglePlayer,
                    "EXT-LVL-ID-02 restart preserves temporary single-player 16");
                DestroyNow(oldRoot.gameObject);

                // Every migrated original is loaded through the same command users execute.
                for (int original = 1; original <= 33; original++)
                {
                    MutinyLevelId id = MutinyLevelId.FromOriginalNumber(original);
                    oldRoot = controller.CurrentLevel;
                    bool entered = gm.ExecuteCommand("enterlevel " + id.AssetName);
                    MutinyLevelRoot root = controller.CurrentLevel;
                    result.Assert(entered && root != null && root != oldRoot && root.gameObject.activeSelf &&
                        controller.CurrentLevelId.Equals(id) && controller.OriginalLevelIndex == original &&
                        controller.LevelXml.name == id.AssetName && root.Team1.AliveCount > 0 &&
                        root.Team2.AliveCount > 0 && !root.Team1.IsAiControlled &&
                        root.Team2.IsAiControlled == (id.Mode == MutinyGameMode.SinglePlayer) &&
                        root.SkyColour == MutinyOriginalBackground.SkyColourForLevel(original),
                        $"EXT-LVL-ID-01 production GM loads {id} (original {original}) with correct teams and sky");
                    if (oldRoot != null) DestroyNow(oldRoot.gameObject);
                }

                // The same number in each namespace must select different authored data.
                Switch(gm, controller, "enterlevel 2_16");
                result.Assert(controller.LevelXml.name == "level_2_16" && controller.OriginalLevelIndex == 31 &&
                    controller.ActiveGameMode == MutinyGameMode.LocalTwoPlayer,
                    "EXT-LVL-ID-02 dual local 16 resolves original 31, distinct from temporary single 16");
                oldRoot = controller.CurrentLevel;
                controller.RestartCurrentLevel();
                result.Assert(controller.LevelXml.name == "level_2_16" && controller.CurrentLevelIndex == 16 &&
                    !controller.CurrentLevel.Team2.IsAiControlled,
                    "EXT-LVL-ID-02 dual restart retains both mode and local number");
                DestroyNow(oldRoot.gameObject);

                // Bare GM numbers always mean single player even while playing versus.
                Switch(gm, controller, "enterlevel 1");
                result.Assert(controller.ActiveGameMode == MutinyGameMode.SinglePlayer &&
                    controller.LevelXml.name == "level_1_01" && controller.CurrentLevel.Team2.IsAiControlled,
                    "GM-15 bare 1 switches from versus to single-player, independent of the previous session");
                controller.AwardSinglePlayerLevelWin(controller.CurrentLevel.Team1);
                int scoreBefore = controller.SinglePlayerScore;
                result.Assert(scoreBefore > 0, "GM-15 creates a session score through the production award entry");
                foreach (string command in new[] { "enterlevel", "enterlevel 0", "enterlevel -1",
                    "enterlevel 1.5", "enterlevel nope", "enterlevel 1 extra", "enterlevel 3_01",
                    "enterlevel 1_00", "enterlevel 2147483648", "enterlevel 1_2147483648",
                    "enterlevelx 1", "enterlevel level_01", "enterlevel 99", "enterlevel 2_99" })
                {
                    oldRoot = controller.CurrentLevel;
                    string history = string.Join("|", gm.RecentSuccessfulCommands);
                    result.Assert(!gm.ExecuteCommand(command) && controller.CurrentLevel == oldRoot &&
                        controller.CurrentLevelId.Equals(new MutinyLevelId(MutinyGameMode.SinglePlayer, 1)) &&
                        controller.ActiveGameMode == MutinyGameMode.SinglePlayer &&
                        controller.SinglePlayerScore == scoreBefore && frontend.CurrentPage == MutinyFrontendPage.Gameplay &&
                        string.Join("|", gm.RecentSuccessfulCommands) == history,
                        $"GM-15-INVALID '{command}' leaves the board, session, page, score and history intact");
                }
                GameObject transitionHost = null;
                try
                {
                    MutinyTransitionManager transition = MutinyTransitionManager.Instance;
                    if (transition == null)
                    {
                        transitionHost = new GameObject("LevelEntryVerification_Transition");
                        transition = transitionHost.AddComponent<MutinyTransitionManager>();
                    }
                    oldRoot = controller.CurrentLevel;
                    string history = string.Join("|", gm.RecentSuccessfulCommands);
                    MutinyTransitionManager.RequestTransition(() => { }, showLoading: false);
                    result.Assert(MutinyTransitionManager.IsTransitionActive && !gm.ExecuteCommand("enterlevel 16") &&
                        controller.CurrentLevel == oldRoot && controller.SinglePlayerScore == scoreBefore &&
                        string.Join("|", gm.RecentSuccessfulCommands) == history,
                        "GM-15 rejects entry during a production transition without disturbing the active board");
                    for (int tick = 0; tick < 10 && MutinyTransitionManager.IsTransitionActive; tick++)
                        transition.StepForVerification(0.32f);
                }
                finally { DestroyNow(transitionHost); }
                Switch(gm, controller, "enterlevel 2");
                result.Assert(controller.SinglePlayerScore == 0,
                    "GM-15 a successful single-player entry resets session score");
                result.Assert(frontend.ReturnToSinglePlayerLevelSelect() && controller.CurrentLevel == null &&
                    frontend.CurrentPage == MutinyFrontendPage.LevelSelect && gm.ExecuteCommand("enterlevel 16"),
                    "GM-15 quit uses the normal selector, from which temporary 16 remains GM-accessible");
                result.Assert(frontend.ShowEnding(0, controller) && frontend.CurrentPage == MutinyFrontendPage.Ending &&
                    gm.ExecuteCommand("enterlevel 3") && frontend.CurrentPage == MutinyFrontendPage.Gameplay,
                    "GM-15 enters a map from the existing ending page without changing the ending route");

                var flow = new MutinyFrontendFlow();
                flow.PressPlay(); flow.PressOnePlayer();
                result.Assert(!flow.TrySelectLevel(16, _ => true) && MutinyFrontendController.SinglePlayerLevelCount == 15 &&
                    MutinyGameHUD.ResolveGameEndPopupKind(GameOverResult.Team1Wins, 15, MutinyGameMode.SinglePlayer) ==
                        MutinyGameEndPopupKind.GameComplete,
                    "EXT-LVL-ID-03 original menu stays at 15 and original final-victory route remains unchanged");
                result.Assert(MutinySaveSystem.HighestUnlockedLevel == unlockedBefore &&
                    string.Join(",", MutinySaveSystem.GetTopCompletedScores()) == highScoresBefore,
                    "GM-15 entry and rejection do not change saved unlocks or completed score");

                // BuildLevel is also a production entry for serialized/editor-created controllers.
                oldRoot = controller.CurrentLevel;
                controller.LevelXml = Resources.Load<TextAsset>("Data/Levels/level_1_16");
                controller.BuildLevel();
                result.Assert(controller.CurrentLevelIndex == 16 && controller.CurrentLevelId.Mode == MutinyGameMode.SinglePlayer,
                    "EXT-LVL-ID-01 exact asset-name parsing produces 16, never concatenated 116");
                if (oldRoot != null) DestroyNow(oldRoot.gameObject);
            }
            catch (Exception exception)
            {
                result.Assert(false, "Level entry verification exception: " + exception);
            }
            finally
            {
                controller.ClearLevel();
                DestroyNow(frontendHost); DestroyNow(host); DestroyNow(cameraHost); DestroyNow(gmHost);
            }
            return result;
        }

        private static void Switch(MutinyGMManager gm, MutinyLevelController controller, string command)
        {
            MutinyLevelRoot oldRoot = controller.CurrentLevel;
            if (!gm.ExecuteCommand(command)) throw new InvalidOperationException("Failed: " + command);
            if (oldRoot != null) DestroyNow(oldRoot.gameObject);
        }

        private static void DestroyNow(GameObject value)
        {
            if (value != null) Object.DestroyImmediate(value);
        }
    }
}
