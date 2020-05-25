﻿using System;
using System.Collections.Generic;
using System.Linq;
using SadRogue.Primitives;

namespace GoRogue
{
    /// <summary>
    /// Provides implementations of various (line-drawing) algorithms which are useful for
    /// for generating points closest to a line between two points on a grid.
    /// </summary>
    public static class Lines
    {
        private const int MODIFIER_X = 0x7fff;
        private const int MODIFIER_Y = 0x7fff;

        /// <summary>
        /// Various supported line-drawing algorithms.
        /// </summary>
        public enum Algorithm
        {
            /// <summary>
            /// Bresenham's line algorithm.
            /// </summary>
            Bresenham,

            /// <summary>
            /// Bresenham's line algorithm, with the points guaranteed to be in start to finish
            /// order. This may be significantly slower than <see cref="Algorithm.Bresenham"/>, so if you really
            /// need ordering, consider<see cref="Algorithm.DDA"/> instead, as it is both faster than Bresenham's
            /// and implicitly ordered.
            /// </summary>
            BresenhamOrdered,

            /// <summary>
            /// DDA line algorithm -- effectively an optimized algorithm for producing Brensham-like
            /// lines. There may be slight differences in appearance when compared to lines created
            /// with Bresenham's, however this algorithm may also be measurably faster. Based on the
            /// algorithm <a href="https://hbfs.wordpress.com/2009/07/28/faster-than-bresenhams-algorithm/">here</a>,
            /// , as well as the Java library Squidlib's implementation.  Points are guaranteed to be in order from
            /// start to finish.
            /// </summary>
            DDA,

            /// <summary>
            /// Line algorithm that takes only orthoganal steps (each grid location on the line
            /// returned is within one cardinal direction of the previous one). Potentially useful
            /// for LOS in games that use MANHATTAN distance. Based on the algorithm
            /// <a href="http://www.redblobgames.com/grids/line-drawing.html#stepping">here</a>.
            /// </summary>
            Ortho
        }

        /// <summary>
        /// Returns an IEnumerable of every point, in order, closest to a line between the two points
        /// specified, using the line drawing algorithm given. The start and end points will be included.
        /// </summary>
        /// <param name="start">The start point of the line.</param>
        /// <param name="end">The end point of the line.</param>
        /// <param name="type">The line-drawing algorithm to use to generate the line.</param>
        /// <returns>
        /// An IEnumerable of every point, in order, closest to a line between the two points
        /// specified (according to the algorithm given).
        /// </returns>
        public static IEnumerable<Point> Get(Point start, Point end, Lines.Algorithm type = Algorithm.Bresenham) => Get(start.X, start.Y, end.X, end.Y, type);

        /// <summary>
        /// Returns an IEnumerable of every point, in order, closest to a line between the two points
        /// specified, using the line drawing algorithm given. The start and end points will be included.
        /// </summary>
        /// <param name="startX">X-Pointinate of the starting point of the line.</param>
        /// <param name="startY">Y-Pointinate of the starting point of the line.</param>
        /// <param name="endX">X-Pointinate of the ending point of the line.</param>
        /// ///
        /// <param name="endY">Y-Pointinate of the ending point of the line.</param>
        /// <param name="type">The line-drawing algorithm to use to generate the line.</param>
        /// <returns>
        /// An IEnumerable of every point, in order, closest to a line between the two points
        /// specified (according to the algorithm given).
        /// </returns>
        public static IEnumerable<Point> Get(int startX, int startY, int endX, int endY, Lines.Algorithm type = Algorithm.Bresenham)
        {
            switch (type)
            {
                case Algorithm.Bresenham:
                    return bresenham(startX, startY, endX, endY);

                case Algorithm.BresenhamOrdered:
                    var line = bresenham(startX, startY, endX, endY).Reverse();
                    if (line.First() == new Point(startX, startY))
                        return line;
                    else
                        return line.Reverse();

                case Algorithm.DDA:
                    return dda(startX, startY, endX, endY);

                case Algorithm.Ortho:
                    return ortho(startX, startY, endX, endY);

                default:
                    throw new Exception("Unsupported line-drawing algorithm.");  // Should not occur
            }
        }

