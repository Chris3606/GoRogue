using System;
using System.Collections.Generic;
using GoRogue.DiceNotation;
using GoRogue.FOV;
using GoRogue.MapGeneration;
using GoRogue.Pathing;
using GoRogue.SenseMapping;
using GoRogue.SpatialMaps;
using GoRogue.UnitTests.Mocks;
using GoRogue.UnitTests.SenseMapping;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;
using Xunit;
using Xunit.Abstractions;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests
{
    public class ManualTestHelpers
    {
        public ManualTestHelpers(ITestOutputHelper output) => _output = output;

        private const int Width = 50;
        private const int Height = 50;
        private static readonly Point s_start = (1, 2);
        private static readonly Point s_end = (17, 14);
        private readonly ITestOutputHelper _output;

        public static Distance[] Distances = { Distance.Chebyshev, Distance.Euclidean, Distance.Manhattan };

        [Fact]
        public void ManualPrintLayeredSpatialMap()
        {
            var map = new LayeredSpatialMap<MockSpatialMapItem>(3, startingLayer: 1,
                layersSupportingMultipleItems: LayerMasker.Default.Mask(2))
            {
                { new MockSpatialMapItem(1), (1, 2) },
                { new MockSpatialMapItem(1), (3, 4) },
                { new MockSpatialMapItem(2), (1, 1) },
                { new MockSpatialMapItem(3), (0, 0) }
            };


            _output.WriteLine("LayeredSpatialMap: ");
            _output.WriteLine(map.ToString());
        }

        [Theory]
        [MemberDataEnumerable(nameof(Distances))]
        public void ManualPrintAStarPaths(Distance distanceCalc)
        {
            var map = MockMaps.Rectangle(Width, Height);

            var pathfinder = new AStar(map, distanceCalc);
            var path = pathfinder.ShortestPath(s_start, s_end);
            TestUtils.NotNull(path);

            TestUtils.PrintHighlightedPoints(map, path.StepsWithStart);

            foreach (var point in path.StepsWithStart)
                _output.WriteLine(point.ToString());
        }

        [Fact]
        public void ManualPrintGoalMap()
        {
            var map = MockMaps.Rectangle(Width, Height);

            var stateMap = new ArrayView<GoalState>(map.Width, map.Height);
            foreach (var pos in stateMap.Positions())
                stateMap[pos] = map[pos] ? GoalState.Clear : GoalState.Obstacle;

            stateMap[Width / 2, Height / 2] = GoalState.Goal;
            stateMap[Width / 2 + 5, Height / 2 + 5] = GoalState.Goal;

            var goalMap = new GoalMap(stateMap, Distance.Euclidean);
            goalMap.Update();

            _output.WriteLine(goalMap.ToString(5, "0.00"));
        }

        [Fact]
        public void ManualPrintDungeonMazeMap()
        {
            var rng = new MizuchiRandom(12345);

            var generator = new Generator(40, 30);
            generator.ConfigAndGenerateSafe(gen =>
            {
                gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps(rng, saveDeadEndChance: 10));
            });

            var wallFloorMap = generator.Context.GetFirstOrDefault<ISettableGridView<bool>>("WallFloor");
            Assert.NotNull(wallFloorMap);

            _output.WriteLine("Generated map: ");
            _output.WriteLine(wallFloorMap!.ExtendToString(elementStringifier: val => val ? "." : "#"));
        }

        [Fact]
        public void ManualPrint2DArray()
        {
            var array = new int[10, 10];
            var i = 0;

            for (var x = 0; x < 10; x++)
                for (var y = 0; y < 10; y++)
                {
                    array[x, y] = i;
                    i++;
                }

            _output.WriteLine(array.ExtendToString());
            _output.WriteLine("\nIn Grid:");
            _output.WriteLine(array.ExtendToStringGrid());
            _output.WriteLine("\nIn Grid with 5 field size:");
            _output.WriteLine(array.ExtendToStringGrid(5));
            _output.WriteLine("\nIn Grid with 5 field size left-align:");
            _output.WriteLine(array.ExtendToStringGrid(-5));
        }

        [Fact]
        public void ManualPrintAdjacencyRule()
        {
            _output.WriteLine(AdjacencyRule.Cardinals.ToString());
            _output.WriteLine(AdjacencyRule.EightWay.ToString());
            _output.WriteLine(AdjacencyRule.Diagonals.ToString());
        }

        [Fact]
        public void ManualPrintDiceExpression()
        {
            var expr = Dice.Parse("1d(1d12+4)+5*3");
            _output.WriteLine(expr.ToString());
        }

        [Fact]
        public void ManualPrintDictionary()
        {
#pragma warning disable CA2244 // Duplicate indexed element initializations; performed on purpose here
            var myDict = new Dictionary<int, string> { [1] = "On", [3] = "Three", [1] = "One", [2] = "Two" };
#pragma warning restore CA2244 // Duplicate indexed element initializations

            _output.WriteLine(myDict.ExtendToString());
        }

        [Fact]
        public void ManualPrintDisjointSet()
        {
            var s = new DisjointSet(5);

            s.MakeUnion(1, 3);
            s.MakeUnion(2, 4);

            _output.WriteLine(s.ToString());
        }

        [Fact]
        public void ManualPrintFOV()
        {
            var map = MockMaps.Rectangle(Width, Height);
            var myFov = new RecursiveShadowcastingFOV(map);
            myFov.Calculate(5, 5, 3);

            _output.WriteLine(myFov.ToString());
            _output.WriteLine("");
            _output.WriteLine(myFov.ToString(3));
        }

        [Fact]
        public void ManualPrintList()
        {
            var myList = new List<int> { 1, 1, 3, 2 };

            _output.WriteLine(myList.ExtendToString());
            _output.WriteLine("\nWith bar separators:");
            _output.WriteLine(myList.ExtendToString(separator: " | "));
        }

        [Fact]
        public void ManualPrintMockMap()
        {
            ISettableGridView<bool> map = (ArrayView<bool>)MockMaps.Rectangle(Width, Height);
            _output.WriteLine(map.ToString());
        }

        [Fact]
        public void ManualPrintMultiSpatialMap()
        {
            var sm = new MultiSpatialMap<MyIDImpl>
            {
                    { new MyIDImpl(1), 1, 2 },
                    { new MyIDImpl(2), 1, 2 },
                    { new MyIDImpl(3), 4, 5 }
            };

            _output.WriteLine(sm.ToString());
        }

        [Fact]
        public void ManualPrintPath()
        {
            var map = MockMaps.Rectangle(Width, Height);
            var pather = new AStar(map, Distance.Manhattan);
            var path = pather.ShortestPath(1, 2, 5, 6);

            _output.WriteLine(path?.ToString() ?? throw new InvalidOperationException("Should be a path."));
        }

        [Fact]
        public void ManualPrintRadius()
        {
            _output.WriteLine(Radius.Circle.ToString());
            _output.WriteLine(Radius.Square.ToString());
            _output.WriteLine(Radius.Diamond.ToString());
        }

        [Fact]
        public void ManualPrintRadiusLocationContext()
        {
            var boundlessAreaProv = new RadiusLocationContext((5, 4), 10);
            var boundedAreaProv = new RadiusLocationContext((5, 4), 10, new Rectangle(0, 0, 10, 10));

            _output.WriteLine(boundlessAreaProv + " is boundless!");
            _output.WriteLine(boundedAreaProv + " is bounded...");
        }

        [Fact]
        public void ManualPrintRectangle()
        {
            var rect = new Rectangle(0, 0, 10, 10);
            _output.WriteLine(rect.ToString());
        }

        [Fact]
        public void ManualPrintSenseMap()
        {
            var map = MockMaps.Rectangle(Width, Height);

            var resMap = new ResMap(map);
            var senseMap = new SenseMap(resMap);

            var source = AlgorithmFactory.CreateSenseSource(SourceType.Shadow, (12, 15), 10, Radius.Circle);
            var source2 = AlgorithmFactory.CreateSenseSource(SourceType.Shadow, (18, 15), 10, Radius.Circle);
            senseMap.AddSenseSource(source);
            senseMap.AddSenseSource(source2);

            senseMap.Calculate();

            _output.WriteLine(senseMap.ToString());
            _output.WriteLine("");
            _output.WriteLine(senseMap.ToString(3));
            _output.WriteLine("");
            _output.WriteLine(source.ToString());
        }

        [Fact]
        public void ManualPrintSet()
        {
            var mySet = new HashSet<int> { 1, 1, 3, 2 };

            _output.WriteLine(mySet.ExtendToString());
            _output.WriteLine("\nWith bar separators:");
            _output.WriteLine(mySet.ExtendToString(separator: " | "));
        }

        [Fact]
        public void ManualPrintSpatialMap()
        {
            var sm = new SpatialMap<MyIDImpl>
            {
                    { new MyIDImpl(1), 1, 2 },
                    { new MyIDImpl(2), 1, 3 },
                    { new MyIDImpl(3), 4, 5 }
            };

            _output.WriteLine(sm.ToString());
        }
    }
}
