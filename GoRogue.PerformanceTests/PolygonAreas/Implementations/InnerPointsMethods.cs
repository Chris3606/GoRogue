using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GoRogue.PerformanceTests.PolygonAreas.Mocks;
using SadRogue.Primitives;
using SadRogue.Primitives.PointHashers;

namespace GoRogue.PerformanceTests.PolygonAreas.Implementations
{
    internal static class InnerPointsMethods
    {
        // Originally implemented method using scan-lines and the odd-even method for detecting if points are inside
        // or outside the polygon
        #region Original GoRogue Scanlines
        public static void ScanlineOddEvenDefault(PolygonAreaMock polygon)
        {
            polygon.InnerPoints = new Area();

            var bounds = polygon.OuterPoints.Bounds;

            // The top and bottom rows can never contain an inner point, so skip them.
            for(int y = bounds.MinExtentY + 1; y < bounds.MaxExtentY; y++)
            {
                var linesEncountered = new List<IReadOnlyArea>();

                // Must include MinExtentX so that it can accurately count lines encountered.
                // Doesn't need MaxExtentX since no inner point can be equal to or greater than that.
                for(int x = bounds.MinExtentX; x < bounds.MaxExtentX; x++)
                {
                    if (polygon.OuterPoints.Contains(x, y))
                    {
                        foreach (var boundary in GetBoundariesContaining(polygon, x, y))
                        {
                            if (boundary.Any(p => p.Y < y))
                            {
                                if (!linesEncountered.Contains(boundary))
                                    linesEncountered.Add(boundary);
                            }
                        }
                    }
                    else
                    {
                        if(linesEncountered.Count % 2 == 1)
                            polygon.InnerPoints.Add(x,y);
                    }
                }
            }
        }
        #endregion

        // Variation of ScanlineOddEvenDefault that uses the known-size hasher for inner points
        #region Scanlines KnownSize Hash
        public static void ScanlineOddEvenKnownSizeHash(PolygonAreaMock polygon)
        {
            var bounds = polygon.OuterPoints.Bounds;

            polygon.InnerPoints = new Area(new KnownSizeHasher(bounds.MaxExtentX));

            // The top and bottom rows can never contain an inner point, so skip them.
            for(int y = bounds.MinExtentY + 1; y < bounds.MaxExtentY; y++)
            {
                var linesEncountered = new List<IReadOnlyArea>();

                // Must include MinExtentX so that it can accurately count lines encountered.
                // Doesn't need MaxExtentX since no inner point can be equal to or greater than that.
                for(int x = bounds.MinExtentX; x < bounds.MaxExtentX; x++)
                {
                    if (polygon.OuterPoints.Contains(x, y))
                    {
                        foreach (var boundary in GetBoundariesContaining(polygon, x, y))
                        {
                            if (boundary.Any(p => p.Y < y))
                            {
                                if (!linesEncountered.Contains(boundary))
                                    linesEncountered.Add(boundary);
                            }
                        }
                    }
                    else
                    {
                        if(linesEncountered.Count % 2 == 1)
                            polygon.InnerPoints.Add(x,y);
                    }
                }
            }
        }
        #endregion

        // Variation of ScanlineOddEvenDefault that omits the usage of LINQ and a foreach loop
        #region ScanlinesNoLinq
        public static void ScanlineOddEvenNoLinq(PolygonAreaMock polygon)
        {
            polygon.InnerPoints = new Area();

            var bounds = polygon.OuterPoints.Bounds;

            // The top and bottom rows can never contain an inner point, so skip them.
            for(int y = bounds.MinExtentY + 1; y < bounds.MaxExtentY; y++)
            {
                var linesEncountered = new List<IReadOnlyArea>();

                // Must include MinExtentX so that it can accurately count lines encountered.
                // Doesn't need MaxExtentX since no inner point can be equal to or greater than that.
                for(int x = bounds.MinExtentX; x < bounds.MaxExtentX; x++)
                {
                    if (polygon.OuterPoints.Contains(x, y))
                    {
                        for (int sIdx = 0; sIdx < polygon.OuterPoints.SubAreas.Count; sIdx++)
                        {
                            var boundary = polygon.OuterPoints.SubAreas[sIdx];
                            if (!boundary.Contains(x, y)) continue;


                            if (HasLessY(boundary, y))
                            {
                                if (!linesEncountered.Contains(boundary))
                                    linesEncountered.Add(boundary);
                            }
                        }
                    }
                    else
                    {
                        if(linesEncountered.Count % 2 == 1)
                            polygon.InnerPoints.Add(x,y);
                    }
                }
            }
        }
        #endregion

