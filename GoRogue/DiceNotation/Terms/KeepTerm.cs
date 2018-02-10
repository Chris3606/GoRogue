using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoRogue.Random;

namespace GoRogue.DiceNotation.Terms
{
    public class KeepTerm : ITerm
    {
        private DiceTerm diceTerm;
        private ITerm keep;

        public KeepTerm(ITerm keep, DiceTerm diceTerm)
        {
            this.keep = keep;
        }

        public int GetResult(IRandom rng)
        {
            int keepVal = keep.GetResult(rng);

            if (keepVal < 0)
                throw new Exceptions.InvalidChooseException();

            diceTerm.GetResult(rng); // Roll so we can check chooses

            if (keepVal > diceTerm.LastMultiplicity)
                throw new Exceptions.InvalidChooseException();

            return diceTerm.DiceResults.OrderByDescending(value => value).Take(keepVal).Sum();
        }

        public override string ToString()
        {
            return "(" + diceTerm + "k" + keep + ")";
        }
    }
}
