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
        [Params(40, 72, 73, 180)]
        public int NumberOfSidesOrCorners;

        [UsedImplicitly]
        [Params(40, 72, 73, 180)]
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

        [Benchmark]
        public PolygonArea CreateRegularStarInnerOneFourthBaseline()
        {
            return PolygonArea.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 4.0), LineAlgorithm);
        }

        [Benchmark]
        public PolygonArea CreateRegularStarInnerOneHalfBaseline()
        {
            return PolygonArea.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 2.0), LineAlgorithm);
        }

        [Benchmark]
        public PolygonArea CreateRegularStarInnerThreeFourthsBaseline()
        {
            return PolygonArea.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius * 0.75), LineAlgorithm);
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

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneFourthOriginal()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 4.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenDefault,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneHalfOriginal()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 2.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenDefault,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerThreeFourthsOriginal()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius * 0.75),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenDefault,
                LineAlgorithm);
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

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneFourthKnownSizeHash()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 4.0),
                DrawFromCornersMethods.OriginalKnownSizeHash, InnerPointsMethods.ScanlineOddEvenKnownSizeHash,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneHalfKnownSizeHash()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 2.0),
                DrawFromCornersMethods.OriginalKnownSizeHash, InnerPointsMethods.ScanlineOddEvenKnownSizeHash,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerThreeFourthsKnownSizeHash()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius * 0.75),
                DrawFromCornersMethods.OriginalKnownSizeHash, InnerPointsMethods.ScanlineOddEvenKnownSizeHash,
                LineAlgorithm);
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

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneFourthNoLinq()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 4.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenNoLinq,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneHalfNoLinq()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 2.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenNoLinq,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerThreeFourthsNoLinq()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius * 0.75),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenNoLinq,
                LineAlgorithm);
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

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneFourthFasterYCheck()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 4.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenFasterYCheck,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneHalfFasterYCheck()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 2.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenFasterYCheck,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerThreeFourthsFasterYCheck()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius * 0.75),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenFasterYCheck,
                LineAlgorithm);
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

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneFourthHashSetEncountered()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 4.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenHashSetEncountered,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneHalfHashSetEncountered()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 2.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenHashSetEncountered,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerThreeFourthsHashSetEncountered()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius * 0.75),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenHashSetEncountered,
                LineAlgorithm);
        }
        #endregion

        // Original scan line method but optimized to not perform an unnecessary iteration over the SubAreas of outer
        // points for each point processed.
        #region ScanLine Omit Redundant Check
        [Benchmark]
        public PolygonAreaMock CreateRegularPolygonOmitRedundantCheck()
        {
            return PolygonAreaMock.RegularPolygon(Center, NumberOfSidesOrCorners, ParameterizedRadius,
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenOmitRedundantCheck, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneFourthOmitRedundantCheck()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 4.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenOmitRedundantCheck,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneHalfOmitRedundantCheck()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 2.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenOmitRedundantCheck,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerThreeFourthsOmitRedundantCheck()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius * 0.75),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenOmitRedundantCheck,
                LineAlgorithm);
        }
        #endregion

        // Original scan line method except for it caches OuterPoints in a hash set and uses that for contains operations.
        #region ScanLine Cache Outer Points
        [Benchmark]
        public PolygonAreaMock CreateRegularPolygonCacheOuterPoints()
        {
            return PolygonAreaMock.RegularPolygon(Center, NumberOfSidesOrCorners, ParameterizedRadius,
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenCacheOuterPoints, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneFourthCacheOuterPoints()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 4.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenCacheOuterPoints,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerOneHalfCacheOuterPoints()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius / 2.0),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenCacheOuterPoints,
                LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateRegularStarInnerThreeFourthsCacheOuterPoints()
        {
            return PolygonAreaMock.RegularStar(Center, NumberOfSidesOrCorners,
                ParameterizedRadius, (int)(ParameterizedRadius * 0.75),
                DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenCacheOuterPoints,
                LineAlgorithm);
        }
        #endregion
    }
}
