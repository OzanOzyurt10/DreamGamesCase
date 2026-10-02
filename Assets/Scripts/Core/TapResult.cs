namespace DreamCase.Core
{
    /// <summary>What a valid tap did: either blasted a cube group or detonated a special item.</summary>
    public sealed class TapResult
    {
        /// <summary>Set when a cube group was blasted; otherwise null.</summary>
        public BlastResult Blast { get; }

        /// <summary>Set when a special item was detonated; otherwise null.</summary>
        public ExplosionResult Explosion { get; }

        private TapResult(BlastResult blast, ExplosionResult explosion)
        {
            Blast = blast;
            Explosion = explosion;
        }

        public static TapResult FromBlast(BlastResult blast) => new TapResult(blast, null);

        public static TapResult FromExplosion(ExplosionResult explosion) => new TapResult(null, explosion);
    }
}
