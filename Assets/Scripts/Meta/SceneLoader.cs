using UnityEngine.SceneManagement;

namespace DreamCase.Meta
{
    /// <summary>The one place that knows the scene names.</summary>
    public static class SceneLoader
    {
        private const string MainSceneName = "MainScene";
        private const string LevelSceneName = "LevelScene";

        public static void LoadMain() => SceneManager.LoadScene(MainSceneName);

        /// <summary>Opens LevelScene; it plays the player's current level.</summary>
        public static void LoadLevel() => SceneManager.LoadScene(LevelSceneName);
    }
}
