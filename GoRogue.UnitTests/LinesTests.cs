using System.Collections.Generic;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests
{
    public class LineTests
    {
        private readonly List<Point> _hardCodedRange = new List<Point>();

        private readonly Point _sw = new Point(3, 4);
        private readonly Point _nw = new Point(1, 1);
        private readonly Point _ne = new Point(5, 0);
        private readonly Point _se = new Point(7, 3);

        public LineTests()
        {
            _hardCodedRange.AddRange(SadRogue.Primitives.Lines.GetLine(_nw, _ne));
            _hardCodedRange.AddRange(SadRogue.Primitives.Lines.GetLine(_ne, _se));
            _hardCodedRange.AddRange(SadRogue.Primitives.Lines.GetLine(_se, _sw));
            _hardCodedRange.AddRange(SadRogue.Primitives.Lines.GetLine(_sw, _nw));
        }

        [Fact]
        public void LeftAtTest()
        {
            Assert.Equal(_nw.X, _hardCodedRange.LeftAt(_nw.Y));
            Assert.Equal(3, _hardCodedRange.LeftAt(0));
            Assert.Equal(3, _hardCodedRange.LeftAt(4));
        }
        [Fact]
        public void RightAtTest()
        {
            Assert.Equal(_ne.X, _hardCodedRange.RightAt(_ne.Y));
            Assert.Equal(_se.X, _hardCodedRange.RightAt(_se.Y));
        }
        [Fact]
        public void TopAtTest()
        {
            Assert.Equal(_ne.Y, _hardCodedRange.TopAt(_ne.X));
            Assert.Equal(_nw.Y, _hardCodedRange.TopAt(_nw.X));
        }
        [Fact]
        public void BottomAtTest()
        {
            Assert.Equal(_se.Y, _hardCodedRange.BottomAt(_se.X));
            Assert.Equal(_sw.Y, _hardCodedRange.BottomAt(_sw.X));
        }
    }
}
