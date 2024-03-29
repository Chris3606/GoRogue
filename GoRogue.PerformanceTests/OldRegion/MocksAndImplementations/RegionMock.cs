﻿using System;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.PointHashers;

namespace GoRogue.PerformanceTests.OldRegion.MocksAndImplementations
{
    /// <summary>
    /// DEPRECATED: Object representing performance testing framework for a previous version of Region which functioned
    /// with only 4 points.  Exists only as a reference point for the corresponding inner points implementations.
    /// </summary>
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
        public Area InnerPoints = null!;

        public Area SouthBoundary;
        public Area NorthBoundary;
        public Area EastBoundary;
        public Area WestBoundary;

        public RegionMock(Point northWest, Point northEast, Point southEast, Point southWest, Action<RegionMock> innerCreation, Lines.Algorithm algoToUse = Lines.Algorithm.Bresenham)
        {
            SouthEastCorner = southEast;
            NorthEastCorner = northEast;
            NorthWestCorner = northWest;
            SouthWestCorner = southWest;

            // This isn't a fully accurate max-x value; a more accurate one could be calculated by taking the max x/y
            // of the corners being used to create the line.  However, it is still mathematically valid.
            int maxX = Math.Max(NorthEastCorner.X, SouthEastCorner.X);
            var hasher = new KnownSizeHasher(maxX);

            // Determine outer boundaries between each corner
            WestBoundary = new Area(Lines.GetLine(NorthWestCorner, SouthWestCorner, algoToUse), hasher);
            SouthBoundary = new Area(Lines.GetLine(SouthWestCorner, SouthEastCorner, algoToUse), hasher);
            EastBoundary = new Area(Lines.GetLine(SouthEastCorner, NorthEastCorner, algoToUse), hasher);
            NorthBoundary = new Area(Lines.GetLine(NorthEastCorner, NorthWestCorner, algoToUse), hasher);
            OuterPoints = new MultiArea { WestBoundary, NorthBoundary, EastBoundary, SouthBoundary };
            innerCreation(this);
            Points = new MultiArea { OuterPoints, InnerPoints };
        }

        public static RegionMock Rectangle(Rectangle r, Action<RegionMock> innerCreation, Lines.Algorithm algorithm = Lines.Algorithm.Bresenham)
            => new RegionMock(r.MinExtent, (r.MaxExtentX, r.MinExtentY),
                r.MaxExtent, (r.MinExtentX, r.MaxExtentY), innerCreation, algorithm);


        public static RegionMock ParallelogramFromTopCorner(Point origin, int width, int height, Action<RegionMock> innerCreation, Lines.Algorithm algorithm = Lines.Algorithm.Bresenham)
        {
            var negative = Direction.YIncreasesUpward ? -1 : 1;

            Point nw = origin;
            Point ne = origin + new Point(width, 0);
            Point se = origin + new Point(width * 2, height * negative);
            Point sw = origin + new Point(width, height * negative);

            return new RegionMock(nw, ne, se, sw, innerCreation, algorithm);
        }

        public static RegionMock ParallelogramFromBottomCorner(Point origin, int width, int height, Action<RegionMock> innerCreation, Lines.Algorithm algorithm = Lines.Algorithm.Bresenham)
        {
            var negative = Direction.YIncreasesUpward ? 1 : -1;

            Point nw = origin + (height, height * negative);
            Point ne = origin + (height + width, height * negative);
            Point se = origin + (width, 0);
            Point sw = origin;

            return new RegionMock(nw, ne, se, sw, innerCreation, algorithm);
        }

        public bool IsCorner(Point position)
            => position == NorthEastCorner || position == NorthWestCorner || position == SouthEastCorner || position == SouthWestCorner;
    }
}
