using GoRogue;
using GoRogue.SenseMapping;
using System;

namespace GoRogue_PerformanceTests
{
    public class Program
    {
        private static readonly int ITERATIONS_FOR_TIMING = 100;
        private static readonly int LIGHT_RADIUS = 10;
        private static readonly Coord LINE_END = Coord.Get(29, 23);
        private static readonly Coord LINE_START = Coord.Get(3, 5);
        private static readonly int MAP_HEIGHT = 250;
        private static readonly int MAP_WIDTH = 250;
        private static readonly Radius RADIUS_STRATEGY = Radius.CIRCLE;
        private static readonly SourceType SOURCE_TYPE = SourceType.RIPPLE;

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

            /*
            Console.WriteLine();
            TestLightingNSource(4);
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
            */

            
            var timeAStar = PathingTests.TimeForAStar(MAP_WIDTH, MAP_HEIGHT, ITERATIONS_FOR_TIMING);
            Console.WriteLine();
            Console.WriteLine($"Time for {ITERATIONS_FOR_TIMING} paths, on {MAP_WIDTH}x{MAP_HEIGHT} map:");
            Console.WriteLine($"\tAStar: {timeAStar}");

            var timeForSmallDiceRoll = DiceNotationTests.TimeForDiceRoll("1d6", ITERATIONS_FOR_TIMING * 10);
            var timeForMediumDiceRoll = DiceNotationTests.TimeForDiceRoll("2d6+3", ITERATIONS_FOR_TIMING * 10);
            var timeForLargeDiceRoll = DiceNotationTests.TimeForDiceRoll("1d(1d12+4)+3", ITERATIONS_FOR_TIMING * 10);

            var timeForSmallDiceExpr = DiceNotationTests.TimeForDiceExpression("1d6", ITERATIONS_FOR_TIMING * 10);
            var timeForMediumDiceExpr = DiceNotationTests.TimeForDiceExpression("2d6+3", ITERATIONS_FOR_TIMING * 10);
            var timeForLargeDiceExpr = DiceNotationTests.TimeForDiceExpression("1d(1d12+4)+3", ITERATIONS_FOR_TIMING * 10);

            var timeForKeepRoll = DiceNotationTests.TimeForDiceRoll("5d6k2+3", ITERATIONS_FOR_TIMING * 10);
            var timeForKeepExpr = DiceNotationTests.TimeForDiceExpression("5d6k2+3", ITERATIONS_FOR_TIMING * 10);

            Console.WriteLine();
            Console.WriteLine($"Time to roll 1d6, 2d6+3, and 5d6k2+3, 1d(1d12+4)+3 dice {ITERATIONS_FOR_TIMING * 10} times: ");
            Console.WriteLine("\tRoll Method:");
            Console.WriteLine($"\t\t1d6         : {timeForSmallDiceRoll}");
            Console.WriteLine($"\t\t2d6+3       : {timeForMediumDiceRoll}");
            Console.WriteLine($"\t\t5d6k2+3      : {timeForKeepRoll}");
            Console.WriteLine($"\t\t1d(1d12+4)+3: {timeForLargeDiceRoll}");

            Console.WriteLine("\tParse Method:");
            Console.WriteLine($"\t\t1d6         : {timeForSmallDiceExpr}");
            Console.WriteLine($"\t\t2d6+3       : {timeForMediumDiceExpr}");
            Console.WriteLine($"\t\t5d6k2+3      : {timeForKeepExpr}");
            Console.WriteLine($"\t\t1d(1d12+4)+3: {timeForLargeDiceExpr}");
            Console.WriteLine();
        }

        private static void TestLightingNSource(int sources)
        {
            var timeMultipleLighting = LightingFOVTests.TimeForNSourcesLighting(MAP_WIDTH, MAP_HEIGHT, LIGHT_RADIUS,
                                                                                ITERATIONS_FOR_TIMING, sources);
            Console.WriteLine($"Time for {ITERATIONS_FOR_TIMING}, calc fov's, {sources} sources, {MAP_WIDTH}x{MAP_HEIGHT} map, Radius {LIGHT_RADIUS}:");
            Console.WriteLine($"\tLighting: {timeMultipleLighting.ToString()}");
        }
    }
}