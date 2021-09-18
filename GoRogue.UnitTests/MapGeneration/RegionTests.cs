using System;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.MapGeneration
{
    public sealed class RegionTests : IDisposable
    {
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
        private readonly Region _area;

        public RegionTests()
        {
            _area = new Region(_nw, _ne, _se, _sw);
        }

        [Fact]
        public void RegionTest()
        {
            Assert.Equal(18, _area.Area.OuterPoints.Count);
            Assert.Equal(7, _area.Area.InnerPoints.Count);
        }
        [Fact]
        public void ToStringOverrideTest()
        {
            var expected = "Region with 0 components and the following ";
            expected += _area.Area;
            Assert.Equal(expected, _area.ToString());
        }

        public void Dispose()
        {

        }
    }
}
