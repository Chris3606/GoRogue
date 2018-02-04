namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Implements a tunnel creation algorithm that sets as walkable a direct line between the two points.  In the case that MAHNATTAN distance is being used,
    /// the line is calculated via the Coord.CardinalPositionOnLine function.  Otherwise, the line is calculated using Coord.PositionsOnLine (brensham's).
    /// </summary>
    public class DirectLineTunnelCreator : ITunnelCreator
    {
        private Distance distanceCalc;

        /// <summary>
        /// Constructor.  Takes the shape that defines a radius, which is used to determine the proper distance calculation.
        /// </summary>
        /// <param name="shape">The shape defining a radius, which determines the distance calculation to use.</param>
        public DirectLineTunnelCreator(Radius shape)
            : this((Distance)shape) { }

        /// <summary>
        /// Constructor. Takes the distance calculation to use, which determines whether brensham's or CardinalPositionOnLine is used to create the tunnel.
        /// </summary>
        /// <param name="distanceCalc">The distance calculation to use.</param>
        public DirectLineTunnelCreator(Distance distanceCalc)
        {
            this.distanceCalc = distanceCalc;
        }

        /// <summary>
        /// Implmenets the algorithm, creating the tunnel as specified in the class description.
        /// </summary>
        /// <param name="map">The map to create the tunnel on.</param>
        /// <param name="start">Start coordinate of the tunnel.</param>
        /// <param name="end">End coordinate of the tunnel.</param>
        public void CreateTunnel(ISettableMapView<bool> map, Coord start, Coord end)
        {
            var lineAlgorithm = (distanceCalc == Distance.MANHATTAN) ? Lines.Algorithm.ORTHO : Lines.Algorithm.BRESENHAM;

            Coord previous = null;
            foreach (var pos in Lines.Get(start, end, lineAlgorithm))
            {
                map[pos] = true;
                // Previous cell, and we're going vertical, go 2 wide so it looks nicer
                // Make sure not to break rectangles (less than last index)!
                if (previous != null) // TODO: Make double wide vert an option
                    if (pos.Y != previous.Y)
                        if (pos.X + 1 < map.Width - 1)
                            map[pos.X + 1, pos.Y] = true;

                previous = pos;
            }
        }
    }
}