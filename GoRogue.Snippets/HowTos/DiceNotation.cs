// ReSharper disable UnusedVariable
#region Usings
using GoRogue.DiceNotation;
#endregion

namespace GoRogue.Snippets.HowTos;
public static class DiceNotation
{
    public static void ExampleCode()
    {
        #region BasicRolling
        int attackRoll = Dice.Roll("1d20+2");
        #endregion

        #region DiceExpression
        DiceExpression expr = Dice.Parse("1d12+3");

        // Returns the minimum possible value for the expression (4)
        int minVal = expr.MinRoll();

        // Returns the maximum possible value for the expression (15)
        int maxVal = expr.MaxRoll();

        // Rolls the expression. Can be called many times on the same DiceExpression
        int roll = expr.Roll();
        #endregion
    }
}
