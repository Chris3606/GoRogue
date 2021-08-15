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
            return Region.Rectangle("RectangleCreation", new Point(0, 0), Size, Size);
        }

        [Benchmark]
        public Region CreateParallelogram()
        {
            return Region.RegularParallelogram("ParallelogramCreation", new Point(0, 0), Size, Size, 10);
        }
    }
}
