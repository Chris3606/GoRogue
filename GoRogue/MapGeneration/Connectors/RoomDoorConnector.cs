using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GoRogue.MapViews;
using Troschuetz.Random;
using GoRogue.Random;

namespace GoRogue.MapGeneration.Connectors
{
	/// <summary>
	/// Opens up room walls to connect tunnels that are placed near rooms.
	/// </summary>
	static public class RoomDoorConnector
	{
		/// <summary>
		/// Detects tunnels near the specified rooms and tries to open a wall spot to represent a door.
		/// </summary>
		/// <param name="map">The map to modify.</param>
		/// <param name="rooms">A collection of rooms to process.</param>
		/// <param name="minSidesToConnect">Minimum sides of the room to process.  Defaults to 1.</param>
		/// <param name="maxSidesToConnect">Maximum sides of the room to process.  Defaults to 4.</param>
		/// <param name="cancelSideConnectionSelectChance">A chance out of 100 to cancel selecting sides to process (per room).  Defaults to 50.</param>
		/// <param name="cancelConnectionPlacementChance">A chance out of 100 to cancel placing a door on a side after one has been placed (per room).
		/// Defaults to 70.</param>
		/// <param name="cancelConnectionPlacementChanceIncrease">Increase the <paramref name="cancelConnectionPlacementChance"/> value by this amount each time a door
		/// is placed (per room).  Defaults to 10.</param>
		/// <returns>A list of rooms and the connections placed.</returns>
		static public IEnumerable<(Rectangle Room, Coord[][] Connections)> ConnectRooms(ISettableMapView<bool> map, IEnumerable<Rectangle> rooms,
						   int minSidesToConnect = 1, int maxSidesToConnect = 4, int cancelSideConnectionSelectChance = 50, int cancelConnectionPlacementChance = 70,
						   int cancelConnectionPlacementChanceIncrease = 10)
			=> ConnectRooms(map, null, rooms, minSidesToConnect, maxSidesToConnect, cancelSideConnectionSelectChance, cancelConnectionPlacementChance,
							cancelConnectionPlacementChanceIncrease);

