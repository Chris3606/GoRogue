using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.PerformanceTests.MapAreaFinders.Implementations
{
    /// <summary>
    /// Variation of <see cref="OriginalMapAreaFinder"/>
    /// </summary>
    [PublicAPI]
    internal class CachedDirectionMapAreaFinder
    {
        private bool[,]? _visited;

        private Direction[] _adjacentDirs = null!;
        private AdjacencyRule _adjacencyMethod;
        public AdjacencyRule AdjacencyMethod
        {
            get => _adjacencyMethod;
            set
            {
                _adjacencyMethod = value;
                _adjacentDirs = _adjacencyMethod.DirectionsOfNeighbors().ToArray();
            }
        }

        public IGridView<bool> AreasView;

        public CachedDirectionMapAreaFinder(IGridView<bool> areasView, AdjacencyRule adjacencyMethod)
        {
            AreasView = areasView;
            _visited = null;
            AdjacencyMethod = adjacencyMethod;
        }

        public static IEnumerable<Area> MapAreasFor(IGridView<bool> map, AdjacencyRule adjacencyMethod)
        {
            var areaFinder = new CachedDirectionMapAreaFinder(map, adjacencyMethod);
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
            var area = new Area();
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

                for (int i = 0; i < _adjacentDirs.Length; i++)
                {
                    var c = position + _adjacentDirs[i];

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
