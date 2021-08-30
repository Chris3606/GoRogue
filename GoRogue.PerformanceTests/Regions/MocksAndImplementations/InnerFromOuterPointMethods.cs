using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using SadRogue.Primitives.PointHashers;

namespace GoRogue.PerformanceTests.Regions.MocksAndImplementations
{
    internal static class InnerFromOuterPointMethods
    {
        // GoRogue's current method
        public static void GoRogueMethod(RegionMock region)
        {
            int maxX = Math.Max(region.NorthEastCorner.X, region.SouthEastCorner.X);
            region.InnerPoints = new Area(new KnownSizeHasher(maxX));

            var outerList = region.OuterPoints.OrderBy(x => x.X).ToList();

            for (int i = outerList[0].X; i < outerList[^1].X; i++)
            {
                List<Point> row = outerList.Where(point => point.X == i).OrderBy(point => point.Y).ToList();
                if(row.Count > 0)
                {
                    for (int j = row[0].Y; j <= row[^1].Y; j++)
                    {
                        var p = new Point(i, j);
                        if(!region.OuterPoints.Contains(p) && !region.IsCorner(p))
                            region.InnerPoints.Add(p);
                    }
                }
            }
        }

        // Method that depends on the ordering of Lines.Get returns.  MUST be used with an ordered line type!
        public static void OrderedLineMethod(RegionMock region)
        {
            int maxX = Math.Max(region.NorthEastCorner.X, region.SouthEastCorner.X);
            region.InnerPoints = new Area(new KnownSizeHasher(maxX));

            // Guaranteed to be in order from North to South
            foreach (var point in region.WestBoundary)
            {
                // Progress along axis, adding all points until we encounter something in a border
                int x = point.X + 1;
                while (true)
                {
                    var curPoint = new Point(x, point.Y);
                    if (region.OuterPoints.Contains(curPoint))
                        break;

                    region.InnerPoints.Add(curPoint);

                    x += 1;
                }
            }

            // If the _eastBoundary contains a lower y-value than the lowest in _westBoundary, then that means the
            // _northBoundary slants upward moving west to east; and thus we missed adding any points above the
            // _westBoundary's northern-most point. So, we'll iterate over any of those and add them.  For comparison,
            // we actually need to care about where the highest and lowest points are; and it will depend on
            // YIncreasesUpwards, so we must account for that as well.
            //
            // Note that _westBoundary is guaranteed to be in order from North to South
            // TODO: The following only works in YIncreasesUpwards: false; need to generalize.
            // The y-value slope running north-south will change depending on this value.
            for (int i = region.EastBoundary.Count - 1; i >= 0; i--)
            {
                var point = region.EastBoundary[i];
                if (point.Y >= region.WestBoundary[0].Y) break;

                int x = point.X - 1;
                while (true)
                {
                    var curPoint = new Point(x, point.Y);
                    if (region.OuterPoints.Contains(curPoint))
                        break;

                    region.InnerPoints.Add(curPoint);

                    x -= 1;
                }
            }

            // Similarly, if the _eastBoundary contains a _higher_ y-value than the highest _westBoundary, then that
            // means the south boundary slants downward from west to east, and as a result we missed some points; so
            // we can now go back and add those.
            // TODO: See above loop YIncreasesUpwards todo.
            foreach (var point in region.EastBoundary)
            {
                if (point.Y <= region.WestBoundary[^1].Y) break;

                int x = point.X - 1;
                while (true)
                {
                    var curPoint = new Point(x, point.Y);
                    if (region.OuterPoints.Contains(curPoint))
                        break;

                    region.InnerPoints.Add(curPoint);

                    x -= 1;
                }
            }
        }

        #region Map Area Finder Method
        public static void MapAreaFinderMethod(RegionMock region)
        {
            // Expand bounds one outside
            var viewBounds = region.OuterPoints.Bounds.Expand(1, 1);

            // Create grid view for map area finder
            var finderView = new LambdaGridView<bool>(viewBounds.Width, viewBounds.Height, pos =>
            {
                if (viewBounds.IsPerimeterPosition(pos)) return true;
                return !region.OuterPoints.Contains(pos);
            });

            // Find areas using map area finder and loop through
            // TODO: Is cardinals sufficient here?
            // TODO: This assumes only 2 areas
            foreach (var area in MapAreaFinder.MapAreasFor(finderView, AdjacencyRule.Cardinals))
            {
                // The area we're looking for is the one that does NOT contain the top left corner
                if (area.Contains(0, 0)) continue;

                region.InnerPoints = area;
                return;
            }

            throw new Exception("Algorithmic badness occured.");
        }

        public static void MapAreaFinderArrayViewCacheMethod(RegionMock region)
        {
            // Expand bounds one outside
            var viewBounds = region.OuterPoints.Bounds.Expand(1, 1);

            // Create grid view for map area finder data, and cache in array map
            var finderView = new ArrayView<bool>(viewBounds.Width, viewBounds.Height);
            finderView.ApplyOverlay(pos =>
            {
                if (viewBounds.IsPerimeterPosition(pos)) return true;
                return !region.OuterPoints.Contains(pos);
            });

            // Find areas using map area finder and loop through
            // TODO: Is cardinals sufficient here?
            // TODO: This assumes only 2 areas
            foreach (var area in MapAreaFinder.MapAreasFor(finderView, AdjacencyRule.Cardinals))
            {
                // The area we're looking for is the one that does NOT contain the top left corner
                if (area.Contains(0, 0)) continue;

                region.InnerPoints = area;
                return;
            }

            throw new Exception("Algorithmic badness occured.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPerimeterPosition(this Rectangle rect, Point pos)
            => pos.X == rect.MinExtentX || pos.X == rect.MaxExtentX || pos.Y == rect.MinExtentY || pos.Y == rect.MaxExtentY;
        #endregion

        // Uses the scan-line algorithm to determine inner points.  Source data is _outerPoints, which is an area.
        public static void ScanLineAreaContainsMethod(RegionMock region)
        {
            int maxX = Math.Max(region.NorthEastCorner.X, region.SouthEastCorner.X);
            region.InnerPoints = new Area(new KnownSizeHasher(maxX));

            var outerBounds = region.OuterPoints.Bounds;
            for (int y = outerBounds.MinExtentY; y <= outerBounds.MaxExtentY; y++)
            {
                // Find intersect points, sorted in X order.  We have to copy since we're basing _outerPoints on Area
                var y1 = y;
                var intersects = Enumerable.Range(outerBounds.MinExtentX, outerBounds.Width)
                    .Where(x => region.OuterPoints.Contains(x, y1)).ToArray();

                for (int i = 0; i < intersects.Length; i += 2)
                {
                    for (int x1 = intersects[i] + 1; x1 < intersects[i + 1]; x1++)
                        region.InnerPoints.Add(x1, y);
                }
            }
        }
    }
}
