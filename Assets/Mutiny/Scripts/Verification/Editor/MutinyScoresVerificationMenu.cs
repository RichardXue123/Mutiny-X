using System;
using Mutiny.Levels;
using Mutiny.Persistence;
using Mutiny.Presentation;
using Mutiny.Simulation;
using UnityEditor;
using UnityEngine;

namespace Mutiny.Verification.Editor
{
    public static class MutinyScoresVerificationMenu
    {
        private const string ScoresKey = "mutiny_completed_scores_v1";

        [MenuItem("Mutiny/Parity/Validate Scores")]
        public static void Validate()
        {
            var flow = new MutinyFrontendFlow();
            Require(flow.CurrentPage == MutinyFrontendPage.Title, "starts on title");
            flow.PressScoresBack();
            Require(flow.CurrentPage == MutinyFrontendPage.Title, "Scores Back is gated outside Scores");
            flow.PressScores();
            Require(flow.CurrentPage == MutinyFrontendPage.Scores, "Scores enters its production page");
            flow.PressPlay();
            Require(flow.CurrentPage == MutinyFrontendPage.Scores, "Play is gated while Scores is visible");
            flow.PressScoresBack();
            Require(flow.CurrentPage == MutinyFrontendPage.Title, "Scores Back returns to title");

            Texture2D rows = Resources.Load<Texture2D>("UI/Frontend/scores_rows");
            Require(rows != null && rows.width == 397 && rows.height == 261,
                "original ten-row red border resource is present at 397x261");
            Texture2D panel = Resources.Load<Texture2D>("UI/Frontend/credits_panel");
            Require(panel != null && panel.width == 462 && panel.height == 352,
                "Scores uses the original 462x352 panel resource");

            bool hadSavedValue = PlayerPrefs.HasKey(ScoresKey);
            string savedValue = hadSavedValue ? PlayerPrefs.GetString(ScoresKey) : string.Empty;
            try
            {
                PlayerPrefs.DeleteKey(ScoresKey);
                PlayerPrefs.Save();
                GameObject controllerObject = new GameObject("ScoresVerificationController");
                GameObject teamObject = new GameObject("ScoresVerificationTeam");
                MutinyLevelController controller = controllerObject.AddComponent<MutinyLevelController>();
                MutinyTeam team = teamObject.AddComponent<MutinyTeam>();
                controller.ConfigureSession(MutinyGameMode.SinglePlayer);
                controller.CurrentLevelIndex = MutinyFrontendController.SinglePlayerLevelCount;
                int firstAward = controller.AwardSinglePlayerLevelWin(team);
                int repeatAward = controller.AwardSinglePlayerLevelWin(team);
                Require(firstAward == 150 && repeatAward == firstAward,
                    "production final-level award is calculated once for an empty-team verification fixture");
                Require(Matches(MutinySaveSystem.GetTopCompletedScores(), 150),
                    "production final-level award automatically records one total score");
                UnityEngine.Object.DestroyImmediate(controllerObject);
                UnityEngine.Object.DestroyImmediate(teamObject);
                PlayerPrefs.DeleteKey(ScoresKey);
                PlayerPrefs.Save();

                controllerObject = new GameObject("ScoresVerificationNonFinalController");
                teamObject = new GameObject("ScoresVerificationNonFinalTeam");
                controller = controllerObject.AddComponent<MutinyLevelController>();
                team = teamObject.AddComponent<MutinyTeam>();
                controller.ConfigureSession(MutinyGameMode.SinglePlayer);
                controller.CurrentLevelIndex = MutinyFrontendController.SinglePlayerLevelCount - 1;
                controller.AwardSinglePlayerLevelWin(team);
                Require(MutinySaveSystem.GetTopCompletedScores().Length == 0,
                    "a non-final campaign level does not enter the history");
                UnityEngine.Object.DestroyImmediate(controllerObject);
                UnityEngine.Object.DestroyImmediate(teamObject);

                controllerObject = new GameObject("ScoresVerificationVersusController");
                teamObject = new GameObject("ScoresVerificationVersusTeam");
                controller = controllerObject.AddComponent<MutinyLevelController>();
                team = teamObject.AddComponent<MutinyTeam>();
                controller.ConfigureSession(MutinyGameMode.LocalTwoPlayer);
                controller.CurrentLevelIndex = MutinyFrontendController.SinglePlayerLevelCount;
                Require(controller.AwardSinglePlayerLevelWin(team) == 0 &&
                        MutinySaveSystem.GetTopCompletedScores().Length == 0,
                    "a local two-player result does not enter the history");
                UnityEngine.Object.DestroyImmediate(controllerObject);
                UnityEngine.Object.DestroyImmediate(teamObject);

                PlayerPrefs.SetString(ScoresKey, "900,800,700,600,500");
                PlayerPrefs.Save();
                Require(!MutinySaveSystem.RecordCompletedScore(100),
                    "a score below the fifth place is rejected");
                Require(Matches(MutinySaveSystem.GetTopCompletedScores(), 900, 800, 700, 600, 500),
                    "low score leaves top five unchanged");
                Require(MutinySaveSystem.RecordCompletedScore(750),
                    "an in-range completion is recorded");
                Require(Matches(MutinySaveSystem.GetTopCompletedScores(), 900, 800, 750, 700, 600),
                    "scores are sorted descending and capped at five");
                Require(MutinySaveSystem.RecordCompletedScore(1000),
                    "a new first-place completion is recorded");
                Require(Matches(MutinySaveSystem.GetTopCompletedScores(), 1000, 900, 800, 750, 700),
                    "new first place shifts the existing history");
            }
            finally
            {
                if (hadSavedValue)
                    PlayerPrefs.SetString(ScoresKey, savedValue);
                else
                    PlayerPrefs.DeleteKey(ScoresKey);
                PlayerPrefs.Save();
            }

            Debug.Log("[Mutiny Parity] Scores navigation, original resources and local top-five persistence passed (SCORE-UI-01/02/03, SCORE-EXT-02/03).");
        }

        private static bool Matches(int[] actual, params int[] expected)
        {
            if (actual == null || actual.Length != expected.Length)
                return false;
            for (int i = 0; i < expected.Length; i++)
                if (actual[i] != expected[i])
                    return false;
            return true;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("SCORES verification failed: " + message);
        }
    }
}
