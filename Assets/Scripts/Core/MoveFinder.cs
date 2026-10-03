using System.Linq;

namespace DreamCase.Core
{
    public static class MoveFinder
    {
        /// <summary>Whether any item on the board can be tapped. Each item decides for itself.</summary>
        public static bool HasAvailableMove(Board board) => board.Items.Any(item => item.CanBeTapped(board));
    }
}
