using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DreamCase.Meta
{
    /// <summary>The lose popup: Close goes back to the menu, Try Again replays the level. Only the first choice counts.</summary>
    public sealed class FailPopupView : MonoBehaviour
    {
        [SerializeField] private Image dim;
        [SerializeField] private RectTransform popup;
        [SerializeField] private TMP_Text title;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button tryAgainButton;

        [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.7f;

        public void Show(int levelNumber, Action onClose, Action onTryAgain)
        {
            gameObject.SetActive(true);
            title.text = $"Level {levelNumber}";
            closeButton.onClick.AddListener(() => Choose(onClose));
            tryAgainButton.onClick.AddListener(() => Choose(onTryAgain));

            dim.color = new Color(0f, 0f, 0f, 0f);
            popup.localScale = Vector3.one * 0.6f;

            DOTween.Sequence()
                .Append(dim.DOFade(dimAlpha, 0.25f))
                .Join(popup.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
                .SetLink(gameObject);
        }

        private void Choose(Action choice)
        {
            closeButton.interactable = false;
            tryAgainButton.interactable = false;
            choice();
        }
    }
}
