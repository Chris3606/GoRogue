using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Implements a connection algorithm that connects all unique map areas in the given map by
    /// connecting each area with the one closest to it.
    /// </summary>
    /// <remarks>
    /// The algorithm functions by first finding all unique areas in the map given by using
    /// MapAreaFinder.MapAreas. Then, we iterate through each area, find the closest area that is not
    /// already conencted to the current area, and create a tunnel between the two. Distance between
    /// to areas is measured as the distance between the center point of the bounding boxes of those
    /// areas. /// Points to connect two areas, as well as method used to create a tunnel between
    /// those two points, are selected via specified parameters.
    /// </remarks>
    static public class ClosestMapAreaConnector
    {
        /// <summary>
        /// Connects the map given using the algorithm described in the class description.
        /// </summary>
        /// <param name="map">The map to connect.</param>
        /// <param name="distanceCalc">The distance calculation that defines distance/neighbors.</param>
        /// <param name="areaConnector">
        /// The area connection strategy to use. Not all methods function on maps with concave areas
        /// -- see respective class documentation for details.
        /// </param>
        /// ///
        /// <param name="tunnelCreator">
        /// The tunnel creation strategy to use. If null is specified, DirectLineTunnelCreator with
        /// the distance calculation specified is used.
        /// </param>
        static public void Connect(ISettableMapView<bool> map, Distance distanceCalc, IAreaConnectionPointSelector areaConnector = null, ITunnelCreator tunnelCreator = null)
        {
            if (areaConnector == null) areaConnector = new RandomConnectionPointSelector();
            if (tunnelCreator == null) tunnelCreator = new DirectLineTunnelCreator(distanceCalc);

            var areas = MapAreaFinder.MapAreasFor(map, distanceCalc).ToList();

            var ds = new DisjointSet(areas.Count);
            while (ds.Count > 1) // Haven't unioned all sets into one
            {
                for (int i = 0; i < areas.Count; i++)
                {
                    int iClosest = findNearestMapArea(areas, distanceCalc, i, ds);

                    var connectionPoints = areaConnector.SelectConnectionPoints(areas[i], areas[iClosest]);

                    tunnelCreator.CreateTunnel(map, connectionPoints.Item1, connectionPoints.Item2);
                    ds.MakeUnion(i, iClosest);
                }
            }
        }

        static private int findNearestMapArea(IReadOnlyList<MapArea> mapAreas, Distance distanceCalc, int mapAreaIndex, DisjointSet ds)
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