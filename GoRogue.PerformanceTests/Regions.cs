using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests
{
    public class Regions
    {
        [UsedImplicitly]
        [Params(10, 50, 100, 200, 500)]
        public int Size;

        [Benchmark]
        public Region CreateRectangle()
        {
            return Region.Rectangle(new Rectangle(0, 0, Size, Size));
        }

        [Benchmark]
        public Region CreateParallelogramTopCorner()
        {
            return Region.ParallelogramFromTopCorner(new Point(0, 0), Size, Size);
        }

        [Benchmark]
        public Region CreateParallelogramBottomCorner()
        {
            return Region.ParallelogramFromBottomCorner(new Point(0, 0), Size, Size);
        }
    }
}
