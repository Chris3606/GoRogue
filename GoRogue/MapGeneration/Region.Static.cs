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
                throw new ArgumentOutOfRangeException("angleRads must be between -1 and 1.");

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
        /// Creates a new Region from a GoRogue.Rectangle
        /// </summary>
        /// <param name="name">the name of this region</param>
        /// <param name="rectangle">the rectangle containing this region</param>
        /// <returns></returns>
        public static Region FromRectangle(string name, Rectangle rectangle)
        {
            return Rectangle(name, rectangle.MinExtent, rectangle.Width, rectangle.Height);
        }
        public static Region RegularParallelogram(string name, Point origin, int width, int height, int rotationDegrees)
        {
            Point nw = origin;
            Point ne = origin + new Point(width, 0);
            Point se = origin + new Point(width * 2, height);
            Point sw = origin + new Point(width, height);
            Region area = new Region(name, se, ne, nw, sw);
            //area = area.Rotate(rotationDegrees);
            return area;
        }

        /// <summary>
        /// Gets the points that are inside of the provided points
        /// </summary>
        /// <param name="outer">An IEnumerable of Points that form a closed region of any shape and size</param>
        /// <returns>All points contained within the outer region</returns>
        public static IEnumerable<Point> InnerFromOuterPoints(IEnumerable<Point> outer)
        {
            outer = outer.OrderBy(x => x.X).ToList();
            for (int i = outer.First().X + 1; i < outer.Last().X; i++)
            {
                List<Point> chunk = outer.Where(w => w.X == i).OrderBy(o => o.Y).ToList();
                if(chunk.Count > 0)
                    for (int j = chunk.First().Y; j <= chunk.Last().Y; j++)
                    {
                        yield return new Point(i, j);
                    }
            }
        }
    }
}
