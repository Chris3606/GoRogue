using Troschuetz.Random;

namespace GoRogue.DiceNotation
{
	/// <summary>
	/// Interface for class representing a parsed dice expression. Returned by <see cref="IParser"/> implementations.
	/// </summary>
	public interface IDiceExpression
	{
		/// <summary>
		/// Returns the maximum possible result of the dice expression (the highest it could be).
		/// </summary>
		/// <remarks>
		/// Typically this can be implemented by calling <see cref="Roll(IGenerator)"/> and passing in a <see cref="GoRogue.Random.MaxRandom"/> instance.
		/// </remarks>
		/// <returns>The maxiumum possible value that could be returned by this dice expression.</returns>
		int MaxRoll();

		/// <summary>
		/// Returns the minimum possible result of the dice expression (the lowest it could be).
		/// </summary>
		/// <remarks>
		/// Typically this can be implemented by calling <see cref="Roll(IGenerator)"/> and passing in a <see cref="GoRogue.Random.MinRandom"/> instance.
		/// </remarks>
		/// <returns>The miniumum possible value that could be returned by this dice expression.</returns>
		int MinRoll();

		/// <summary>
		/// Rolls the expression using the RNG given, returning the result.
		/// </summary>
		/// <param name="rng">The RNG to use. If null is specified, the default RNG is used.</param>
		/// <returns>The result obtained by rolling the dice expression.</returns>
		int Roll(IGenerator rng = null);
	}
}