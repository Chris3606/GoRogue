using System.Collections.Generic;
using GoRogue.MapViews;
using GoRogue.Random;
using Troschuetz.Random;
using System.Linq;
using System.Runtime.CompilerServices;
using SadRogue.Primitives;

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
		/// <param name="maxTrimIterations">Maximum number of passes to make looking for dead ends.  Defaults to infinity.</param>
		/// <param name="rng">Rng to use.  Defaults to <see cref="SingletonRandom.DefaultRNG"/>.</param>
		public static void Trim(ISettableMapView<bool> map, int saveDeadEndChance = 0, int maxTrimIterations = -1, IGenerator rng = null)
			=> Trim(map, MapAreaFinder.MapAreasFor(map, AdjacencyRule.Cardinals), saveDeadEndChance, maxTrimIterations, rng);

		/// <summary>
		/// Trims current small dead-end paths from the given list of map areas, and removes them from the given map.
		/// </summary>
		/// <param name="map">Map to remove-dead-end paths from.</param>
		/// <param name="areas">Map areas to check for dead ends.  Dead ends not contained as one of these map areas will be ignored.</param>
		/// <param name="saveDeadEndChance">The chance out of 100 that a given dead end is left alone. Defaults to 0.</param>
		/// <param name="maxTrimIterations">Maximum number of passes to make looking for dead ends.  Defaults to infinity.</param>
		/// <param name="rng">Rng to use.  Defaults to <see cref="SingletonRandom.DefaultRNG"/>.</param>
		public static void Trim(ISettableMapView<bool> map, IEnumerable<MapArea> areas, int saveDeadEndChance = 100, int maxTrimIterations = -1, IGenerator rng = null)
		{
			if (rng == null)
				rng = SingletonRandom.DefaultRNG;

			foreach (var area in areas)
			{
				HashSet<Point> safeDeadEnds = new HashSet<Point>();
				HashSet<Point> deadEnds = new HashSet<Point>();

				int iteration = 1;
				while (maxTrimIterations == -1 || iteration <= maxTrimIterations)
				{
					foreach (var point in area.Positions)
					{
						var points = AdjacencyRule.EightWay.NeighborsClockwise(point).ToArray();
						var directions = AdjacencyRule.EightWay.DirectionsOfNeighborsClockwise(Direction.None).ToList();

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
										case Direction.Types.Up:
											found = !map[points[(int)Direction.Types.UpLeft]] &&
													!map[points[(int)Direction.Types.UpRight]] &&
													!map[points[(int)Direction.Types.Left]] &&
													!map[points[(int)Direction.Types.Right]];
											break;

										case Direction.Types.Down:
											found = !map[points[(int)Direction.Types.DownLeft]] &&
													!map[points[(int)Direction.Types.DownRight]] &&
													!map[points[(int)Direction.Types.Left]] &&
													!map[points[(int)Direction.Types.Right]];
											break;

										case Direction.Types.Right:
											found = !map[points[(int)Direction.Types.UpRight]] &&
													!map[points[(int)Direction.Types.DownRight]] &&
													!map[points[(int)Direction.Types.Up]] &&
													!map[points[(int)Direction.Types.Down]];
											break;

										case Direction.Types.Left:
											found = !map[points[(int)Direction.Types.UpLeft]] &&
													!map[points[(int)Direction.Types.DownLeft]] &&
													!map[points[(int)Direction.Types.Up]] &&
													!map[points[(int)Direction.Types.Down]];
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

					iteration++;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool PercentageCheck(int outOfHundred, IGenerator rng) => outOfHundred > 0 && rng.Next(101) < outOfHundred;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool PercentageCheck(double outOfHundred, IGenerator rng) => outOfHundred > 0d && rng.NextDouble() < outOfHundred;
	}
}
