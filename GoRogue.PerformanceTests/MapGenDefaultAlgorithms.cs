using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using JetBrains.Annotations;

namespace GoRogue.PerformanceTests
{
    public class MapGenDefaultAlgorithms
    {
        public IEnumerable<(int width, int height)> MapSizes => new[]
        {
            (25, 25),
            (50, 25),
            (25, 50),
            (50, 50),
            (100, 100),
            (100, 200),
            (500, 500)
        };

        [UsedImplicitly]
        [ParamsSource(nameof(MapSizes))]
        public (int width, int height) MapSize;

        [Benchmark]
        public GenerationContext DungeonMazeMap()
        {
            var generator = new Generator(MapSize.width, MapSize.height);
            generator.ConfigAndGenerateSafe(gen =>
            {
                gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps());
            });

            return generator.Context;
        }
    }
}
