using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using Xunit;
using Xunit.Abstractions;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.MapGeneration
{
    public class PolygonAreaTests
    {
        #region testdata
        /*   0 1 2 3 4 5 6 7
         * 0        /--*
         * 1   *+--+    \
         * 2     \       \
         * 3      \    +--+*
         * 4       *--/
         * 5
         */
        private readonly Point _sw = new Point(3, 4);
        private readonly Point _nw = new Point(1, 1);
        private readonly Point _ne = new Point(5, 0);
        private readonly Point _se = new Point(7, 3);
        private readonly PolygonArea _area;

        private readonly ITestOutputHelper _output;

        public PolygonAreaTests(ITestOutputHelper output)
        {
            _area = new PolygonArea(_nw, _ne, _se, _sw);
            _output = output;
        }

        public readonly struct PolygonTestCase
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

        public static readonly IEnumerable<int> PolygonPointCount = new List<int>
            {5, 8, 9, 16, 17, 31, 32, 33, 47, 48, 49, 64, 65, 72, 73, 89, 90, 91, 101, 239, 347, 409};

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

        private string GetPolygonString(PolygonArea region)
        {
            var bounds = region.Bounds;
            var final = new StringBuilder();

            // Generate x scale
            final.Append(' ');
            for (int i = bounds.MinExtentX; i < bounds.MaxExtentX; i++)
                final.Append($" {i + bounds.MinExtentX}");
            final.Append('\n');

            HashSet<Point> corners = new HashSet<Point>(region.Corners);

            for (int y = bounds.MinExtentY; y <= bounds.MaxExtentY; y++)
            {
                final.Append($"{y + bounds.MinExtentY}");
                for (int x = bounds.MinExtentX; x <= bounds.MaxExtentX; x++)
                {
                    if (corners.Contains((x, y)))
                        final.Append('#');
                    else if (region.OuterPoints.Contains((x, y)))
                        final.Append('+');
                    else if (region.InnerPoints.Contains((x, y)))
                        final.Append('.');
                    else
                        final.Append(' ');
                }

                final.Append('\n');
            }

            return final.ToString();
        }

        #endregion

        [Theory]
        [MemberDataEnumerable(nameof(PolygonTestCases))]
        public void PolygonSanityCheck(PolygonTestCase testCase)
        {
            var polygon = new PolygonArea(Lines.Algorithm.DDA, testCase.Corners);
            Assert.Equal(testCase.CornerCount, polygon.Corners.Count);
            Assert.Equal(testCase.ExpectedOuterPoints, polygon.OuterPoints.Count);
            Assert.Equal(testCase.ExpectedInnerPoints, polygon.InnerPoints.Count);
            _output.WriteLine(GetPolygonString(polygon));
        }

        [Fact]
        public void TopTest() => Assert.Equal(0, _area.Top);

        [Fact]
        public void BottomTest() => Assert.Equal(4, _area.Bottom);
        [Fact]
        public void LeftTest() => Assert.Equal(1, _area.Left);

        [Fact]
        public void RightTest() => Assert.Equal(7, _area.Right);


        #region creation tests
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
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p1, p2, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p2, p3, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p3, p4, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p4, p5, algorithm))));
            Assert.True(polygon.OuterPoints.Contains(new Area(Lines.Get(p5, p1, algorithm))));
            _output.WriteLine(GetPolygonString(polygon));
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

        [Fact]
        public void PolygonFromRectangle()
        {
            var rect = new Rectangle(0, 0, 15, 10);
            var polygon = PolygonArea.Rectangle(rect);

            //Each Corner is included in two boundaries, so total number of points is offset by 4
            Assert.Equal(rect.Area + 4, polygon.Count);
            Assert.Equal(rect.PerimeterPositions().Count() + 4, polygon.OuterPoints.Count);
            Assert.Equal(new HashSet<Point>(rect.Expand(-1, -1).Positions()), new HashSet<Point>(polygon.InnerPoints));
            _output.WriteLine(GetPolygonString(polygon));
        }

        [Fact]
        public void PolygonParallelogram()
        {
            var polygon = PolygonArea.Parallelogram((30, 30), 10, 15);
            _output.WriteLine(GetPolygonString(polygon));
            Assert.Equal(4, polygon.Corners.Count);
            Assert.Equal(126, polygon.InnerPoints.Count);
            Assert.Equal(54, polygon.OuterPoints.Count);
            _output.WriteLine(GetPolygonString(polygon));
        }

        private void AssertPolygonSidesAreEqual(PolygonArea polygon, bool isStar)
        {
            var outer = polygon.OuterPoints;

            //we need to track duplicates as well so that we can sort them and find the median
            var allInts = new List<int>();
            var intsEncountered = new Dictionary<int, int>();

            int total = 0;
            var divisor = outer.SubAreas.Count;

            foreach (var boundary in outer.SubAreas)
            {
                var count = boundary.Count;
                total += count;
                //if we haven't seen it, track it once.
                if (!intsEncountered.ContainsKey(count))
                    intsEncountered.Add(count, 1);

                //otherwise, increase the amount of times we've seen it
                else
                    intsEncountered[count]++;

                allInts.Add(count);
            }

            var mean = total / divisor;

            allInts.Sort();
            var median = allInts[allInts.Count / 2];
            //now we're finished using allInts

            var mode = intsEncountered.OrderByDescending(kvp => kvp.Value).First().Key;
            var variance = mode / 4;
            variance = variance > 2 ? variance : 2;

            string message = isStar ? "Regular Star" : "Regular Polygon";
            var cornerCount = isStar ? polygon.Corners.Count / 2 : polygon.Corners.Count;

            message += $" with {cornerCount} corners has a lopside. \r\n";
            message += $"Mean: {mean}\r\nMedian: {median}\r\nMode: {mode}\r\nHas a side of length: ";

            foreach (var i in intsEncountered.Keys)
            {
                bool withinMode = i >= mode - variance && i <= mode + variance;
                bool withinMedian = i >= median - variance && i <= median + variance;
                Assert.True(withinMode || withinMedian, message + i);
            }
        }

        [Theory]
        [MemberDataEnumerable(nameof(PolygonPointCount))]
        public void RegularPolygonTest(int cornerAmount)
        {
            var polygon = PolygonArea.RegularPolygon((0, 0), cornerAmount, 300);
            AssertPolygonSidesAreEqual(polygon, false);
        }


        [Theory]
        [MemberDataEnumerable(nameof(PolygonPointCount))]
        public void RegularStarTest(int cornerAmount)
        {
            var polygon = PolygonArea.RegularStar((0, 0), cornerAmount, 300, 150);
            AssertPolygonSidesAreEqual(polygon, true);
        }

        #endregion
        #region transformation tests
        [Fact]
        public void RotateTest()
        {
            /* (0,0 & 0,1)
             * ###
             * #  ##
             * #    ##
             *  #     ##
             *  #       ##
             *   #        ##
             *   #          ## (14, 6)
             *    #         #
             *    #        #
             *     #      #
             *     #     #
             *      #   #
             *      #  #
             *       ##
             *       # (6, 14)
             */
            float degrees = 45.0f;
            Point centerOfRotation = new Point(6, 14);
            PolygonArea prior = new PolygonArea(new Point(0, 0), new Point(0, 1), new Point(14, 6), centerOfRotation);
            PolygonArea copyOfPrior = new PolygonArea(new Point(0, 0), new Point(0, 1), new Point(14, 6), centerOfRotation);
            PolygonArea post = prior.Rotate(degrees, centerOfRotation);

            _output.WriteLine("\nRotated Region:");
            _output.WriteLine(GetPolygonString(post));

            Assert.Equal(prior.Bottom, post.Bottom);
            Assert.True(prior.Left < post.Left);
            Assert.True(prior.Right < post.Right);
            Assert.True(copyOfPrior.Matches(prior));
        }

        [Fact] //around 0
        public void FlipVerticalTest()
        {
            var newArea = _area.FlipVertical(0);
            _output.WriteLine("\nOriginal Region:");
            _output.WriteLine(GetPolygonString(_area));
            _output.WriteLine("\nVertically Flipped Region:");
            _output.WriteLine(GetPolygonString(newArea));

            //north-south values have flipped and became negative
            Assert.Contains((_sw.X, -_sw.Y), newArea.Corners);
            Assert.Contains((_ne.X, -_ne.Y), newArea.Corners);
            Assert.Contains((_nw.X, -_nw.Y), newArea.Corners);
            Assert.Contains((_se.X, -_se.Y), newArea.Corners);
        }

        [Fact] //around 0
        public void FlipHorizontalTest()
        {
            var newArea = _area.FlipHorizontal(0);
            _output.WriteLine("\nHorizontally Flipped Region:");
            _output.WriteLine(GetPolygonString(newArea));

            // east-west values should have reversed and became negative
            Assert.Contains((-_sw.X, _sw.Y), newArea.Corners);
            Assert.Contains((-_ne.X, _ne.Y), newArea.Corners);
            Assert.Contains((-_nw.X, _nw.Y), newArea.Corners);
            Assert.Contains((-_se.X, _se.Y), newArea.Corners);
        }

        [Fact]
        public void TransposeTest() //around (0,0)
        {
            var transposed = _area.Transpose(0, 0);
            _output.WriteLine("\nTransposed Region:");
            _output.WriteLine(GetPolygonString(transposed));

            //should have same number of corners
            Assert.Equal(4, transposed.Corners.Count);

            //ne, se, sw reverse
            Assert.Contains((_ne.Y, _ne.X), transposed.Corners);
            Assert.Contains((_sw.Y, _sw.X), transposed.Corners);
            Assert.Contains((_se.Y, _se.X), transposed.Corners);
            Assert.Contains((_nw.Y, _nw.X), transposed.Corners);
        }

        [Fact]
        public void TranslateTest()
        {
            var translated = _area.Translate(2, 3);
            _output.WriteLine("\nTranslated Region:");
            _output.WriteLine(GetPolygonString(translated));

            Assert.Contains(_ne + (2, 3), translated.Corners);
            Assert.Contains(_nw + (2, 3), translated.Corners);
            Assert.Contains(_se + (2, 3), translated.Corners);
            Assert.Contains(_sw + (2, 3), translated.Corners);
        }
        #endregion

        #region Comparison Tests

        [Fact]
        public void MatchesTest()
        {
            // Two identical polygons (separate instances)
            var poly1 = new PolygonArea((0, 1), (5, 0), (1, 2));
            var poly2 = new PolygonArea((0, 1), (5, 0), (1, 2));

            // Equivalent polygon just with the corners defined using a different starting point
            var poly3 = new PolygonArea((5, 0), (1, 2), (0, 1));

            // Not equivalent polygon
            var poly4 = new PolygonArea((5, 0), (0, 1), (1, 2));

            Assert.True(poly1.Matches(poly2));
            Assert.True(poly1.Matches(poly3));

            Assert.False(poly1.Matches(poly4));

            // Repeat the tests with the objects casted to an interface to ensure the comparison propagates
            var area1 = poly1 as IReadOnlyArea;
            var area2 = poly2 as IReadOnlyArea;
            var area3 = poly3 as IReadOnlyArea;
            var area4 = poly4 as IReadOnlyArea;

            Assert.True(area1.Matches(area2));
            Assert.True(area1.Matches(area3));

            Assert.False(area1.Matches(area4));
        }
        #endregion

    }
}
