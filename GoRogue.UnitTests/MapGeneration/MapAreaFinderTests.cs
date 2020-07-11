using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit;
using Xunit.Abstractions;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.MapGeneration
{
    public class MapAreaFinderTests
    {
        private const int _width = 50;
        private const int _height = 50;

        private static readonly IMapView<bool>[] _maps =
        {
            MockMaps.Rectangle(_width, _height), MockMaps.CardinalBisection(_width, _height, 1),
            MockMaps.DiagonalBisection(_width, _height, 1), MockMaps.CardinalBisection(_width, _height, 2),
            MockMaps.DiagonalBisection(_width, _height, 2), MockMaps.DisconnectedSquares(_width, _height)
        };

        private static readonly (AdjacencyRule rule, int expectedAreas)[][] _expectedAreas =
        {
            // Single rectangle bisection
            new[] { (AdjacencyRule.Cardinals, 1), (AdjacencyRule.Diagonals, 2), (AdjacencyRule.EightWay, 1) },
            // Single cardinal bisection
            new[] { (AdjacencyRule.Cardinals, 2), (AdjacencyRule.Diagonals, 4), (AdjacencyRule.EightWay, 2) },
            // Single diagonal bisection
            new[] { (AdjacencyRule.Cardinals, 2), (AdjacencyRule.Diagonals, 3), (AdjacencyRule.EightWay, 1) },
            // Double cardinal bisection
            new[] { (AdjacencyRule.Cardinals, 4), (AdjacencyRule.Diagonals, 8), (AdjacencyRule.EightWay, 4) },
            // Double diagonal bisection
            new[] { (AdjacencyRule.Cardinals, 4), (AdjacencyRule.Diagonals, 4), (AdjacencyRule.EightWay, 1) },
            // Disconnected squares
            new[] { (AdjacencyRule.Cardinals, 25), (AdjacencyRule.Diagonals, 50), (AdjacencyRule.EightWay, 25) }
        };

        private readonly ITestOutputHelper _output;

        public MapAreaFinderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public static IEnumerable<(IMapView<bool>, AdjacencyRule, int)> TestData
        {
            get
            {
                Assert.Equal(_maps.Length, _expectedAreas.Length);
                foreach (var (item, index) in _maps.Enumerate())
                    foreach (var (rule, expectedAreas) in _expectedAreas[index])
                        yield return (item, rule, expectedAreas);
            }
        }

        [Theory]
        [MemberDataTuple(nameof(TestData))]
        public void MapAreasTest(IMapView<bool> map, AdjacencyRule adjacency, int expected)
        {
            _output.WriteLine("Map used:");
            _output.WriteLine(map.ExtendToString(elementStringifier: val => val ? "." : "#"));

            var maf = new MapAreaFinder(map, adjacency);
            var answer = maf.MapAreas().ToList();

            Assert.Equal(expected, answer.Count);
        }
    }
}
