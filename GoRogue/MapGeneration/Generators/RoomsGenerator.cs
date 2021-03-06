﻿using GoRogue.MapViews;
using GoRogue.Random;
using System;
using System.Collections.Generic;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Generators
{
	/// <summary>
	/// Carves out non-overlapping rooms in a map. Rooms generated will not overlap with themselves,
	/// or any existing open areas on the given map. Rooms will not be connected -- for automatic
	/// connection of rooms generated by this generator, see
	/// <see cref="QuickGenerators.GenerateDungeonMazeMap(ISettableMapView{bool}, int, int, int, int, float, float, int, int, int, int, int, int, int, int, int, int)"/>
	/// and overloads.
	/// </summary>
	static public class RoomsGenerator
	{
		/// <summary>
		/// Carves random rectangles out of the map, setting the interior of the room to true. Does
		/// not set the wall of the rooms to false.
		/// </summary>
		/// <param name="map">The map to modify.</param>
		/// <param name="minRooms">Minimum amount of rooms to generate.</param>
		/// <param name="maxRooms">Maximum amount of rooms to generate.</param>
		/// <param name="roomMinSize">The minimum size of the room. Forces an odd number.</param>
		/// <param name="roomMaxSize">The maximum size of the room. Forces an odd number.</param>
		/// <param name="roomSizeRatioX">
		/// The ratio of the room width to the height. Defaults to 1.0.
		/// </param>
		/// <param name="roomSizeRatioY">
		/// The ratio of the room height to the width. Defaults to 1.0.
		/// </param>
		/// <param name="maxCreationAttempts">
		/// The max times to re-generate a room that cannot be placed before giving up on placing
		/// that room. Defaults to 10.
		/// </param>
		/// <param name="maxPlacementAttempts">
		/// The max times to attempt to place a room in a map without intersection, before giving up
		/// and re-generating that room. Defaults to 10.
		/// </param>
		/// <returns>A collection of rectangles representing the interior of the rooms that were added to the map.</returns>
		static public IEnumerable<Rectangle> Generate(ISettableMapView<bool> map, int minRooms, int maxRooms, int roomMinSize, int roomMaxSize,
													   float roomSizeRatioX = 1f, float roomSizeRatioY = 1f, int maxCreationAttempts = 10, int maxPlacementAttempts = 10)
			=> Generate(map, null, minRooms, maxRooms, roomMinSize, roomMaxSize, roomSizeRatioX, roomSizeRatioY, maxCreationAttempts, maxPlacementAttempts);

		/// <summary>
		/// Carves random rectangles out of the map, placing rooms at odd x and y positions, with odd width/height.
		/// Sets the interior of the room to true. Does not set the wall of the rooms to false.
		/// </summary>
		/// <param name="map">The map to modify.</param>
		/// <param name="rng">RNG to use.</param>
		/// <param name="minRooms">Minimum amount of rooms to generate.</param>
		/// <param name="maxRooms">Maximum amount of rooms to generate.</param>
		/// <param name="roomMinSize">The minimum size of the room. Forces an odd number.</param>
		/// <param name="roomMaxSize">The maximum size of the room. Forces an odd number.</param>
		/// <param name="roomSizeRatioX">
		/// The ratio of the room width to the height. Defaults to 1.0.
		/// </param>
		/// <param name="roomSizeRatioY">
		/// The ratio of the room height to the width. Defaults to 1.0.
		/// </param>
		/// <param name="maxCreationAttempts">
		/// The max times to re-generate a room that cannot be placed before giving up on placing
		/// that room. Defaults to 10.
		/// </param>
		/// <param name="maxPlacementAttempts">
		/// The max times to attempt to place a room in a map without intersection, before giving up
		/// and re-generating that room. Defaults to 10.
		/// </param>
		/// <returns>A collection of rectangles representing the interior of the rooms that were added to the map.</returns>
		static public IEnumerable<Rectangle> Generate(ISettableMapView<bool> map, IGenerator rng, int minRooms, int maxRooms, int roomMinSize, int roomMaxSize,
													   float roomSizeRatioX = 1f, float roomSizeRatioY = 1f, int maxCreationAttempts = 10, int maxPlacementAttempts = 10)
		{
			if (minRooms > maxRooms)
				throw new ArgumentOutOfRangeException(nameof(minRooms), "The minimum amount of rooms must be less than or equal to the maximum amount of rooms.");

			if (roomMinSize > roomMaxSize)
				throw new ArgumentOutOfRangeException(nameof(roomMinSize), "The minimum size of a room must be less than or equal to the maximum size of a room.");

			if (roomSizeRatioX == 0f)
				throw new ArgumentOutOfRangeException(nameof(roomSizeRatioX), "Ratio cannot be zero.");

			if (roomSizeRatioY == 0f)
				throw new ArgumentOutOfRangeException(nameof(roomSizeRatioX), "Ratio cannot be zero.");

			if (rng == null) rng = SingletonRandom.DefaultRNG;

			var roomCounter = rng.Next(minRooms, maxRooms + 1);
			var rooms = new List<Rectangle>(roomCounter);

			while (roomCounter != 0)
			{
				int tryCounterCreate = maxCreationAttempts;
				var placed = false;

				while (tryCounterCreate != 0)
				{
					var roomSize = rng.Next(roomMinSize, roomMaxSize + 1);
					var width = (int)(roomSize * roomSizeRatioX);  // this helps with non square fonts. So rooms dont look odd
					var height = (int)(roomSize * roomSizeRatioY);

					// When accounting for font ratios, these adjustments help prevent all rooms
					// having the same looking square format
					var adjustmentBase = roomSize / 4;

					if (adjustmentBase != 0)
					{
						var adjustment = rng.Next(-adjustmentBase, adjustmentBase + 1);
						var adjustmentChance = rng.Next(0, 2);

						if (adjustmentChance == 0)
							width += (int)(adjustment * roomSizeRatioX);
						else if (adjustmentChance == 1)
							height += (int)(adjustment * roomSizeRatioY);
					}

					width = Math.Max(roomMinSize, width);
					height = Math.Max(roomMinSize, height);

					// Keep room interior odd, helps with placement + tunnels around the outside.
					if (width % 2 == 0)
						width += 1;

					if (height % 2 == 0)
						height += 1;

					var roomInnerRect = new Rectangle(0, 0, width, height);

					int tryCounterPlace = maxPlacementAttempts;

					while (tryCounterPlace != 0)
					{
						int xPos = 0, yPos = 0;

						while (xPos % 2 == 0) // Generate an odd value
							xPos = rng.Next(3, map.Width - roomInnerRect.Width - 3);
						while (yPos % 2 == 0)
							yPos = rng.Next(3, map.Height - roomInnerRect.Height - 3);
						
						roomInnerRect = roomInnerRect.WithPosition(xPos, yPos);

						var roomBounds = roomInnerRect.Expand(3, 3);

						bool intersected = false;
						foreach (var point in roomBounds.Positions())
						{
							if (map[point])
							{
								intersected = true;
								break;
							}
						}

						if (intersected)
						{
							tryCounterPlace--;
							continue;
						}

						foreach (var point in roomInnerRect.Positions())
							map[point] = true;

						placed = true;
						rooms.Add(roomInnerRect);
						break;
					}

					if (placed)
						break;

					tryCounterCreate--;
				}

				roomCounter--;
			}

			return rooms;
		}
	}
}