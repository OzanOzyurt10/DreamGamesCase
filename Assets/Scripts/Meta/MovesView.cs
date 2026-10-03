using DG.Tweening;
using TMPro;
using UnityEngine;

namespace DreamCase.Meta
{
    /// <summary>The move counter in the top panel.</summary>
    public sealed class MovesView : MonoBehaviour
    {
        [SerializeField] private TMP_Text count;

        private const int NothingShown = -1;
        private int shownMoves = NothingShown;

        public void Show(int movesLeft)
        {
            if (movesLeft == shownMoves)
                return;

            count.text = movesLeft.ToString();

            // Pop the number when a move is spent, but not when the panel first appears.
            if (shownMoves != NothingShown)
            {
                count.transform.DOComplete();
                count.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f).SetLink(gameObject);
            }
            shownMoves = movesLeft;
        }
    }
}
