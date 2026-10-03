using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DreamCase.Meta
{
    /// <summary>
    /// The win screen: the board dims, "Perfect!" and a spinning star pop in with a burst of sparkles, then
    /// "Tap to Continue" appears. A tap anywhere is accepted only from that moment, so it can't be skipped by accident.
    /// </summary>
    public sealed class CelebrationView : MonoBehaviour
    {
        [SerializeField] private Image dim;
        [SerializeField] private RectTransform title;
        [SerializeField] private RectTransform star;
        [SerializeField] private TMP_Text tapToContinue;
        [SerializeField] private ParticleSystem sparkles;
        [SerializeField] private Button continueButton;

        [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.7f;

        public void Play(Action onContinue)
        {
            gameObject.SetActive(true);
            continueButton.interactable = false;
            continueButton.onClick.AddListener(() =>
            {
                continueButton.interactable = false;
                onContinue();
            });

            dim.color = new Color(0f, 0f, 0f, 0f);
            title.localScale = Vector3.zero;
            star.localScale = Vector3.zero;
            tapToContinue.alpha = 0f;

            DOTween.Sequence()
                .Append(dim.DOFade(dimAlpha, 0.3f))
                .Append(title.DOScale(1f, 0.4f).SetEase(Ease.OutBack))
                .Append(star.DOScale(1f, 0.5f).SetEase(Ease.OutBack))
                .Join(star.DORotate(new Vector3(0f, 0f, -360f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic))
                .AppendCallback(sparkles.Play)
                .AppendInterval(0.4f)
                .Append(tapToContinue.DOFade(1f, 0.3f))
                .AppendCallback(() =>
                {
                    continueButton.interactable = true;
                    PlayIdleLoops();
                })
                .SetLink(gameObject);
        }

        /// <summary>The star breathes and the prompt blinks until the player taps.</summary>
        private void PlayIdleLoops()
        {
            star.DOScale(1.08f, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
            tapToContinue.DOFade(0.4f, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        }
    }
}
