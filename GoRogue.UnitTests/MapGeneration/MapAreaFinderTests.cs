using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using SadRogue.Primitives;
using Xunit; //using GoRogue.UnitTests.Mocks;

namespace GoRogue.UnitTests.MapGeneration
{
    public class MapAreaFinderTests
    {
        //private const int width = 50;
        //private const int height = 50;

        public static readonly List<(IMapView<bool>, AdjacencyRule, int)> TestData =
            new List<(IMapView<bool>, AdjacencyRule, int)>();

        /*
        private readonly AdjacencyRule[] _adjacencies =
        {
            AdjacencyRule.Cardinals, AdjacencyRule.Diagonals, AdjacencyRule.EightWay
        };

        private readonly int[] _expected =
        {
            1, 1, 1, //single rectangle
            2, 2, 2, //single cardinal
            2, 1, 1, //single diagonal
            4, 4, 4, //double cardinal
            4, 1, 1, //double diagonal
            5, 5, 5 //disconnected squares
        };

        private readonly IMapView<bool>[] _maps =
        {
            MockFactory.Rectangle(width, height), MockFactory.CardinalBisection(width, height, 1),
            MockFactory.DiagonalBisection(width, height, 1), MockFactory.CardinalBisection(width, height, 2),
            MockFactory.DiagonalBisection(width, height, 2), MockFactory.DisconnectedSquares(width, height)
        };
        */

        /*
        public MapAreaFinderTests()
        {
            var i = 0;
            foreach (var map in _maps)
                foreach (var rule in _adjacencies)
                {
                    //... combinate?
                }
        }
        */

        //[Theory]
        //[MemberData(nameof(TestData))]
        public void MapAreasTest(IMapView<bool> map, AdjacencyRule adjacency, int expected)
        {
            var maf = new MapAreaFinder(map, adjacency);
            var answer = maf.MapAreas().ToList();

            Assert.Equal(expected, answer.Count);
        }
    }
}
