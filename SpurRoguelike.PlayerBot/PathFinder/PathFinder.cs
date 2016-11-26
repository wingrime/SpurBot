using SpurRoguelike.Core.Primitives;
using SpurRoguelike.Core.Views;
using System.Collections.Generic;
using System.Linq;

namespace SpurRoguelike.PlayerBot
{
    internal class PathFinder
    {
        private readonly LevelView level;
        private readonly Location playerLocation;

        public PathFinder(LevelView level, Location playerLocation)
        {
            this.level = level;
            this.playerLocation = playerLocation;
        }

        public Path FindPath(Location target)
        {
            if (playerLocation.IsInStepRange(target))
                return new Path(target);

            HashSet<Location> directVisitedSet;
            List<Path> routesFrom;

            InitPaths(playerLocation, out directVisitedSet, out routesFrom);

            HashSet<Location> reverseVisitedSet;
            List<Path> routesTo;
            InitPaths(target, out reverseVisitedSet, out routesTo);

            var route = RecursiveSearch(target, routesFrom, routesTo, directVisitedSet, reverseVisitedSet);

            return route;
        }

        private static void InitPaths(Location target, out HashSet<Location> visited, out List<Path> pathList)
        {
            visited = new HashSet<Location>();
            pathList = new List<Path>();
            pathList.Add(new Path(target));
        }

        private IEnumerable<Path> GetAdjPaths(IEnumerable<Path> paths, HashSet<Location> visited)
        {
            var searchPath = new List<Path>();

            foreach (Path path in paths)
            {
                IEnumerable<Location> adjacents = path.Last().ValidAdjacents(level);

                List<Path> searchedPaths = adjacents
                    .Where(l => !visited.Contains(l))
                    .Select(l => new Path(path, l)).ToList();

                visited.UnionWith(adjacents);
                searchPath.AddRange(searchedPaths);
            }

            return searchPath;
        }

        private Path IntersectPath(IEnumerable<Path> directPaths,
                                   IEnumerable<Path> reversePaths,
                                   IEnumerable<Location> intersection)
        {
            // If we have to paths such end with a same cell
            Path directPath = directPaths
                .Where(m => intersection.Contains(m.Last())
                            && reversePaths.Any(n => n.Contains(m.Last())))
                .OrderBy(m => m.SafetyRank(level.Monsters))
                .ThenBy(m => m.Count)
                .First();

            Path reversePath = reversePaths
                .Where(m => m.Any(n => directPath.Contains(n)))
                .OrderBy(m => m.SafetyRank(level.Monsters))
                .ThenBy(m => m.Count)
                .First();

            reversePath.Reverse();
            return new Path(directPath.Concat(reversePath.Skip(1)).Skip(1));
        }

        private Path RecursiveSearch(Location target,
                                     IEnumerable<Path> pathForward,
                                     IEnumerable<Path> pathBackward,
                                     HashSet<Location> forwardVisited,
                                     HashSet<Location> backwardVisited)
        {
            IEnumerable<Path> directPaths = GetAdjPaths(pathForward, forwardVisited);

            IEnumerable<Path> reversePaths = GetAdjPaths(pathBackward, backwardVisited);

            IEnumerable<Location> intersection = backwardVisited.Intersect(forwardVisited);

            if (intersection.Any())
                return IntersectPath(directPaths, reversePaths, intersection);

            if (directPaths.Any() && reversePaths.Any())
                return RecursiveSearch(target, directPaths, reversePaths, forwardVisited, backwardVisited);

            return new Path();
        }
    }
}