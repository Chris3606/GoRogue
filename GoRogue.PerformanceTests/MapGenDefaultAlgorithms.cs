using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;

namespace GoRogue.PerformanceTests
{
    public class MapGenDefaultAlgorithms
    {
        //private readonly Generator _generator;

        //public MapGenDefaultAlgorithms()
        //{
        //_generator = new Generator(40, 50);
        //}

        [Benchmark]
        public GenerationContext DungeonMazeMap()
        {
            var generator = new Generator(50, 40);
            generator.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps());

            generator.Generate();
            return generator.Context;
        }
    }
}