        // Variation of ScanlineOddEvenDefault that changes the `Any` call to a more refined approach for checking
        // for lesser y-values in a boundary line
        #region Scanlies FasterYValueCheck
        public static void ScanlineOddEvenFasterYCheck(PolygonAreaMock polygon)
        {
            polygon.InnerPoints = new Area();

            var bounds = polygon.OuterPoints.Bounds;

            // The top and bottom rows can never contain an inner point, so skip them.
            for(int y = bounds.MinExtentY + 1; y < bounds.MaxExtentY; y++)
            {
                var linesEncountered = new List<IReadOnlyArea>();

                // Must include MinExtentX so that it can accurately count lines encountered.
                // Doesn't need MaxExtentX since no inner point can be equal to or greater than that.
                for(int x = bounds.MinExtentX; x < bounds.MaxExtentX; x++)
                {
                    if (polygon.OuterPoints.Contains(x, y))
                    {
                        foreach (var boundary in GetBoundariesContaining(polygon, x, y))
                        {
                            // Since these areas are 2D (ordered) lines, if there exists any point that is a lesser
                            // y-value than the current, it must be one of the ends
                            if (boundary[0].Y < y || boundary[^1].Y < y)
                            {
                                if (!linesEncountered.Contains(boundary))
                                    linesEncountered.Add(boundary);
                            }
                        }
                    }
                    else
                    {
                        if(linesEncountered.Count % 2 == 1)
                            polygon.InnerPoints.Add(x,y);
                    }
                }
            }
        }
        #endregion

        // Variation of ScanlineOddEvenDefault that changes the linesEncountered list to a hash set.
        #region Scanlines HashSet for Encountered
        public static void ScanlineOddEvenHashSetEncountered(PolygonAreaMock polygon)
        {
            polygon.InnerPoints = new Area();

            var bounds = polygon.OuterPoints.Bounds;

            // The top and bottom rows can never contain an inner point, so skip them.
            for(int y = bounds.MinExtentY + 1; y < bounds.MaxExtentY; y++)
            {
                var linesEncountered = new HashSet<IReadOnlyArea>();

                // Must include MinExtentX so that it can accurately count lines encountered.
                // Doesn't need MaxExtentX since no inner point can be equal to or greater than that.
                for(int x = bounds.MinExtentX; x < bounds.MaxExtentX; x++)
                {
                    if (polygon.OuterPoints.Contains(x, y))
                    {
                        foreach (var boundary in GetBoundariesContaining(polygon, x, y))
                        {
                            if (boundary.Any(p => p.Y < y))
                            {
                                if (!linesEncountered.Contains(boundary))
                                    linesEncountered.Add(boundary);
                            }
                        }
                    }
                    else
                    {
                        if(linesEncountered.Count % 2 == 1)
                            polygon.InnerPoints.Add(x,y);
                    }
                }
            }
        }
        #endregion

        // Helper functions used in various algorithms
        #region Helper Functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasLessY(IReadOnlyArea area, int y)
        {
            for (int i = 0; i < area.Count; i++)
                if (area[i].Y < y)
                    return true;

            return false;
        }

        private static IEnumerable<IReadOnlyArea> GetBoundariesContaining(PolygonAreaMock polygon, int x, int y)
            => polygon.OuterPoints.SubAreas.Where(sa => sa.Contains(x, y));
        #endregion
    }
}
