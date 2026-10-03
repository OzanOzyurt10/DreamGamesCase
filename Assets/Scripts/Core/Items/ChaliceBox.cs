using System;

namespace DreamCase.Core.Items
{
    /// <summary>
    /// A 2x2 obstacle: one object referenced by four board cells. Doesn't fall.
    /// Door phase: any damage source opens the door and deals exactly that one damage.
    /// Chalice phase: each point of damage (cells hit by an explosion, or blasted cubes next to it)
    /// takes one chalice; the box is cleared when all chalices are collected.
    /// </summary>
    public sealed class ChaliceBox : Obstacle
    {
        public const int ChaliceCount = 10;

        private const int NoSource = 0;

        private int doorOpeningSourceId = NoSource;

        public override int Width => 2;
        public override int Height => 2;
        public override bool CanFall => false;

        public bool IsDoorOpen => doorOpeningSourceId != NoSource;
        public int ChalicesLeft { get; private set; } = ChaliceCount;

        internal override DamageOutcome TakeDamage(DamageInfo damage)
        {
            if (!IsDoorOpen)
            {
                doorOpeningSourceId = damage.SourceId;
                return DamageOutcome.Damaged;
            }

            // The source that opened the door has dealt its one damage; its other hits don't reach the chalices.
            if (damage.SourceId == doorOpeningSourceId)
                return DamageOutcome.None;

            ChalicesLeft = Math.Max(0, ChalicesLeft - damage.AffectedCells);
            return ChalicesLeft > 0 ? DamageOutcome.Damaged : DamageOutcome.Cleared;
        }
    }
}
