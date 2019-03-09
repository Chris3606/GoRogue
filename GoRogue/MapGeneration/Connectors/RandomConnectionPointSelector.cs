using GoRogue.Random;
using System;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Connectors
{
	/// <summary>
	/// Implements a the selection algorithm that simply selects random points from the given
	/// areas' positions lists, using the RNG specified, or the default rng if null is given.
	/// </summary>
	public class RandomConnectionPointSelector : IAreaConnectionPointSelector
	{
		private IGenerator rng;

		/// <summary>
		/// Constructor. Specifies the RNG to use, or null if the default RNG should be used.
		/// </summary>
		/// <param name="rng">The RNG to use, or null if the default RNG should be used.</param>
		public RandomConnectionPointSelector(IGenerator rng = null)
		{
			if (rng == null)
				this.rng = SingletonRandom.DefaultRNG;
			else
				this.rng = rng;
		}

		/// <summary>
		/// Selects and returns a random point from each map area's positions list.
		/// </summary>
		/// <param name="area1">First map area to connect.</param>
		/// <param name="area2">Second area to connect</param>
		/// <returns>A tuple containing the selected positions.</returns>
		public Tuple<Coord, Coord> SelectConnectionPoints(IReadOnlyMapArea area1, IReadOnlyMapArea area2) =>
			new Tuple<Coord, Coord>(area1.Positions.RandomItem(rng), area2.Positions.RandomItem(rng));
	}
}