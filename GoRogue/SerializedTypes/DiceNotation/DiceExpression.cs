using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using GoRogue.DiceNotation;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.DiceNotation
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="DiceExpression"/>
    /// </summary>
    [PublicAPI]
    [DataContract]
    [SuppressMessage("ReSharper", "CA1815")] // Type should only be used for serialization
    public struct DiceExpressionSerialized
    {
        /// <summary>
        /// Expression in dice notation representing the expression.
        /// </summary>
        [DataMember] public string Expression;

        /// <summary>
        /// Converts <see cref="DiceExpression"/> to <see cref="DiceExpressionSerialized"/>.
        /// </summary>
        /// <param name="expression"/>
        /// <returns/>
        public static implicit operator DiceExpressionSerialized(DiceExpression expression)
            => FromDiceExpression(expression);

        /// <summary>
        /// Converts <see cref="DiceExpressionSerialized"/> to <see cref="DiceExpression"/>.
        /// </summary>
        /// <param name="expression"/>
        /// <returns/>
        public static implicit operator DiceExpression(DiceExpressionSerialized expression)
            => expression.ToDiceExpression();

        /// <summary>
        /// Converts <see cref="DiceExpression"/> to <see cref="DiceExpressionSerialized"/>.
        /// </summary>
        /// <param name="expression"/>
        /// <returns/>
        public static DiceExpressionSerialized FromDiceExpression(DiceExpression expression)
            => new DiceExpressionSerialized { Expression = expression.ToString() };

        /// <summary>
        /// Converts <see cref="DiceExpressionSerialized"/> to <see cref="DiceExpression"/>.
        /// </summary>
        /// <returns/>
        public DiceExpression ToDiceExpression() => Dice.Parse(Expression);
    }
}
