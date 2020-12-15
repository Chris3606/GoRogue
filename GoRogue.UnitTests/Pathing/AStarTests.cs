using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using GoRogue.Pathing;
using GoRogue.UnitTests.Mocks;
using Roy_T.AStar.Graphs;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Xunit;
using Xunit.Abstractions;
using XUnit.ValueTuples;
using Distance = SadRogue.Primitives.Distance;

namespace GoRogue.UnitTests.Pathing
{
    public class AStarTests
    {
        private static readonly ISettableGridView<bool> _basicMap = MockMaps.Rectangle(100, 70);
        private static readonly Point _basicMapStart = (1, 2);
        private static readonly Point _basicMapEnd = (17, 14);

        private static readonly (bool assumeEndpointsWalkable, bool startWalkable, bool endWalkable, bool expectValidPath)[] _endpointWalkableCases =
        {
            (true, true, false, true),
            (true, false, true, true),
            (true, false, false, true),
            (true, true, true, true),
            (false, true, false, false),
            (false, false, true, false),
            (false, false, false, false),
            (false, true, true, true)
        };

        public static IEnumerable<(bool, bool, bool, bool, Distance)> EndpointWalkableCases
            => _endpointWalkableCases.Combinate(
                TestUtils.Enumerable(Distance.Chebyshev, Distance.Euclidean, Distance.Manhattan));

        private static readonly IGridView<bool>[] _testMaps =
        {
            MockMaps.Rectangle(50, 35),
            new Generator(50, 35)
                .AddSteps(DefaultAlgorithms.DungeonMazeMapSteps())
                .Generate()
                .Context.GetFirstOrDefault<IGridView<bool>>() ?? throw new InvalidOperationException("Null map."),
        };

        // Number of paths generated/tested against per test map
        private const int _pathsPerMap = 10;

        private static IEnumerable<List<(Point start, Point end)>> RandomTestPoints
        {
            get
            {
                foreach (var item in _testMaps)
                {
                    var testPoints = new List<(Point start, Point end)>();
                    for (int i = 0; i < _pathsPerMap; i++)
                    {
                        Point start = item.RandomPosition(true);
                        Point end = start;
                        while (end == start)
                            end = item.RandomPosition(true);

                        testPoints.Add((start, end));
                    }

                    yield return testPoints;
                }
            }
        }

        public static IEnumerable<(IGridView<bool>, Point, Point, Distance)> TestPaths
        {
            get
            {
                var mapAndPoints = new List<(IGridView<bool>, Point, Point)>();
                foreach (var (map, testPoints) in _testMaps.Zip(RandomTestPoints))
                    foreach (var (start, end) in testPoints)
                        mapAndPoints.Add((map, start, end));

                return mapAndPoints.Combinate(TestUtils.Enumerable(Distance.Chebyshev, Distance.Euclidean, Distance.Manhattan));
            }
        }

        private readonly ITestOutputHelper _output;

