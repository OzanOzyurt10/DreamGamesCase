namespace DreamCase.Core
{
    /// <summary>The single place that decides which special item a group size earns. Used for both creation and hints.</summary>
    public static class BlastRewards
    {
        private const int RocketMinGroupSize = 4;
        private const int TntMinGroupSize = 6;

        public static BlastReward ForGroupSize(int groupSize)
        {
            if (groupSize >= TntMinGroupSize)
                return BlastReward.Tnt;
            if (groupSize >= RocketMinGroupSize)
                return BlastReward.Rocket;
            return BlastReward.None;
        }
    }
}
