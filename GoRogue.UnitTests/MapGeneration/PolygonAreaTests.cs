using System;
using System.Collections.Generic;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.MapGeneration
{
    public class PolygonAreaTests
    {
        public struct PolygonTestCase
        {
            public int CornerCount => Corners.Length;
            public readonly int ExpectedOuterPoints;
            public readonly int ExpectedInnerPoints;
            public readonly Point[] Corners;

            public PolygonTestCase(int expectedOuterPoints, int expectedInnerPoints,
                params Point[] corners)
            {
                ExpectedOuterPoints = expectedOuterPoints;
                ExpectedInnerPoints = expectedInnerPoints;
                Corners = corners;
            }
        }

        public static readonly IEnumerable<Lines.Algorithm> OrderedAlgorithms = new List<Lines.Algorithm>()
        {
            Lines.Algorithm.BresenhamOrdered,
            Lines.Algorithm.DDA,
        };

        public static readonly IEnumerable<Lines.Algorithm> UnorderedAlgorithms = new List<Lines.Algorithm>
        {
            Lines.Algorithm.Orthogonal,
            Lines.Algorithm.Bresenham,
        };

        //All points lovingly calculated by hand
        public static readonly IEnumerable<PolygonTestCase> PolygonTestCases = new List<PolygonTestCase>
        {
            //triangle
            new PolygonTestCase(25, 20, (0,0), (8,2), (2,8)),

            //rectangle
            new PolygonTestCase(30, 28, (0,0), (5,0), (5,8), (0,8)),

            //pentagon
            new PolygonTestCase(29, 18, (0, 4), (8, 3), (7, 8), (2, 8), (0, 3)),

            //hexagon
            new PolygonTestCase(24, 22, (9, 9), (12, 11),(12, 14),(9, 16),(6, 14), (6, 11)),

            //my initials
            new PolygonTestCase(117, 56, (1,1),(6,1),(6,3),(4,3),(5,6),
                (5,7),(4,7),(4,12),(13,12),(10,11), (8,9),(7,7),(7,6),(8,4),(10,2), (13,1),(15,2),(16,4),
                (11,4),(10,7), (11,9),(16,9),(15,11),(15,13),(1,13))
        };

        [Theory]
        [MemberDataEnumerable(nameof(PolygonTestCases))]
        public void PolygonSanityCheck(PolygonTestCase testCase)
        {
            var polygon = new PolygonArea(Lines.Algorithm.DDA, testCase.Corners);
            Assert.Equal(testCase.CornerCount, polygon.Corners.Count);
            Assert.Equal(testCase.ExpectedOuterPoints, polygon.OuterPoints.Count);
            Assert.Equal(testCase.ExpectedInnerPoints, polygon.InnerPoints.Count);
        }

        [Theory]
        [MemberDataEnumerable(nameof(OrderedAlgorithms))]
        public void PolygonAreaUsesSpecifiedAlgorithmTest(Lines.Algorithm algorithm)
        {
            var p1 = new Point(1, 1);
            var p2 = new Point(12, 5);
            var p3 = new Point(10, 7);
            var p4 = new Point(7, 9);
            var p5 = new Point(5, 7);

            var polygon = new PolygonArea(algorithm, p1, p2, p3, p4, p5);
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p1,p2, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p2,p3, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p3,p4, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p4,p5, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p5,p1, algorithm))));
        }

        [Theory]
        [MemberDataEnumerable(nameof(UnorderedAlgorithms))]
        public void PolygonAlgorithmsMustBeOrderedTest(Lines.Algorithm algorithm)
        {
            var p1 = new Point(1, 1);
            var p2 = new Point(12, 5);
            var p3 = new Point(10, 7);

            Assert.Throws<ArgumentException>(() => new PolygonArea(algorithm, p1, p2, p3));
        }
    }
}
