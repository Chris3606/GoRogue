using System;
using System.Collections.Generic;
using GoRogue.Random;

namespace GoRogue.MapGeneration.Connectors
{
    static public class RandomPointArea
    {
        static public void Connect(ISettableMapOf<bool> map, IRandom rng, bool randomizeAreaOrder = true)
        {
            var areaFinder = new MapAreaFinder(map, Distance.MANHATTAN);
            areaFinder.FindMapAreas();

            // TODO: Temp, need to refactor MapAreaFinder to a static function
            var areas = areaFinder.MapAreas;
            if (randomizeAreaOrder)
            {
                areas = new List<MapArea>(areas);
            }

            var ds = new DisjointSet(areaFinder.Count);
            while (ds.Count > 1) // Haven't unioned all sets into one
            {
                for (int i = 0; i < areaFinder.Count; i++)
                {
                    int iClosest = findNearestMapArea(areaFinder.MapAreas, i, ds);

                    int iCoordIndex = rng.Next(areaFinder.MapAreas[i].Positions.Count - 1);
                    int iClosestCoordIndex = rng.Next(areaFinder.MapAreas[iClosest].Positions.Count - 1);
                    // Choose from MapArea to make sure we actually get an open Coord on both sides
                    List<Coord> tunnelPositions = Coord.CardinalPositionsOnLine(areaFinder.MapAreas[i].Positions[iCoordIndex],
                                                                        areaFinder.MapAreas[iClosest].Positions[iClosestCoordIndex]);

                    Coord previous = null;
                    foreach (var pos in tunnelPositions)
                    {
                        if (pos == null)
                            throw new Exception("Bad nullage");

                        if (map == null)
                            throw new Exception("Really bad nullage");

                        map[pos] = true;
                        // Previous cell, and we're going vertical, go 2 wide so it looks nicer
                        // Make sure not to break rectangles (less than last index)!
                        if (previous != null)
                            if (pos.Y != previous.Y)
                                if (pos.X + 1 < map.Width - 1)
                                    map[pos.X + 1, pos.Y] = true;

                        previous = pos;
                    }
                    ds.MakeUnion(i, iClosest);
                }
            }
        }

        static private int findNearestMapArea(IList<MapArea> mapAreas, int mapAreaIndex, DisjointSet ds)
        {
            int closestIndex = mapAreaIndex;
            double distance = double.MaxValue;

            for (int i = 0; i < mapAreas.Count; i++)
            {
                if (i == mapAreaIndex)
                    continue;

                if (ds.InSameSet(i, mapAreaIndex))
                    continue;

                double distanceBetween = Distance.MANHATTAN.DistanceBetween(mapAreas[mapAreaIndex].Bounds.Center, mapAreas[i].Bounds.Center);
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
