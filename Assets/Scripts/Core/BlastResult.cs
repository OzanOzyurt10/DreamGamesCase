using System.Collections.Generic;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>What a valid tap changed on the board, so the view can animate it afterwards.</summary>
    public sealed class BlastResult
    {
        public IReadOnlyList<Cube> BlastedCubes { get; }

        public BlastResult(IReadOnlyList<Cube> blastedCubes)
        {
            BlastedCubes = blastedCubes;
        }
    }
}
