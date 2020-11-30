using System;
using System.Collections.Generic;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.MapGeneration
{
    public sealed class RegionTests : IDisposable
    {
        private readonly Point _sw = new Point(3, 4);
        private readonly Point _nw = new Point(1, 1);
        private readonly Point _ne = new Point(5, 0);
        private readonly Point _se = new Point(7, 3);
        private readonly Region _area;


        private readonly Point _start = new Point(1, 1);
        //private static readonly Point _end = (17, 14);
        private const int _width = 9;
        private const int _height = 7;

        //private readonly int rise = 1;
        //private readonly int run = 4;
        private const int _degrees = 22;

        public RegionTests()
        {
            _area = new Region("forbidden zone", _nw, _ne, _se, _sw);
        }

        [Fact]
        public void RegionTest()
        {
            Assert.Equal(14, _area.OuterPoints.Count);
            Assert.Equal(20, _area.InnerPoints.Count);
            Assert.Equal(5, _area.NorthBoundary.Count);
            Assert.Equal(4, _area.WestBoundary.Count);
            Assert.Equal(5, _area.SouthBoundary.Count);
            Assert.Equal(4, _area.EastBoundary.Count);
        }
        [Fact]
        public void ToStringOverrideTest()
        {
            Assert.Equal("forbidden zone: NW(1,1)=> NE(5,0)=> SE(7,3)=> SW(3,4)", _area.ToString());
        }
        [Fact]
        public void ContainsTest()
        {
            Assert.False(_area.Contains(new Point(-5, -5)));
            Assert.False(_area.Contains(new Point(1, 2)));
            Assert.False(_area.Contains(new Point(9, 8)));
            Assert.False(_area.Contains(new Point(6, 15)));
            Assert.True(_area.Contains(new Point(2, 1)));
            Assert.True(_area.Contains(new Point(4, 1)));
            Assert.True(_area.Contains(new Point(6, 3)));
            Assert.True(_area.Contains(new Point(3, 3)));
        }
        [Fact]
        public void OverlapTest()
        {
            Point tl = new Point(0, 0);
            Point tr = new Point(2, 0);
            Point br = new Point(2, 2);
            Point bl = new Point(0, 2);
            Region a2 = new Region("zone of terror", tl, tr, br, bl);
            IEnumerable<Point> answer = _area.Overlap(a2);

            foreach (Point c in answer)
            {
                Assert.True(_area.Contains(c));
                Assert.True(a2.Contains(c));
            }
        }
        [Fact]
        public void LeftAtTest()
        {
            Assert.Equal(_nw.X, _area.LeftAt(_nw.Y));
            Assert.Equal(4, _area.LeftAt(_area.Top));
            Assert.Equal(3, _area.LeftAt(_area.Bottom));
        }
        [Fact]
        public void RightAtTest()
        {
            Assert.Equal(_ne.X, _area.RightAt(_ne.Y));
            Assert.Equal(5, _area.RightAt(_area.Top));
            Assert.Equal(5, _area.RightAt(_area.Bottom));
        }
        [Fact]
        public void TopAtTest()
        {
            Assert.Equal(_ne.Y, _area.TopAt(_ne.X));
            Assert.Equal(_nw.Y, _area.TopAt(_nw.X));
            Assert.Equal(0, _area.TopAt(5));
        }
        [Fact]
        public void BottomAtTest()
        {
            Assert.Equal(_se.Y, _area.BottomAt(_se.X));
            Assert.Equal(_sw.Y, _area.BottomAt(_sw.X));
            Assert.Equal(4, _area.BottomAt(5));
        }
        [Fact]
        public void TopTest()
        {
            Assert.Equal(0, _area.Top);
        }
        [Fact]
        public void BottomTest()
        {
            Assert.Equal(4, _area.Bottom);
        }
        [Fact]
        public void LeftTest()
        {
            Assert.Equal(1, _area.Left);
        }
        [Fact]
        public void RightTest()
        {
            Assert.Equal(7, _area.Right);
        }
        [Fact]
        public void ShiftWithParametersTest()
        {
            Point two = new Point(2, 2);
            Region a1 = _area;
            Region a2 = _area.Shift(two);

            foreach (Point inner in a2.InnerPoints.Positions)
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
            Region mainArea = new Region("parent area", northWest: nw, northEast: ne, southEast: se, southWest: sw);

            nw = new Point(1, 1);
            se = new Point(5, 5);
            sw = new Point(1, 5);
            ne = new Point(5, 1);
            Region imposingSubArea = new Region("imposing sub area", northWest: nw, northEast: ne, southEast: se, southWest: sw);

            nw = new Point(4, 4);
            se = new Point(8, 8);
            sw = new Point(4, 8);
            ne = new Point(8, 4);
            Region hostSubArea = new Region("host sub area", northWest: nw, northEast: ne, southEast: se, southWest: sw);

            mainArea.AddSubRegion(hostSubArea);
            mainArea.AddSubRegion(imposingSubArea);

            mainArea.DistinguishSubRegions();
            hostSubArea = mainArea.GetSubRegion("imposing sub area");
            imposingSubArea = mainArea.GetSubRegion("host sub area");

            foreach (Point c in imposingSubArea.InnerPoints.Positions)
            {
                Assert.True(mainArea.Contains(c), "Main area somehow had a Point removed.");
                Assert.False(hostSubArea.Contains(c), "sub area contains a Point in imposing area.");
            }
            foreach (Point c in hostSubArea.InnerPoints.Positions)
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
            int aCountBefore = a.OuterPoints.Count;
            int bCountBefore = b.OuterPoints.Count;
            Region.AddConnectionBetween(a, b);
            Assert.Equal(1, a.Connections.Count);
            Assert.Equal(1, b.Connections.Count);

            Assert.Equal(aCountBefore - 1, a.OuterPoints.Count);
            Assert.Equal(bCountBefore - 1, b.OuterPoints.Count);

        }
        [Fact]
        public void RemoveOverlappingOuterPointsTest()
        {
            Region a = Region.FromRectangle("Area A", new Rectangle(new Point(1, 1), new Point(3, 4)));
            Region b = Region.FromRectangle("Area B", new Rectangle(new Point(3, 0), new Point(6, 5)));

            int aCountBefore = a.OuterPoints.Count;
            int bCountBefore = b.OuterPoints.Count;
            a.RemoveOverlappingOuterPoints(b);
            Assert.True(a.OuterPoints.Count < aCountBefore);
            Assert.Equal(b.OuterPoints.Count, bCountBefore);
        }
        [Fact]
        public void RemoveOverlappingInnerpointsTest()
        {
            Region a = Region.FromRectangle("Area A", new Rectangle(new Point(1, 1), new Point(3, 4)));
            Region b = Region.FromRectangle("Area B", new Rectangle(new Point(3, 0), new Point(6, 5)));

            int aCountBefore = a.OuterPoints.Count;
            int bCountBefore = b.OuterPoints.Count;
            a.RemoveOverlappingInnerPoints(b);
            Assert.True(a.OuterPoints.Count < aCountBefore, "No connecting points were removed from Area A");
            Assert.Equal(b.OuterPoints.Count, bCountBefore);
        }
        [Fact]
        public void RemoveOverlappingPointsTest()
        {
            Region a = Region.FromRectangle("Area A", new Rectangle(new Point(3, 3), new Point(5, 5)));
            Region b = Region.FromRectangle("Area B", new Rectangle(new Point(1, 1), new Point(7, 7)));

            int bCountBefore = b.OuterPoints.Count;
            a.RemoveOverlappingPoints(b);

            Assert.Empty(a.OuterPoints.Positions);
            Assert.Empty(a.InnerPoints.Positions);
            Assert.Equal(bCountBefore,b.OuterPoints.Count);
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
            Region prior = new Region("bermuda triangle", new Point(0, 0), new Point(0, 1), new Point(14, 6), centerOfRotation);
            Region copyOfPrior = new Region("bermuda triangle", new Point(0, 0), new Point(0, 1), new Point(14, 6), centerOfRotation);
            Region post = prior.Rotate(degrees, centerOfRotation);
            Assert.Equal(prior.Bottom, post.Bottom);
            Assert.Equal(prior.SouthWestCorner, post.SouthWestCorner);
            Assert.True(prior.Left < post.Left);
            Assert.True(prior.Right < post.Right);
            Assert.True(prior.SouthEastCorner.X < post.SouthEastCorner.X);
            Assert.True(prior.SouthEastCorner.Y < post.SouthEastCorner.Y);

            Assert.True(copyOfPrior.Matches(prior));
        }

        [Fact]
        public void RectangleTest()
        {

            Region room = Region.Rectangle("my office", _start, _width, _height, _degrees);

            Point nw = room.NorthWestCorner;
            Point sw = room.SouthWestCorner;
            Point se = room.SouthEastCorner;
            Point ne = room.NorthEastCorner;

            Assert.Equal(new Point(1, 1), nw);

            Assert.True(nw.X > sw.X);
            Assert.True(sw.Y > nw.Y);
            Assert.True(se.X > sw.X);
            Assert.True(se.Y > sw.Y);
            Assert.True(ne.X > nw.X);
            Assert.True(ne.Y > nw.Y);
            Assert.True(ne.X > se.X);
            Assert.True(se.Y > ne.Y);
        }
        [Fact]
        public static void FromRectangleTest()
        {
            Rectangle rectangle = new Rectangle(new Point(1, 1), new Point(5, 5));
            Region area = Region.FromRectangle("square", rectangle);
            Assert.Equal(rectangle.Width + 1, area.Width);
            Assert.Equal(rectangle.Height + 1, area.Height);
            Assert.Equal(20, area.OuterPoints.Count);
            Assert.Equal(6, area.NorthBoundary.Count);
            Assert.Equal(6, area.SouthBoundary.Count);
            Assert.Equal(6, area.EastBoundary.Count);
            Assert.Equal(6, area.WestBoundary.Count);
        }
        [Fact]
        public void RegularParallelogramTest()
        {
            const int length = 25;
            Point origin = new Point(length, length);
            Point horizontalBound = origin + new Point(length, 0);
            Region parallelogram = Region.RegularParallelogram("My Parallelogram", origin, length, length, 0);
            Assert.True(parallelogram.Contains(origin), "Didn't contain the origin.");
            Assert.True(parallelogram.Contains(origin + length), "Didn't contain expected coordinate of " + (origin + length).ToString());
            Assert.True(parallelogram.Contains(horizontalBound), "Didn't contain expected top-right corner");
            Assert.False(parallelogram.Contains(origin + new Point(0, 2)), "Contained unexpected spot due south of the origin.");
        }

        [Fact]
        public void HasRegionTest()
        {
            Region house = new Region("house", northWest: _nw, northEast: _ne, southEast: _se, southWest: _sw);

            house.AddSubRegion(new Region("parlor", northWest: _nw, northEast: _ne, southEast: _se, southWest: _sw));
            house.AddSubRegion(new Region("ballroom", northWest: _nw, northEast: _ne, southEast: _se, southWest: _sw));
            house.AddSubRegion(new Region("kitchenette", northWest: _nw, northEast: _ne, southEast: _se, southWest: _sw));
            Assert.False(house.HasSubRegion("house"));
            Assert.False(house.HasSubRegion("studio"));
            Assert.True(house.HasSubRegion("ballroom"));
            Assert.True(house.HasSubRegion("kitchenette"));
            Assert.True(house.HasSubRegion("parlor"));
        }

        [Fact]
        public void InnerFromOuterPointsTest()
        {
            Assert.Equal(20, _area.InnerPoints.Positions.Count);
        }

        [Fact]
        public void SubRegionsTest()
        {
            Region house = new Region("house", northWest: _nw, northEast: _ne, southEast: _se, southWest: _sw);
            house.AddSubRegion(new Region("parlor", northWest: _nw + 1, northEast: _ne + 3, southEast: _se + 3, southWest: _sw + 2));
            Assert.Equal(1, house.SubRegions.Count);
            house.AddSubRegion(new Region("hall", northWest: _nw + 1, northEast: _ne + 3, southEast: _se + 3, southWest: _sw + 2));
            Assert.Equal(2, house.SubRegions.Count);
            house.AddSubRegion(new Region("shitter", northWest: _nw + 1, northEast: _ne + 3, southEast: _se + 3, southWest: _sw + 2));
            Assert.Equal(3, house.SubRegions.Count);

            Region expected = new Region("parlor", northWest: _nw + 1, northEast: _ne + 3, southEast: _se + 3, southWest: _sw + 2);

            Assert.True(expected.Matches(house.GetSubRegion("parlor")));


            house.RemoveSubRegion("hall");
            Assert.Equal(2, house.SubRegions.Count);
        }
        public void Dispose()
        {

        }
    }
}
