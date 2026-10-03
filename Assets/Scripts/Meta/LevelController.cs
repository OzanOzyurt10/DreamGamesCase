using System;
using System.Collections;
using DG.Tweening;
using DreamCase.Core;
using DreamCase.View;
using UnityEngine;

namespace DreamCase.Meta
{
    /// <summary>
    /// Runs the level's turn loop as a state machine: Idle → Resolving → Falling → (Shuffling) → CheckEnd → Idle, or Ended.
    /// The model resolves a tap instantly; this class lets the view play the result and accepts new taps only
    /// when the board is idle.
    /// </summary>
    public sealed class LevelController : MonoBehaviour
    {
        private enum State
        {
            Idle,
            Resolving,
            Falling,
            Shuffling,
            CheckEnd,
            Ended,
        }

        /// <summary>Raised once, when the level is won (true) or lost (false).</summary>
        public event Action<bool> Ended;

        private LevelSession session;
        private BoardView boardView;
        private BoardInput boardInput;
        private LevelHud hud;
        private State state;

        /// <summary>Method injection: MonoBehaviours cannot take constructor arguments.</summary>
        public void Initialize(LevelSession session, BoardView boardView, BoardInput boardInput, LevelHud hud)
        {
            this.session = session;
            this.boardView = boardView;
            this.boardInput = boardInput;
            this.hud = hud;

            // A starting board without a move is shuffled before it is first drawn, so there is nothing to animate.
            session.TryShuffle(out _);
            boardView.Build(session.Board);
            hud.Initialize(session);
            RefreshHints();
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

        private IEnumerator PlayTurn(TapResult result)
        {
            state = State.Resolving;
            hud.RefreshMoves(session);
            var resolving = result.Blast != null
                ? boardView.AnimateBlast(result.Blast)
                : boardView.AnimateExplosion(result.Explosion);
            yield return resolving.WaitForCompletion();

            // The model already knows the new goal counts; show them once the obstacles have visibly broken.
            hud.RefreshGoals(session);

            state = State.Falling;
            yield return boardView.AnimateFalls(session.Settle()).WaitForCompletion();

            if (session.TryShuffle(out var shuffled))
            {
                state = State.Shuffling;
                yield return boardView.AnimateShuffle(shuffled).WaitForCompletion();
            }

            state = State.CheckEnd;
            CheckEnd();
        }

        /// <summary>Win and loss are only decided here, after the board has fully settled.</summary>
        private void CheckEnd()
        {
            if (session.IsWon || session.IsLost)
            {
                state = State.Ended;
                Ended?.Invoke(session.IsWon);
                return;
            }

            RefreshHints();
            state = State.Idle;
        }

        private void RefreshHints() => boardView.ShowHints(HintFinder.Find(session.Board));
    }
}
