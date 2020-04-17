using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Implements a connection algorithm that connects all unique map areas in the given map by
    /// connecting each area with the one closest to it, based on closeness of the area's center points.
    /// </summary>
    /// <remarks>
    /// The algorithm functions by first finding all unique areas in the map given by using
    /// <see cref="MapAreaFinder.MapAreas"/>. Then, we iterate through each area, find the closest area that is not
    /// already conencted to the current area.  Distance between to areas is measured as the distance between the
    /// center point of the bounding boxes of those areas. Points to connect two areas, as well as method used to create
    /// a tunnel between those two points, are selected via specified parameters.
    /// </remarks>
    public static class ClosestMapAreaConnector
    {
        /// <summary>
        /// Connects the given map using the algorithm described in the class description.  Map areas
        /// are automatically determined using a <see cref="MapAreaFinder"/>.
        /// </summary>
        /// <param name="map">The map to connect.</param>
        /// <param name="distanceCalc">The distance calculation that defines distance/neighbors.</param>
        /// <param name="areaConnector">
        /// The area connection strategy to use. Not all methods function on maps with concave areas
        /// -- see respective class documentation for details.
        /// </param>
        /// <param name="tunnelCreator">
        /// The tunnel creation strategy to use. If null is specified, <see cref="DirectLineTunnelCreator"/> with
        /// the distance calculation specified is used.
        /// </param>
        public static void Connect(ISettableMapView<bool> map, Distance distanceCalc, IAreaConnectionPointSelector? areaConnector = null, ITunnelCreator? tunnelCreator = null)
        {
            var areas = MapAreaFinder.MapAreasFor(map, distanceCalc).ToList();
            Connect(map, areas, distanceCalc, areaConnector, tunnelCreator);
        }

        /// <summary>
        /// Connects the map areas given on the given map using the algorithm described in the class description.
        /// </summary>
        /// <param name="map">The map to connect.</param>
        /// <param name="mapAreas">The map areas to connect on the given map.</param>
        /// <param name="distanceCalc">The distance calculation that defines distance/neighbors.</param>
        /// <param name="areaConnector">
        /// The area connection strategy to use. Not all methods function on maps with concave areas
        /// -- see respective class documentation for details.
        /// </param>
        /// ///
        /// <param name="tunnelCreator">
        /// The tunnel creation strategy to use. If null is specified, <see cref="DirectLineTunnelCreator"/> with
        /// the distance calculation specified is used.
        /// </param>
        public static void Connect(ISettableMapView<bool> map, IReadOnlyList<Area> mapAreas, Distance distanceCalc, IAreaConnectionPointSelector? areaConnector = null, ITunnelCreator? tunnelCreator = null)
        {
            if (areaConnector == null) areaConnector = new RandomConnectionPointSelector();
            if (tunnelCreator == null) tunnelCreator = new DirectLineTunnelCreator(distanceCalc);

            var ds = new DisjointSet(mapAreas.Count);
            while (ds.Count > 1) // Haven't unioned all sets into one
            {
                for (int i = 0; i < mapAreas.Count; i++)
                {
                    int iClosest = findNearestMapArea(mapAreas, distanceCalc, i, ds);

                    var connectionPoints = areaConnector.SelectConnectionPoints(mapAreas[i], mapAreas[iClosest]);

                    tunnelCreator.CreateTunnel(map, connectionPoints.Item1, connectionPoints.Item2);
                    ds.MakeUnion(i, iClosest);
                }
            }
        }

        private static int findNearestMapArea(IReadOnlyList<IReadOnlyArea> mapAreas, Distance distanceCalc, int mapAreaIndex, DisjointSet ds)
        {
            int closestIndex = mapAreaIndex;
            double distance = double.MaxValue;

            for (int i = 0; i < mapAreas.Count; i++)
            {
                if (i == mapAreaIndex)
                    continue;

                if (ds.InSameSet(i, mapAreaIndex))
                    continue;

                double distanceBetween = distanceCalc.Calculate(mapAreas[mapAreaIndex].Bounds.Center, mapAreas[i].Bounds.Center);
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
