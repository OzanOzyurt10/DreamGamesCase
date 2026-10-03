using UnityEngine;

namespace DreamCase.Meta
{
    /// <summary>Stores the player's current level in PlayerPrefs so it survives restarts.</summary>
    public sealed class ProgressService
    {
        public const int LevelCount = 10;

        private const string CurrentLevelKey = "current_level";

        // LevelCount + 1 means every level is finished. Callers use IsFinished / SetFinished instead.
        private const int FinishedValue = LevelCount + 1;

        public int CurrentLevel => PlayerPrefs.GetInt(CurrentLevelKey, 1);

        public bool IsFinished => CurrentLevel >= FinishedValue;

        public void SetCurrentLevel(int level) => Save(Mathf.Clamp(level, 1, LevelCount));

        public void SetFinished() => Save(FinishedValue);

        /// <summary>Moves the player past the level they just won; after the last level, everything is finished.</summary>
        public void CompleteLevel(int levelNumber)
        {
            if (levelNumber >= LevelCount)
                SetFinished();
            else
                SetCurrentLevel(levelNumber + 1);
        }

        private static void Save(int value)
        {
            PlayerPrefs.SetInt(CurrentLevelKey, value);
            PlayerPrefs.Save();
        }
    }
}
