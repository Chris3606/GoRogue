using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.MapGeneration
{
    public class MapAreaFinderTests
    {
        public static readonly List<(MockMap, AdjacencyRule, int)> TestData = new List<(MockMap, AdjacencyRule, int)>()
        {

        };
        public readonly MockMap[] Maps =
        {
            new MockMap(),
            new MockMap().CardinalBisection(1),
            new MockMap().DiagonalBisection(1),
            new MockMap().CardinalBisection(2),
            new MockMap().DiagonalBisection(2),
            new MockMap().DisconnectedSquares(),
            new MockMap().Spiral(),
        };

        public readonly AdjacencyRule[] Adjacencies =
        {
            AdjacencyRule.Cardinals,
            AdjacencyRule.Diagonals,
            AdjacencyRule.EightWay
        };

        public MapAreaFinderTests()
        {
            foreach(MockMap map in Maps)
            {
                foreach(AdjacencyRule rule in Adjacencies)
                {

                }
            }
        }


        //[Theory]
        //[MemberData(nameof(TestData))]
        public void MapAreasTest(MockMap map, AdjacencyRule adjacency)
        {
            map.SetExpectations(adjacency);
            MapAreaFinder maf = new MapAreaFinder(map.WalkabilityView, adjacency);
            List<Area> answer = maf.MapAreas().ToList();
            //expected areas lovingly crafted by hand
            Assert.Equal(1, answer.Count);
        }
    }
}
