using GoRogue.Random;
using GoRogue.DiceNotation.Terms;
using System;

namespace GoRogue.DiceNotation
{
    public class DiceExpression : IDiceExpression
    {
        private ITerm termToEvaluate;

        public DiceExpression(ITerm termToEvaluate)
        {
            this.termToEvaluate = termToEvaluate;
        }

        public int MaxRoll() => Roll(new MaxRandom());
        public int MinRoll() => Roll(new MinRandom());

        public int Roll(IRandom rng = null)
        {
            if (rng == null)
                rng = SingletonRandom.DefaultRNG;

            return termToEvaluate.GetResult(rng);
        }
    }
}
