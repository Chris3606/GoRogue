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
        /// The subregions of this region
        /// </summary>
        public IReadOnlyList<Region> SubRegions => _subRegions.AsReadOnly();
        private readonly List<Region> _subRegions = new List<Region>();

        /// <summary>
        /// All of the boundary points of this region.
        /// </summary>
        /// <remarks>
        /// Points in this collection can be removed, which might make them different
        /// from the sum of each of the Boundary Points
        /// </remarks>
        public IReadOnlyArea OuterPoints => _outerPoints;
        private readonly Area _outerPoints = new Area();

        /// <summary>
        /// All of the points inside this region, excluding boundary points and connections
        /// </summary>
        public IReadOnlyArea InnerPoints => _innerPoints;
        private readonly Area _innerPoints = new Area();

        /// <summary>
        /// All of the outer points along the southern boundary
        /// </summary>
        public IReadOnlyArea SouthBoundary => _southBoundary;
        private readonly Area _southBoundary = new Area();

        /// <summary>
        /// All of the outer points along the northern boundary
        /// </summary>
        public IReadOnlyArea NorthBoundary => _northBoundary;
        private readonly Area _northBoundary = new Area();

        /// <summary>
        /// All of the outer points along the eastern boundary
        /// </summary>
        public IReadOnlyArea EastBoundary => _eastBoundary;
        private readonly Area _eastBoundary = new Area();

        /// <summary>
        /// All of the outer points along the western boundary
        /// </summary>
        public IReadOnlyArea WestBoundary => _westBoundary;
        private readonly Area _westBoundary = new Area();

        /// <summary>
        /// All of the points that are considered "connections" to other regions
        /// </summary>
        /// <remarks>
        /// You should perform transformations (rotation, etc) before Connections are made.
        /// </remarks>
        public IReadOnlyArea Connections => _connections;
        private readonly Area _connections = new Area();

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
        public Area Points => (Area) _outerPoints.Positions.Concat(_innerPoints.Positions).Concat(_connections.Positions);

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
    }
}
