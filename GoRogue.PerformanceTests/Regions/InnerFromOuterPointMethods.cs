using System.Collections.Generic;
using System.Linq;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.Regions
{
    internal static class InnerFromOuterPointMethods
    {
        // GoRogue's current method
        public static Area GoRogueMethod(RegionMock region)
        {
            var outerList = region.OuterPoints.OrderBy(x => x.X).ToList();

            if(outerList.Count == 0)
                return new Area();

            region.InnerPoints = new Area();

            for (int i = outerList[0].X + 1; i < outerList[^1].X; i++)
            {
                List<Point> row = outerList.Where(point => point.X == i).OrderBy(point => point.Y).ToList();
                if(row.Count > 0)
                {
                    for (int j = row[0].Y; j <= row[^1].Y; j++)
                    {
                        region.InnerPoints.Add(new Point(i, j));
                    }
                }
            }

            return region.InnerPoints;
        }

        // Method that depends on the ordering of Lines.Get returns.  MUST be used with an ordered line type!
        public static Area OrderedLineMethod(RegionMock region)
        {
            region.InnerPoints = new Area();

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

            return region.InnerPoints;
        }
    }
}
