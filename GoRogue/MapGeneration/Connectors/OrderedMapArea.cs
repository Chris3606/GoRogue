using System.Collections.Generic;
using GoRogue.Random;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Same as ClosestMapArea but connects random rooms instead of closest ones, or rooms in the order specified.
    /// </summary>
    static public class OrderedMapArea
    {
        // Random order, or same based on map areas if randomizeOrder is false.
        static public void Connect(ISettableMapOf<bool> map, Distance distanceCalc, AreaConnectionStrategy areaConnector, IRandom rng = null, bool randomizeOrder = true)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            var areas = MapAreaFinder.MapAreasFor(map, distanceCalc).ToList();
            if (randomizeOrder)
                areas.FisherYatesShuffle(rng);

            Connect(map, areaConnector, areas, rng);
        }

        // Defined order set by list passed in.
        static public void Connect(ISettableMapOf<bool> map, AreaConnectionStrategy areaConnector, IList<MapArea> mapAreas, IRandom rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

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

                TunnelGeneration.HorizontalVerticalTunnel(map, prevRoomConnection, curRoomConnection, rng);
            }
        }
    }
}
