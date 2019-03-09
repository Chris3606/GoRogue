using Troschuetz.Random;

namespace GoRogue.DiceNotation.Terms
{
	/// <summary>
	/// Term representing the addition operator -- adds two terms together.
	/// </summary>
	public class AddTerm : ITerm
	{
		/// <summary>
		/// Constructor. Takes the two terms to add.
		/// </summary>
		/// <param name="term1">Left-hand side.</param>
		/// <param name="term2">Right-hand side.</param>
		public AddTerm(ITerm term1, ITerm term2)
		{
			Term1 = term1;
			Term2 = term2;
		}

		/// <summary>
		/// First term (left-hand side).
		/// </summary>
		public ITerm Term1 { get; private set; }

		/// <summary>
		/// Second term (right-hand side).
		/// </summary>
		public ITerm Term2 { get; private set; }

		/// <summary>
		/// Adds its two terms together, evaluating those two terms as necessary.
		/// </summary>
		/// <param name="rng">The rng to use, passed to other terms.</param>
		/// <returns>The result of adding <see cref="Term1"/> and <see cref="Term2"/>.</returns>
		public int GetResult(IGenerator rng)
		{
			return Term1.GetResult(rng) + Term2.GetResult(rng);
		}

		/// <summary>
		/// Converts to a parenthesized string.
		/// </summary>
		/// <returns>A parenthesized string representing the term.</returns>
		public override string ToString()
		{
			return "(" + Term1 + "+" + Term2 + ")";
		}
	}
}