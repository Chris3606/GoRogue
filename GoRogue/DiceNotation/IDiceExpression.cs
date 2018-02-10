using System;
using System.Collections.Generic;
using System.Text;
using GoRogue.Random;

namespace GoRogue.DiceNotation
{
    public interface IDiceExpression
    {
        int Roll(IRandom rng = null);

        int MinRoll();
        int MaxRoll();
    }
}
