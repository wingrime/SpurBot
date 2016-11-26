using SpurRoguelike.Core.Primitives;
using SpurRoguelike.Core.Views;
using System.Collections.Generic;
using System.Linq;

namespace SpurRoguelike.PlayerBot
{
    internal static class LocationExtensions
    {
        public static int Distance(this Location from, Location to)
        {
            return (to - from).Size();
        }

        public static bool IsInAttackRange(this Location l, Location other)
        {
            return Offset.AttackOffsets.Any(o => l + o == other);
        }

        public static bool IsInStepRange(this Location l, Location other)
        {
            return Offset.StepOffsets.Any(o => l + o == other);
        }

        public static bool IsStepValid(this Location to, LevelView view)
        {
            if (to.X < 0 || to.X >= view.Field.Width ||
                to.Y < 0 || to.Y >= view.Field.Height)
                return false;

            return view.Field[to] != CellType.Trap
                && view.Field[to] != CellType.Wall
                && view.Field[to] != CellType.Exit
                && !view.GetMonsterAt(to).HasValue
                && !view.GetItemAt(to).HasValue;
        }

        public static IEnumerable<Location> ValidAdjacents(this Location from, LevelView level)
        {
            return Offset.StepOffsets
                .Select(m => (from + m))
                .Where(m => m.IsStepValid(level));
        }
    }
}