        public AStarTests(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void PathReversing()
        {
            // Because Path constructor is internal to avoid confusion, we use AStar to return a
            // path object instead of creating one manually
            var pathfinder = new AStar(_basicMap, Distance.Chebyshev);

            var actualPath = pathfinder.ShortestPath(_basicMapStart, _basicMapEnd);
            var expectedPath = new List<Point>();

            // Must be a valid path
            TestUtils.NotNull(actualPath);
            Assert.True(actualPath.Length > 0);

            // Add path values to a list
            expectedPath.AddRange(actualPath.StepsWithStart);

            expectedPath.Reverse();
            actualPath.Reverse();

            Assert.Equal(expectedPath, actualPath.StepsWithStart);
        }

        // Tests that the endpoints are assumed walkable IFF the appropriate flag is set.
        [Theory]
        [MemberDataTuple(nameof(EndpointWalkableCases))]
        public void EndpointsWalkableAssumptions(bool assumeEndpointsWalkable,
                                                 bool startWalkable,
                                                 bool endWalkable,
                                                 bool expectValidPath,
                                                 Distance distanceCalc)
        {
            var oldStart = _basicMap[_basicMapStart];
            var oldEnd = _basicMap[_basicMapEnd];

            var pathfinder = new AStar(_basicMap, distanceCalc);

            // Set walkability map appropriately, create path, and assert expected result
            _basicMap[_basicMapStart] = startWalkable;
            _basicMap[_basicMapEnd] = endWalkable;
            var path = pathfinder.ShortestPath(_basicMapStart, _basicMapEnd, assumeEndpointsWalkable);
            if (expectValidPath)
            {
                TestUtils.NotNull(path);
                Assert.True(path.Length > 0);
            }
            else
                Assert.Null(path);

            // Reset walkability map for next test
            _basicMap[_basicMapStart] = oldStart;
            _basicMap[_basicMapEnd] = oldEnd;
        }

        [Theory]
        [MemberDataTuple(nameof(TestPaths))]
        public void ProducesAdjacentAndWalkablePaths(IGridView<bool> map, Point start, Point end, Distance distanceCalc)
        {
            // Print out map so we can replicate it
            _output.WriteLine("Map:");
            _output.WriteLine(map.ExtendToString(elementStringifier: val => val ? "." : "#"));

            // Create pathfinder and calculate shortest path
            var pathfinder = new AStar(map, distanceCalc);
            var path = pathfinder.ShortestPath(start, end);

            // Test cases must have valid paths
            TestUtils.NotNull(path);
            Assert.True(path.Length > 0);

            // All values in the path must be adjacent.  We do not compare the actual distance, as Euclidean
            // would weight diagonals as >1, but they still must be among the adjacent (connected) nodes.
            foreach (var (item, index) in path.StepsWithStart.Enumerate())
            {
                // Must be walkable
                Assert.True(map[item]);

                // There isn't a next value to check so don't check adjacency
                if (index == path.LengthWithStart - 1)
                    continue;

                // Otherwise, make sure this value is adjacent to the next one.
                Assert.Contains(path.GetStepWithStart(index + 1),
                       ((AdjacencyRule)distanceCalc).Neighbors(item));
            }
        }

        [Theory]
        [MemberDataTuple(nameof(TestPaths))]
        public void ProducesShortestPaths(IGridView<bool> map, Point start, Point end, Distance distanceCalc)
        {
            // Calculate velocities to represent potential edge weights
            var velocity = Velocity.FromMetersPerSecond(1);
            var diagonalVelocity = distanceCalc.Type switch
            {
                Distance.Types.Manhattan => Velocity.FromMetersPerSecond(0),
                Distance.Types.Chebyshev => Velocity.FromMetersPerSecond(1),
                Distance.Types.Euclidean => Velocity.FromMetersPerSecond(
                    (float)(1.0 / Distance.Euclidean.Calculate((0, 0), (1, 1)))),
                _ => throw new NotSupportedException("Test failed to support all given distance types.")
            };

            // Create RoyT graph equivalent the map we're testing
            var graph = new ArrayView<Node>(map.Width, map.Height);
            foreach (var pos in graph.Positions())
                graph[pos] = new Node(new Position(pos.X, pos.Y));

            foreach (var pos in map.Positions())
                if (map[pos])
                    foreach (var neighborDir in ((AdjacencyRule)distanceCalc).DirectionsOfNeighbors())
                    {
                        var neighbor = pos + neighborDir;
                        if (!map[neighbor])
                            continue;

                        if (neighborDir.IsCardinal())
                        {
                            graph[pos].Connect(graph[neighbor], velocity);
                            graph[neighbor].Connect(graph[pos], velocity);
                        }
                        else
                        {
                            graph[pos].Connect(graph[neighbor], diagonalVelocity);
                            graph[neighbor].Connect(graph[pos], diagonalVelocity);
                        }
                    }

            // Calculate path with RoyT
            var pathFinder = new PathFinder();
            var path = pathFinder.FindPath(graph[start], graph[end], velocity);

            // calculate path with GoRogue
            var grPathfinder = new AStar(map, distanceCalc);
            var grPath = grPathfinder.ShortestPath(start, end);

            // All test points must produce a valid path
            TestUtils.NotNull(grPath);
            Assert.True(grPath.Length > 0);

            _output.WriteLine("Map:");
            _output.WriteLine(map.ExtendToString(elementStringifier: val => val ? "." : "#"));

            _output.WriteLine("\nRoyT Path:");
            _output.WriteLine(path.Edges.ExtendToString());

            _output.WriteLine("\nGoRogue Path:");
            _output.WriteLine(grPath.ToString());

            // The paths won't be the same path (there are very often multiple equally shortest paths).
            // Additionally, because RoyT uses the euclidean heuristic, it will produce some non-shortest
            // paths for some cases.  But we verify that our paths are adjacent and only over walkable terrain
            // in other tests, so as long as well at least as short as RoyT's path, it should be correct.
            // Since RoyT returns steps (pairs of points), their length is equivalent to our length WITHOUT
            // the start.
            Assert.True(grPath.Length <= path.Edges.Count);
        }
    }
}
