using DG.Tweening;

namespace DreamCase.Meta
{
    /// <summary>What happens when a level ends: a win saves progress and returns to the menu; a loss replays the level.</summary>
    public sealed class LevelEndFlow
    {
        /// <summary>A short pause so the player sees the final board before the scene changes.</summary>
        private const float DelayBeforeLeaving = 0.6f;

        private readonly ProgressService progress;
        private readonly int levelNumber;

        public LevelEndFlow(ProgressService progress, int levelNumber)
        {
            this.progress = progress;
            this.levelNumber = levelNumber;
        }

        public void OnLevelEnded(bool won)
        {
            if (won)
            {
                progress.CompleteLevel(levelNumber);
                DOVirtual.DelayedCall(DelayBeforeLeaving, SceneLoader.LoadMain);
            }
            else
            {
                DOVirtual.DelayedCall(DelayBeforeLeaving, SceneLoader.LoadLevel);
            }
        }
    }
}
