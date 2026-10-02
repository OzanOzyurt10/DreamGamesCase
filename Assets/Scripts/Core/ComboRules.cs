using System.Collections.Generic;
using System.Linq;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>The single place that decides how a group of adjacent special items explodes together.</summary>
    internal static class ComboRules
    {
        /// <summary>Radius 3 is a 7x7 area.</summary>
        private const int TntTntRadius = 3;

        /// <summary>
        /// Starts the one explosion of a combo, centered on the tapped cell:
        /// at least two TNTs explode in 7x7; one TNT with rockets fires 3 rows and 3 columns of rockets;
        /// rockets only fire one horizontal and one vertical rocket (a + shape).
        /// </summary>
        public static void Detonate(IReadOnlyList<SpecialItem> members, GridPosition center, ExplosionResolver explosions, int sourceId)
        {
            const int tick = 0;
            int tntCount = members.OfType<Tnt>().Count();

            if (tntCount >= 2)
            {
                explosions.ExplodeArea(center, TntTntRadius, tick, sourceId);
            }
            else if (tntCount == 1)
            {
                for (int offset = -1; offset <= 1; offset++)
                {
                    explosions.FireRocket(new GridPosition(center.X, center.Y + offset), RocketDirection.Horizontal, tick, sourceId);
                    explosions.FireRocket(new GridPosition(center.X + offset, center.Y), RocketDirection.Vertical, tick, sourceId);
                }
            }
            else
            {
                explosions.FireRocket(center, RocketDirection.Horizontal, tick, sourceId);
                explosions.FireRocket(center, RocketDirection.Vertical, tick, sourceId);
            }
        }
    }
}
