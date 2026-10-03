using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DreamCase.Meta
{
    /// <summary>One goal in the top panel: an obstacle icon with how many are left, or a check mark when none are.</summary>
    public sealed class GoalEntryView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text count;
        [SerializeField] private Image check;

        private const int NothingShown = -1;
        private int shownCount = NothingShown;

        public void SetIcon(Sprite sprite) => icon.sprite = sprite;

        public void Show(int remaining)
        {
            if (remaining == shownCount)
                return;

            bool done = remaining == 0;
            count.text = remaining.ToString();
            count.gameObject.SetActive(!done);
            check.gameObject.SetActive(done);

            // Pop the icon when the number changes, but not when the panel first appears.
            if (shownCount != NothingShown)
            {
                icon.transform.DOComplete();
                icon.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f).SetLink(gameObject);
            }
            shownCount = remaining;
        }
    }
}
