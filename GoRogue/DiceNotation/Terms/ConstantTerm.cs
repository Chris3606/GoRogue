using GoRogue.Random;

namespace GoRogue.DiceNotation.Terms
{
    public class ConstantTerm : ITerm
    {
        private int value;

        public ConstantTerm(int value)
        {
            this.value = value;
        }

        public int GetResult(IRandom rng) => value;

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
