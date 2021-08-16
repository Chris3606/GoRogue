using System;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.Regions
{
    // Object representing a region's internal data and the general method of construction.  Necessary to allow the
    // benchmark functions to actually return a value which depends on the creation method, to avoid any of it being
    // optimized out.
    [PublicAPI]
    public class RegionMock
    {
        // These are properties to more accurately reflect any overhead that comes with getter-setter properties,
        // as opposed to straight fields

        public Point SouthEastCorner { get; set; }
        public Point SouthWestCorner { get; set; }
        public Point NorthWestCorner { get; set; }
        public Point NorthEastCorner { get; set; }

        // These are fields because that is how the creation functions in the real Region class will access them.

        public MultiArea Points;
        public MultiArea OuterPoints;
        public Area InnerPoints;

        public Area SouthBoundary;
        public Area NorthBoundary;
        public Area EastBoundary;
        public Area WestBoundary;

        public RegionMock(Point northWest, Point northEast, Point southEast, Point southWest, Func<RegionMock, Area> innerCreation, Lines.Algorithm algoToUse = Lines.Algorithm.Bresenham)
        {
            SouthEastCorner = southEast;
            NorthEastCorner = northEast;
            NorthWestCorner = northWest;
            SouthWestCorner = southWest;
            WestBoundary = new Area(Lines.Get(NorthWestCorner, SouthWestCorner, algoToUse));
            SouthBoundary = new Area(Lines.Get(SouthWestCorner, SouthEastCorner, algoToUse));
            EastBoundary = new Area(Lines.Get(SouthEastCorner, NorthEastCorner, algoToUse));
            NorthBoundary = new Area(Lines.Get(NorthEastCorner, NorthWestCorner, algoToUse));
            OuterPoints = new MultiArea {WestBoundary, NorthBoundary, EastBoundary, SouthBoundary};
            InnerPoints = innerCreation(this);
            Points = new MultiArea {OuterPoints, InnerPoints};
        }

        public static RegionMock Rectangle(Rectangle r, Func<RegionMock, Area> innerCreation, Lines.Algorithm algorithm = Lines.Algorithm.Bresenham)
            => new RegionMock(r.MinExtent, (r.MaxExtentX, r.MinExtentY),
                r.MaxExtent, (r.MinExtentX, r.MaxExtentY), innerCreation, algorithm);


         public static RegionMock ParallelogramFromTopCorner(Point origin, int width, int height, Func<RegionMock, Area> innerCreation, Lines.Algorithm algorithm = Lines.Algorithm.Bresenham)
         {
             var negative = Direction.YIncreasesUpward ? 1 : -1;

             Point nw = origin;
             Point ne = origin + new Point(width, 0);
             Point se = origin + new Point(width * 2, height * negative);
             Point sw = origin + new Point(width, height * negative);

             return new RegionMock(nw, ne, se, sw, innerCreation, algorithm);
         }

         public static RegionMock ParallelogramFromBottomCorner(Point origin, int width, int height, Func<RegionMock, Area> innerCreation, Lines.Algorithm algorithm = Lines.Algorithm.Bresenham)
         {
             var negative = Direction.YIncreasesUpward ? 1 : -1;

             Point nw = origin + (height, height * negative);
             Point ne = origin + (height + width, height * negative);
             Point se = origin + (width, 0);
             Point sw = origin;

             return new RegionMock(nw, ne, se, sw, innerCreation, algorithm);
         }
    }
}
