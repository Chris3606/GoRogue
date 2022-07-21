using System;
using System.Collections.Generic;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using SadRogue.Primitives.PointHashers;

namespace GoRogue.PerformanceTests.MapAreaFinders.Implementations
{
    /// <summary>
    /// Variation of <see cref="OriginalMapAreaFinder"/> that uses the known-size hashing method for its returned
    /// areas.
    /// </summary>
    public class OriginalSizeHashMapAreaFinder
    {
        private bool[,]? _visited;

        public AdjacencyRule AdjacencyMethod;

        public IGridView<bool> AreasView;

        public OriginalSizeHashMapAreaFinder(IGridView<bool> areasView, AdjacencyRule adjacencyMethod)
        {
            AreasView = areasView;
            _visited = null;
            AdjacencyMethod = adjacencyMethod;
        }

        public static IEnumerable<Area> MapAreasFor(IGridView<bool> map, AdjacencyRule adjacencyMethod)
        {
            var areaFinder = new OriginalSizeHashMapAreaFinder(map, adjacencyMethod);
            return areaFinder.MapAreas();
        }

        public IEnumerable<Area> MapAreas()
        {
            if (_visited == null || _visited.GetLength(1) != AreasView.Height || _visited.GetLength(0) != AreasView.Width)
                _visited = new bool[AreasView.Width, AreasView.Height];
            else
                Array.Clear(_visited, 0, _visited.Length);

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
            var area = new Area(new KnownSizeHasher(AreasView.Width));
            stack.Push(position);

            while (stack.Count != 0)
            {
                position = stack.Pop();
                // Already visited, or not part of any mapArea.  Also only called from functions that have allocated
                // visited
                if (_visited![position.X, position.Y] || !AreasView[position])
                    continue;

                area.Add(position);
                _visited[position.X, position.Y] = true;

                foreach (var c in AdjacencyMethod.Neighbors(position))
                {
                    // Out of bounds, thus not actually a neighbor
                    if (c.X < 0 || c.Y < 0 || c.X >= AreasView.Width || c.Y >= AreasView.Height)
                        continue;

                    if (AreasView[c] && !_visited[c.X, c.Y])
                        stack.Push(c);
                }
            }

            return area;
        }
    }
}
