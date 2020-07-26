using System;
using System.Collections.Generic;
using System.Linq;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    public partial class Region
    {
        /// <summary>
        /// Returns a new Region from a parameter set similar to GoRogue.Rectangle
        /// </summary>
        /// <param name="name">The Name of the region</param>
        /// <param name="start"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angleRads"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Region Rectangle(string name, Point start, int width, int height, double angleRads = 0)
        {
            if (angleRads <= -1 || angleRads >= 1)
                throw new ArgumentOutOfRangeException(nameof(angleRads), "angleRads must be between -1 and 1.");

            int hRatio = (int)(height * angleRads);
            int wRatio = (int)(width * angleRads);
            int east = start.X + width - wRatio;
            int west = start.X - wRatio;
            int north = start.Y + hRatio;
            int south = start.Y + height + hRatio;
            return new Region(
                name,
                nw: start,
                se: new Point(east - wRatio, south),
                ne: new Point(east, north),
                sw: new Point(west, south - hRatio)
            );
        }

        /// <summary>
        /// Creates a new Region from a GoRogue.Rectangle.
        /// </summary>
        /// <param name="name">The name of this region.</param>
        /// <param name="rectangle">The rectangle containing this region.</param>
        /// <returns/>
        public static Region FromRectangle(string name, Rectangle rectangle)
        {
            return Rectangle(name, rectangle.MinExtent, rectangle.Width, rectangle.Height);
        }

        /// <summary>
        /// Creates a new Region in the shape of a parallelogram.
        /// </summary>
        /// <param name="name">The name of this region.</param>
        /// <param name="origin">Origin of the parallelogram.</param>
        /// <param name="width">Width of the parallelogram.</param>
        /// <param name="height">Height of the parallelogram.</param>
        /// <param name="rotationDegrees">Rotation of the parallelogram.</param>
        /// <returns/>
        public static Region RegularParallelogram(string name, Point origin, int width, int height, int rotationDegrees)
        {
            Point nw = origin;
            Point ne = origin + new Point(width, 0);
            Point se = origin + new Point(width * 2, height);
            Point sw = origin + new Point(width, height);
            Region area = new Region(name, se, ne, nw, sw);
            area = area.Rotate(rotationDegrees, true, origin);
            return area;
        }

        /// <summary>
        /// Gets the points that are inside of the provided points
        /// </summary>
        /// <param name="outer">An IEnumerable of Points that form a closed region of any shape and size</param>
        /// <returns>All points contained within the outer region</returns>
        public static IEnumerable<Point> InnerFromOuterPoints(IEnumerable<Point> outer)
        {
            if(outer.Any())
                return new List<Point>();
            List<Point> points = new List<Point>();
            outer = outer.OrderBy(x => x.X).ToList();
            for (int i = outer.First().X + 1; i < outer.Last().X; i++)
            {
                List<Point> chunk = outer.Where(w => w.X == i).OrderBy(o => o.Y).ToList();
                if(chunk.Count > 0)
                {
                    for (int j = chunk[0].Y; j <= chunk[^1].Y; j++)
                    {
                        points.Add(new Point(i, j));
                    }
                }
            }

            return points;
        }

        /// <summary>
        /// True if the regions have the same corners/centers; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator ==(Region? left, Region? right)
        {
            if (left is null || right is null)
                return ReferenceEquals(left, right); // They're both null if this returns true;

            bool equals = left.Name == right.Name;
            if (left.NorthWestCorner != right.NorthWestCorner) equals = false;
            if (left.SouthWestCorner != right.SouthWestCorner) equals = false;
            if (left.NorthEastCorner != right.NorthEastCorner) equals = false;
            if (left.SouthEastCorner != right.SouthEastCorner) equals = false;
            if (left.Center != right.Center) equals = false;
            return equals;
        }

        /// <summary>
        /// False if the regions have the same corners and centers; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator !=(Region? left, Region? right) => !(left == right);
    }
}
