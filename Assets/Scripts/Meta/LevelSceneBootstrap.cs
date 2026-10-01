using DreamCase.Core;
using DreamCase.View;
using UnityEngine;

namespace DreamCase.Meta
{
    /// <summary>
    /// Composition root of LevelScene: the one place where the scene's objects are created and wired together.
    /// </summary>
    public sealed class LevelSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField] private BoardInput boardInput;
        [SerializeField] private LevelController levelController;

        private void Start()
        {
            var progress = new ProgressService();
            var level = LevelLoader.Load(progress.CurrentLevel);

            var itemFactory = new ItemFactory(new System.Random());
            var board = new BoardBuilder(itemFactory).Build(level);
            var session = new LevelSession(board, level.move_count, itemFactory);

            levelController.Initialize(session, boardView, boardInput);
        }
    }
}
