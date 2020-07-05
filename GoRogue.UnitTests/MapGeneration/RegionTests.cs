using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.MapGeneration
{
    public class RegionTests : IDisposable
    {
        private readonly Point sw = new Point(3, 4);
        private readonly Point nw = new Point(1, 1);
        private readonly Point ne = new Point(5, 0);
        private readonly Point se = new Point(7, 3);
        Region area;

        public RegionTests()
        {
            area = new Region("forbidden zone", se, ne, nw, sw);
        }
        [Fact]
        public void AreaTest()
        {
            Assert.Equal(14, area.OuterPoints.Count());
            Assert.Equal(22, area.InnerPoints.Count());
            Assert.Equal(5, area.NorthBoundary.Count());
            Assert.Equal(4, area.WestBoundary.Count());
            Assert.Equal(5, area.SouthBoundary.Count());
            Assert.Equal(4, area.EastBoundary.Count());
        }
        [Fact]
        public void ToStringOverrideTest()
        {
            Assert.Equal("forbidden zone", area.ToString());
        }
        [Fact]
        public void ContainsTest()
        {
            Region area = new Region("forbidden zone", se, ne, nw, sw);
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
            Assert.Equal(4, area.LeftAt(area.Top)); //the rise/run of the top left-right meetx y=0 at x=3
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

            nw = new Point(1, 1);
            se = new Point(5, 5);
            sw = new Point(1, 5);
            ne = new Point(5, 1);
            Region imposingSubArea = new Region("imposing sub area", ne: ne, nw: nw, se: se, sw: sw);

            nw = new Point(4, 4);
            se = new Point(8, 8);
            sw = new Point(4, 8);
            ne = new Point(8, 4);
            Region hostSubArea = new Region("host sub area", ne: ne, nw: nw, se: se, sw: sw);

            ((List<Region>)mainArea.SubRegions).Add(hostSubArea);
            ((List<Region>)mainArea.SubRegions).Add(imposingSubArea);

            mainArea.DistinguishSubRegions();
            hostSubArea = mainArea["imposing sub area"];
            imposingSubArea = mainArea["host sub area"];
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
        public void RemoveOverlappingOuterpointsTest()
        {
            Region a = Region.FromRectangle("Area A", new Rectangle(new Point(1, 1), new Point(3, 4)));
            Region b = Region.FromRectangle("Area B", new Rectangle(new Point(3, 0), new Point(6, 5)));

            int aCountBefore = a.OuterPoints.Count();
            int bCountBefore = b.OuterPoints.Count();
            a.RemoveOverlappingOuterpoints(b);
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
            a.RemoveOverlappingInnerpoints(b);
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
            Assert.Equal(b.OuterPoints.Count(), bCountBefore);
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
            Rectangle rectangle = new Rectangle(new Point(1, 1), new Point(32, 48));
            Region prior = new Region("bermuda triangle", new Point(14, 6), new Point(0, 1), new Point(0, 0), new Point(6, 14));
            Region copyOfPrior = new Region("bermuda triangle", new Point(14, 6), new Point(0, 1), new Point(0, 0), new Point(6, 14));
            Region post = prior.Rotate(degrees, false, new Point(6,14));
            Assert.Equal(prior.Bottom, post.Bottom);
            Assert.Equal(prior.SouthWestCorner, post.SouthWestCorner);
            Assert.True(prior.Left < post.Left);
            Assert.True(prior.Right < post.Right);
            Assert.True(prior.SouthEastCorner.X < post.SouthEastCorner.X);
            Assert.True(prior.SouthEastCorner.Y < post.SouthEastCorner.Y);

            //Assert.AreEqual(copyOfPrior, prior);

            //prior.Rotate(degrees, true, new Point(6, 14));

            //Assert.AreEqual(prior, post);
        }

        public void Dispose()
        {

        }
    }
}
