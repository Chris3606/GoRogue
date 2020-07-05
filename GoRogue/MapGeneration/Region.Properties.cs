using System;
using System.Collections.Generic;
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
        public IEnumerable<Region> SubRegions { get; private set; } = new List<Region>();

        /// <summary>
        /// All of the boundary points of this region
        /// </summary>
        public IEnumerable<Point> OuterPoints { get; private set; } = new List<Point>();

        /// <summary>
        /// All of the points inside this region, excluding boundary points and connections
        /// </summary>
        public IEnumerable<Point> InnerPoints { get; private set; } = new List<Point>();

        /// <summary>
        /// All of the outer points along the southern boundary
        /// </summary>
        public IEnumerable<Point> SouthBoundary { get; private set; } //or southeast boundary in the case of diamonds

        /// <summary>
        /// All of the outer points along the northern boundary
        /// </summary>
        public IEnumerable<Point> NorthBoundary { get; private set; }

        /// <summary>
        /// All of the outer points along the eastern boundary
        /// </summary>
        public IEnumerable<Point> EastBoundary { get; private set; }

        /// <summary>
        /// All of the outer points along the western boundary
        /// </summary>
        public IEnumerable<Point> WestBoundary { get; private set; }

        /// <summary>
        /// All of the points that are considered "connections" to other regions
        /// </summary>
        /// <remarks>
        /// You should perform transformations (rotation, etc) before Connections are made.
        /// </remarks>
        public IEnumerable<Point> Connections { get; private set; } = new List<Point>();

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
        public int Top => NorthEastCorner.Y <= NorthWestCorner.Y ? NorthEastCorner.Y : NorthWestCorner.Y;

        /// <summary>
        /// The bottom-most Y-value of the region's four corners
        /// </summary>
        public int Bottom => SouthEastCorner.Y <= SouthWestCorner.Y ? SouthWestCorner.Y : SouthEastCorner.Y;

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
        public int Width => Right - Left;

        /// <summary>
        /// how tall this region is
        /// </summary>
        public int Height => Bottom - Top;

        /// <summary>
        /// All points in this region
        /// </summary>
        public IEnumerable<Point> Points { get => OuterPoints.Concat(InnerPoints).Concat(Connections).ToList(); }

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

        /// <summary>
        /// Get a SubRegion by its name
        /// </summary>
        /// <param name="name">the name of the region to find</param>
        public Region this[string name] => SubRegions.FirstOrDefault(r => r.Name == name);


    }
}