		/// <summary>
		/// Detects tunnels near the specified rooms and tries to open a wall spot to represent a door.
		/// </summary>
		/// <param name="map">The map to modify.</param>
		/// <param name="rng">The RNG to use.</param>
		/// <param name="rooms">A collection of rooms to process.</param>
		/// <param name="minSidesToConnect">Minimum sides of the room to process.  Defaults to 1.</param>
		/// <param name="maxSidesToConnect">Maximum sides of the room to process.  Defaults to 4.</param>
		/// <param name="cancelSideConnectionSelectChance">A chance out of 100 to cancel selecting sides to process (per room).  Defaults to 50.</param>
		/// <param name="cancelConnectionPlacementChance">A chance out of 100 to cancel placing a door on a side after one has been placed (per room).
		/// Defaults to 70.</param>
		/// <param name="cancelConnectionPlacementChanceIncrease">Increase the <paramref name="cancelConnectionPlacementChance"/> value by this amount each time a door
		/// is placed (per room).  Defaults to 10.</param>
		/// <returns>A list of rooms and the connections placed.</returns>
		static public IEnumerable<(Rectangle Room, Coord[][] Connections)> ConnectRooms(ISettableMapView<bool> map, IGenerator rng, IEnumerable<Rectangle> rooms,
						   int minSidesToConnect = 1, int maxSidesToConnect = 4, int cancelSideConnectionSelectChance = 50, int cancelConnectionPlacementChance = 70,
						   int cancelConnectionPlacementChanceIncrease = 10)
		{
			if (maxSidesToConnect > 4)
				throw new ArgumentOutOfRangeException(nameof(maxSidesToConnect), "Maximum sides to connection cannot be greater than 4.");
			if (minSidesToConnect < 0)
				throw new ArgumentOutOfRangeException(nameof(minSidesToConnect), "Minimum sides to connection cannot be less than 0.");
			if (minSidesToConnect > maxSidesToConnect)
				throw new ArgumentOutOfRangeException(nameof(minSidesToConnect), "The minimum sides with connections must be less than or equal to the maximum amount of sides with connections.");

			if (rng == null) rng = SingletonRandom.DefaultRNG;

			var roomHallwayConnections = new List<(Rectangle Room, Coord[][] Connections)>();

			/*
			- Get all points along a side
			- if point count for side is > 0
			  - mark side for placement
			- if total sides marked > max
			  - loop total sides > max
				- randomly remove side
			- if total sides marked > min
			  - loop sides
				- CHECK side placement cancel check OK
				  - unmark side
				- if total sides marked == min
				  -break loop
			- Loop sides
			  - Loop points
				- If point passes availability (no already chosen point next to point)
				  - CHECK point placement OK
					- Add point to list
			*/

			foreach (var room in rooms)
			{
				var outerRect = room.Expand(1, 1);
				var innerRect = room;

				// Get all points along each side
				List<Coord>[] validPoints = new List<Coord>[4];

				const int INDEX_UP = 0;
				const int INDEX_DOWN = 1;
				const int INDEX_RIGHT = 2;
				const int INDEX_LEFT = 3;

				validPoints[INDEX_UP] = new List<Coord>();
				validPoints[INDEX_DOWN] = new List<Coord>();
				validPoints[INDEX_RIGHT] = new List<Coord>();
				validPoints[INDEX_LEFT] = new List<Coord>();

				// Along top/bottom edges
				for (int x = 1; x < outerRect.Width - 1; x++)
				{
					var point = outerRect.Position.Translate(x, 0);
					var testPoint = point + Direction.UP;

					// Top
					if (!IsPointMapEdge(map, testPoint) && !IsPointWall(map, testPoint))
						validPoints[INDEX_UP].Add(point);

					point = outerRect.Position.Translate(x, outerRect.Height - 1);
					testPoint = point + Direction.DOWN;

					// Bottom
					if (!IsPointMapEdge(map, testPoint) && !IsPointWall(map, testPoint))
						validPoints[INDEX_DOWN].Add(point);
				}

				// Along the left/right edges
				for (int y = 1; y < outerRect.Height - 1; y++)
				{
					var point = outerRect.Position.Translate(0, y);
					var testPoint = point + Direction.LEFT;

					// Left
					if (!IsPointMapEdge(map, testPoint) && !IsPointWall(map, testPoint))
						validPoints[INDEX_RIGHT].Add(point);

					point = outerRect.Position.Translate(outerRect.Width - 1, y);
					testPoint = point + Direction.RIGHT;

					// Right
					if (!IsPointMapEdge(map, testPoint) && !IsPointWall(map, testPoint))
						validPoints[INDEX_LEFT].Add(point);
				}

				// - if point count for side is > 0, it's a valid side.
				bool[] validSides = new bool[4];
				var sidesTotal = 0;

				for (int i = 0; i < 4; i++)
				{
					// - mark side for placement
					validSides[i] = validPoints[i].Count != 0;
					if (validSides[i])
						sidesTotal++;
				}


				// - if total sides marked > max
				if (sidesTotal > maxSidesToConnect)
				{
					var sides = new List<int>(sidesTotal);

					for (var i = 0; i < 4; i++)
					{
						if (validSides[i])
							sides.Add(i);
					}

					// - loop total sides > max
					while (sidesTotal > maxSidesToConnect)
					{
						// - randomly remove side
						var index = sides[rng.Next(sides.Count)];
						sides.RemoveAt(index);
						validSides[index] = false;
						sidesTotal--;
					}
				}

				// - if total sides marked > min
				if (sidesTotal > minSidesToConnect)
				{
					// - loop sides
					for (var i = 0; i < 4; i++)
					{
						if (validSides[i])
						{
							// - CHECK side placement cancel check OK
							if (PercentageCheck(cancelSideConnectionSelectChance, rng))
							{
								validSides[i] = false;
								sidesTotal--;
							}
						}

						// - if total sides marked == min
						if (sidesTotal == minSidesToConnect)
							break;
					}
				}

				List<Coord>[] finalConnectionPoints = new List<Coord>[4];
				finalConnectionPoints[0] = new List<Coord>();
				finalConnectionPoints[1] = new List<Coord>();
				finalConnectionPoints[2] = new List<Coord>();
				finalConnectionPoints[3] = new List<Coord>();

				// - loop sides
				for (var i = 0; i < 4; i++)
				{
					if (validSides[i])
					{
						var currentChance = cancelConnectionPlacementChance;
						var loopMax = 100;

						// - Loop points
						while (loopMax != 0 && validPoints[i].Count != 0)
						{
							// Get point and pull it out of the list
							var point = validPoints[i][rng.Next(validPoints[i].Count)];
							validPoints[i].Remove(point);

							// - If point passes availability (no already chosen point next to point)
							if (IsPointByTwoWalls(map, point))
							{
								//     - Add point to list
								finalConnectionPoints[i].Add(point);
								map[point] = true;
							}

							// Found a point, so start reducing the chance for another
							if (finalConnectionPoints[i].Count != 0)
							{

								if (PercentageCheck(currentChance, rng))
								{
									break;
								}

								currentChance += cancelConnectionPlacementChanceIncrease;
							}

							loopMax--;
						}

						// If we went too long in the loop and nothing was selected, force one.
						if (loopMax == 0 && finalConnectionPoints[i].Count == 0)
						{
							var point = validPoints[i][rng.Next(validPoints[i].Count)];
							finalConnectionPoints[i].Add(point);
							map[point] = true;
						}
					}
				}

				if (finalConnectionPoints[0].Count == 0
					&& finalConnectionPoints[1].Count == 0
					&& finalConnectionPoints[2].Count == 0
					&& finalConnectionPoints[3].Count == 0)
					Debugger.Break();

				roomHallwayConnections.Add((room, finalConnectionPoints.Select(l => l.ToArray()).ToArray()));
			}

			return roomHallwayConnections;
		}

		static bool PercentageCheck(int outOfHundred, IGenerator rng) => outOfHundred > 0 && rng.Next(101) < outOfHundred;

		static bool IsPointByTwoWalls(IMapView<bool> map, Coord location)
		{
			var points = AdjacencyRule.CARDINALS.Neighbors(location);
			var area = new Rectangle(0, 0, map.Width, map.Height);
			var counter = 0;

			foreach (var point in points)
			{
				if (area.Contains(point) && map[point] == false)
					counter++;
			}

			return counter == 2;
		}
		static bool IsPointWall(IMapView<bool> map, Coord location)
		{
			return !map[location];
		}

		static bool IsPointMapEdge(IMapView<bool> map, Coord location, bool onlyEdgeTest = false)
		{
			if (onlyEdgeTest)
				return location.X == 0 || location.X == map.Width - 1 || location.Y == 0 || location.Y == map.Height - 1;
			return location.X <= 0 || location.X >= map.Width - 1 || location.Y <= 0 || location.Y >= map.Height - 1;

		}
	}
}
