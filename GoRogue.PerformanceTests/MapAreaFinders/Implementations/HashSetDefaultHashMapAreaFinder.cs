using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.PerformanceTests.MapAreaFinders.Implementations
{
    /// <summary>
    /// Variation of <see cref="OriginalMapAreaFinder"/> that uses a hash set in place of map-sized array for
    /// contains operations; using the default Point hashing algorithm for areas and hash sets.
    /// </summary>
    [PublicAPI]
    internal class HashSetDefaultHashMapAreaFinder
    {

        private HashSet<Point> _visited;
        public AdjacencyRule AdjacencyMethod;

        public IGridView<bool> AreasView;

        public HashSetDefaultHashMapAreaFinder(IGridView<bool> areasView, AdjacencyRule adjacencyMethod)
        {
            AreasView = areasView;
            _visited = new HashSet<Point>();
            AdjacencyMethod = adjacencyMethod;
        }

        public static IEnumerable<Area> MapAreasFor(IGridView<bool> map, AdjacencyRule adjacencyMethod)
        {
            var areaFinder = new HashSetDefaultHashMapAreaFinder(map, adjacencyMethod);
            return areaFinder.MapAreas();
        }

        public IEnumerable<Area> MapAreas()
        {
            _visited.Clear();

            for (var x = 0; x < AreasView.Width; x++)
                for (var y = 0; y < AreasView.Height; y++)
                {
                    var area = Visit(new Point(x, y));

                    if (!ReferenceEquals(null, area) && area.Count != 0)
                        yield return area;
                }
        }

        private Area? Visit(Point position)
        {
            // Don't bother allocating a MapArea, because the starting point isn't valid.
            if (!AreasView[position])
                return null;

            var stack = new Stack<Point>();
            var area = new Area();
            stack.Push(position);

            while (stack.Count != 0)
            {
                position = stack.Pop();
                // Already visited, or not part of any mapArea.  Also only called from functions that have allocated
                // visited
                if (_visited.Contains(position) || !AreasView[position])
                    continue;

                area.Add(position);
                _visited.Add(position);

                foreach (var c in AdjacencyMethod.Neighbors(position))
                {
                    // Out of bounds, thus not actually a neighbor
                    if (c.X < 0 || c.Y < 0 || c.X >= AreasView.Width || c.Y >= AreasView.Height)
                        continue;

                    if (AreasView[c] && !_visited.Contains(c))
                        stack.Push(c);
                }
            }

            return area;
        }
    }
}
