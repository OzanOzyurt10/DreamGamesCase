using System.Collections;
using DG.Tweening;
using DreamCase.Core;
using DreamCase.View;
using UnityEngine;

namespace DreamCase.Meta
{
    /// <summary>
    /// Runs the level's turn loop as a state machine. The model resolves a tap instantly; this class
    /// then lets the view play the result and accepts new taps only when the board is idle.
    /// </summary>
    public sealed class LevelController : MonoBehaviour
    {
        private enum State
        {
            Idle,
            Resolving,
            Falling,
        }

        private LevelSession session;
        private BoardView boardView;
        private BoardInput boardInput;
        private State state;

        /// <summary>Method injection: MonoBehaviours cannot take constructor arguments.</summary>
        public void Initialize(LevelSession session, BoardView boardView, BoardInput boardInput)
        {
            this.session = session;
            this.boardView = boardView;
            this.boardInput = boardInput;

            boardView.Build(session.Board);
            boardInput.CellTapped += OnCellTapped;
            state = State.Idle;
        }

        private void OnDestroy()
        {
            if (boardInput != null)
                boardInput.CellTapped -= OnCellTapped;
        }

        private void OnCellTapped(GridPosition cell)
        {
            if (state != State.Idle)
                return;

            var result = session.TryTap(cell);
            if (result != null)
                StartCoroutine(PlayTurn(result));
        }

        private IEnumerator PlayTurn(BlastResult result)
        {
            state = State.Resolving;
            yield return boardView.AnimateBlast(result).WaitForCompletion();

            state = State.Falling;
            yield return boardView.AnimateFalls(session.Settle()).WaitForCompletion();

            state = State.Idle;
        }
    }
}
