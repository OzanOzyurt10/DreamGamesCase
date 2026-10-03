using DG.Tweening;

namespace DreamCase.Meta
{
    /// <summary>
    /// What happens when a level ends. A win saves progress at once (so quitting during the celebration can't lose it),
    /// then celebrates and returns to the menu. A loss offers going back to the menu or trying again.
    /// </summary>
    public sealed class LevelEndFlow
    {
        /// <summary>A short pause so the player sees the final board before the end screen covers it.</summary>
        private const float DelayBeforeEndScreen = 0.6f;

        private readonly ProgressService progress;
        private readonly int levelNumber;
        private readonly CelebrationView celebration;
        private readonly FailPopupView failPopup;

        public LevelEndFlow(ProgressService progress, int levelNumber, CelebrationView celebration, FailPopupView failPopup)
        {
            this.progress = progress;
            this.levelNumber = levelNumber;
            this.celebration = celebration;
            this.failPopup = failPopup;
        }

        public void OnLevelEnded(bool won)
        {
            if (won)
            {
                progress.CompleteLevel(levelNumber);
                DOVirtual.DelayedCall(DelayBeforeEndScreen, () => celebration.Play(SceneLoader.LoadMain));
            }
            else
            {
                DOVirtual.DelayedCall(DelayBeforeEndScreen,
                    () => failPopup.Show(levelNumber, SceneLoader.LoadMain, SceneLoader.LoadLevel));
            }
        }
    }
}
