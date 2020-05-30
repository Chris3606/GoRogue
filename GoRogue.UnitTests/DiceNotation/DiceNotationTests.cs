using GoRogue.DiceNotation;
using Xunit;

namespace GoRogue.UnitTests.DiceNotation
{
    public class DiceNotationTests
    {
        [Fact]
        public void AdvancedDice()
        {
            var expr = Dice.Parse("1d(1d12+4)+3");
            assertMinMaxValues(expr, 4, 19);
            assertReturnedInRange(expr, 4, 19);
        }

        [Fact]
        public void KeepDiceAdd()
        {
            var expr = Dice.Parse("5d6k2+3");
            assertMinMaxValues(expr, 5, 15);
            assertReturnedInRange(expr, 5, 15);
        }

        [Fact]
        public void MultipleDice()
        {
            var expr = Dice.Parse("2d6");
            assertMinMaxValues(expr, 2, 12);
            assertReturnedInRange(expr, 2, 12);
        }

        [Fact]
        public void MultipleDiceAdd()
        {
            var expr = Dice.Parse("2d6+3");
            assertMinMaxValues(expr, 5, 15);
            assertReturnedInRange(expr, 5, 15);
        }

        [Fact]
        public void MultipleDiceAddMultiply()
        {
            var expr = Dice.Parse("(2d6+2)*3");
            assertMinMaxValues(expr, 12, 42);
            assertReturnedInRange(expr, 12, 42);
        }

        [Fact]
        public void MultipleDiceMultiply()
        {
            var expr = Dice.Parse("3*2d6");
            assertMinMaxValues(expr, 6, 36);
            assertReturnedInRange(expr, 6, 36);
        }

        [Fact]
        public void SingleDice()
        {
            var expr = Dice.Parse("1d6");
            assertMinMaxValues(expr, 1, 6);
            assertReturnedInRange(expr, 1, 6);
        }

        [Fact]
        public void SingleDiceAdd()
        {
            var expr = Dice.Parse("1d6+3");
            assertMinMaxValues(expr, 4, 9);
            assertReturnedInRange(expr, 4, 9);
        }

        [Fact]
        public void SingleDiceAddMultiply()
        {
            var expr = Dice.Parse("3*(1d6+2)");
            assertMinMaxValues(expr, 9, 24);
            assertReturnedInRange(expr, 9, 24);
        }

        [Fact]
        public void SingleDiceMultiply()
        {
            var expr = Dice.Parse("1d6*3");
            assertMinMaxValues(expr, 3, 18);
            assertReturnedInRange(expr, 3, 18);
        }

        private void assertMinMaxValues(IDiceExpression expr, int min, int max)
        {
            Assert.Equal(min, expr.MinRoll());
            Assert.Equal(max, expr.MaxRoll());
        }

        private void assertReturnedInRange(IDiceExpression expr, int min, int max)
        {
            for (var i = 0; i < 100; i++)
            {
                var result = expr.Roll();

                var inRange = result >= min && result <= max;
                Assert.Equal(true, inRange);
            }
        }
    }
}
