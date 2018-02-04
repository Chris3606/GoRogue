namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Interface for implementing an algorithm for creating a tunnel between two positions on a walkability map.
    /// </summary>
    public interface ITunnelCreator
    {
        /// <summary>
        /// Implements the algorithm, creating the tunnel between the two points (ensuring there is a path of positions set to true between those two points.
        /// </summary>
        /// <param name="map">Map to create the tunnel on.</param>
        /// <param name="start">Start position to connect.</param>
        /// <param name="end">End position to connect.</param>
        void CreateTunnel(ISettableMapView<bool> map, Coord start, Coord end);
    }
}