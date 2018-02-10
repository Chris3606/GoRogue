using GoRogue.Random;

namespace GoRogue.DiceNotation.Terms
{
    public interface ITerm
    {
        int GetResult(IRandom rng);
    }
}
