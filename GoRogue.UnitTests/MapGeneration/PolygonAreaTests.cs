using System;
using System.Collections.Generic;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.MapGeneration
{
    public class PolygonAreaTests
    {
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
        private readonly Point _triPointOne = (0, 0);
        private readonly Point _triPointTwo = (8, 2);
        private readonly Point _triPointThree = (2, 8);

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
        private readonly Point _rectNorthWest = (0, 0);
        private readonly Point _rectNorthEast = (5, 0);
        private readonly Point _rectSouthWest = (0, 8);
        private readonly Point _rectSouthEast = (5, 8);

        /*
         * Hand-Calculated
         */
        private readonly Point _pentPointOne = (0, 4);
        private readonly Point _pentPointTwo = (8, 3);
        private readonly Point _pentPointThree = (7, 8);
        private readonly Point _pentPointFour = (2, 8);
        private readonly Point _pentPointFive = (0, 3);

        /*
         * Hand-Calculated
         */
        private readonly Point _hexPointOne = (9, 9);
        private readonly Point _hexPointTwo = (12, 11);
        private readonly Point _hexPointThree = (12, 14);
        private readonly Point _hexPointFour = (9, 16);
        private readonly Point _hexPointFive = (6, 14);
        private readonly Point _hexPointSix = (6, 11);

        /*
         * Hand-Calculated
         */
        private readonly Point[] _initialCorners = new[]
        {
            new Point(1,1),new Point(6,1),new Point(6,3),new Point(4,3),new Point(5,6),
            new Point(5,7),new Point(4,7),new Point(4,12),new Point(13,12),new Point(10,11),
            new Point(8,9),new Point(7,7),new Point(7,6),new Point(8,4),new Point(10,2),
            new Point(13,1),new Point(15,2),new Point(16,4),new Point(11,4),new Point(10,7),
            new Point(11,9),new Point(16,9),new Point(15,11),new Point(15,13),new Point(1,13),
        };

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
        public void TriangularPolygonAreaTest()
        {
            var triangle = new PolygonArea(Lines.Algorithm.DDA, _triPointOne, _triPointTwo, _triPointThree);
            Assert.Equal(3, triangle.Corners.Count);
            Assert.Equal(25, triangle.OuterPoints.Count);
            Assert.Equal(20, triangle.InnerPoints.Count);
        }

        [Fact]
        public void RectangularPolygonAreaTest()
        {
            var rectangle = new PolygonArea(Lines.Algorithm.DDA, _rectNorthEast, _rectNorthWest, _rectSouthWest, _rectSouthEast);
            Assert.Equal(4, rectangle.Corners.Count);
            Assert.Equal(30, rectangle.OuterPoints.Count);
            Assert.Equal(28, rectangle.InnerPoints.Count);
        }

        [Fact]
        public void PentagonalPolygonAreaTest()
        {
            var pentagon = new PolygonArea(Lines.Algorithm.DDA, _pentPointOne, _pentPointTwo, _pentPointThree,
                _pentPointFour, _pentPointFive);
            Assert.Equal(5, pentagon.Corners.Count);
            Assert.Equal(29, pentagon.OuterPoints.Count);
            Assert.Equal(18, pentagon.InnerPoints.Count);
        }

        [Fact]
        public void HexagonalPolygonAreaTest()
        {
            var hexagon = new PolygonArea(Lines.Algorithm.DDA, _hexPointOne, _hexPointTwo, _hexPointThree, _hexPointFour,
                _hexPointFive, _hexPointSix);
            Assert.Equal(6, hexagon.Corners.Count);
            Assert.Equal(24, hexagon.OuterPoints.Count);
            Assert.Equal(22, hexagon.InnerPoints.Count);
        }

        [Fact]
        public void ComplexPolygonAreaTest()
        {
            var initials = new PolygonArea(_initialCorners);
            Assert.Equal(25, initials.Corners.Count);
            Assert.Equal(117, initials.OuterPoints.Count);
            Assert.Equal(56, initials.InnerPoints.Count);
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
