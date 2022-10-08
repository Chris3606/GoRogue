using System;
using GoRogue.Random;
using SadRogue.Primitives;
using ShaiRandom.Generators;
using Xunit;

namespace GoRogue.UnitTests.Random
{
    public class GeneratorExtensionsTests
    {
        [Fact]
        public void RectangleRandomPositionsValid()
        {
            var rect = new Rectangle(1, 2, 8, 5);

            var p1 = MinRandom.Instance.RandomPosition(rect);
            Assert.True(rect.Contains(p1));
            Assert.Equal(rect.MinExtent, p1);

            var p2 = MaxRandom.Instance.RandomPosition(rect);
            Assert.True(rect.Contains(p2));
            Assert.Equal(rect.MaxExtent, p2);
        }

        [Fact]
        public void RectangleRandomPositionsEmptyRectangle()
        {
            var r1 = new Rectangle(1, 2, 0, 1);
            Assert.Throws<ArgumentException>(() => GlobalRandom.DefaultRNG.RandomPosition(r1));

            var r2 = new Rectangle(1, 2, 1, 0);
            Assert.Throws<ArgumentException>(() => GlobalRandom.DefaultRNG.RandomPosition(r2));
        }

        [Fact]
        public void RectangleRandomPositionsRetriesExceeded()
        {
            var rect = new Rectangle(1, 2, 8, 5);

            Assert.Throws<MaxAttemptsReachedException>(()
                => GlobalRandom.DefaultRNG.RandomPosition(rect, _ => false, 10));
        }
    }
}
