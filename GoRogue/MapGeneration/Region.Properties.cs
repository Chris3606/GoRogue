using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    public partial class Region
    {
        /// <summary>
        /// The name of this region
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// how far the region raises or lowers in elevation
        /// </summary>
        public int Rise { get; private set; }
        /// <summary>
        /// how far the region extends in the x-axis
        /// </summary>
        public int Run { get; private set; }

        /// <summary>
        /// The subregions of this region (i.e., rooms in a house)
        /// </summary>
        public IReadOnlyList<Region> SubRegions => _subRegions.AsReadOnly();
        private List<Region> _subRegions = new List<Region>();

        /// <summary>
        /// All of the boundary points of this region
        /// </summary>
        public IReadOnlyList<Point> OuterPoints => _outerPoints.AsReadOnly();
        private List<Point> _outerPoints = new List<Point>();

        /// <summary>
        /// All of the points inside this region, excluding boundary points and connections
        /// </summary>
        public IReadOnlyList<Point> InnerPoints => _innerPoints.AsReadOnly();
        private List<Point> _innerPoints = new List<Point>();

        /// <summary>
        /// All of the outer points along the southern boundary
        /// </summary>
        public IReadOnlyList<Point> SouthBoundary => _southBoundary.AsReadOnly();
        private List<Point> _southBoundary = new List<Point>();

        /// <summary>
        /// All of the outer points along the northern boundary
        /// </summary>
        public IReadOnlyList<Point> NorthBoundary => _northBoundary.AsReadOnly();
        private List<Point> _northBoundary = new List<Point>();

        /// <summary>
        /// All of the outer points along the eastern boundary
        /// </summary>
        public IReadOnlyList<Point> EastBoundary => _eastBoundary.AsReadOnly();
        private List<Point> _eastBoundary = new List<Point>();

        /// <summary>
        /// All of the outer points along the western boundary
        /// </summary>
        public IReadOnlyList<Point> WestBoundary => _westBoundary.AsReadOnly();
        private List<Point> _westBoundary = new List<Point>();

        /// <summary>
        /// All of the points that are considered "connections" to other regions
        /// </summary>
        /// <remarks>
        /// You should perform transformations (rotation, etc) before Connections are made.
        /// </remarks>
        public IReadOnlyList<Point> Connections => _connections;
        private List<Point> _connections = new List<Point>();

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
        public Point SouthEastCorner { get; private set; }

        /// <summary>
        /// The South-West corner of the region
        /// </summary>
        public Point SouthWestCorner { get; private set; }

        /// <summary>
        /// The North-West corner of the region
        /// </summary>
        public Point NorthWestCorner { get; private set; }

        /// <summary>
        /// the North-East corner of the region
        /// </summary>
        public Point NorthEastCorner { get; private set; }

        //these don't respect inverted-Y

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
        public IEnumerable<Point> Points => OuterPoints.Concat(InnerPoints).Concat(Connections);

        /// <summary>
        /// The Center point of this region
        /// </summary>
        public Point Center => new Point((Left + Right) / 2, (Top + Bottom) / 2);

        /// <summary>
        /// The value of the left-most Point in the region at elevation y
        /// </summary>
        /// <param name="y">The elevation to evaluate</param>
        /// <returns>The X-value of a Point</returns>
        public int LeftAt(int y) => OuterPoints.LeftAt(y);
        /// <summary>
        /// The value of the right-most Point in the region at elevation y
        /// </summary>
        /// <param name="y">The elevation to evaluate</param>
        /// <returns>The X-value of a Point</returns>
        public int RightAt(int y) => OuterPoints.RightAt(y);
        /// <summary>
        /// The value of the top-most Point in the region at longitude x
        /// </summary>
        /// <param name="x">The longitude to evaluate</param>
        /// <returns>The Y-value of a Point</returns>
        public int TopAt(int x) => OuterPoints.TopAt(x);
        /// <summary>
        /// The value of the bottom-most Point in the region at longitude x
        /// </summary>
        /// <param name="x">The longitude to evaluate</param>
        /// <returns>The Y-value of a Point</returns>
        public int BottomAt(int x) => OuterPoints.BottomAt(x);
    }
}
