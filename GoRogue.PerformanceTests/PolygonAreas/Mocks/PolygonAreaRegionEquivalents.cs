using GoRogue.MapGeneration;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.PolygonAreas.Mocks
{
    // Functions that generate PolygonAreas that are equivalent to regions, for comparison
    internal static class PolygonAreaRegionEquivalents
    {
        public static PolygonArea Rectangle(Rectangle r, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
            => new PolygonArea( algorithm, r.MinExtent, (r.MaxExtentX, r.MinExtentY),
                r.MaxExtent, (r.MinExtentX, r.MaxExtentY));


        public static PolygonArea ParallelogramFromTopCorner(Point origin, int width, int height, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
        {
            var negative = Direction.YIncreasesUpward ? -1 : 1;

            Point nw = origin;
            Point ne = origin + new Point(width, 0);
            Point se = origin + new Point(width * 2, height * negative);
            Point sw = origin + new Point(width, height * negative);

            return new PolygonArea(algorithm, nw, ne, se, sw);
        }

        public static PolygonArea ParallelogramFromBottomCorner(Point origin, int width, int height, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
        {
            var negative = Direction.YIncreasesUpward ? 1 : -1;

            Point nw = origin + (height, height * negative);
            Point ne = origin + (height + width, height * negative);
            Point se = origin + (width, 0);
            Point sw = origin;

            return new PolygonArea(algorithm, nw, ne, se, sw);
        }
    }
}
