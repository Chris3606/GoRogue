using System;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.MapGeneration.TunnelCreators
{
    /// <summary>
    /// Implements a tunnel creation algorithm that sets as walkable a direct line between the two
    /// points. In the case that <see cref="Distance.Manhattan" /> is being used, the line is calculated via the
    /// <see cref="Lines.Algorithm.Orthogonal" /> algorithm.  Otherwise, the line is calculated using
    /// <see cref="Lines.Algorithm.Bresenham" />.
    /// </summary>
    [PublicAPI]
    public class DirectLineTunnelCreator : ITunnelCreator
    {
        private readonly AdjacencyRule _adjacencyRule;
        private readonly bool _doubleWideVertical;

        /// <summary>
        /// Constructor. Takes the distance calculation to use, which determines whether <see cref="Lines.Algorithm.Orthogonal" />
        /// or <see cref="Lines.Algorithm.Bresenham" /> is used to create the tunnel.
        /// </summary>
        /// <param name="adjacencyRule">
        /// Method of adjacency to respect when creating tunnels. Cannot be diagonal.
        /// </param>
        /// <param name="doubleWideVertical">Whether or not to create vertical tunnels as 2-wide.</param>
        public DirectLineTunnelCreator(AdjacencyRule adjacencyRule, bool doubleWideVertical = true)
        {
            if (adjacencyRule == AdjacencyRule.Diagonals)
                throw new ArgumentException("Cannot use diagonal adjacency to create tunnels", nameof(adjacencyRule));
            _adjacencyRule = adjacencyRule;
            _doubleWideVertical = doubleWideVertical;
        }

        /// <inheritdoc />
        public Area CreateTunnel(ISettableGridView<bool> map, Point start, Point end)
        {
            var lineAlgorithm = _adjacencyRule == AdjacencyRule.Cardinals
                ? Lines.Algorithm.Orthogonal
                : Lines.Algorithm.Bresenham;
            var area = new Area();

            var previous = Point.None;
            foreach (var pos in Lines.Get(start, end, lineAlgorithm))
            {
                map[pos] = true;
                area.Add(pos);
                // Previous cell, and we're going vertical, go 2 wide so it looks nicer Make sure not
                // to break rectangles (less than last index)!
                if (_doubleWideVertical && previous != Point.None && pos.Y != previous.Y && pos.X + 1 < map.Width - 1)
                {
                    var wideningPos = pos + (1, 0);
                    map[wideningPos] = true;
                    area.Add(wideningPos);
                }

                previous = pos;
            }

            return area;
        }

        /// <inheritdoc />
        public Area CreateTunnel(ISettableGridView<bool> map, int startX, int startY, int endX, int endY)
            => CreateTunnel(map, new Point(startX, startY), new Point(endX, endY));
    }
}
