using System.Collections.Generic;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>The special items that merged into the tapped cell to explode as one combo.</summary>
    public sealed class ComboMerge
    {
        public GridPosition Center { get; }
        public IReadOnlyList<SpecialItem> Members { get; }

        public ComboMerge(GridPosition center, IReadOnlyList<SpecialItem> members)
        {
            Center = center;
            Members = members;
        }
    }
}
