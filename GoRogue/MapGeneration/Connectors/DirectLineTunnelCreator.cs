using GoRogue.MapViews;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Implements a tunnel creation algorithm that sets as walkable a direct line between the two
    /// points. In the case that MAHNATTAN distance is being used, the line is calculated via the
    /// Coord.CardinalPositionOnLine function. Otherwise, the line is calculated using
    /// Coord.PositionsOnLine (brensham's).
    /// </summary>
    public class DirectLineTunnelCreator : ITunnelCreator
    {
        private AdjacencyRule adjacencyRule;

        /// <summary>
        /// Constructor. Takes the distance calculation to use, which determines whether brensham's
        /// or CardinalPositionOnLine is used to create the tunnel.
        /// </summary>
        /// <param name="adjacencyRule">
        /// Method of adjacency to respect when creating tunnels. Cannot be diagonal.
        /// </param>
        public DirectLineTunnelCreator(AdjacencyRule adjacencyRule)
        {
            if (adjacencyRule == AdjacencyRule.DIAGONALS) throw new System.ArgumentException("Cannot use diagonal adjacency to create tunnels", nameof(adjacencyRule));
            this.adjacencyRule = adjacencyRule;
        }

        /// <summary>
        /// Implements the algorithm, creating the tunnel as specified in the class description.
        /// </summary>
        /// <param name="map">The map to create the tunnel on.</param>
        /// <param name="start">Start coordinate of the tunnel.</param>
        /// <param name="end">End coordinate of the tunnel.</param>
        public void CreateTunnel(ISettableMapView<bool> map, Coord start, Coord end)
        {
            var lineAlgorithm = (adjacencyRule == AdjacencyRule.CARDINALS) ? Lines.Algorithm.ORTHO : Lines.Algorithm.BRESENHAM;

            Coord previous = null;
            foreach (var pos in Lines.Get(start, end, lineAlgorithm))
            {
                map[pos] = true;
                // Previous cell, and we're going vertical, go 2 wide so it looks nicer Make sure not
                // to break rectangles (less than last index)!
                if (previous != null) // TODO: Make double wide vert an option
                    if (pos.Y != previous.Y)
                        if (pos.X + 1 < map.Width - 1)
                            map[pos.X + 1, pos.Y] = true;

                previous = pos;
            }
        }

        /// <summary>
        /// Implements the algorithm, creating the tunnel as specified in the class description.
        /// </summary>
        /// <param name="map">The map to create the tunnel on.</param>
        /// <param name="startX">X-value of the start position of the tunnel.</param>
        /// <param name="startY">Y-value of the start position of the tunnel.</param>
        /// <param name="endX">X-value of the end position of the tunnel.</param>
        /// <param name="endY">Y-value of the end position of the tunnel.</param>
        public void CreateTunnel(ISettableMapView<bool> map, int startX, int startY, int endX, int endY) => CreateTunnel(map, Coord.Get(startX, startY), Coord.Get(endX, endY));
    }
}