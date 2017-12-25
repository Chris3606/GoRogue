using System;
using System.Collections.Generic;
using GoRogue.Random;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Default algorithms for creating a connecting tunnel between two points.
    /// </summary>
    static public class TunnelGeneration
    {
        // TODO: Pull tunnel generation out of CellAut, put here.  Allow specification of delegate function
        // to generate tunnels as well.
        /// <summary>
        /// Creates a tunnel by simply setting as walkable a direct line between the two points specified.  In the case
        /// of MANHATTAN distance, this is a line as calculated by CardinalPositionOnLine.  In the case of EUCLIDIAN or
        /// CHEBYSHEV distance, this is a line as calculated by PositionsOnLine (brensham's).  The tunnel will be
        /// 2-wide horizontally in most cases (this will likely be optional later).
        /// </summary>
        /// <param name="map">Map to create tunnel on.</param>
        /// <param name="distanceCalc">Distance calculation to use -- determines whether the tunnel respects 4-way
        /// or 8-way movement restrictions.</param>
        /// <param name="start">Start point of tunnel.</param>
        /// <param name="end">End point of tunnel.</param>
        static public void DirectLineTunnel(ISettableMapOf<bool> map, Distance distanceCalc, Coord start, Coord end)
        {
            List<Coord> tunnelPositions = (distanceCalc == Distance.MANHATTAN) ? Coord.CardinalPositionsOnLine(start, end) : Coord.PositionsOnLine(start, end);

            Coord previous = null;
            foreach (var pos in tunnelPositions)
            {
                map[pos] = true;
                // Previous cell, and we're going vertical, go 2 wide so it looks nicer
                // Make sure not to break rectangles (less than last index)!
                if (previous != null) // TODO: Make double wide vert an option
                    if (pos.Y != previous.Y)
                        if (pos.X + 1 < map.Width - 1)
                            map[pos.X + 1, pos.Y] = true;

                previous = pos;
            }
        }

        // TODO: Temp: These will implement and ITunnelGenerator later.  An enum will map to all given classes, seperate
        // one taking function isntead of that enum type will be how to specify arbitrary ones.
        static public void HorizontalVerticalTunnel(ISettableMapOf<bool> map, Coord start, Coord end, IRandom rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            if (rng.Next(2) == 0) // Favors vertical tunnels
            {
                createHTunnel(map, start.X, end.X, start.Y);
                createVTunnel(map, start.Y, end.Y, end.X);
            }
            else
            {
                createVTunnel(map, start.Y, end.Y, start.X);
                createHTunnel(map, start.X, end.X, end.Y);
            }
        }

        static private void createHTunnel(ISettableMapOf<bool> map, int xStart, int xEnd, int yPos)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); ++x)
                map[x, yPos] = true;
        }

        static private void createVTunnel(ISettableMapOf<bool> map, int yStart, int yEnd, int xPos)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); ++y)
                map[xPos, y] = true;
        }
    }
}
