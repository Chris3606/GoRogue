using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.Regions
{
    public class RegionCreation
    {
        [UsedImplicitly]
        //[Params(10, 50, 100, 200, 500)]
        [Params(10)]
        public int Size;

        #region Actual GoRogue Region (baseline)
        [Benchmark]
        public Region CreateRectangleBaseline()
        {
            return Region.Rectangle(new Rectangle(0, 0, Size, Size));
        }

        [Benchmark]
        public Region CreateParallelogramTopCornerBaseline()
        {
            return Region.ParallelogramFromTopCorner(new Point(0, 0), Size, Size);
        }

        [Benchmark]
        public Region CreateParallelogramBottomCornerBaseline()
        {
            return Region.ParallelogramFromBottomCorner(new Point(0, 0), Size, Size);
        }
        #endregion

        #region GoRogue Method (implemented in mock)
        [Benchmark]
        public RegionMock CreateRectangleGoRogue()
        {
            return RegionMock.Rectangle(new Rectangle(0, 0, Size, Size), InnerFromOuterPointMethods.GoRogueMethod);
        }

        [Benchmark]
        public RegionMock CreateParallelogramTopCornerGoRogue()
        {
            return RegionMock.ParallelogramFromTopCorner(new Point(0, 0), Size, Size, InnerFromOuterPointMethods.GoRogueMethod);
        }

        [Benchmark]
        public RegionMock CreateParallelogramBottomCornerGoRogue()
        {
            return RegionMock.ParallelogramFromBottomCorner(new Point(0, 0), Size, Size, InnerFromOuterPointMethods.GoRogueMethod);
        }
        #endregion

        #region Ordered Line Method
        [Benchmark]
        public RegionMock CreateRectangleOrderedLine()
        {
            return RegionMock.Rectangle(new Rectangle(0, 0, Size, Size), InnerFromOuterPointMethods.OrderedLineMethod, Lines.Algorithm.DDA);
        }

        [Benchmark]
        public RegionMock CreateParallelogramTopCornerOrderedLine()
        {
            return RegionMock.ParallelogramFromTopCorner(new Point(0, 0), Size, Size, InnerFromOuterPointMethods.OrderedLineMethod, Lines.Algorithm.DDA);
        }

        [Benchmark]
        public RegionMock CreateParallelogramBottomCornerOrderedLine()
        {
            return RegionMock.ParallelogramFromBottomCorner(new Point(0, 0), Size, Size, InnerFromOuterPointMethods.OrderedLineMethod, Lines.Algorithm.DDA);
        }
        #endregion


    }
}
