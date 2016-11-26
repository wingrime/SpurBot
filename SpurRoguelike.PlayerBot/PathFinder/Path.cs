using SpurRoguelike.Core.Primitives;
using SpurRoguelike.Core.Views;
using System.Collections.Generic;
using System.Linq;

namespace SpurRoguelike.PlayerBot
{
    internal class Path : List<Location>
    {
        public Path()
        {
        }

        public Path(Location location) : base()
        {
            base.Add(location);
        }

        public Path(Path route, Location node) : base(route)
        {
            Add(node);
        }

        public Path(IEnumerable<Location> locations) : base(locations)
        {
        }

        public int SafetyRank(IEnumerable<PawnView> levelMonsters)
        {
            return levelMonsters.Count(m => Offset.AttackOffsets.Any(o => this.Any(l => l + o == m.Location)));
        }
    }
}