using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GoRogue.UnitTests.MapGeneration
{
    public class RegionTests : IDisposable
    {
        private readonly Point sw = new Point(3, 4);
        private readonly Point nw = new Point(1, 1);
        private readonly Point ne = new Point(5, 0);
        private readonly Point se = new Point(7, 3);
        Region area;

        private const int _width = 50;
        private const int _height = 50;
        private static readonly Point _start = (1, 2);
        private static readonly Point _end = (17, 14);
        private readonly ITestOutputHelper _output;


        private readonly Point start = new Point(1, 1);
        private readonly int width = 9;
        private readonly int height = 7;
        private readonly int rise = 1;
        private readonly int run = 4;
        private readonly double angleRadians = 0.25;

        public RegionTests(ITestOutputHelper output)
        {
            area = new Region("forbidden zone", se, ne, nw, sw);
            _output = output;
            PrintRegion(area);
        }

        private void PrintRegion(Region region)
        {
            var map = new char[region.Width + 1, region.Height + 1];
            _output.WriteLine(region.Name + ", Width: " + region.Width + ", Height: " + region.Height);

            for (int y = 0; y <= region.Height; y++)
            {
                string line = "";
                for (int x = 0; x <= region.Width; x++)
                {
                    Point here = new Point(x, y);
                    if (region.Connections.Contains(here))
                        map[x, y] = '+';
                    else if (region.OuterPoints.Contains(here))
                        map[x, y] = '#';
                    else if (region.InnerPoints.Contains(here))
                        map[x, y] = '.';
                    else
                        map[x, y] = ' ';
                    line += map[x, y];
                }
                _output.WriteLine(line);
            }
        }
        [Fact]
        public void RegionTest()
        {
            Assert.Equal(14, area.OuterPoints.Count());
            Assert.Equal(20, area.InnerPoints.Count());
            Assert.Equal(5, area.NorthBoundary.Count());
            Assert.Equal(4, area.WestBoundary.Count());
            Assert.Equal(5, area.SouthBoundary.Count());
            Assert.Equal(4, area.EastBoundary.Count());
        }
        [Fact]
        public void ToStringOverrideTest()
        {
            Assert.Equal("forbidden zone: NW(1,1)=> NE(5,0)=> SE(7,3)=> SW(3,4)", area.ToString());
        }
        [Fact]
        public void ContainsTest()
        {
            Assert.False(area.Contains(new Point(-5, -5)));
            Assert.False(area.Contains(new Point(1, 2)));
            Assert.False(area.Contains(new Point(9, 8)));
            Assert.False(area.Contains(new Point(6, 15)));
            Assert.True(area.Contains(new Point(2, 1)));
            Assert.True(area.Contains(new Point(4, 1)));
            Assert.True(area.Contains(new Point(6, 3)));
            Assert.True(area.Contains(new Point(3, 3)));
        }
        [Fact]
        public void OverlapTest()
        {
            Point tl = new Point(0, 0);
            Point tr = new Point(2, 0);
            Point br = new Point(2, 2);
            Point bl = new Point(0, 2);
            Region a2 = new Region("zone of terror", br, tr, tl, bl);
            PrintRegion(a2);
            List<Point> answer = area.Overlap(a2).ToList();

            foreach (Point c in answer)
            {
                Assert.True(area.Contains(c));
                Assert.True(a2.Contains(c));
            }
        }
        [Fact]
        public void LeftAtTest()
        {
            Assert.Equal(nw.X, area.LeftAt(nw.Y));
            Assert.Equal(4, area.LeftAt(area.Top));
            Assert.Equal(3, area.LeftAt(area.Bottom));
        }
        [Fact]
        public void RightAtTest()
        {
            Assert.Equal(ne.X, area.RightAt(ne.Y));
            Assert.Equal(5, area.RightAt(area.Top));
            Assert.Equal(5, area.RightAt(area.Bottom));
        }
        [Fact]
        public void TopAtTest()
        {
            Assert.Equal(ne.Y, area.TopAt(ne.X));
            Assert.Equal(nw.Y, area.TopAt(nw.X));
            Assert.Equal(0, area.TopAt(5));
        }
        [Fact]
        public void BottomAtTest()
        {
            Assert.Equal(se.Y, area.BottomAt(se.X));
            Assert.Equal(sw.Y, area.BottomAt(sw.X));
            Assert.Equal(4, area.BottomAt(5));
        }
        [Fact]
        public void TopTest()
        {
            Assert.Equal(0, area.Top);
        }
        [Fact]
        public void BottomTest()
        {
            Assert.Equal(4, area.Bottom);
        }
        [Fact]
        public void LeftTest()
        {
            Assert.Equal(1, area.Left);
        }
        [Fact]
        public void RightTest()
        {
            Assert.Equal(7, area.Right);
        }
        [Fact]
        public void ShiftWithParametersTest()
        {
            Point two = new Point(2, 2);
            Region a1 = area;
            Region a2 = area.Shift(two);

            foreach (Point inner in a2.InnerPoints)
                Assert.True(a1.Contains(inner - two));
        }

        [Fact]
        public void DistinguishSubAreasTest()
        {

            /* 0123456
             * XXXXXXX
             * X     X
             * X     X
             * X     X
             * X     Xxxx
             * X     X  x
             * XXXXXXX  x
             *    x     x
             *    xxxxxxx
             */
            Point nw = new Point(0, 0);
            Point sw = new Point(0, 9);
            Point se = new Point(9, 9);
            Point ne = new Point(9, 0);
            Region mainArea = new Region("parent area", ne: ne, nw: nw, se: se, sw: sw);
            PrintRegion(mainArea);
            nw = new Point(1, 1);
            se = new Point(5, 5);
            sw = new Point(1, 5);
            ne = new Point(5, 1);
            Region imposingSubArea = new Region("imposing sub area", ne: ne, nw: nw, se: se, sw: sw);
            PrintRegion(imposingSubArea);

            nw = new Point(4, 4);
            se = new Point(8, 8);
            sw = new Point(4, 8);
            ne = new Point(8, 4);
            Region hostSubArea = new Region("host sub area", ne: ne, nw: nw, se: se, sw: sw);
            PrintRegion(hostSubArea);

            mainArea.Add(hostSubArea);
            mainArea.Add(imposingSubArea);
            PrintRegion(mainArea);

            mainArea.DistinguishSubRegions();
            PrintRegion(mainArea);
            hostSubArea = mainArea.GetRegion("imposing sub area");
            imposingSubArea = mainArea.GetRegion("host sub area");
            foreach (Point c in imposingSubArea.InnerPoints)
            {
                Assert.True(mainArea.Contains(c), "Main area somehow had a Point removed.");
                Assert.False(hostSubArea.Contains(c), "sub area contains a Point in imposing area.");
            }
            foreach (Point c in hostSubArea.InnerPoints)
            {
                Assert.True(mainArea.Contains(c), "Main area somehow lost a Point.");
                Assert.False(imposingSubArea.Contains(c), "a Point should have been removed from the sub area but was not.");
            }
        }

        [Fact]
        public void AddConnectionBetweenTest()
        {
            Region a = Region.Rectangle("test-tangle", new Point(0, 0), 6, 6);
            Region b = Region.Rectangle("rest-ert", new Point(a.Right, a.Top), 5, 5);
            int aCountBefore = a.OuterPoints.Count();
            int bCountBefore = b.OuterPoints.Count();
            Region.AddConnectionBetween(a, b);
            Assert.Equal(1, a.Connections.Count());
            Assert.Equal(1, b.Connections.Count());

            Assert.Equal(aCountBefore - 1, a.OuterPoints.Count());
            Assert.Equal(bCountBefore - 1, b.OuterPoints.Count());

        }
        [Fact]
        public void RemoveOverlappingOuterPointsTest()
        {
            Region a = Region.FromRectangle("Area A", new Rectangle(new Point(1, 1), new Point(3, 4)));
            Region b = Region.FromRectangle("Area B", new Rectangle(new Point(3, 0), new Point(6, 5)));

            int aCountBefore = a.OuterPoints.Count();
            int bCountBefore = b.OuterPoints.Count();
            a.RemoveOverlappingOuterPoints(b);
            Assert.True(a.OuterPoints.Count() < aCountBefore);
            Assert.Equal(b.OuterPoints.Count(), bCountBefore);
        }
        [Fact]
        public void RemoveOverlappingInnerpointsTest()
        {
            Region a = Region.FromRectangle("Area A", new Rectangle(new Point(1, 1), new Point(3, 4)));
            Region b = Region.FromRectangle("Area B", new Rectangle(new Point(3, 0), new Point(6, 5)));

            int aCountBefore = a.OuterPoints.Count();
            int bCountBefore = b.OuterPoints.Count();
            a.RemoveOverlappingInnerPoints(b);
            Assert.True(a.OuterPoints.Count() < aCountBefore, "No connecting points were removed from Area A");
            Assert.Equal(b.OuterPoints.Count(), bCountBefore);
        }
        [Fact]
        public void RemoveOverlappingPointsTest()
        {
            Region a = Region.FromRectangle("Area A", new Rectangle(new Point(3, 3), new Point(5, 5)));
            Region b = Region.FromRectangle("Area B", new Rectangle(new Point(1, 1), new Point(7, 7)));

            int aCountBefore = a.OuterPoints.Count();
            int bCountBefore = b.OuterPoints.Count();
            a.RemoveOverlappingPoints(b);

            Assert.Empty(a.OuterPoints);
            Assert.Empty(a.InnerPoints);
            Assert.Equal(bCountBefore,b.OuterPoints.Count());
        }
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
            Point centerOfRotation = new Point(6,14);
            Region prior = new Region("bermuda triangle", new Point(14, 6), new Point(0, 1), new Point(0, 0), centerOfRotation);
            Region copyOfPrior = new Region("bermuda triangle", new Point(14, 6), new Point(0, 1), new Point(0, 0), centerOfRotation);
            Region post = prior.Rotate(degrees, false, centerOfRotation);
            Assert.Equal(prior.Bottom, post.Bottom);
            Assert.Equal(prior.SouthWestCorner, post.SouthWestCorner);
            Assert.True(prior.Left < post.Left);
            Assert.True(prior.Right < post.Right);
            Assert.True(prior.SouthEastCorner.X < post.SouthEastCorner.X);
            Assert.True(prior.SouthEastCorner.Y < post.SouthEastCorner.Y);

            Assert.Equal(copyOfPrior, prior);

            Region rightAnglePost = prior.Rotate(degrees * 2, false, centerOfRotation);
            post.Rotate(degrees, true, centerOfRotation);
            Assert.Equal(rightAnglePost, post);
        }

        [Fact]
        public void RectangleTest()
        {

            Region room = Region.Rectangle("my office", start, width, height, angleRadians);

            Point nw = room.NorthWestCorner;
            Point sw = room.SouthWestCorner;
            Point se = room.SouthEastCorner;
            Point ne = room.NorthEastCorner;

            Assert.Equal(nw, new Point(1, 1));

            Assert.True(nw.X > sw.X);
            Assert.True(sw.Y > nw.Y);
            Assert.True(se.X > sw.X);
            Assert.True(se.Y > sw.Y);
            Assert.True(ne.X > nw.X);
            Assert.True(ne.Y > nw.Y);
            Assert.True(ne.X > se.X);
            Assert.True(se.Y > ne.Y);

            double topDiff = Distance.Euclidean.Calculate(nw, ne);
            double rightDiff = Distance.Euclidean.Calculate(se, ne);
            double bottomDiff = Distance.Euclidean.Calculate(sw, se);
            double leftDiff = Distance.Euclidean.Calculate(nw, sw);
            Assert.Equal(topDiff, bottomDiff);
            Assert.Equal(leftDiff, rightDiff);
        }
        [Fact]
        public static void FromRectangleTest()
        {
            Rectangle rectangle = new Rectangle(new Point(1, 1), new Point(5, 5));
            Region area = Region.FromRectangle("square", rectangle);
            Assert.Equal(rectangle.Width + 1, area.Width);
            Assert.Equal(rectangle.Height + 1, area.Height);
            Assert.Equal(20, area.OuterPoints.Count());
            Assert.Equal(6, area.NorthBoundary.Count());
            Assert.Equal(6, area.SouthBoundary.Count());
            Assert.Equal(6, area.EastBoundary.Count());
            Assert.Equal(6, area.WestBoundary.Count());
        }
        [Fact]
        public void RegularParallelogramTest()
        {
            int length = 25;
            Point origin = new Point(length, length);
            Point horizontalBound = origin + new Point(length, 0);
            Point verticalBound = origin + new Point(0, length);
            Region parallelogram = Region.RegularParallelogram("My Parallelogram", origin, length, length, 0);
            Assert.True(parallelogram.Contains(origin), "Didn't contain the origin.");
            Assert.True(parallelogram.Contains(origin + length), "Didn't contain expected coordinate of " + (origin + length).ToString());
            Assert.True(parallelogram.Contains(horizontalBound), "Didn't contain expected top-right corner");
            Assert.False(parallelogram.Contains(origin + new Point(0, 2)), "Contained unexpected spot due south of the origin.");
        }

        [Fact]
        public void HasRegionTest()
        {
            Region house = new Region("house", ne: ne, nw: nw, se: se, sw: sw);

            house.Add(new Region("parlor", ne: ne, nw: nw, se: se, sw: sw));
            house.Add(new Region("ballroom", ne: ne, nw: nw, se: se, sw: sw));
            house.Add(new Region("kitchenette", ne: ne, nw: nw, se: se, sw: sw));
            Assert.False(house.HasRegion("house"));
            Assert.False(house.HasRegion("studio"));
            Assert.True(house.HasRegion("ballroom"));
            Assert.True(house.HasRegion("kitchenette"));
            Assert.True(house.HasRegion("parlor"));
        }

        [Fact]
        public void SubRegionsTest()
        {
            Region house = new Region("house", ne: ne, nw: nw, se: se, sw: sw);
            house.Add(new Region("parlor", ne: ne + 3, nw: nw + 1, se: se + 3, sw: sw + 2));
            Assert.Equal(1, house.SubRegions.Count);
            house.Add(new Region("hall", ne: ne + 3, nw: nw + 1, se: se + 3, sw: sw + 2));
            Assert.Equal(2, house.SubRegions.Count);
            house.Add(new Region("shitter", ne: ne + 3, nw: nw + 1, se: se + 3, sw: sw + 2));
            Assert.Equal(3, house.SubRegions.Count);

            Region expected = new Region("parlor", ne: ne + 3, nw: nw + 1, se: se + 3, sw: sw + 2);

            Assert.Equal(expected, house.GetRegion("parlor"));


            house.Remove("hall");
            Assert.Equal(2, house.SubRegions.Count);
        }
        public void Dispose()
        {

        }
    }
}
