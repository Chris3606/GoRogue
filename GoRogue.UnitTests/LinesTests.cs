using System;
using System.Collections.Generic;
using System.Linq;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests
{
    public class LineTests
    {
        private static readonly (int, int) _end = (8, 6);
        private const int _mapHeight = 10;
        private const int _mapWidth = 10;
        private static readonly (int, int) _start = (1, 1);
        List<Point> _hardCodedRange = new List<Point>();

        private readonly Point sw = new Point(3, 4);
        private readonly Point nw = new Point(1, 1);
        private readonly Point ne = new Point(5, 0);
        private readonly Point se = new Point(7, 3);

        private readonly Rectangle _orderingTests = new Rectangle(1, 2, 20, 15);

        public LineTests()
        {
            _hardCodedRange.AddRange(SadRogue.Primitives.Lines.GetLine(nw, ne));
            _hardCodedRange.AddRange(SadRogue.Primitives.Lines.GetLine(ne, se));
            _hardCodedRange.AddRange(SadRogue.Primitives.Lines.GetLine(se, sw));
            _hardCodedRange.AddRange(SadRogue.Primitives.Lines.GetLine(sw, nw));
        }

        [Fact]
        public void LeftAtTest()
        {
            Assert.Equal(nw.X, _hardCodedRange.LeftAt(nw.Y));
            Assert.Equal(3, _hardCodedRange.LeftAt(0));
            Assert.Equal(3, _hardCodedRange.LeftAt(4));
        }
        [Fact]
        public void RightAtTest()
        {
            Assert.Equal(ne.X, _hardCodedRange.RightAt(ne.Y));
            Assert.Equal(se.X, _hardCodedRange.RightAt(se.Y));
        }
        [Fact]
        public void TopAtTest()
        {
            Assert.Equal(ne.Y, _hardCodedRange.TopAt(ne.X));
            Assert.Equal(nw.Y, _hardCodedRange.TopAt(nw.X));
        }
        [Fact]
        public void BottomAtTest()
        {
            Assert.Equal(se.Y, _hardCodedRange.BottomAt(se.X));
            Assert.Equal(sw.Y, _hardCodedRange.BottomAt(sw.X));
        }
    }
}
