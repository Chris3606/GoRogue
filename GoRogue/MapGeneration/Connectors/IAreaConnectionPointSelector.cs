using System;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Interface for implementing an algorithm for selecting the Coords to connect in order to
    /// connect two given map areas.
    /// </summary>
    public interface IAreaConnectionPointSelector
    {
        /// <summary>
        /// Implements the algorithm. Returns a tuple of two Coords -- the first Coord is the
        /// position in area1 to use, the second Coord is the position in area2 to use.
        /// </summary>
        /// <param name="area1">
        /// First MapArea to connect.
        /// </param>
        /// <param name="area2">
        /// Second MapArea to connect.
        /// </param>
        /// <returns>
        /// A tuple containing the coordinates from each MapArea to connect -- the first item in the
        /// tuple is the Coord in area1, the second is the Coord in area2.
        /// </returns>
        Tuple<Coord, Coord> SelectConnectionPoints(MapArea area1, MapArea area2);
    }
}