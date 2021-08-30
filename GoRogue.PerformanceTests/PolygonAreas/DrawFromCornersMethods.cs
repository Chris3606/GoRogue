using System;
using SadRogue.Primitives;
using SadRogue.Primitives.PointHashers;

namespace GoRogue.PerformanceTests.PolygonAreas
{
    public static class DrawFromCornersMethods
    {
        // Original method implemented in GoRogue that just draws lines using default hashing algorithms
        #region Original GoRogue Method
        public static void OriginalDefault(PolygonAreaMock polygon)
        {
            for (int i = 0; i < polygon.Corners.Count - 1; i++)
                polygon.OuterPoints.Add(new Area(Lines.Get(polygon.Corners[i], polygon.Corners[i+1], polygon.LineAlgorithm)));

            polygon.OuterPoints.Add(new Area(Lines.Get(polygon.Corners[^1], polygon.Corners[0], polygon.LineAlgorithm)));
        }
        #endregion

        // Original method, except for it uses the known-size hashing algorithm for the areas it creates from the lines
        #region Known Size Hasher
        public static void OriginalKnownSizeHash(PolygonAreaMock polygon)
        {
            for (int i = 0; i < polygon.Corners.Count - 1; i++)
            {
                var hasher = new KnownSizeHasher(Math.Max(polygon.Corners[i].X, polygon.Corners[i + 1].X));
                polygon.OuterPoints.Add(new Area(Lines.Get(polygon.Corners[i], polygon.Corners[i + 1],
                    polygon.LineAlgorithm), hasher));
            }

            var hasherEnd = new KnownSizeHasher(Math.Max(polygon.Corners[^1].X, polygon.Corners[0].X));
            polygon.OuterPoints.Add(new Area(Lines.Get(polygon.Corners[^1], polygon.Corners[0], polygon.LineAlgorithm), hasherEnd));
        }
        #endregion
    }
}
