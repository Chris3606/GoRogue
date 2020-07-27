using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;

namespace GoRogue.PerformanceTests
{
    public class MapGenDefaultAlgorithms
    {

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
