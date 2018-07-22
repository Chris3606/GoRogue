using GoRogue.MapViews;
using GoRogue.Random;
using System.Collections.Generic;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Generators
{
    /// <summary>
    /// Generates a map by attempting to randomly place the specified number of rooms, ranging in
    /// size between the specified min size and max size, trying the specified number of times to
    /// position a room without overlap before discarding the room entirely. The given map will have
    /// a value of false set to all non-passable tiles, and true set to all passable ones.
    /// </summary>
    static public class RandomRoomsGenerator
    {
        /// <summary>
        /// Generates the map. After this function has been completed, non-passable tiles will have a
        /// value of false in the ISettableMapView given, and passable ones will have a value of true.
        /// </summary>
        /// <remarks>
        /// It is guaranteed that the "set" function of the ISettableMapView passed in will only be
        /// called once per tile, unless the type is ArrayMap of bool, in which case the operation is
        /// inexpensive and calling it multiples times costs little extra, and saves an internal allocation.
        /// </remarks>
        /// <param name="map">The map to set values to.</param>
        /// <param name="maxRooms">The maximum number of rooms to attempt to place on the map.</param>
        /// <param name="roomMinSize">The minimum size in width and height of each room.</param>
        /// <param name="roomMaxSize">The maximum size in width and height of each room.</param>
        /// <param name="attemptsPerRoom">
        /// The maximum number of times the position of a room will be generated to try to position
        /// it properly (eg. without overlapping with other rooms), before simply discarding the room.
        /// </param>
        /// <param name="rng">
        /// The RNG to use to place rooms and determine room size. If null is specified, the default
        /// RNG is used.
        /// </param>
        /// <param name="connectUsingDefault">
        /// Whether or not to ensure the rooms generated are connected. If this is true,
        /// OrderedMapAreaConnector.Connect will be used to connect the areas in a random order,
        /// using the RNG given to this function, and a CenterBoundsConnectionPointSelector, which
        /// will connect the center of room to each other.
        /// </param>
        static public void Generate(ISettableMapView<bool> map, int maxRooms, int roomMinSize, int roomMaxSize, int attemptsPerRoom, IGenerator rng = null,
                                     bool connectUsingDefault = true)
        {
            if (maxRooms <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(maxRooms), "maxRooms must be greater than 0.");

            if (roomMinSize <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(roomMinSize), "roomMinSize must be greater than 0.");

            if (roomMaxSize < roomMinSize)
                throw new System.ArgumentOutOfRangeException(nameof(roomMaxSize), "roomMaxSize must be greater than or equal to roomMinSize.");

            if (attemptsPerRoom <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(attemptsPerRoom), "attemptsPerRoom must be greater than 0.");

            if (map.Width - roomMaxSize < 0)
                throw new System.ArgumentOutOfRangeException(nameof(roomMaxSize), "roomMaxSize must be smaller than map.");

            if (rng == null) rng = SingletonRandom.DefaultRNG;

            ArrayMap<bool> tempMap = map as ArrayMap<bool>;
            bool wasArrayMap = tempMap != null;

            if (!wasArrayMap) tempMap = new ArrayMap<bool>(map.Width, map.Height);

            // To account for walls, dimensions specified were inner dimensions
            roomMinSize += 2;
            roomMaxSize += 2;

            for (int x = 0; x < tempMap.Width; x++)
                for (int y = 0; y < tempMap.Height; y++)
                    tempMap[x, y] = false;

            var rooms = new List<Rectangle>();
            for (int r = 0; r < maxRooms; r++)
            {
                int roomWidth = rng.Next(roomMinSize, roomMaxSize + 1);
                int roomHeight = rng.Next(roomMinSize, roomMaxSize + 1);

                int roomXPos = rng.Next(tempMap.Width - roomWidth + 1);
                int roomYPos = rng.Next(tempMap.Height - roomHeight + 1);

                var newRoom = new Rectangle(roomXPos, roomYPos, roomWidth, roomHeight);
                bool newRoomIntersects = checkOverlap(newRoom, rooms);

                int positionAttempts = 1;
                while (newRoomIntersects && positionAttempts < attemptsPerRoom)
                {
                    roomXPos = rng.Next(tempMap.Width - roomWidth + 1);
                    roomYPos = rng.Next(tempMap.Height - roomHeight + 1);

                    newRoom = new Rectangle(roomXPos, roomYPos, roomWidth, roomHeight);
                    newRoomIntersects = checkOverlap(newRoom, rooms);

                    positionAttempts++;
                }

                if (!newRoomIntersects)
                    rooms.Add(newRoom);
            }

            foreach (var room in rooms)
                createRoom(tempMap, room);

            if (connectUsingDefault)
                Connectors.OrderedMapAreaConnector.Connect(tempMap, AdjacencyRule.CARDINALS, new Connectors.CenterBoundsConnectionPointSelector(), rng: rng);

            if (!wasArrayMap)
            {
                for (int x = 0; x < tempMap.Width; x++)
                    for (int y = 0; y < tempMap.Height; y++)
                        map[x, y] = tempMap[x, y];
            }
        }

        static private bool checkOverlap(Rectangle room, List<Rectangle> existingRooms)
        {
            foreach (var existingRoom in existingRooms)
                if (room.Intersects(existingRoom))
                    return true;

            return false;
        }

        static private void createRoom(ISettableMapView<bool> map, Rectangle room)
        {
            for (int x = room.X + 1; x < room.MaxExtentX; x++)
                for (int y = room.Y + 1; y < room.MaxExtentY; y++)
                    map[x, y] = true;
        }
    }
}