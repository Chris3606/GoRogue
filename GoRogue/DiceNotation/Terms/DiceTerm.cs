using GoRogue.Random;
using System.Collections.Generic;

namespace GoRogue.DiceNotation.Terms
{
    public class DiceTerm : ITerm
    {
        public ITerm Multiplicity { get; private set; }
        public ITerm Sides { get; private set; }

        public int LastMultiplicity { get; private set; }
        public int LastSidedness { get; private set; }

        private List<int> _diceResults;
        public IEnumerable<int> DiceResults { get => _diceResults; }

        public DiceTerm(ITerm multiplicity, ITerm sides)
        {
            Multiplicity = multiplicity;
            Sides = sides;

            _diceResults = new List<int>();
        }

        public int GetResult(IRandom rng)
        {
            _diceResults.Clear();
            int sum = 0;
            LastMultiplicity = Multiplicity.GetResult(rng);
            LastSidedness = Sides.GetResult(rng);

            if (LastMultiplicity < 0)
                throw new Exceptions.InvalidMultiplicityException();

            if (LastSidedness <= 0)
                throw new Exceptions.ImpossibleDieException();


            for (int i = 0; i < LastMultiplicity; i++)
            {
                int diceVal = rng.Next(1, LastSidedness);
                sum += diceVal;
                _diceResults.Add(diceVal);
            }

            return sum;
        }

        public override string ToString()
        {
            return "(" + Multiplicity + "d" + Sides + ")";
        }
    }
}
