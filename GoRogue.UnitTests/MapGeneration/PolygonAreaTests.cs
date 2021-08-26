using System.Collections.Generic;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.MapGeneration
{
    public class PolygonAreaTests
    {
        /*
         *  0 1 2 3 4 5 6 7 8
         * 0# - - - - #
         * 1- . . . . -
         * 2- . . . . -
         * 3- . . . . -
         * 4- . . . . -
         * 5- . . . . -
         * 6- . . . . -
         * 7- . . . . -
         * 8# - - - - #
         */
        PolygonArea _rectangle = new PolygonArea(new[] {new Point(0, 0), new Point(5, 0), new Point(5, 8), new Point(0, 8)});

        /* (roughly)
         *  0 1 2 3 4 5 6 7 8
         * 0# - -
         * 1- . . - -
         * 2- . . . . #
         * 3  - . . -
         * 4  - . . -
         * 5  - . -
         * 6   -..-
         * 7   -.-
         * 8    #
         */
        private PolygonArea _triangle = new PolygonArea(new[] {new Point(0, 0), new Point(8, 2), new Point(2, 8)});

        public static readonly IEnumerable<object[]> Algorithms = new List<object[]>()
        {
            new object[] {Lines.Algorithm.BresenhamOrdered},
            new object[] {Lines.Algorithm.Bresenham},
            new object[] {Lines.Algorithm.DDA},
            new object[] {Lines.Algorithm.Orthogonal},
        };

            [Fact]
        public void PolygonAreaTest()
        {
            Assert.Equal(4, _rectangle.Corners.Count);
            Assert.Equal(30, _rectangle.OuterPoints.Count);
            Assert.Equal(28, _rectangle.InnerPoints.Count);

            Assert.Equal(3, _triangle.Corners.Count);
            Assert.Equal(25, _triangle.OuterPoints.Count);
            Assert.Equal(22, _triangle.InnerPoints.Count);
        }

        [Theory]
        [MemberData(nameof(Algorithms))]
        public void PolygonAreaUsesSpecifiedAlgorithmTest(Lines.Algorithm algorithm)
        {
            var p1 = new Point(1, 1);
            var p2 = new Point(12, 5);
            var p3 = new Point(10, 7);
            var p4 = new Point(7, 9);
            var p5 = new Point(5, 7);

            var polygon = new PolygonArea(new[] {p1, p2, p3, p4, p5}, algorithm);
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p1,p2, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p2,p3, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p3,p4, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p4,p5, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p5,p1, algorithm))));

        }
    }
}
