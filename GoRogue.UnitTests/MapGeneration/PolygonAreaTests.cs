using System;
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
        private PolygonArea _rectangle;
        private readonly Point _rectNorthWest = (0, 0);
        private readonly Point _rectNorthEast = (5, 0);
        private readonly Point _rectSouthWest = (0, 8);
        private readonly Point _rectSouthEast = (5, 8);

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
        private PolygonArea _triangle;
        private readonly Point _triPointOne = (0, 0);
        private readonly Point _triPointTwo = (8, 2);
        private readonly Point _triPointThree = (2, 8);


        public PolygonAreaTests()
        {
            _rectangle = new PolygonArea(Lines.Algorithm.DDA, _rectNorthEast, _rectNorthWest, _rectSouthWest, _rectSouthEast);
            _triangle = new PolygonArea(Lines.Algorithm.DDA, _triPointOne, _triPointTwo, _triPointThree);
        }


        public static readonly IEnumerable<object[]> OrderedAlgorithms = new List<object[]>()
        {
            new object[] {Lines.Algorithm.BresenhamOrdered},
            new object[] {Lines.Algorithm.DDA},
        };
        public static readonly IEnumerable<object[]> UnorderedAlgorithms = new List<object[]>()
        {
            new object[] {Lines.Algorithm.Orthogonal},
            new object[] {Lines.Algorithm.Bresenham},
        };

        [Fact]
        public void PolygonAreaTest()
        {
            Assert.Equal(4, _rectangle.Corners.Count);
            Assert.Equal(30, _rectangle.OuterPoints.Count);
            Assert.Equal(28, _rectangle.InnerPoints.Count);

            Assert.Equal(3, _triangle.Corners.Count);
            Assert.Equal(25, _triangle.OuterPoints.Count);
            Assert.Equal(20, _triangle.InnerPoints.Count);
        }

        [Theory]
        [MemberData(nameof(OrderedAlgorithms))]
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

        [Theory]
        [MemberData(nameof(UnorderedAlgorithms))]
        public void PolygonAlgorithmsMustBeOrderedTest(Lines.Algorithm algorithm)
        {
            var p1 = new Point(1, 1);
            var p2 = new Point(12, 5);
            var p3 = new Point(10, 7);

            Assert.Throws<ArgumentException>(() => new PolygonArea(algorithm, p1, p2, p3));

        }
    }
}