        private static IEnumerable<Point> bresenham(int startX, int startY, int endX, int endY)
        {
            bool steep = Math.Abs(endY - startY) > Math.Abs(endX - startX);
            if (steep)
            {
                Utility.Swap(ref startX, ref startY);
                Utility.Swap(ref endX, ref endY);
            }

            if (startX > endX)
            {
                Utility.Swap(ref startX, ref endX);
                Utility.Swap(ref startY, ref endY);
            }

            int dx = endX - startX;
            int dy = Math.Abs(endY - startY);

            int err = dx / 2;
            int yStep = (startY < endY ? 1 : -1);
            int y = startY;

            for (int x = startX; x <= endX; x++)
            {
                if (steep)
                    yield return new Point(y, x);
                else
                    yield return new Point(x, y);

                err -= dy;
                if (err < 0)
                {
                    y += yStep;
                    err += dx;
                }
            }
        }

        private static IEnumerable<Point> dda(int startX, int startY, int endX, int endY)
        {
            int dx = endX - startX;
            int dy = endY - startY;

            int nx = Math.Abs(dx);
            int ny = Math.Abs(dy);

            // Calculate octant value
            int octant = ((dy < 0) ? 4 : 0) | ((dx < 0) ? 2 : 0) | ((ny > nx) ? 1 : 0);
            int frac = 0;
            int mn = Math.Max(nx, ny);

            int move;

            if (mn == 0)
            {
                yield return new Point(startX, startY);
                yield break;
            }

            if (ny == 0)
            {
                if (dx > 0)
                    for (int x = startX; x <= endX; x++)
                        yield return new Point(x, startY);
                else
                    for (int x = startX; x >= endX; x--)
                        yield return new Point(x, startY);

                yield break;
            }

            if (nx == 0)
            {
                if (dy > 0)
                    for (int y = startY; y <= endY; y++)
                        yield return new Point(startX, y);
                else
                    for (int y = startY; y >= endY; y--)
                        yield return new Point(startX, y);

                yield break;
            }

            switch (octant)
            {
                case 0: // +x, +y
                    move = (ny << 16) / nx;
                    for (int primary = startX; primary <= endX; primary++, frac += move)
                        yield return new Point(primary, startY + ((frac + MODIFIER_Y) >> 16));
                    break;

                case 1:
                    move = (nx << 16) / ny;
                    for (int primary = startY; primary <= endY; primary++, frac += move)
                        yield return new Point(startX + ((frac + MODIFIER_X) >> 16), primary);
                    break;

                case 2: // -x, +y
                    move = (ny << 16) / nx;
                    for (int primary = startX; primary >= endX; primary--, frac += move)
                        yield return new Point(primary, startY + ((frac + MODIFIER_Y) >> 16));
                    break;

                case 3:
                    move = (nx << 16) / ny;
                    for (int primary = startY; primary <= endY; primary++, frac += move)
                        yield return new Point(startX - ((frac + MODIFIER_X) >> 16), primary);
                    break;

                case 6: // -x, -y
                    move = (ny << 16) / nx;
                    for (int primary = startX; primary >= endX; primary--, frac += move)
                        yield return new Point(primary, startY - ((frac + MODIFIER_Y) >> 16));
                    break;

                case 7:
                    move = (nx << 16) / ny;
                    for (int primary = startY; primary >= endY; primary--, frac += move)
                        yield return new Point(startX - ((frac + MODIFIER_X) >> 16), primary);
                    break;

                case 4: // +x, -y
                    move = (ny << 16) / nx;
                    for (int primary = startX; primary <= endX; primary++, frac += move)
                        yield return new Point(primary, startY - ((frac + MODIFIER_Y) >> 16));
                    break;

                case 5:
                    move = (nx << 16) / ny;
                    for (int primary = startY; primary >= endY; primary--, frac += move)
                        yield return new Point(startX + ((frac + MODIFIER_X) >> 16), primary);
                    break;
            }
        }

        private static IEnumerable<Point> ortho(int startX, int startY, int endX, int endY)
        {
            int dx = endX - startX;
            int dy = endY - startY;

            int nx = Math.Abs(dx);
            int ny = Math.Abs(dy);

            int signX = (dx > 0) ? 1 : -1;
            int signY = (dy > 0) ? 1 : -1;

            int workX = startX;
            int workY = startY;

            yield return new Point(startX, startY);

            for (int ix = 0, iy = 0; ix < nx || iy < ny;)
            {
                if ((0.5 + ix) / nx < (0.5 + iy) / ny)
                {
                    workX += signX;
                    ix++;
                }
                else
                {
                    workY += signY;
                    iy++;
                }

                yield return new Point(workX, workY);
            }
        }
    }
}
