using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A region of the map with four sides of arbitrary shape and size
    /// </summary>
    [PublicAPI]
    public class Region : IReadOnlyArea//, IMatchable<Region>
    {
        #region properties

        /// <summary>
        /// All of the boundary points of this region.
        /// </summary>
        /// <remarks>
        /// Points in this collection can be removed, which might make them different
        /// from the sum of each of the Boundary Points
        /// </remarks>
        public IReadOnlyArea OuterPoints => _outerPoints;
        private readonly Area _outerPoints;

        /// <summary>
        /// All of the points inside this region, excluding boundary points and connections
        /// </summary>
        public IReadOnlyArea InnerPoints => _innerPoints;
        private readonly Area _innerPoints;

        /// <summary>
        /// All of the outer points along the southern boundary
        /// </summary>
        public IReadOnlyArea SouthBoundary => _southBoundary;
        private readonly Area _southBoundary;

        /// <summary>
        /// All of the outer points along the northern boundary
        /// </summary>
        public IReadOnlyArea NorthBoundary => _northBoundary;
        private readonly Area _northBoundary;

        /// <summary>
        /// All of the outer points along the eastern boundary
        /// </summary>
        public IReadOnlyArea EastBoundary => _eastBoundary;
        private readonly Area _eastBoundary;

        /// <summary>
        /// All of the outer points along the western boundary
        /// </summary>
        public IReadOnlyArea WestBoundary => _westBoundary;
        private readonly Area _westBoundary;

        /// <summary>
        /// The left-most X-value of the region's four corners
        /// </summary>
        public int Left => SouthWestCorner.X <= NorthWestCorner.X ? SouthWestCorner.X : NorthWestCorner.X;

        /// <summary>
        /// The right-most X-value of the region's four corners
        /// </summary>
        public int Right => SouthEastCorner.X >= NorthEastCorner.X ? SouthEastCorner.X : NorthEastCorner.X;

        /// <summary>
        /// The top-most Y-value of the region's four corners
        /// </summary>
        public int Top
        {
            get
            {
                return Direction.YIncreasesUpward
                    ? (NorthEastCorner.Y > NorthWestCorner.Y ? NorthEastCorner.Y : NorthWestCorner.Y):
                      (NorthEastCorner.Y < NorthWestCorner.Y ? NorthEastCorner.Y : NorthWestCorner.Y);
            }
        }

        /// <summary>
        /// The bottom-most Y-value of the region's four corners
        /// </summary>
        public int Bottom
        {
            get
            {
                return Direction.YIncreasesUpward
                    ? (SouthEastCorner.Y < SouthWestCorner.Y ? SouthEastCorner.Y : SouthWestCorner.Y):
                      (SouthEastCorner.Y > SouthWestCorner.Y ? SouthEastCorner.Y : SouthWestCorner.Y);
            }
        }

        /// <summary>
        /// The South-East corner of the region
        /// </summary>
        public readonly Point SouthEastCorner;

        /// <summary>
        /// The South-West corner of the region
        /// </summary>
        public readonly Point SouthWestCorner;

        /// <summary>
        /// The North-West corner of the region
        /// </summary>
        public readonly Point NorthWestCorner;

        /// <summary>
        /// the North-East corner of the region
        /// </summary>
        public readonly Point NorthEastCorner;

        /// <summary>
        /// How Wide this region is
        /// </summary>
        public int Width => 1 + Right - Left;

        /// <summary>
        /// how tall this region is
        /// </summary>
        public int Height => 1 + (Direction.YIncreasesUpward ? Top - Bottom : Bottom - Top);

        /// <summary>
        /// All points in this region
        /// </summary>
        public IEnumerable<Point> Points => _outerPoints.Concat(_innerPoints);

        /// <summary>
        /// The Center point of this region
        /// </summary>
        public Point Center => new Point((Left + Right) / 2, (Top + Bottom) / 2);

        /// <summary>
        /// The value of the left-most Point in the region at elevation y
        /// </summary>
        /// <param name="y">The elevation to evaluate</param>
        /// <returns>The X-value of a Point</returns>
        public int LeftAt(int y) => _outerPoints.LeftAt(y);

        /// <summary>
        /// The value of the right-most Point in the region at elevation y
        /// </summary>
        /// <param name="y">The elevation to evaluate</param>
        /// <returns>The X-value of a Point</returns>
        public int RightAt(int y) => _outerPoints.RightAt(y);

        /// <summary>
        /// The value of the top-most Point in the region at longitude x
        /// </summary>
        /// <param name="x">The longitude to evaluate</param>
        /// <returns>The Y-value of a Point</returns>
        public int TopAt(int x) => _outerPoints.TopAt(x);

        /// <summary>
        /// The value of the bottom-most Point in the region at longitude x
        /// </summary>
        /// <param name="x">The longitude to evaluate</param>
        /// <returns>The Y-value of a Point</returns>
        public int BottomAt(int x) => _outerPoints.BottomAt(x);

        /// <inheritdoc />
        public Rectangle Bounds => _outerPoints.Bounds;

        /// <inheritdoc />
        public int Count => Points.Count();

        /// <inheritdoc />
        public Point this[int index] => Points.ToArray()[index];

        #endregion


        #region IReadOnlyArea Implementation
        /// <inheritdoc />
        public bool Matches(IReadOnlyArea? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            // Quick checks that can short-circuit a function that would otherwise require looping over all points
            if (Count != other.Count)
                return false;

            if (Bounds != other.Bounds)
                return false;

            foreach (Point pos in Points)
                if (!other.Contains(pos))
                    return false;

            return true;
        }

        /// <inheritdoc />
        public bool Contains(IReadOnlyArea area)
            => _innerPoints.Contains(area) || _outerPoints.Contains(area);

        /// <inheritdoc />
        public bool Contains(Point here)
            => _outerPoints.Contains(here) || _innerPoints.Contains(here);

        /// <inheritdoc />
        public bool Contains(int positionX, int positionY)
            => Contains((positionX, positionY));

        /// <inheritdoc />
        public bool Intersects(IReadOnlyArea area)
            => _innerPoints.Intersects(area) || _outerPoints.Intersects(area);

        /// <inheritdoc />
        public IEnumerator<Point> GetEnumerator() => Points.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region Generation

        /// <summary>
        /// A region of the map with four corners of arbitrary shape and size
        /// </summary>
        /// <param name="northWest">the North-West corner of this region</param>
        /// <param name="northEast">the North-East corner of this region</param>
        /// <param name="southEast">the South-East corner of this region</param>
        /// <param name="southWest">the South-West corner of this region</param>
        public Region(Point northWest, Point northEast, Point southEast, Point southWest)
        {
            SouthEastCorner = southEast;
            NorthEastCorner = northEast;
            NorthWestCorner = northWest;
            SouthWestCorner = southWest;

            _westBoundary = new Area(Lines.Get(NorthWestCorner, SouthWestCorner));
            _southBoundary = new Area(Lines.Get(SouthWestCorner, SouthEastCorner));
            _eastBoundary = new Area(Lines.Get(SouthEastCorner, NorthEastCorner));
            _northBoundary = new Area(Lines.Get(NorthEastCorner, NorthWestCorner));
            _outerPoints = new Area(_westBoundary.Concat(_eastBoundary).Concat(_southBoundary).Concat(_northBoundary));
            _innerPoints = InnerFromOuterPoints(_outerPoints);
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
         public static Region Rectangle(Point origin, int width, int height, int degrees = 0)
         {
             var northEast = origin + (width - 1, 0);
             var southEast = origin + (width - 1, height - 1);
             var southWest = origin + (0, height - 1);
             Region answer = new Region(origin, northEast, southEast, southWest);
             return answer.Rotate(degrees, origin);
         }

         /// <summary>
         /// Creates a new Region from a GoRogue.Rectangle.
         /// </summary>
         /// <param name="name">The name of this region.</param>
         /// <param name="rectangle">The rectangle containing this region.</param>
         /// <returns/>
         public static Region FromRectangle(string name, Rectangle rectangle) => Rectangle(rectangle.MinExtent, rectangle.Width, rectangle.Height);


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
             Region area = new Region(nw, ne, se, sw);
             area = area.Rotate(rotationDegrees, origin);
             return area;
         }
        #endregion

        /// <summary>
        /// Returns a string detailing the region's corner locations.
        /// </summary>
        /// <returns/>
        public override string ToString()
            => $"Region: NW{NorthWestCorner.ToString()}=> NE{NorthEastCorner.ToString()}=> SE{SouthEastCorner.ToString()}=> SW{SouthWestCorner.ToString()}";



        #region Management

        /// <summary>
        /// Is this Point one of the corners of the Region?
        /// </summary>
        /// <param name="here">the point to evaluate</param>
        /// <returns>whether or not the point is within the region</returns>
        public bool IsCorner(Point here) =>
            here == NorthEastCorner || here == NorthWestCorner || here == SouthEastCorner || here == SouthWestCorner;


        /// <summary>
        /// Removes a point from the region.
        /// </summary>
        /// <param name="point">The point to remove.</param>
        /// <remarks>
        /// This removes a point from the inner, outer points, and connections. Does not remove a point
        /// from a boundary, since those are generated from the corners.
        /// </remarks>
        public void Remove(in Point point)
        {
            while (_outerPoints.Contains(point))
                _outerPoints.Remove(point);
            while (_innerPoints.Contains(point))
                _innerPoints.Remove(point);
        }

        /// <summary>
        /// Rotates a region around it's center.
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate this region</param>
        /// <returns>A region equal to the original region rotated by the given degree</returns>
        public virtual Region Rotate(double degrees) => Rotate(degrees, Center);

        /// <summary>
        /// Rotates this region by an arbitrary number of degrees
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate this region</param>
        /// <param name="origin">The Point around which to rotate</param>
        /// <returns>This region, rotated.</returns>
        public virtual Region Rotate(double degrees, Point origin)
        {
            degrees = MathHelpers.WrapAround(degrees, 360);

            //figure out the new corners post-rotation
            List<Point> corners = new List<Point>();
            Point southwest = SouthWestCorner.Rotate(degrees, origin);
            corners.Add(southwest);
            Point southeast = SouthEastCorner.Rotate(degrees, origin);
            corners.Add(southeast);
            Point northwest = NorthWestCorner.Rotate(degrees, origin);
            corners.Add(northwest);
            Point northeast = NorthEastCorner.Rotate(degrees, origin);
            corners.Add(northeast);

            //order the new corner by Y-value
            corners = corners.OrderBy(corner => Direction.YIncreasesUpward ? -corner.Y : corner.Y).ToList();

            //split that list in half and then sort by X-value
            Point[] topTwo = { corners[0], corners[1] };
            topTwo = topTwo.OrderBy(c => c.X).ToArray();
            northwest = topTwo[0];
            northeast = topTwo[1];

            Point[] bottomTwo = { corners [2], corners [3] };
            bottomTwo = bottomTwo.OrderBy(c => c.X).ToArray();
            southwest = bottomTwo[0];
            southeast = bottomTwo[1];

            return new Region(northwest, northeast, southeast, southwest);
        }
        #endregion
    }
}
