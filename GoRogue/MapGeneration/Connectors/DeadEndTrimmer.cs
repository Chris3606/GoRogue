using System.Collections.Generic;
using GoRogue.MapViews;
using GoRogue.Random;
using Troschuetz.Random;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GoRogue.MapGeneration.Connectors
{
	/// <summary>
	/// Implements an algorithm that will prune small dead-end locations (locations surrounded by 3 walls), from a map/list of map areas.
	/// </summary>
	public static class DeadEndTrimmer
	{
		/// <summary>
		/// Trims current small dead-end paths from the given map.
		/// </summary>
		/// <param name="map">Map to remove-dead-end paths from.</param>
		/// <param name="saveDeadEndChance">The chance out of 100 that a given dead end is left alone. Defaults to 0.</param>
		/// <param name="rng">Rng to use.  Defaults to SingletonRandom.DefaultRNG.</param>
		public static void Trim(ISettableMapView<bool> map, int saveDeadEndChance = 0, IGenerator rng = null)
			=> Trim(map, MapAreaFinder.MapAreasFor(map, AdjacencyRule.CARDINALS), saveDeadEndChance, rng);

		/// <summary>
		/// Trims current small dead-end paths from the given list of map areas, and removes them from the given map.
		/// </summary>
		/// <param name="map">Map to remove-dead-end paths from.</param>
		/// <param name="areas">Map areas to check for dead ends.  Dead ends not contained as one of these map areas will be ignored.</param>
		/// <param name="saveDeadEndChance">The chance out of 100 that a given dead end is left alone. Defaults to 0.</param>
		/// <param name="rng">Rng to use.  Defaults to SingletonRandom.DefaultRNG.</param>
		public static void Trim(ISettableMapView<bool> map, IEnumerable<MapArea> areas, int saveDeadEndChance = 100, IGenerator rng = null)
		{
			if (rng == null)
				rng = SingletonRandom.DefaultRNG;

			foreach (var area in areas)
			{
				HashSet<Coord> safeDeadEnds = new HashSet<Coord>();
				HashSet<Coord> deadEnds = new HashSet<Coord>();

				while (true)
				{
					foreach (var point in area.Positions)
					{
						var points = AdjacencyRule.EIGHT_WAY.NeighborsClockwise(point).ToArray();
						var directions = AdjacencyRule.EIGHT_WAY.DirectionsOfNeighborsClockwise(Direction.NONE).ToList();

						for (int i = 0; i < 8; i += 2)
						{
							if (map[points[i]])
							{
								var oppDir = directions[i] + 4;
								bool found = false;

								// If we get here, source direction is a floor, opposite direction
								// should be wall
								if (!map[points[(int)oppDir.Type]])
								{
									switch (oppDir.Type)
									{
										// Check for a wall pattern in the map. In this case
										// something like where X is a wall XXX X X
										case Direction.Types.UP:
											found = !map[points[(int)Direction.Types.UP_LEFT]] &&
													!map[points[(int)Direction.Types.UP_RIGHT]] &&
													!map[points[(int)Direction.Types.LEFT]] &&
													!map[points[(int)Direction.Types.RIGHT]];
											break;

										case Direction.Types.DOWN:
											found = !map[points[(int)Direction.Types.DOWN_LEFT]] &&
													!map[points[(int)Direction.Types.DOWN_RIGHT]] &&
													!map[points[(int)Direction.Types.LEFT]] &&
													!map[points[(int)Direction.Types.RIGHT]];
											break;

										case Direction.Types.RIGHT:
											found = !map[points[(int)Direction.Types.UP_RIGHT]] &&
													!map[points[(int)Direction.Types.DOWN_RIGHT]] &&
													!map[points[(int)Direction.Types.UP]] &&
													!map[points[(int)Direction.Types.DOWN]];
											break;

										case Direction.Types.LEFT:
											found = !map[points[(int)Direction.Types.UP_LEFT]] &&
													!map[points[(int)Direction.Types.DOWN_LEFT]] &&
													!map[points[(int)Direction.Types.UP]] &&
													!map[points[(int)Direction.Types.DOWN]];
											break;
									}
								}

								if (found)
									deadEnds.Add(point);

								break;
							}
						}
					}

					deadEnds.ExceptWith(safeDeadEnds); // Remove safeDeadEnds from deadEnds
					area.Remove(deadEnds); // Remove dead ends from positions

					if (deadEnds.Count == 0)
						break;

					foreach (var point in deadEnds)
					{
						if (PercentageCheck(saveDeadEndChance, rng))
							safeDeadEnds.Add(point);
						else
							map[point] = false;
					}

					deadEnds.Clear();
					break; // We only do 1 pass, to avoid erasing the entire map.
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool PercentageCheck(int outOfHundred, IGenerator rng) => outOfHundred > 0 && rng.Next(101) < outOfHundred;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool PercentageCheck(double outOfHundred, IGenerator rng) => outOfHundred > 0d && rng.NextDouble() < outOfHundred;
	}
}
