using GoRogue.DiceNotation;
using JetBrains.Annotations;
using Xunit;
using XUnit.ValueTuples;

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

        [AssertionMethod]
        private static void AssertReturnedInRange(IDiceExpression expr, int min, int max)
        {
            for (var i = 0; i < 100; i++)
            {
                var result = expr.Roll();
                Assert.True(result >= min && result <= max);
            }
        }

        public static (string expr, int min, int max)[] DiceExpressions  = new[]
        {
            // Advanced Dice Expression
            ("1d(1d12+4)+3", 4, 19),
            // Keep and Add
            ("5d6k2+3", 5, 15),
            // Multiple dice
            ("2d6", 2, 12),
            // Multiple dice add
            ("2d6+3", 5, 15),
            // Multiple dice add and multiply
            ("(2d6+2)*3", 12, 42),
            // Multiple dice with multiply
            ("3*2d6", 6, 36),
            // Single dice
            ("1d6", 1, 6),
            // Single dice with add
            ("1d6+3", 4, 9),
            // Single dice with add and multiply
            ("3*(1d6+2)", 9, 24),
            // Single dice with multiply
            ("1d6*3", 3, 18)
        };

        [Theory]
        [MemberDataTuple(nameof(DiceExpressions))]
        public void DiceExpressionValues(string expression, int min, int max)
        {
            var expr = Dice.Parse(expression);
            AssertMinMaxValues(expr, min, max);
            AssertReturnedInRange(expr, min, max);
        }
    }
}
