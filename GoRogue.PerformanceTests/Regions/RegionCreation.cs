using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.Regions
{
    /// <summary>
    /// Set of tests for region creation methods.
    /// </summary>
    /// <remarks>
    /// The tests with "Baseline" at the end of their name use the actual current GoRogue Region class.
    ///
    /// The tests with "GoRogue" at the end of their name implement the same method, but within the RegionMock system.
    /// This allows verification that the RegionMock system itself is not introducing significant change in performance.
    ///
    /// Each set of other benchmarks test a new method of creating the inner points.
    /// </remarks>
    public class RegionCreation
    {
        #region Test Parameters
        [UsedImplicitly]
        //[Params(10, 50, 100, 200, 500)]
        [Params(10)]
        public int Size;

        // Tests currently use DDA (which is NOT Region's current default), because one of the methods being tested
        // requires a line generation algorithm that returns points in order from start to finish; therefore we must
        // limit ourselves to DDA or BresenhamOrdered (Orthogonal requires further testing)
        [UsedImplicitly]
        [Params(Lines.Algorithm.DDA)]
        public Lines.Algorithm LineAlgorithm;

        // Origin point for all shapes
        [UsedImplicitly]
        public Point Origin = new Point(0, 0);

        #endregion

        // Actual GoRogue method, using GoRogue's region class
        #region Actual GoRogue Region (baseline)
        [Benchmark]
        public Region CreateRectangleBaseline()
        {
            return Region.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), LineAlgorithm);
        }

        [Benchmark]
        public Region CreateParallelogramTopCornerBaseline()
        {
            return Region.ParallelogramFromTopCorner(Origin, Size, Size, LineAlgorithm);
        }

        [Benchmark]
        public Region CreateParallelogramBottomCornerBaseline()
        {
            return Region.ParallelogramFromBottomCorner(Origin, Size, Size, LineAlgorithm);
        }
        #endregion

        // Same method implemented in GoRogue region, but implemented in mock test framework
        #region GoRogue Method (implemented in mock)
        [Benchmark]
        public RegionMock CreateRectangleGoRogue()
        {
            return RegionMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), InnerFromOuterPointMethods.GoRogueMethod, LineAlgorithm);
        }

        [Benchmark]
        public RegionMock CreateParallelogramTopCornerGoRogue()
        {
            return RegionMock.ParallelogramFromTopCorner(Origin, Size, Size, InnerFromOuterPointMethods.GoRogueMethod, LineAlgorithm);
        }

        [Benchmark]
        public RegionMock CreateParallelogramBottomCornerGoRogue()
        {
            return RegionMock.ParallelogramFromBottomCorner(Origin, Size, Size, InnerFromOuterPointMethods.GoRogueMethod, LineAlgorithm);
        }
        #endregion

        // Method that relies on ordering of lines returned from Lines.Get to iterate across rows.  These methods
        // REQUIRE that the line-drawing method be something that returns points in order from start to finish.
        #region Ordered Line Method
        [Benchmark]
        public RegionMock CreateRectangleOrderedLine()
        {
            return RegionMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), InnerFromOuterPointMethods.OrderedLineMethod, LineAlgorithm);
        }

        [Benchmark]
        public RegionMock CreateParallelogramTopCornerOrderedLine()
        {
            return RegionMock.ParallelogramFromTopCorner(Origin, Size, Size, InnerFromOuterPointMethods.OrderedLineMethod, LineAlgorithm);
        }

        [Benchmark]
        public RegionMock CreateParallelogramBottomCornerOrderedLine()
        {
            return RegionMock.ParallelogramFromBottomCorner(Origin, Size, Size, InnerFromOuterPointMethods.OrderedLineMethod, LineAlgorithm);
        }
        #endregion

        // Method that creates a map view one bigger than the area (with all edges "passable", and uses that as the
        // map view for a map area finder (flood fill).  This calculates all areas, of which there should be only
        // 2; the one that does not contain points on the outer edge is then the area we're looking for.
        #region Map Area Finder Method
        [Benchmark]
        public RegionMock CreateRectangleMapAreaFinder()
        {
            return RegionMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), InnerFromOuterPointMethods.MapAreaFinderMethod, LineAlgorithm);
        }

        [Benchmark]
        public RegionMock CreateParallelogramTopCornerMapAreaFinder()
        {
            return RegionMock.ParallelogramFromTopCorner(Origin, Size, Size, InnerFromOuterPointMethods.MapAreaFinderMethod, LineAlgorithm);
        }

        [Benchmark]
        public RegionMock CreateParallelogramBottomCornerMapAreaFinder()
        {
            return RegionMock.ParallelogramFromBottomCorner(Origin, Size, Size, InnerFromOuterPointMethods.MapAreaFinderMethod, LineAlgorithm);
        }
        #endregion

        // Same as MapAreaViewMethod, but the grid view it uses is a cache of the values for each cell
        #region Map Area Finder Cached Method
        [Benchmark]
        public RegionMock CreateRectangleMapAreaFinderCached()
        {
            return RegionMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), InnerFromOuterPointMethods.MapAreaFinderArrayViewCacheMethod, LineAlgorithm);
        }

        [Benchmark]
        public RegionMock CreateParallelogramTopCornerMapAreaFinderCached()
        {
            return RegionMock.ParallelogramFromTopCorner(Origin, Size, Size, InnerFromOuterPointMethods.MapAreaFinderArrayViewCacheMethod, LineAlgorithm);
        }

        [Benchmark]
        public RegionMock CreateParallelogramBottomCornerMapAreaFinderCached()
        {
            return RegionMock.ParallelogramFromBottomCorner(Origin, Size, Size, InnerFromOuterPointMethods.MapAreaFinderArrayViewCacheMethod, LineAlgorithm);
        }
        #endregion

        // Implements scan-line technique, using `_outerPoints.Contains` on each value of a row  to determine intersect
        // points
        #region Scan Line Area Contains Method
        [Benchmark]
        public RegionMock CreateRectangleScanLineAreaContains()
        {
            return RegionMock.Rectangle(new Rectangle(Origin.X, Origin.Y, Size, Size), InnerFromOuterPointMethods.ScanLineAreaContainsMethod, LineAlgorithm);
        }

        [Benchmark]
        public RegionMock CreateParallelogramTopCornerScanLineAreaContains()
        {
            return RegionMock.ParallelogramFromTopCorner(Origin, Size, Size, InnerFromOuterPointMethods.ScanLineAreaContainsMethod, LineAlgorithm);
        }

        [Benchmark]
        public RegionMock CreateParallelogramBottomCornerScanLineAreaContains()
        {
            return RegionMock.ParallelogramFromBottomCorner(Origin, Size, Size, InnerFromOuterPointMethods.ScanLineAreaContainsMethod, LineAlgorithm);
        }
        #endregion
    }
}
