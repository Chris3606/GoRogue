using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using GoRogue.PerformanceTests.PolygonAreas.Implementations;
using GoRogue.PerformanceTests.PolygonAreas.Mocks;
using JetBrains.Annotations;
using Point = SadRogue.Primitives.Point;

namespace GoRogue.PerformanceTests.PolygonAreas
{
    /// <summary>
    /// Set of tests for PolygonArea creation methods that create regular shapes (RegularPolygon and RegularStar).
    /// </summary>
    /// <remarks>
    /// The tests with "Baseline" at the end of their name use the actual current GoRogue PolygonArea class.
    ///
    /// The tests with "Default" at the end of their name implement the same method, but within the PolygonAreaMock system.
    /// This allows verification that the PolygonAreaMock system itself is not introducing significant change in performance.
    /// TODO: These currently do not exist because right now they would be identical to ones with the suffix Original
    ///
    /// The tests with "Original" at the end of their name implement the method originally in GoRogue, with no optimizations.
    /// These are what all of the "optimized" methods are based on; unless the method is an entirely new algorithm, they
    /// each make exactly one category of change from the Original, so that comparing performance impact is possible.
    ///
    /// Each set of other benchmarks test a new method of creating the inner points or drawing the lines, or one optimization
    /// from the original.
    /// </remarks>
    public class PolygonAreaRegularPolygonCreation
    {
        #region Test Parameters
        [UsedImplicitly]
        [Params(3)]
        public int NumberOfSidesOrCorners;

        [UsedImplicitly]
        [Params(5, 10)]
        public int ParameterizedRadius;

        // Tests currently use DDA; note that we must limit ourselves ordered lines, so DDA or BresenhamOrdered
        // (Orthogonal requires further testing)
        [UsedImplicitly]
        [Params(Lines.Algorithm.DDA)]
        public Lines.Algorithm LineAlgorithm;

        // Center point for all shapes
        [UsedImplicitly]
        public Point Center = new Point(0, 0);
        #endregion

        // Actual GoRogue method, using GoRogue's PolygonArea class
        #region Actual GoRogue PolygonArea (baseline)
        [Benchmark]
        public PolygonArea CreateRegularPolygonBaseline()
        {
            return PolygonArea.RegularPolygon(Center, NumberOfSidesOrCorners, ParameterizedRadius, LineAlgorithm);
        }
        #endregion

        // Method originally implemented in GoRogue PolygonArea, with no optimizations
        #region Original GoRogue Method
        [Benchmark]
        public PolygonAreaMock CreateRegularPolygonOriginal()
        {
            return PolygonAreaMock.RegularPolygon(Center, NumberOfSidesOrCorners, ParameterizedRadius,
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenDefault, LineAlgorithm);
        }
        #endregion

        // Original scan line method but with known size hashing used for all areas
        #region ScanLine KnownSize Hashing
        [Benchmark]
        public PolygonAreaMock CreateRegularPolygonKnownSizeHash()
        {
            return PolygonAreaMock.RegularPolygon(Center, NumberOfSidesOrCorners, ParameterizedRadius,
                DrawFromCornersMethods.OriginalKnownSizeHash, InnerPointsMethods.ScanlineOddEvenKnownSizeHash, LineAlgorithm);
        }

        #endregion

        // Original scan line method but with no LINQ usage
        #region ScanLine No Linq
        [Benchmark]
        public PolygonAreaMock CreateRegularPolygonNoLinq()
        {
            return PolygonAreaMock.RegularPolygon(Center, NumberOfSidesOrCorners, ParameterizedRadius,
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenNoLinq, LineAlgorithm);
        }
        #endregion

        // Original scan line method but with the call to Any replaced with a faster y-value check
        #region ScanLine Faster Y-Check
        [Benchmark]
        public PolygonAreaMock CreateRegularPolygonFasterYCheck()
        {
            return PolygonAreaMock.RegularPolygon(Center, NumberOfSidesOrCorners, ParameterizedRadius,
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenFasterYCheck, LineAlgorithm);
        }
        #endregion

        // Original scan line method but with the linesEncountered list turned into a HashSet.
        #region ScanLine HashSet Encountered
        [Benchmark]
        public PolygonAreaMock CreateRegularPolygonHashSetEncountered()
        {
            return PolygonAreaMock.RegularPolygon(Center, NumberOfSidesOrCorners, ParameterizedRadius,
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenHashSetEncountered, LineAlgorithm);
        }
        #endregion
    }
}
