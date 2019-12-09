using GoRogue.MapViews;
using GoRogue.Random;
using System;
using System.Collections.Generic;
using SadRogue.Primitives;
using System.Linq;
using Troschuetz.Random;

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
		/// <param name="minSidesToConnect">Minimum sides of the room to process. Defaults to 1.</param>
		/// <param name="maxSidesToConnect">Maximum sides of the room to process. Defaults to 4.</param>
		/// <param name="cancelSideConnectionSelectChance">
		/// A chance out of 100 to cancel selecting sides to process (per room). Defaults to 50.
		/// </param>
		/// <param name="cancelConnectionPlacementChance">
		/// A chance out of 100 to cancel placing a door on a side after one has been placed (per
		/// room). Defaults to 70.
		/// </param>
		/// <param name="cancelConnectionPlacementChanceIncrease">
		/// Increase the <paramref name="cancelConnectionPlacementChance"/> value by this amount each
		/// time a door is placed (per room). Defaults to 10.
		/// </param>
		/// <returns>A list of rooms and the connections placed.</returns>
		static public IEnumerable<(Rectangle Room, Point[][] Connections)> ConnectRooms(ArrayMap<bool> map, IEnumerable<Rectangle> rooms,
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
		/// <param name="minSidesToConnect">Minimum sides of the room to process. Defaults to 1.</param>
		/// <param name="maxSidesToConnect">Maximum sides of the room to process. Defaults to 4.</param>
		/// <param name="cancelSideConnectionSelectChance">
		/// A chance out of 100 to cancel selecting sides to process (per room). Defaults to 50.
		/// </param>
		/// <param name="cancelConnectionPlacementChance">
		/// A chance out of 100 to cancel placing a door on a side after one has been placed (per
		/// room). Defaults to 70.
		/// </param>
		/// <param name="cancelConnectionPlacementChanceIncrease">
		/// Increase the <paramref name="cancelConnectionPlacementChance"/> value by this amount each
		/// time a door is placed (per room). Defaults to 10.
		/// </param>
		/// <returns>A list of rooms and the connections placed.</returns>
		static public IEnumerable<(Rectangle Room, Point[][] Connections)> ConnectRooms(ArrayMap<bool> map, IGenerator rng, IEnumerable<Rectangle> rooms,
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

			var roomHallwayConnections = new List<(Rectangle Room, Point[][] Connections)>();

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

				// - Get all points along each side
				List<Point>[] validPoints = new List<Point>[4];

				for (int i = 0; i < validPoints.Length; i++)
					validPoints[i] = new List<Point>();

				int sideIndex = 0;
				foreach (var side in AdjacencyRule.Cardinals.DirectionsOfNeighbors())
				{
					foreach (var innerPoint in innerRect.PositionsOnSide(side))
					{
						var point = innerPoint + side; // Calculate the outer point
						var testPoint = point + side; // Keep going to see where opening this connection point would lead
						if (!IsPointMapEdge(map, testPoint) && !IsPointWall(map, testPoint) && IsPointWall(map, point))
							validPoints[sideIndex].Add(point);
					}
					sideIndex++;
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
						var index = sides.RandomIndex(rng);
						
						validSides[sides[index]] = false;
						sides.RemoveAt(index);
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

				List<Point>[] finalConnectionPoints = new List<Point>[4];
				finalConnectionPoints[0] = new List<Point>();
				finalConnectionPoints[1] = new List<Point>();
				finalConnectionPoints[2] = new List<Point>();
				finalConnectionPoints[3] = new List<Point>();

				// - loop sides
				for (var i = 0; i < 4; i++)
				{
					if (validSides[i])
					{
						var currentChance = cancelConnectionPlacementChance;

						// - Loop points
						while (validPoints[i].Count != 0)
						{
							// Get point and pull it out of the list
							var point = validPoints[i][rng.Next(validPoints[i].Count)];
							validPoints[i].Remove(point);

							// If point passes availability (no already chosen point next to point)
							// In cases where room connectivity is already dealt with by maze connection this
							// may fail but that's ok, already connection on that side at that point,
							// so it still guarantees that it is connected.
							if (IsPointByTwoWalls(map, point))
							{
								// Add point to list
								finalConnectionPoints[i].Add(point);
								map[point] = true;
							}

							// Found a point, so start reducing the chance for another
							if (finalConnectionPoints[i].Count != 0)
							{
								if (PercentageCheck(currentChance, rng))
									break;

								currentChance += cancelConnectionPlacementChanceIncrease;
							}
						}
					}
				}

				roomHallwayConnections.Add((room, finalConnectionPoints.Select(l => l.ToArray()).ToArray()));
			}

			return roomHallwayConnections;
		}

		private static bool IsPointByTwoWalls(IMapView<bool> map, Point location)
		{
			var points = AdjacencyRule.Cardinals.Neighbors(location);
			var area = new Rectangle(0, 0, map.Width, map.Height);
			var counter = 0;

			foreach (var point in points)
			{
				if (area.Contains(point) && map[point] == false)
					counter++;
			}

			return counter >= 2;
		}

		private static bool IsPointMapEdge(IMapView<bool> map, Point location, bool onlyEdgeTest = false)
		{
			if (onlyEdgeTest)
				return location.X == 0 || location.X == map.Width - 1 || location.Y == 0 || location.Y == map.Height - 1;
			return location.X <= 0 || location.X >= map.Width - 1 || location.Y <= 0 || location.Y >= map.Height - 1;
		}

		private static bool IsPointWall(IMapView<bool> map, Point location)
		{
			return !map[location];
		}

		private static bool PercentageCheck(int outOfHundred, IGenerator rng) => outOfHundred > 0 && rng.Next(101) < outOfHundred;
	}
}
