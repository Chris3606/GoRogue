using System.Collections.Generic;
using GoRogue.Random;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Same as ClosestMapArea but connects random rooms instead of determining the closest one, or connects rooms in the order specified if you give it a list of map
    /// areas.
    /// </summary>
    static public class OrderedMapArea
    {
        // Random order, or same based on map areas if randomizeOrder is false.
        static public void Connect(ISettableMapOf<bool> map, Distance distanceCalc, IAreaConnectionPointSelector areaConnector = null,
                                    IRandom rng = null, bool randomizeOrder = true)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;
            if (areaConnector == null) areaConnector = new RandomConnectionPointSelector(rng);

            var areas = MapAreaFinder.MapAreasFor(map, distanceCalc).ToList();
            if (randomizeOrder)
                areas.FisherYatesShuffle(rng);

            Connect(map, areas, areaConnector, rng);
        }

        // Defined order set by list passed in.
        static public void Connect(ISettableMapOf<bool> map, IList<MapArea> mapAreas, IAreaConnectionPointSelector areaConnector = null, IRandom rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;
            if (areaConnector == null) areaConnector = new RandomConnectionPointSelector(rng);

            for (int i = 1; i < mapAreas.Count; i++)
            {
                var connectionPoints = areaConnector.SelectConnectionPoints(mapAreas[i - 1], mapAreas[i]);
                TunnelGeneration.HorizontalVerticalTunnel(map, connectionPoints.Item1, connectionPoints.Item2, rng);
            }
        }
    }
}
