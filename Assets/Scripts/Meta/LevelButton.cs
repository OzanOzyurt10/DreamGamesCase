using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DreamCase.Meta
{
    /// <summary>MainScene's button: shows the current level, or "Finished" once every level is done.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class LevelButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private float pressedScale = 0.9f;
        [SerializeField] private float pressDuration = 0.16f;

        private ProgressService progress;
        private Button button;

        /// <summary>Method injection from MainSceneBootstrap.</summary>
        public void Initialize(ProgressService progress)
        {
            this.progress = progress;
            label.text = progress.IsFinished ? "Finished" : $"Level {progress.CurrentLevel}";

            button = GetComponent<Button>();
            button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            if (progress.IsFinished)
                return;

            // One load only, even if the player taps again during the press animation.
            button.interactable = false;
            transform.DOScale(pressedScale, pressDuration / 2f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject)
                .OnComplete(SceneLoader.LoadLevel);
        }
    }
}
