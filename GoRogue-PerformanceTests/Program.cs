using GoRogue;
using GoRogue.SenseMapping;
using System;

namespace GoRogue_PerformanceTests
{
    public class Program
    {
        private static readonly int MAP_WIDTH = 250;
        private static readonly int MAP_HEIGHT = 250;
        private static readonly SourceType SOURCE_TYPE = SourceType.RIPPLE;
        private static readonly int LIGHT_RADIUS = 10;
        private static readonly Radius RADIUS_STRATEGY = Radius.CIRCLE;
        private static readonly int ITERATIONS_FOR_TIMING = 100;

        private static readonly Coord LINE_START = Coord.Get(3, 5);
        private static readonly Coord LINE_END = Coord.Get(29, 23);

        private static void TestLightingNSource(int sources)
        {
            var timeMultipleLighting = LightingFOVTests.TimeForNSourcesLighting(MAP_WIDTH, MAP_HEIGHT, LIGHT_RADIUS,
                                                                                ITERATIONS_FOR_TIMING, sources);
            Console.WriteLine($"Time for {ITERATIONS_FOR_TIMING}, calc fov's, {sources} sources, {MAP_WIDTH}x{MAP_HEIGHT} map, Radius {LIGHT_RADIUS}:");
            Console.WriteLine($"\tLighting: {timeMultipleLighting.ToString()}");
        }

        private static void Main()
        {
            /*
            long lightingMem = LightingFOVTests.MemorySingleLightSourceLighting(MAP_WIDTH, MAP_HEIGHT, LIGHT_RADIUS);
            long fovMem = LightingFOVTests.MemorySingleLightSourceFOV(MAP_WIDTH, MAP_HEIGHT, LIGHT_RADIUS);
            Console.WriteLine($"Memory for {MAP_WIDTH}x{MAP_HEIGHT}, Radius {LIGHT_RADIUS}:");
            Console.WriteLine($"\tLighting: {lightingMem} bytes");
            Console.WriteLine($"\tFOV     : {fovMem} bytes");
            */
            var timeSingleLighting = LightingFOVTests.TimeForSingleLightSourceLighting(MAP_WIDTH, MAP_HEIGHT, SOURCE_TYPE,
                                                                         LIGHT_RADIUS, RADIUS_STRATEGY, ITERATIONS_FOR_TIMING);
            //var timeSingleFOV = LightingFOVTests.TimeForSingleLightSourceFOV(MAP_WIDTH, MAP_HEIGHT,
            //                                                             LIGHT_RADIUS, ITERATIONS_FOR_TIMING);
            Console.WriteLine();
            Console.WriteLine($"Time for {ITERATIONS_FOR_TIMING} calculates, single source, {MAP_WIDTH}x{MAP_HEIGHT} map, Radius {LIGHT_RADIUS}:");
            Console.WriteLine($"\tSenseMap: {timeSingleLighting.ToString()}");
            //Console.WriteLine($"\tFOV     : {timeSingleFOV.ToString()}");

            
            Console.WriteLine();
            TestLightingNSource(2);

            Console.WriteLine();
            TestLightingNSource(3);

            Console.WriteLine();
            TestLightingNSource(4);
            /*
            var timeSingleDijkstra = PathingTests.TimeForSingleSourceDijkstra(MAP_WIDTH, MAP_HEIGHT, ITERATIONS_FOR_TIMING);
            Console.WriteLine();
            Console.WriteLine($"Time for {ITERATIONS_FOR_TIMING} dijkstra map calculates, single source, {MAP_WIDTH}x{MAP_HEIGHT} map, 1 goal at (5, 5):");
            Console.WriteLine($"\t{timeSingleDijkstra}");

            var timeBres = LineTests.TimeForLineGeneration(LINE_START, LINE_END, Lines.Algorithm.BRESENHAM, ITERATIONS_FOR_TIMING);
            var timeDDA = LineTests.TimeForLineGeneration(LINE_START, LINE_END, Lines.Algorithm.DDA, ITERATIONS_FOR_TIMING);
            var timeOrtho = LineTests.TimeForLineGeneration(LINE_START, LINE_END, Lines.Algorithm.ORTHO, ITERATIONS_FOR_TIMING);

            Console.WriteLine();
            Console.WriteLine($"Time for {ITERATIONS_FOR_TIMING} generations of line from {LINE_START} to {LINE_END}:");
            Console.WriteLine($"\tBresenham: {timeBres}");
            Console.WriteLine($"\tDDA      : {timeDDA}");
            Console.WriteLine($"\tOrtho    : {timeOrtho}");
            
            
            var timeAStar = PathingTests.TimeForAStar(MAP_WIDTH, MAP_HEIGHT, ITERATIONS_FOR_TIMING);
            Console.WriteLine();
            Console.WriteLine($"Time for {ITERATIONS_FOR_TIMING} paths, on {MAP_WIDTH}x{MAP_HEIGHT} map:");
            Console.WriteLine($"\tAStar: {timeAStar}");
            */
        }
    }
}