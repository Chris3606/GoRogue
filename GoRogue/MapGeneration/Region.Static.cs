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
        /// <param name="origin"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Region Rectangle(string name, Point origin, int width, int height, int degrees = 0)
        {
            var northEast = origin + new Point(width - 1, 0);
            var southEast = origin + new Point(width - 1, height - 1);
            var southWest = origin + new Point(0, height - 1);
            Region answer = new Region(name, origin, northEast, southEast, southWest);
            return answer.Rotate(degrees, origin);
        }

        /// <summary>
        /// Creates a new Region from a GoRogue.Rectangle.
        /// </summary>
        /// <param name="name">The name of this region.</param>
        /// <param name="rectangle">The rectangle containing this region.</param>
        /// <returns/>
        public static Region FromRectangle(string name, Rectangle rectangle) => Rectangle(name, rectangle.MinExtent, rectangle.Width, rectangle.Height);


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
            Region area = new Region(name, nw, ne, se, sw);
            area = area.Rotate(rotationDegrees, origin);
            return area;
        }

        /// <summary>
        /// Gets the points that are inside of the provided points
        /// </summary>
        /// <param name="outer">An IEnumerable of Points that form a closed region of any shape and size</param>
        /// <returns>All points contained within the outer region</returns>
        public static Area InnerFromOuterPoints(IEnumerable<Point> outer)
        {
            var outerList = outer.OrderBy(x => x.X).ToList();

            if(outerList.Count == 0)
                return new Area();

            Area points = new Area();

            for (int i = outerList[0].X + 1; i < outerList[^1].X; i++)
            {
                List<Point> row = outerList.Where(point => point.X == i).OrderBy(point => point.Y).ToList();
                if(row.Count > 0)
                {
                    for (int j = row[0].Y; j <= row[^1].Y; j++)
                    {
                        points.Add(new Point(i, j));
                    }
                }
            }

            return points;
        }
    }
}
