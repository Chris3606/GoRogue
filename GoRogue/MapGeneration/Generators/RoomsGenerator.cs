using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GoRogue.MapViews;
using GoRogue.Random;

namespace GoRogue.MapGeneration.Generators
{
    /// <summary>
    /// Carves out rooms in a map that is all walls.
    /// </summary>
    static public class RoomsGenerator
    {
        /// <summary>
        /// Carves random rectangles out of the map, setting the interior of the room to <see langword="false"/>. Does not set the wall of the rooms to true.
        /// </summary>
        /// <param name="map">The map to modify.</param>
        /// <param name="minRooms">Minimum amount of rooms to generate.</param>
        /// <param name="maxRooms">Maximum amount of rooms to generate.</param>
        /// <param name="roomMinSize">The minimum size of the room. Forces an odd number.</param>
        /// <param name="roomMaxSize">The maximum size of the room. Forces an odd number.</param>
        /// <param name="roomSizeRatioX">The ratio of the room width to the height.</param>
        /// <param name="roomSizeRatioY">The ratio of the room height to the width.</param>
        /// <returns>A collection of room rectangles.</returns>
        static public IEnumerable<Rectangle> Generate(ISettableMapView<bool> map, int minRooms, int maxRooms,
                                                               int roomMinSize, int roomMaxSize,
                                                               float roomSizeRatioX, float roomSizeRatioY)
        {
            if (minRooms > maxRooms)
                throw new ArgumentOutOfRangeException(nameof(minRooms), "The minimum amount of rooms must be less than or equal to the maximum amount of rooms.");

            if (roomMinSize > roomMaxSize)
                throw new ArgumentOutOfRangeException(nameof(roomMinSize), "The minimum size of a room must be less than or equal to the maximum size of a room.");

            if (roomSizeRatioX == 0f)
                throw new ArgumentOutOfRangeException(nameof(roomSizeRatioX), "Ratio cannot be zero.");

            if (roomSizeRatioY == 0f)
                throw new ArgumentOutOfRangeException(nameof(roomSizeRatioX), "Ratio cannot be zero.");


            var roomCounter = SingletonRandom.DefaultRNG.Next(minRooms, maxRooms + 1);
            var rooms = new List<Rectangle>(roomCounter);

            while (roomCounter != 0)
            {
                var tryCounterCreate = 10;
                var placed = false;

                while (tryCounterCreate != 0)
                {
                    var roomSize = SingletonRandom.DefaultRNG.Next(roomMinSize, roomMaxSize + 1);
                    var width = (int)(roomSize * roomSizeRatioX);  // this helps with non square fonts. So rooms dont look odd
                    var height = (int)(roomSize * roomSizeRatioY);


                    // When accounting for font ratios, these adjustments help prevent all rooms having the same looking square format
                    var adjustmentBase = roomSize / 4;

                    if (adjustmentBase != 0)
                    {
                        var adjustment = SingletonRandom.DefaultRNG.Next(-adjustmentBase, adjustmentBase + 1);
                        var adjustmentChance = SingletonRandom.DefaultRNG.Next(0, 2);

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

                    var tryCounterPlace = 10;

                    while (tryCounterPlace != 0)
                    {
                        bool intersected = false;

                        roomInnerRect = roomInnerRect.Move(Coord.Get(SingletonRandom.DefaultRNG.Next(3, map.Width - roomInnerRect.Width - 3), SingletonRandom.DefaultRNG.Next(3, map.Height - roomInnerRect.Height - 3)));

                        // 
                        var roomBounds = roomInnerRect.Expand(3, 3);

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
