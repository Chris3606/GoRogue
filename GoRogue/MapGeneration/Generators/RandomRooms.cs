using System;
using System.Collections.Generic;
using GoRogue.Random;

namespace GoRogue.MapGeneration.Generators
{
    /// <summary>
    /// Generates a map by attempting to randomly place the specified number of rooms, ranging in size between the specified
    /// min size and max size, trying the specified number of times to position a room without overlap before discarding the room entirely.
    /// The given map will have a value of false set to all non-passable tiles, and true set to all passable ones.
    /// </summary>
    /// <remarks>
    /// This algorithm may set the cell value for each position more than once.  As such, it is highly recommended to use
    /// and ArrayMapOf as the ISettableMapOf given.  Any translation/interpretation of the result can
    /// be performed after the ArrayMapOf is set.  Attempting to do any expensive allocation/operations
    /// in the setting functions of a SettableMapOf given to the algorithm may result in performance deterioration.
    /// </remarks>
    static public class RandomRooms
    {
        /// <summary>
        /// Generates the map.  After this function has been completed, non-passable tiles will have a value of false
        /// in the ISettableMapOf given, and passable ones will have a value of true.
        /// </summary>
        /// <param name="map">The map to set values to.</param>
        /// <param name="maxRooms">The maximum number of rooms to attempt to place on the map.</param>
        /// <param name="roomMinSize">The minimum size in width and height of each room.</param>
        /// <param name="roomMaxSize">The maximum size in width and height of each room.</param>
        /// <param name="retriesPerRoom">If a room is placed in a way that overlaps with another room, the maximum number of times the position will be regenerated to try to position it properly, before simply discarding the room.</param>
        /// <param name="rng">The RNG to use to place rooms and determine room size.</param>
        static public void Generate(ISettableMapOf<bool> map, int maxRooms, int roomMinSize, int roomMaxSize, int retriesPerRoom, IRandom rng)
        {
            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                    map[x, y] = false;

            var rooms = new List<GoRogue.Rectangle>();
            for (int r = 0; r < maxRooms; r++)
            {
                int roomWidth = rng.Next(roomMinSize, roomMaxSize);
                int roomHeight = rng.Next(roomMinSize, roomMaxSize);

                int roomXPos = rng.Next(map.Width - roomWidth - 1);
                int roomYPos = rng.Next(map.Height - roomHeight - 1);

                var newRoom = new GoRogue.Rectangle(roomXPos, roomYPos, roomWidth, roomHeight);
                bool newRoomIntersects = checkOverlap(newRoom, rooms);

                int positionAttempts = 1;
                while (newRoomIntersects && positionAttempts < retriesPerRoom)
                {
                    roomXPos = rng.Next(map.Width - roomWidth - 1);
                    roomYPos = rng.Next(map.Height - roomHeight - 1);

                    newRoom = new GoRogue.Rectangle(roomXPos, roomYPos, roomWidth, roomHeight);
                    newRoomIntersects = checkOverlap(newRoom, rooms);

                    positionAttempts++;
                }

                if (!newRoomIntersects)
                    rooms.Add(newRoom);
            }

            foreach (var room in rooms)
                createRoom(map, room);

            for (int i = 1; i < rooms.Count; i++)
            {
                Coord prevRoomCenter = rooms[i - 1].Center;
                Coord currRoomCenter = rooms[i].Center;

                if (rng.Next(2) == 0) // Favors vertical tunnels
                {
                    createHTunnel(map, prevRoomCenter.X, currRoomCenter.X, prevRoomCenter.Y);
                    createVTunnel(map, prevRoomCenter.Y, currRoomCenter.Y, currRoomCenter.X);
                }
                else
                {
                    createVTunnel(map, prevRoomCenter.Y, currRoomCenter.Y, prevRoomCenter.X);
                    createHTunnel(map, prevRoomCenter.X, currRoomCenter.X, currRoomCenter.Y);
                }
            }
        }

        // TODO: ConnectRooms function that can connect the rooms properly, in method specific
        // to this generation type.

        static private bool checkOverlap(GoRogue.Rectangle room, List<GoRogue.Rectangle> existingRooms)
        {
            foreach (var existingRoom in existingRooms)
                if (room.Intsersects(existingRoom))
                    return true;

            return false;
        }
        static private void createRoom(ISettableMapOf<bool> map, GoRogue.Rectangle room)
        {
            for (int x = room.X + 1; x < room.MaxX; x++)
                for (int y = room.Y + 1; y < room.MaxY; y++)
                    map[x, y] = true;
        }

        static private void createHTunnel(ISettableMapOf<bool> map, int xStart, int xEnd, int yPos)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); ++x)
                map[x, yPos] = true;
        }

        static private void createVTunnel(ISettableMapOf<bool> map, int yStart, int yEnd, int xPos)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); ++y)
                map[xPos, y] = true;
        }
    }
}
