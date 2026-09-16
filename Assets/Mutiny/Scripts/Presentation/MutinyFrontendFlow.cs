using System;

namespace Mutiny.Presentation
{
    public enum MutinyFrontendPage
    {
        Title,
        GameSelect,
        LevelSelect,
        Gameplay
    }

    /// <summary>
    /// Production state machine for the original single-player front-end route.
    /// Keeping navigation separate from drawing makes the actual route directly verifiable.
    /// </summary>
    public sealed class MutinyFrontendFlow
    {
        public MutinyFrontendPage CurrentPage { get; private set; } = MutinyFrontendPage.Title;

        public void PressPlay()
        {
            if (CurrentPage == MutinyFrontendPage.Title)
                CurrentPage = MutinyFrontendPage.GameSelect;
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
    }
}
