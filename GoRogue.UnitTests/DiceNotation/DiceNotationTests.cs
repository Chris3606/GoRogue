using GoRogue.DiceNotation;
using JetBrains.Annotations;
using Xunit;

namespace GoRogue.UnitTests.DiceNotation
{
    public class DiceNotationTests
    {
        [AssertionMethod]
        private static void AssertMinMaxValues(IDiceExpression expr, int min, int max)
        {
            Assert.Equal(min, expr.MinRoll());
            Assert.Equal(max, expr.MaxRoll());
        }

        private static void AssertReturnedInRange(IDiceExpression expr, int min, int max)
        {
            for (var i = 0; i < 100; i++)
            {
                var result = expr.Roll();
                Assert.True(result >= min && result <= max);
            }
        }

        [Fact]
        public void AdvancedDice()
        {
            var expr = Dice.Parse("1d(1d12+4)+3");
            AssertMinMaxValues(expr, 4, 19);
            AssertReturnedInRange(expr, 4, 19);
        }

        [Fact]
        public void KeepDiceAdd()
        {
            var expr = Dice.Parse("5d6k2+3");
            AssertMinMaxValues(expr, 5, 15);
            AssertReturnedInRange(expr, 5, 15);
        }

        [Fact]
        public void MultipleDice()
        {
            var expr = Dice.Parse("2d6");
            AssertMinMaxValues(expr, 2, 12);
            AssertReturnedInRange(expr, 2, 12);
        }

        [Fact]
        public void MultipleDiceAdd()
        {
            var expr = Dice.Parse("2d6+3");
            AssertMinMaxValues(expr, 5, 15);
            AssertReturnedInRange(expr, 5, 15);
        }

        [Fact]
        public void MultipleDiceAddMultiply()
        {
            var expr = Dice.Parse("(2d6+2)*3");
            AssertMinMaxValues(expr, 12, 42);
            AssertReturnedInRange(expr, 12, 42);
        }

        [Fact]
        public void MultipleDiceMultiply()
        {
            var expr = Dice.Parse("3*2d6");
            AssertMinMaxValues(expr, 6, 36);
            AssertReturnedInRange(expr, 6, 36);
        }

        [Fact]
        public void SingleDice()
        {
            var expr = Dice.Parse("1d6");
            AssertMinMaxValues(expr, 1, 6);
            AssertReturnedInRange(expr, 1, 6);
        }

        [Fact]
        public void SingleDiceAdd()
        {
            var expr = Dice.Parse("1d6+3");
            AssertMinMaxValues(expr, 4, 9);
            AssertReturnedInRange(expr, 4, 9);
        }

        [Fact]
        public void SingleDiceAddMultiply()
        {
            var expr = Dice.Parse("3*(1d6+2)");
            AssertMinMaxValues(expr, 9, 24);
            AssertReturnedInRange(expr, 9, 24);
        }

        [Fact]
        public void SingleDiceMultiply()
        {
            var expr = Dice.Parse("1d6*3");
            AssertMinMaxValues(expr, 3, 18);
            AssertReturnedInRange(expr, 3, 18);
        }
    }
}
