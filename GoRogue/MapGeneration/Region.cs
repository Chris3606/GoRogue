using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SadRogue.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A region of the map with four sides of arbitrary shape and size
    /// </summary>
    [PublicAPI]
    public class Region : IReadOnlyArea
    {
        private Point _northWestCorner;
        private Point _northEastCorner;
        private Point _southEastCorner;
        private Point _southWestCorner;
        private MultiArea _points;
        private MultiArea _outerPoints;
        private Area _innerPoints;
        private Area _southBoundary;
        private Area _northBoundary;
        private Area _eastBoundary;
        private Area _westBoundary;

        /// <summary>
        /// A region of the map with four corners of arbitrary shape and size
        /// </summary>
        /// <param name="northWest">the North-West corner of this region</param>
        /// <param name="northEast">the North-East corner of this region</param>
        /// <param name="southEast">the South-East corner of this region</param>
        /// <param name="southWest">the South-West corner of this region</param>
        public Region(Point northWest, Point northEast, Point southEast, Point southWest)
        {
            _southEastCorner = southEast;
            _northEastCorner = northEast;
            _northWestCorner = northWest;
            _southWestCorner = southWest;
            _westBoundary = new Area(Lines.Get(NorthWestCorner, SouthWestCorner));
            _southBoundary = new Area(Lines.Get(SouthWestCorner, SouthEastCorner));
            _eastBoundary = new Area(Lines.Get(SouthEastCorner, NorthEastCorner));
            _northBoundary = new Area(Lines.Get(NorthEastCorner, NorthWestCorner));
            _outerPoints = new MultiArea() {_westBoundary, _northBoundary, _eastBoundary, _southBoundary};
            _innerPoints = SetInnerFromOuterPoints();
            _points = new MultiArea() {_outerPoints, _innerPoints};
        }

        #region properties
        /// <summary>
        /// The South-East corner of the region
        /// </summary>
        public Point SouthEastCorner => _southEastCorner;

        /// <summary>
        /// The South-West corner of the region
        /// </summary>
        public Point SouthWestCorner => _southWestCorner;

        /// <summary>
        /// The North-West corner of the region
        /// </summary>
        public Point NorthWestCorner => _northWestCorner;

        /// <summary>
        /// the North-East corner of the region
        /// </summary>
        public Point NorthEastCorner => _northEastCorner;

        /// <summary>
        /// All of the boundary points of this region.
        /// </summary>
        public IReadOnlyArea OuterPoints => _outerPoints;

        /// <summary>
        /// All of the points inside this region, excluding boundary points
        /// </summary>
        public IReadOnlyArea InnerPoints => _innerPoints;

        /// <summary>
        /// All of the outer points along the southern boundary
        /// </summary>
        public IReadOnlyArea SouthBoundary => _southBoundary;

        /// <summary>
        /// All of the outer points along the northern boundary
        /// </summary>
        public IReadOnlyArea NorthBoundary => _northBoundary;

        /// <summary>
        /// All of the outer points along the eastern boundary
        /// </summary>
        public IReadOnlyArea EastBoundary => _eastBoundary;

        /// <summary>
        /// All of the outer points along the western boundary
        /// </summary>
        public IReadOnlyArea WestBoundary => _westBoundary;

        /// <summary>
        /// The left-most X-value of the region's four corners
        /// </summary>
        public int Left => Math.Min(SouthWestCorner.X, NorthWestCorner.X);

        /// <summary>
        /// The right-most X-value of the region's four corners
        /// </summary>
        public int Right => Math.Max(SouthEastCorner.X, NorthEastCorner.X);

        /// <summary>
        /// The top-most Y-value of the region's four corners
        /// </summary>
        public int Top => Direction.YIncreasesUpward ?
            Math.Max(_northEastCorner.Y, _northWestCorner.Y) :
            Math.Min(_northEastCorner.Y, _northWestCorner.Y);

        /// <summary>
        /// The bottom-most Y-value of the region's four corners
        /// </summary>
        public int Bottom => Direction.YIncreasesUpward ?
            Math.Min(_southEastCorner.Y, _southWestCorner.Y) :
            Math.Max(_southEastCorner.Y, _southWestCorner.Y);

        /// <summary>
        /// How Wide this region is
        /// </summary>
        public int Width => Right - Left + 1;

        /// <summary>
        /// how tall this region is
        /// </summary>
        public int Height => (Direction.YIncreasesUpward ? Top - Bottom : Bottom - Top) + 1;

        /// <summary>
        /// The Center point of this region
        /// </summary>
        public Point Center => new Point((Left + Right) / 2, (Top + Bottom) / 2);

        /// <inheritdoc />
        public Rectangle Bounds => _outerPoints.Bounds;

        /// <inheritdoc />
        public int Count => _points.Count;

        /// <inheritdoc />
        public Point this[int index] => _points[index];
        #endregion

        #region access functions
        //Functions to access information about regions

        /// <summary>
        /// Returns a string detailing the region's corner locations.
        /// </summary>
        public override string ToString()
            => $"Region: NW{NorthWestCorner}=> NE{NorthEastCorner}=> SE{SouthEastCorner}=> SW{SouthWestCorner}";

        /// <summary>
        /// Is this Point one of the corners of the Region?
        /// </summary>
        /// <param name="position">the point to evaluate</param>
        public bool IsCorner(Point position)
            => position == _northEastCorner || position == _northWestCorner || position == _southEastCorner || position == _southWestCorner;

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
        public bool Matches(IReadOnlyArea? area) => _points.Matches(area);

        /// <inheritdoc />
        public bool Contains(IReadOnlyArea? area) => _points.Contains(area);

        /// <inheritdoc />
        public bool Contains(Point position) => _points.Contains(position);

        /// <inheritdoc />
        public bool Contains(int positionX, int positionY) => Contains((positionX, positionY));

        /// <inheritdoc />
        public bool Intersects(IReadOnlyArea area) => _points.Intersects(area);

        /// <inheritdoc />
        public IEnumerator<Point> GetEnumerator() => _points.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region creation
        //non-constructor methods to help with Region Creation

        /// <summary>
        /// Gets the inner points from the boundaries
        /// </summary>
        /// <returns></returns>
        private Area SetInnerFromOuterPoints()
        {
            var outerList = _outerPoints.OrderBy(x => x.X).ToList();

            if(outerList.Count == 0)
                return new Area();

            _innerPoints = new Area();

            for (int i = outerList[0].X + 1; i < outerList[^1].X; i++)
            {
                List<Point> row = outerList.Where(point => point.X == i).OrderBy(point => point.Y).ToList();
                if(row.Count > 0)
                {
                    for (int j = row[0].Y; j <= row[^1].Y; j++)
                    {
                        _innerPoints.Add(new Point(i, j));
                    }
                }
            }

            return _innerPoints;
        }

        /// <summary>
        /// Creates a new Region from a GoRogue.Rectangle.
        /// </summary>
        /// <param name="r">The rectangle</param>
        /// <returns>A new region in the shape of a rectangle</returns>
        public static Region Rectangle(Rectangle r)
            => new Region(r.MinExtent, (r.MaxExtentX, r.MinExtentY),
                r.MaxExtent, (r.MinExtentX, r.MaxExtentY));


         /// <summary>
         /// Creates a new Region in the shape of a parallelogram, with diagonals going down and right.
         /// </summary>
         /// <param name="origin">Origin of the parallelogram.</param>
         /// <param name="width">Width of the parallelogram.</param>
         /// <param name="height">Height of the parallelogram.</param>
         /// <returns>A new region in the shape of a parallelogram</returns>
         public static Region ParallelogramFromTopCorner(Point origin, int width, int height)
         {
             var negative = Direction.YIncreasesUpward ? 1 : -1;

             Point nw = origin;
             Point ne = origin + new Point(width, 0);
             Point se = origin + new Point(width * 2, height * negative);
             Point sw = origin + new Point(width, height * negative);

             return new Region(nw, ne, se, sw);
         }

         /// <summary>
         /// Creates a new Region in the shape of a parallelogram, with diagonals going up and right.
         /// </summary>
         /// <param name="origin">Origin of the parallelogram.</param>
         /// <param name="width">The horizontal length of the top and bottom sides.</param>
         /// <param name="height">Height of the parallelogram.</param>
         /// <returns>A new region in the shape of a parallelogram</returns>
         public static Region ParallelogramFromBottomCorner(Point origin, int width, int height)
         {
             var negative = Direction.YIncreasesUpward ? 1 : -1;

             Point nw = origin + (height, height * negative);
             Point ne = origin + (height + width, height * negative);
             Point se = origin + (width, 0);
             Point sw = origin;

             return new Region(nw, ne, se, sw);
         }
        #endregion

        #region Transformation
        //currently, these all return NEW regions, instead of rotating the region in-place

        /// <summary>
        /// Moves the Region in the indicated direction.
        /// </summary>
        /// <param name="dx">The X-value by which to shift the region</param>
        /// <param name="dy">The Y-value by which to shift the region</param>
        /// <returns></returns>
        public Region Translate(int dx, int dy)
            => Translate(new Point(dx, dy));

        /// <summary>
        /// Moves the region in the indicated direction.
        /// </summary>
        /// <param name="delta">The amount (X and Y) to translate this region by.</param>
        /// <returns>A new, translated region</returns>
        public Region Translate(Point delta)
        {
            var nw = NorthWestCorner + delta;
            var ne = NorthEastCorner + delta;
            var se = SouthEastCorner + delta;
            var sw = SouthWestCorner + delta;
            return new Region(nw, ne, se, sw);
        }
        /// <summary>
        /// Rotates a region around it's center.
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate this region</param>
        /// <returns>A region, equal to the original region rotated by the given degree</returns>
        public Region Rotate(double degrees) => Rotate(degrees, Center);

        /// <summary>
        /// Rotates this region by an arbitrary number of degrees
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate this region</param>
        /// <param name="origin">The Point around which to rotate</param>
        /// <returns>A region, equal to the original region rotated by the given degree</returns>
        public Region Rotate(double degrees, Point origin)
        {
            degrees = MathHelpers.WrapAround(degrees, 360);

            //figure out the new corners post-rotation
            Point[] corners = new []
            {
                _southWestCorner.Rotate(degrees, origin),
                _southEastCorner.Rotate(degrees, origin),
                _northWestCorner.Rotate(degrees, origin),
                _northEastCorner.Rotate(degrees, origin),
            };

            //order the new corner by Y-value
            corners = corners.OrderBy(corner => Direction.YIncreasesUpward ? -corner.Y : corner.Y).ToArray();

            //split that list in half and then sort by X-value
            Point[] topTwo = { corners[0], corners[1] };
            topTwo = topTwo.OrderBy(c => c.X).ToArray();
            var northWest = topTwo[0];
            var northEast  = topTwo[1];

            Point[] bottomTwo = { corners [2], corners [3] };
            bottomTwo = bottomTwo.OrderBy(c => c.X).ToArray();
            var southWest = bottomTwo[0];
            var southEast = bottomTwo[1];

            return new Region(northWest, northEast, southEast, southWest);
        }

        /// <summary>
        /// Returns a new region, flipped horizontally around an X-axis
        /// </summary>
        /// <param name="x">The value around which to flip.</param>
        /// <returns>A region, equal to the original region flipped around the desired X-axis</returns>
        public Region FlipHorizontal(int x)
        {
            var nw = (_northWestCorner - (x, 0)) * (-1,1) + (x, 0);
            var ne = (_northEastCorner - (x, 0)) * (-1,1) + (x, 0);
            var se = (_southEastCorner - (x, 0)) * (-1,1) + (x, 0);
            var sw = (_southWestCorner - (x, 0)) * (-1,1) + (x, 0);

            return new Region(nw, ne, se, sw);
        }

        /// <summary>
        /// Returns a new region, flipped vertically around a Y-axis
        /// </summary>
        /// <param name="y">The value around which to flip.</param>
        /// <returns>A region, equal to the original region flipped around the desired Y-axis</returns>
        public Region FlipVertical(int y)
        {
            var nw = (_northWestCorner - (0, y)) * (1, -1) + (0, y);
            var ne = (_northEastCorner - (0, y)) * (1, -1) + (0, y);
            var se = (_southEastCorner - (0, y)) * (1, -1) + (0, y);
            var sw = (_southWestCorner - (0, y)) * (1, -1) + (0, y);

            return new Region(nw, ne, se, sw);
        }

        /// <summary>
        /// Returns a new region with X and Y values inverted, respective to a diagonal line
        /// </summary>
        /// <param name="x">Any X-value of a point which intersects the line around which to transpose</param>
        /// <param name="y">Any Y-value of a Point which intersects the line around which to transpose</param>
        public Region Transpose(int x, int y)
            => Transpose((x, y));

        /// <summary>
        /// Returns a new region with X and Y values inverted, respective to a diagonal line
        /// </summary>
        /// <param name="xy">Any point which intersects the line around which to transpose</param>
        public Region Transpose(Point xy)
        {
            var nw = _northWestCorner - xy;
            nw = (nw.Y, nw.X) + xy;
            var ne = _northEastCorner - xy;
            ne = (ne.Y, ne.X) + xy;
            var se = _southEastCorner - xy;
            se = (se.Y, se.X) + xy;
            var sw = _southWestCorner - xy;
            sw = (sw.Y, sw.X) + xy;

            return new Region(nw, ne, se, sw);
        }
        #endregion
    }
}
