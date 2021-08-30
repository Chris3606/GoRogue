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
    ///
    /// Each set of other benchmarks test a new method of creating the inner points or drawing the lines.
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

        // Actual GoRogue method, using GoRogue's PolygonArea class (and some helper methods that generate equivalent
        // areas as Region would)
        #region Actual GoRogue Region (baseline)
        [Benchmark]
        public PolygonArea CreateRectangleBaseline()
        {
            return PolygonAreaRegionEquivalents.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), LineAlgorithm);
        }

        [Benchmark]
        public PolygonArea CreateParallelogramTopCornerBaseline()
        {
            return PolygonAreaRegionEquivalents.ParallelogramFromTopCorner(Origin, Size, Size, LineAlgorithm);
        }

        [Benchmark]
        public PolygonArea CreateParallelogramBottomCornerBaseline()
        {
            return PolygonAreaRegionEquivalents.ParallelogramFromBottomCorner(Origin, Size, Size, LineAlgorithm);
        }
        #endregion

        // Same method implemented in GoRogue PolygonArea, but implemented in mock test framework
        #region GoRogue Method (implemented in mock)
        [Benchmark]
        public PolygonAreaMock CreateRectangleGoRogue()
        {
            return PolygonAreaMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenDefault, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramTopCornerGoRogue()
        {
            return PolygonAreaMock.ParallelogramFromTopCorner(Origin, Size, Size, DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenDefault, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramBottomCornerGoRogue()
        {
            return PolygonAreaMock.ParallelogramFromBottomCorner(Origin, Size, Size, DrawFromCornersMethods.OriginalDefault, InnerPointsMethods.ScanlineOddEvenDefault, LineAlgorithm);
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
            return PolygonAreaMock.ParallelogramFromTopCorner(Origin, Size, Size, DrawFromCornersMethods.OriginalKnownSizeHash, InnerPointsMethods.ScanlineOddEvenKnownSizeHash, LineAlgorithm);
        }

        [Benchmark]
        public PolygonAreaMock CreateParallelogramBottomCornerKnownSizeHash()
        {
            return PolygonAreaMock.ParallelogramFromBottomCorner(Origin, Size, Size, DrawFromCornersMethods.OriginalKnownSizeHash, InnerPointsMethods.ScanlineOddEvenKnownSizeHash, LineAlgorithm);
        }
        #endregion
    }
}
