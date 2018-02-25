using GoRogue.Random;
using System.Collections.Generic;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Same as ClosestMapAreaConnector, but connects random rooms instead of determining the closest
    /// one, or connects rooms in the order specified if you give it a list of MapAreas.
    /// </summary>
    static public class OrderedMapAreaConnector
    {
        /// <summary>
        /// Connects the areas by determining all unique areas on the map given using a
        /// MapAreaFinder, then, if randomizeOrder is true, performing a Fisher Yates shuffle of that
        /// list of areas found. It then simply connects areas adjacent to each other in that list,
        /// using the methods specified to determine points within two areas to connect, and how to
        /// create the tunnel between the two points.
        /// </summary>
        /// <param name="map">The map to connect.</param>
        /// <param name="adjacencyRule">
        /// Method of adjacency to respect when determining map areas. Cannot be diagonal.
        /// </param>
        /// <param name="areaConnector">
        /// The method to use to determine the points from two areas to make a tunnel between, in
        /// order to connect those two areas. If null is specified, a RandomConnectionPointSelector
        /// is used, that uses the RNG passed into this function.
        /// </param>
        /// <param name="tunnelCreator">
        /// The tunnel creation strategy to use. If null is specified,
        /// HorizontalVerticalTunnelCreator that utilizes the RNG passed into this function is used.
        /// </param>
        /// <param name="rng">The rng to use. If null is specified, the default rng is assumed.</param>
        /// <param name="randomizeOrder">
        /// Whether or not to randomize which room is connected to which -- if this is set to false,
        /// they will be conencted in the exact order they are returned from the MapAreaFinder.
        /// </param>
        static public void Connect(ISettableMapView<bool> map, AdjacencyRule adjacencyRule, IAreaConnectionPointSelector areaConnector = null,
                                    ITunnelCreator tunnelCreator = null, IRandom rng = null, bool randomizeOrder = true)
        {
            if (adjacencyRule == AdjacencyRule.DIAGONALS) throw new System.ArgumentException("Cannot specify diagonal adjacency for map connections.", nameof(adjacencyRule));
            if (rng == null) rng = SingletonRandom.DefaultRNG;
            if (areaConnector == null) areaConnector = new RandomConnectionPointSelector(rng);
            if (tunnelCreator == null) tunnelCreator = new HorizontalVerticalTunnelCreator(rng);

            var areas = MapAreaFinder.MapAreasFor(map, adjacencyRule).ToList();
            if (randomizeOrder)
                areas.FisherYatesShuffle(rng);

            Connect(map, areas, areaConnector, tunnelCreator);
        }

        /// <summary>
        /// Connects the areas by simply connecting areas adjacent to each other in the passed in
        /// list of areas, using the methods specified to determine points within two areas to
        /// connect, and how to create the tunnel between the two points.
        /// </summary>
        /// <param name="map">The map to connect.</param>
        /// <param name="mapAreas">
        /// The list of map areas to connect, in the order they should be connected.
        /// </param>
        /// <param name="areaConnector">
        /// The method to use to determine the points from two areas to make a tunnel between, in
        /// order to connect those two areas. If null is specified, a RandomConnectionPointSelector
        /// is used, that uses the default RNG.
        /// </param>
        /// <param name="tunnelCreator">
        /// The tunnel creation strategy to use. If null is specified,
        /// HorizontalVerticalTunnelCreator that utilizes the default RNG is used.
        /// </param>
        static public void Connect(ISettableMapView<bool> map, IReadOnlyList<MapArea> mapAreas, IAreaConnectionPointSelector areaConnector = null,
                                    ITunnelCreator tunnelCreator = null)
        {
            if (areaConnector == null) areaConnector = new RandomConnectionPointSelector();
            if (tunnelCreator == null) tunnelCreator = new HorizontalVerticalTunnelCreator();

            for (int i = 1; i < mapAreas.Count; i++)
            {
                var connectionPoints = areaConnector.SelectConnectionPoints(mapAreas[i - 1], mapAreas[i]);
                tunnelCreator.CreateTunnel(map, connectionPoints.Item1, connectionPoints.Item2);
            }
        }
    }
}