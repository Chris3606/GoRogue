using System;
using GoRogue.DiceNotation;
using JetBrains.Annotations;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.DiceNotation
{
    public class DiceNotationTests
    {
        [AssertionMethod]
        private static void AssertMinMaxValues(DiceExpression expr, int min, int max)
        {
            Assert.Equal(min, expr.MinRoll());
            Assert.Equal(max, expr.MaxRoll());
        }

        [AssertionMethod]
        private static void AssertReturnedInRange(DiceExpression expr, int min, int max)
        {
            // Necessary if we're using negative numbers as multipliers
            int lMin = Math.Min(min, max);
            int lMax = Math.Max(min, max);

            for (var i = 0; i < 100; i++)
            {
                var result = expr.Roll();
                Assert.True(result >= lMin && result <= lMax);
            }
        }

        // NOTE: The test case min and max values are the expected min and max values for the expression
        // assuming the MINIMUM roll of the dice, and assuming the MAXIMUM roll of the dice, in that order.
        // This means that they min and max values may be reversed if the expression contains a negative multiplier.
        public static (string expr, int min, int max)[] DiceExpressions =
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
            ("1d6*3", 3, 18),
            // Unary negation (only constant)
            ("-1", -1, -1),
            // Negative combined with other operators
            ("1d6+-1", 0, 5),
            // Negative combined with minus
            ("1d6--1", 2, 7),
            // Negative combined with multiply
            ("1d6*-1", -1, -6),
            // Negative combined with multiply and paren
            ("-2*(1d6+7)", -16, -26)
        };

        public static string[] InvalidDiceExpressions =
        {
            "1d6*/3",
            "1d6-/3",
            "1d6-)3"
        };

        [Theory]
        [MemberDataTuple(nameof(DiceExpressions))]
        public void DiceExpressionValues(string expression, int min, int max)
        {
            var expr = Dice.Parse(expression);
            AssertMinMaxValues(expr, min, max);
            AssertReturnedInRange(expr, min, max);
        }

        [Theory]
        [MemberDataEnumerable(nameof(InvalidDiceExpressions))]
        public void DiceExpressionInvalid(string expression)
        {
            Assert.Throws<InvalidOperationException>(() => Dice.Parse(expression));
        }
    }
}
