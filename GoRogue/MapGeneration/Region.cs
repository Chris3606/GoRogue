using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GoRogue.Components;
using GoRogue.Components.ParentAware;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A region of the map with four sides of arbitrary shape and size
    /// </summary>
    [PublicAPI]
    public class Region : IReadOnlyArea, IObjectWithComponents
    {
        #region properties

        /// <summary>
        /// The GoRogueComponents on this region
        /// </summary>
        public IComponentCollection GoRogueComponents { get; set; }

        /// <summary>
        /// All of the boundary points of this region.
        /// </summary>
        public IReadOnlyArea OuterPoints => new Area(_westBoundary.Concat(_northBoundary).Concat(_eastBoundary).Concat(_southBoundary));
        // public IReadOnlyArea OuterPoints => _outerPoints;
        // private readonly Area _outerPoints = new Area();

        /// <summary>
        /// All of the points inside this region, excluding boundary points
        /// </summary>
        public IReadOnlyArea InnerPoints => _innerPoints;
        private Area _innerPoints = new Area();

        /// <summary>
        /// All of the outer points along the southern boundary
        /// </summary>
        public IReadOnlyArea SouthBoundary => _southBoundary;
        private Area _southBoundary = new Area();

        /// <summary>
        /// All of the outer points along the northern boundary
        /// </summary>
        public IReadOnlyArea NorthBoundary => _northBoundary;
        private Area _northBoundary = new Area();

        /// <summary>
        /// All of the outer points along the eastern boundary
        /// </summary>
        public IReadOnlyArea EastBoundary => _eastBoundary;
        private Area _eastBoundary = new Area();

        /// <summary>
        /// All of the outer points along the western boundary
        /// </summary>
        public IReadOnlyArea WestBoundary => _westBoundary;
        private Area _westBoundary = new Area();

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
        public Point SouthEastCorner { get; protected set; }

        /// <summary>
        /// The South-West corner of the region
        /// </summary>
        public Point SouthWestCorner { get; protected set; }

        /// <summary>
        /// The North-West corner of the region
        /// </summary>
        public Point NorthWestCorner { get; protected set; }

        /// <summary>
        /// the North-East corner of the region
        /// </summary>
        public Point NorthEastCorner { get; protected set; }

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
        public IEnumerable<Point> Positions => OuterPoints.Concat(_innerPoints);

        /// <summary>
        /// The Center point of this region
        /// </summary>
        public Point Center => new Point((Left + Right) / 2, (Top + Bottom) / 2);

        /// <summary>
        /// The value of the left-most Point in the region at elevation y
        /// </summary>
        /// <param name="y">The elevation to evaluate</param>
        /// <returns>The X-value of a Point</returns>
        public int LeftAt(int y) => _westBoundary.LeftAt(y);

        /// <summary>
        /// The value of the right-most Point in the region at elevation y
        /// </summary>
        /// <param name="y">The elevation to evaluate</param>
        /// <returns>The X-value of a Point</returns>
        public int RightAt(int y) => _eastBoundary.RightAt(y);

        /// <summary>
        /// The value of the top-most Point in the region at longitude x
        /// </summary>
        /// <param name="x">The longitude to evaluate</param>
        /// <returns>The Y-value of a Point</returns>
        public int TopAt(int x) => _northBoundary.TopAt(x);

        /// <summary>
        /// The value of the bottom-most Point in the region at longitude x
        /// </summary>
        /// <param name="x">The longitude to evaluate</param>
        /// <returns>The Y-value of a Point</returns>
        public int BottomAt(int x) => _southBoundary.BottomAt(x);

        /// <inheritdoc />
        public Rectangle Bounds => _innerPoints.Bounds;

        /// <inheritdoc />
        public int Count => Positions.Count();

        /// <inheritdoc />
        public Point this[int index] => Positions.ToArray()[index];
        #endregion

        #region IReadOnlyArea
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

            foreach (Point pos in Positions)
                if (!other.Contains(pos))
                    return false;

            return true;
        }

        /// <inheritdoc />
        public bool Contains(IReadOnlyArea area)
            => _innerPoints.Contains(area) || _eastBoundary.Contains(area) || _westBoundary.Contains(area) ||
               _southBoundary.Contains(area) || _northBoundary.Contains(area);

        /// <inheritdoc />
        public bool Contains(Point here)
            => _innerPoints.Contains(here) || _eastBoundary.Contains(here) || _westBoundary.Contains(here) ||
               _southBoundary.Contains(here) || _northBoundary.Contains(here);

        /// <inheritdoc />
        public bool Contains(int positionX, int positionY)
            => Contains((positionX, positionY));

        /// <inheritdoc />
        public bool Intersects(IReadOnlyArea area)
            => _innerPoints.Intersects(area) || _eastBoundary.Intersects(area) || _westBoundary.Intersects(area) ||
               _southBoundary.Intersects(area) || _northBoundary.Intersects(area);

        /// <inheritdoc />
        public IEnumerator<Point> GetEnumerator() => Positions.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region Creation
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
            GenerateAreasFromCorners();
            GoRogueComponents = new ComponentCollection();
        }

        private void GenerateAreasFromCorners()
        {
            _westBoundary = new Area(Lines.Get(NorthWestCorner, SouthWestCorner));
            _southBoundary = new Area(Lines.Get(SouthWestCorner, SouthEastCorner));
            _eastBoundary = new Area(Lines.Get(SouthEastCorner, NorthEastCorner));
            _northBoundary = new Area(Lines.Get(NorthEastCorner, NorthWestCorner));
            _innerPoints = InnerFromOuterPoints(OuterPoints);
        }

        private void ClearAreas()
        {
            _southBoundary.Remove(_southBoundary);
            _northBoundary.Remove(_northBoundary);
            _eastBoundary.Remove(_eastBoundary);
            _westBoundary.Remove(_westBoundary);
            _innerPoints.Remove(_innerPoints);
        }


        /// <summary>
        /// Gets the points that are inside of the provided points
        /// </summary>
        /// <param name="outer">An IEnumerable of Points that form a closed region of any shape and size</param>
        /// <returns>All points contained within the outer region</returns>
        public static Area InnerFromOuterPoints(IEnumerable<Point> outer)
            => InnerFromOuterPoints(new Area(outer));

        /// <summary>
        /// Gets the points contains within a hollow area
        /// </summary>
        /// <param name="outer">The outer area</param>
        /// <returns></returns>
        public static Area InnerFromOuterPoints(Area outer)
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
        /// <param name="origin"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Region Rectangle(Point origin, int width, int height, int degrees = 0)
            => FromRectangle(new Rectangle(origin, origin + (width, height)));

        /// <summary>
        /// Creates a new Region from a GoRogue.Rectangle.
        /// </summary>
        /// <param name="r">The rectangle</param>
        /// <returns/>
        public static Region FromRectangle(Rectangle r)
            => new Region(r.MinExtent, (r.MaxExtentX, r.MinExtentY), r.MaxExtent, (r.MinExtentX, r.MaxExtentY));


         /// <summary>
         /// Creates a new Region in the shape of a parallelogram, with diagonals going down and right.
         /// </summary>
         /// <param name="origin">Origin of the parallelogram.</param>
         /// <param name="width">Width of the parallelogram.</param>
         /// <param name="height">Height of the parallelogram.</param>
         /// <returns/>
         public static Region ParallelogramFromTopCorner(Point origin, int width, int height)
         {
             Point nw = origin;
             Point ne = origin + new Point(width, 0);
             Point se = origin + new Point(width * 2, height);
             Point sw = origin + new Point(width, height);

             return new Region(nw, ne, se, sw);;
         }

         /// <summary>
         /// Creates a new Region in the shape of a parallelogram, with diagonals going up and right.
         /// </summary>
         /// <param name="origin">Origin of the parallelogram.</param>
         /// <param name="width">The horizontal length of the top and bottom sides.</param>
         /// <param name="height">Height of the parallelogram.</param>
         /// <returns/>
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

        /// <summary>
        /// Returns a string detailing the region's corner locations.
        /// </summary>
        /// <returns/>
        public override string ToString()
            => $"Region: NW{NorthWestCorner.ToString()}=> NE{NorthEastCorner.ToString()}=> SE{SouthEastCorner.ToString()}=> SW{SouthWestCorner.ToString()}";

        /// <summary>
        /// Is this Point one of the corners of the Region?
        /// </summary>
        /// <param name="here">the point to evaluate</param>
        /// <returns>whether or not the point is within the region</returns>
        public bool IsCorner(Point here) =>
            here == NorthEastCorner || here == NorthWestCorner || here == SouthEastCorner || here == SouthWestCorner;


        #region Transformation
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
            while (_westBoundary.Contains(point))
                _westBoundary.Remove(point);
            while (_eastBoundary.Contains(point))
                _eastBoundary.Remove(point);
            while (_northBoundary.Contains(point))
                _northBoundary.Remove(point);
            while (_southBoundary.Contains(point))
                _southBoundary.Remove(point);
            while (_innerPoints.Contains(point))
                _innerPoints.Remove(point);
        }

        /// <summary>
        /// Rotates a region around it's center.
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate this region</param>
        /// <returns>A region equal to the original region rotated by the given degree</returns>
        public virtual Region Rotate(double degrees)
            => Rotate(degrees, Center);

        /// <summary>
        /// Rotates this region by an arbitrary number of degrees
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate this region</param>
        /// <param name="origin">The Point around which to rotate</param>
        /// <returns>This region, rotated.</returns>
        public virtual Region Rotate(double degrees, Point origin)
        {
            degrees %= 360;

            //figure out the new corners post-rotation
            Point[] corners = new []
            {
                SouthWestCorner.Rotate(degrees, origin),
                SouthEastCorner.Rotate(degrees, origin),
                NorthWestCorner.Rotate(degrees, origin),
                NorthEastCorner.Rotate(degrees, origin),
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
        /// <returns>This region, flipped</returns>
        public virtual Region FlipHorizontal(int x)
        {
            var nw = (NorthWestCorner - (x, 0)) * (-1,1) + (x, 0);
            var ne = (NorthEastCorner - (x, 0)) * (-1,1) + (x, 0);
            var se = (SouthEastCorner - (x, 0)) * (-1,1) + (x, 0);
            var sw = (SouthWestCorner - (x, 0)) * (-1,1) + (x, 0);

            return new Region(nw, ne, se, sw);
        }

        /// <summary>
        /// Returns a new region, flipped vertically around a Y-axis
        /// </summary>
        /// <param name="y">The value around which to flip.</param>
        /// <returns></returns>
        public virtual Region FlipVertical(int y)
        {
            var nw = (NorthWestCorner - (0, y)) * (1, -1) + (0, y);
            var ne = (NorthEastCorner - (0, y)) * (1, -1) + (0, y);
            var se = (SouthEastCorner - (0, y)) * (1, -1) + (0, y);
            var sw = (SouthWestCorner - (0, y)) * (1, -1) + (0, y);

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
            var nw = (NorthWestCorner - xy);
            nw = (nw.Y, nw.X) + xy;
            var ne = (NorthEastCorner - xy);
            ne = (ne.Y, ne.X) + xy;
            var se = (SouthEastCorner - xy);
            se = (se.Y, se.X) + xy;
            var sw = (SouthWestCorner - xy);
            sw = (sw.Y, sw.X) + xy;

            return new Region(nw, ne, se, sw);
        }
        #endregion
    }
}
