using GoRogue.Random;

namespace GoRogue.DiceNotation.Terms
{
    public class SubtractTerm : ITerm
    {
        public ITerm Term1 { get; private set; }
        public ITerm Term2 { get; private set; }

        public SubtractTerm(ITerm term1, ITerm term2)
        {
            Term1 = term1;
            Term2 = term2;
        }

        public int GetResult(IRandom rng)
        {
            return Term1.GetResult(rng) - Term2.GetResult(rng);
        }

        public override string ToString()
        {
            return "(" + Term1 + "-" + Term2 + ")";
        }
    }
}
