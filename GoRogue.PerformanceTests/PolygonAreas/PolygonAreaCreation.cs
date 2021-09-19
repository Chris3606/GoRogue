using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using GoRogue.PerformanceTests.PolygonAreas.Implementations;
using GoRogue.PerformanceTests.PolygonAreas.Mocks;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.PolygonAreas
{
    /// <summary>
    /// Set of tests for PolygonArea creation methods.
    /// </summary>
    /// <remarks>
    /// The tests with "Baseline" at the end of their name use the actual current GoRogue Region class.
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
    public class PolygonAreaCreation
    {
        #region Test Parameters
        [UsedImplicitly]
        [Params(10, 50, 100, 200, 500)]
        public int Size;

        // Tests currently use DDA; note that we must limit ourselves ordered lines, so DDA or BresenhamOrdered
        // (Orthogonal requires further testing)
        [UsedImplicitly]
        [Params(Lines.Algorithm.DDA)]
        public Lines.Algorithm LineAlgorithm;

        // Origin point for all shapes that have one
        [UsedImplicitly]
        public Point Origin = new Point(0, 0);
        #endregion

        // Actual GoRogue method, using GoRogue's PolygonArea class
        #region Actual GoRogue PolygonArea (baseline)
        [Benchmark]
        public PolygonArea CreateRectangleBaseline()
        {
            return PolygonArea.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), LineAlgorithm);
        }

        [Benchmark]
        public PolygonArea CreateParallelogramTopCornerBaseline()
        {
            return PolygonArea.Parallelogram(Origin, Size, Size, true, LineAlgorithm);
        }

        [Benchmark]
        public PolygonArea CreateParallelogramBottomCornerBaseline()
        {
            return PolygonArea.Parallelogram(Origin, Size, Size, false, LineAlgorithm);
        }
        #endregion

        // Method originally implemented in GoRogue PolygonArea, with no optimizations
        #region Original GoRogue Method
        [Benchmark]
        public PolygonAreaMock CreateRectangleOriginal()
        {
            return PolygonAreaMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenDefault, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramTopCornerOriginal()
        {
            return PolygonAreaMock.Parallelogram(Origin, Size, Size, DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenDefault, true, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramBottomCornerOriginal()
        {
            return PolygonAreaMock.Parallelogram(Origin, Size, Size, DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenDefault, false, LineAlgorithm);
        }
        #endregion

        // Original scan line method but with known size hashing used for all areas
        #region ScanLine KnownSize Hashing
        [Benchmark]
        public PolygonAreaMock CreateRectangleKnownSizeHash()
        {
            return PolygonAreaMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), DrawFromCornersMethods.OriginalKnownSizeHash, InnerPointsMethods.ScanlineOddEvenKnownSizeHash, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramTopCornerKnownSizeHash()
        {
            return PolygonAreaMock.Parallelogram(Origin, Size, Size, DrawFromCornersMethods.OriginalKnownSizeHash, InnerPointsMethods.ScanlineOddEvenKnownSizeHash, true, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramBottomCornerKnownSizeHash()
        {
            return PolygonAreaMock.Parallelogram(Origin, Size, Size, DrawFromCornersMethods.OriginalKnownSizeHash, InnerPointsMethods.ScanlineOddEvenKnownSizeHash, false, LineAlgorithm);
        }
        #endregion

        // Original scan line method but with no LINQ usage
        #region ScanLine No Linq
        [Benchmark]
        public PolygonAreaMock CreateRectangleNoLinq()
        {
            return PolygonAreaMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenNoLinq, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramTopCornerNoLinq()
        {
            return PolygonAreaMock.Parallelogram(Origin, Size, Size, DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenNoLinq, true, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramBottomCornerNoLinq()
        {
            return PolygonAreaMock.Parallelogram(Origin, Size, Size, DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenNoLinq, false, LineAlgorithm);
        }
        #endregion

        // Original scan line method but with the call to Any replaced with a faster y-value check
        #region ScanLine Faster Y-Check
        [Benchmark]
        public PolygonAreaMock CreateRectangleFasterYCheck()
        {
            return PolygonAreaMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenFasterYCheck, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramTopCornerFasterYCheck()
        {
            return PolygonAreaMock.Parallelogram(Origin, Size, Size, DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenFasterYCheck, true, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramBottomCornerFasterYCheck()
        {
            return PolygonAreaMock.Parallelogram(Origin, Size, Size, DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenFasterYCheck, false, LineAlgorithm);
        }
        #endregion

        // Original scan line method but with the linesEncountered list turned into a HashSet.
        #region ScanLine HashSet Encountered
        [Benchmark]
        public PolygonAreaMock CreateRectangleHashSetEncountered()
        {
            return PolygonAreaMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenHashSetEncountered, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramTopCornerHashSetEncountered()
        {
            return PolygonAreaMock.Parallelogram(Origin, Size, Size, DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenHashSetEncountered, true, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramBottomCornerHashSetEncountered()
        {
            return PolygonAreaMock.Parallelogram(Origin, Size, Size, DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenHashSetEncountered, false, LineAlgorithm);
        }
        #endregion
    }
}
