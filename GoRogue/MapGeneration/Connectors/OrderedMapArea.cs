using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Random;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Same as ClosestMapArea but connects random rooms instead of closest ones, or rooms in the order specified.
    /// </summary>
    static public class OrderedMapArea
    {
        static public void Connect(ISettableMapOf<bool> map, Distance distanceCalc, AreaConnectionStrategy areaConnector, bool randomizeOrder = true)
            => Connect(map, distanceCalc, areaConnector, SingletonRandom.DefaultRNG, randomizeOrder);
        // Random order, or same based on map areas if randomizeOrder is false.
        static public void Connect(ISettableMapOf<bool> map, Distance distanceCalc, AreaConnectionStrategy areaConnector, IRandom rng, bool randomizeOrder = true)
        {
            var areas = MapAreaFinder.MapAreasFor(map, distanceCalc).ToList();
            if (randomizeOrder)
                areas.FisherYatesShuffle(rng);

            Connect(map, areaConnector, rng, areas);
        }

        static public void Connect(ISettableMapOf<bool> map, AreaConnectionStrategy areaConnector, IList<MapArea> mapAreas)
            => Connect(map, areaConnector, SingletonRandom.DefaultRNG, mapAreas);
        // Defined order set by list passed in.
        static public void Connect(ISettableMapOf<bool> map, AreaConnectionStrategy areaConnector, IRandom rng, IList<MapArea> mapAreas)
        {
            for (int i = 1; i < mapAreas.Count; i++)
            {
                Coord prevRoomConnection;
                Coord curRoomConnection;

                if (areaConnector == AreaConnectionStrategy.RANDOM_POINT)
                {
                    prevRoomConnection = mapAreas[i - 1].Positions.RandomItem(rng);
                    curRoomConnection = mapAreas[i].Positions.RandomItem(rng);
                }
                else
                {
                    prevRoomConnection = mapAreas[i - 1].Bounds.Center;
                    curRoomConnection = mapAreas[i].Bounds.Center;
                }

                TunnelGeneration.HorizontalVerticalTunnel(map, rng, prevRoomConnection, curRoomConnection);
            }
        }
    }
}
