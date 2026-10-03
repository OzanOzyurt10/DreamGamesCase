using DreamCase.Core;
using UnityEngine;

namespace DreamCase.Meta
{
    /// <summary>The top panel of LevelScene: goals on the left, moves on the right.</summary>
    public sealed class LevelHud : MonoBehaviour
    {
        [SerializeField] private GoalsView goals;
        [SerializeField] private MovesView moves;

        public void Initialize(LevelSession session)
        {
            goals.Initialize(session.RemainingObstacles());
            moves.Show(session.MovesLeft);
        }

        public void RefreshMoves(LevelSession session) => moves.Show(session.MovesLeft);

        public void RefreshGoals(LevelSession session) => goals.Refresh(session.RemainingObstacles());
    }
}
