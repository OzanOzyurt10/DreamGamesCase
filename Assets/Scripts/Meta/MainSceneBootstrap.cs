using UnityEngine;

namespace DreamCase.Meta
{
    /// <summary>Composition root of MainScene.</summary>
    public sealed class MainSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private LevelButton levelButton;

        private void Start()
        {
            levelButton.Initialize(new ProgressService());
        }
    }
}
