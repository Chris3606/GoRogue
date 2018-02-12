using GoRogue.DiceNotation;
using System;
using System.Diagnostics;

namespace GoRogue_PerformanceTests
{
    internal class DiceNotationTests
    {
        public static TimeSpan TimeForDiceExpression(string expression, int iterations)
        {
            var s = new Stopwatch();
            var result = Dice.Parse(expression);
            int val;

            s.Start();
            for (int i = 0; i < iterations; i++)
                val = result.Roll();
            s.Stop();

            return s.Elapsed;
        }

        public static TimeSpan TimeForDiceRoll(string expression, int iterations)
        {
            var s = new Stopwatch();
            int val;

            s.Start();
            for (int i = 0; i < iterations; i++)
                val = Dice.Roll(expression);
            s.Stop();

            return s.Elapsed;
        }
    }
}