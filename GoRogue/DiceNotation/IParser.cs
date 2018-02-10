using System;
using System.Collections.Generic;
using System.Text;

namespace GoRogue.DiceNotation
{
    public interface IParser
    {
        IDiceExpression Parse(string diceNotation);
    }
}
