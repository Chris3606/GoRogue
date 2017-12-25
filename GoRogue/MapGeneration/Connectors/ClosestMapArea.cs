using System.Collections.Generic;
using GoRogue.Random;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Implements a connection algorithm that connects all unique map areas in the given map by connecting each
    /// area with the one closest to it.
    /// </summary>
    /// <remarks>
    /// The algorithm functions by first finding all unique areas in the map given by using MapAreaFinder.MapAreas.
    /// Then, we iterate through each area, find the closest area that is not already conencted to the current area,
    /// and create a tunnel between the two.  Distance between to areas is measured as the distance between the center
    /// point of the bounding boxes of those areas.
    /// 
    /// If the RANDOM_POINT AreaConnectionStrategy is selected, two random points are selected that are actually within
    /// the given areas -- thus this will connect maps with concave-shaped areas.  If CENTER_BOUNDS is selected,
    /// maps with concave areas may not be connected properly.
    /// </remarks>
    static public class ClosestMapArea
    {
        /// <summary>
        /// Connects the map given using the algorithm described in the class description.  If null is specified
        /// as the RNG, the default RNG is used.
        /// </summary>
        /// <param name="map">The map to connect.</param>
        /// <param name="shape">The shape of a radius -- used to determine distance calculation.</param>
        /// <param name="areaConnector">The area connection strategy to use.  Not all methods function on maps with
        /// concave areas -- see AreaConnectionStrategy enum documentation for details.</param>
        /// <param name="rng">The rng to use for any random numbers needed. null will result in the default RNG being used.</param>
        static public void Connect(ISettableMapOf<bool> map, Radius shape, AreaConnectionStrategy areaConnector, IRandom rng = null) =>
            Connect(map, (Distance)shape, areaConnector, rng);

        /// <summary>
        /// Connects the map given using the algorithm described in the class description. If null is specified
        /// as the RNG, the default RNG is used.
        /// </summary>
        /// <param name="map">The map to connect.</param>
        /// <param name="distanceCalc">The distance calculation that defines distance/neighbors.</param>
        /// <param name="areaConnector">The area connection strategy to use.  Not all methods function on maps with
        /// concave areas -- see AreaConnectionStrategy enum documentation for details.</param>
        /// <param name="rng">The rng to use for any random numbers needed.  null will result in the default RNG being used.</param>
        static public void Connect(ISettableMapOf<bool> map, Distance distanceCalc, AreaConnectionStrategy areaConnector, IRandom rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            var areas = MapAreaFinder.MapAreasFor(map, distanceCalc).ToList();

            var ds = new DisjointSet(areas.Count);
            while (ds.Count > 1) // Haven't unioned all sets into one
            {
                for (int i = 0; i < areas.Count; i++)
                {
                    int iClosest = findNearestMapArea(areas, distanceCalc, i, ds);

                    Coord iCoord = (areaConnector == AreaConnectionStrategy.RANDOM_POINT) ? 
                                      areas[i].Positions.RandomItem(rng) : areas[i].Bounds.Center;
                    Coord iClosestCoord = (areaConnector == AreaConnectionStrategy.RANDOM_POINT) ?
                                          areas[iClosest].Positions.RandomItem(rng) : areas[iClosest].Bounds.Center;

                    TunnelGeneration.DirectLineTunnel(map, distanceCalc, iCoord, iClosestCoord);
                    ds.MakeUnion(i, iClosest);
                }
            }
        }

        static private int findNearestMapArea(IList<MapArea> mapAreas, Distance distanceCalc, int mapAreaIndex, DisjointSet ds)
        {
            int closestIndex = mapAreaIndex;
            double distance = double.MaxValue;

            for (int i = 0; i < mapAreas.Count; i++)
            {
                if (i == mapAreaIndex)
                    continue;

                if (ds.InSameSet(i, mapAreaIndex))
                    continue;

                double distanceBetween = distanceCalc.DistanceBetween(mapAreas[mapAreaIndex].Bounds.Center, mapAreas[i].Bounds.Center);
                if (distanceBetween < distance)
                {
                    distance = distanceBetween;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
    }
}
