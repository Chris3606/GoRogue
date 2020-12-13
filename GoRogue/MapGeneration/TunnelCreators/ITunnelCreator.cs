using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.MapGeneration.TunnelCreators
{
    /// <summary>
    /// Interface for implementing an algorithm for creating a tunnel between two positions on a
    /// walkability map.
    /// </summary>
    [PublicAPI]
    public interface ITunnelCreator
    {
        /// <summary>
        /// Implements the algorithm, creating the tunnel between the two points (ensuring there is a
        /// path of positions set to true between those two points).
        /// </summary>
        /// <param name="map">_grid to create the tunnel on.</param>
        /// <param name="tunnelStart">Start position to connect.</param>
        /// <param name="tunnelEnd">End position to connect.</param>
        /// <returns>An area containing all points that are part of the tunnel.</returns>
        Area CreateTunnel(ISettableGridView<bool> map, Point tunnelStart, Point tunnelEnd);

        /// <summary>
        /// Implements the algorithm, creating the tunnel between the two points (ensuring there is a
        /// path of positions set to true between those two points).
        /// </summary>
        /// <param name="map">_grid to create the tunnel on.</param>
        /// <param name="startX">X-value of the start position to connect.</param>
        /// <param name="startY">Y-value of the start position to connect.</param>
        /// <param name="endX">X-value of the end position to connect.</param>
        /// <param name="endY">Y-value of the end position to connect.</param>
        /// <returns>An area containing all points that are part of the tunnel.</returns>
        Area CreateTunnel(ISettableGridView<bool> map, int startX, int startY, int endX, int endY);
    }
}
