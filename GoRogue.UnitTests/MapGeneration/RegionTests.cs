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

        public RegionTests()
        {
            _area = new Region(_nw, _ne, _se, _sw);
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
            Assert.Equal("Region: NW(1,1)=> NE(5,0)=> SE(7,3)=> SW(3,4)", _area.ToString());
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
            Region prior = new Region(new Point(0, 0), new Point(0, 1), new Point(14, 6), centerOfRotation);
            Region copyOfPrior = new Region(new Point(0, 0), new Point(0, 1), new Point(14, 6), centerOfRotation);
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
        public void InnerFromOuterPointsTest()
        {
            Assert.Equal(20, _area.InnerPoints.Count);
        }

        public void Dispose()
        {

        }
    }
}
