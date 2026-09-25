using System;

namespace Mutiny.Presentation
{
    public enum MutinyFrontendPage
    {
        Title,
        GameSelect,
        LevelSelect,
        TwoPlayerLevelSelect,
        Help,
        Gameplay,
        Ending,
        Credits,
        Scores
    }

    /// <summary>
    /// Production state machine for the original front-end routes.
    /// Keeping navigation separate from drawing makes the actual route directly verifiable.
    /// </summary>
    public sealed class MutinyFrontendFlow
    {
        public MutinyFrontendPage CurrentPage { get; private set; } = MutinyFrontendPage.Title;
        public int SelectedTwoPlayerLevel { get; private set; } = 16;

        public void PressPlay()
        {
            if (CurrentPage == MutinyFrontendPage.Title)
                CurrentPage = MutinyFrontendPage.GameSelect;
        }

        public void PressHelp()
        {
            if (CurrentPage == MutinyFrontendPage.Title)
                CurrentPage = MutinyFrontendPage.Help;
        }

        public void PressHelpBack()
        {
            if (CurrentPage == MutinyFrontendPage.Help)
                CurrentPage = MutinyFrontendPage.Title;
        }

        public void PressCredits()
        {
            if (CurrentPage == MutinyFrontendPage.Title)
                CurrentPage = MutinyFrontendPage.Credits;
        }

        public void PressCreditsBack()
        {
            if (CurrentPage == MutinyFrontendPage.Credits)
                CurrentPage = MutinyFrontendPage.Title;
        }

        public void PressScores()
        {
            if (CurrentPage == MutinyFrontendPage.Title)
                CurrentPage = MutinyFrontendPage.Scores;
        }

        public void PressScoresBack()
        {
            if (CurrentPage == MutinyFrontendPage.Scores)
                CurrentPage = MutinyFrontendPage.Title;
        }

        public void PressGameSelectBack()
        {
            if (CurrentPage == MutinyFrontendPage.GameSelect)
                CurrentPage = MutinyFrontendPage.Title;
        }

        public void PressOnePlayer()
        {
            if (CurrentPage == MutinyFrontendPage.GameSelect)
                CurrentPage = MutinyFrontendPage.LevelSelect;
        }

        public void PressTwoPlayer()
        {
            if (CurrentPage == MutinyFrontendPage.GameSelect)
                CurrentPage = MutinyFrontendPage.TwoPlayerLevelSelect;
        }

        public void PressTwoPlayerLevelSelectBack()
        {
            if (CurrentPage == MutinyFrontendPage.TwoPlayerLevelSelect)
                CurrentPage = MutinyFrontendPage.GameSelect;
        }

        public bool StepTwoPlayerLevel(int delta)
        {
            if (CurrentPage != MutinyFrontendPage.TwoPlayerLevelSelect ||
                (delta != -1 && delta != 1))
                return false;
            int next = SelectedTwoPlayerLevel + delta;
            if (next < 16 || next > 33)
                return false;
            SelectedTwoPlayerLevel = next;
            return true;
        }

        public bool TrySelectTwoPlayerLevel(Func<int, bool> hasLevelData)
        {
            if (CurrentPage != MutinyFrontendPage.TwoPlayerLevelSelect ||
                hasLevelData == null || !hasLevelData(SelectedTwoPlayerLevel))
                return false;
            CurrentPage = MutinyFrontendPage.Gameplay;
            return true;
        }

        public bool ReturnToTwoPlayerLevelSelect()
        {
            if (CurrentPage != MutinyFrontendPage.Gameplay)
                return false;
            CurrentPage = MutinyFrontendPage.TwoPlayerLevelSelect;
            return true;
        }

        public void PressLevelSelectBack()
        {
            if (CurrentPage == MutinyFrontendPage.LevelSelect)
                CurrentPage = MutinyFrontendPage.GameSelect;
        }

        public bool TrySelectLevel(int level, Func<int, bool> isUnlocked)
        {
            if (CurrentPage != MutinyFrontendPage.LevelSelect ||
                level < 1 || level > MutinyFrontendController.SinglePlayerLevelCount ||
                isUnlocked == null || !isUnlocked(level))
            {
                return false;
            }

            CurrentPage = MutinyFrontendPage.Gameplay;
            return true;
        }

        /// <summary>
        /// Original QuitGameButton sends a one-player game to level_select_1p,
        /// rather than the title page.
        /// </summary>
        public bool ReturnToSinglePlayerLevelSelect()
        {
            if (CurrentPage != MutinyFrontendPage.Gameplay)
                return false;

            CurrentPage = MutinyFrontendPage.LevelSelect;
            return true;
        }

        public bool ShowEnding()
        {
            if (CurrentPage != MutinyFrontendPage.Gameplay)
                return false;
            CurrentPage = MutinyFrontendPage.Ending;
            return true;
        }

        public bool ReturnFromEndingToTitle()
        {
            if (CurrentPage != MutinyFrontendPage.Ending)
                return false;
            CurrentPage = MutinyFrontendPage.Title;
            return true;
        }
    }
}
