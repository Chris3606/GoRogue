using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Interface for implementing an algorithm for creating a tunnel between two positions on a
    /// walkability map.
    /// </summary>
    public interface ITunnelCreator
    {
        /// <summary>
        /// Implements the algorithm, creating the tunnel between the two points (ensuring there is a
        /// path of positions set to true between those two points).
        /// </summary>
        /// <param name="map">Map to create the tunnel on.</param>
        /// <param name="tunnelStart">Start position to connect.</param>
        /// <param name="tunnelEnd">End position to connect.</param>
        void CreateTunnel(ISettableMapView<bool> map, Point tunnelStart, Point tunnelEnd);

        /// <summary>
        /// Implements the algorithm, creating the tunnel between the two points (ensuring there is a
        /// path of positions set to true between those two points.
        /// </summary>
        /// <param name="map">Map to create the tunnel on.</param>
        /// <param name="startX">X-value of the start position to connect.</param>
        /// <param name="startY">Y-value of the start position to connect.</param>
        /// <param name="endX">X-value of the end position to connect.</param>
        /// <param name="endY">Y-value of the end position to connect.</param>
        void CreateTunnel(ISettableMapView<bool> map, int startX, int startY, int endX, int endY);
    }
}
