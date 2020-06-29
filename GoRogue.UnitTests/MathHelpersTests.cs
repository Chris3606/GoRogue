using Xunit;

namespace GoRogue.UnitTests
{
    public class MathHelperTests
    {
        [Fact]
        public void RoundToMultiple()
        {
            var result = MathHelpers.RoundToMultiple(3, 3);
            Assert.Equal(3, result);

            result = MathHelpers.RoundToMultiple(2, 3);
            Assert.Equal(3, result);

            result = MathHelpers.RoundToMultiple(4, 3);
            Assert.Equal(6, result);

            result = MathHelpers.RoundToMultiple(-1, 3);
            Assert.Equal(0, result);
        }
    }
}
