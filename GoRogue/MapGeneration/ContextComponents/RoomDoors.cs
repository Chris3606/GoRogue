using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.ContextComponents
{
    /// <summary>
    /// A list of openings in room walls, categorized by side they're on.  Typically created via a <see cref="DoorList" />.
    /// </summary>
    [PublicAPI]
    public class RoomDoors
    {
        private readonly RectangleEdgePositionsList _positionsList;


        /// <summary>
        /// Creates a new list of doors for a given room.
        /// </summary>
        /// <param name="room">The room having its doors tracked.</param>
        public RoomDoors(Rectangle room) => _positionsList = new RectangleEdgePositionsList(room.Expand(1, 1));

        /// <summary>
        /// Positions of doors on the top wall of the room.
        /// </summary>
        public IReadOnlyList<Point> TopDoors => _positionsList.TopPositions;

        /// <summary>
        /// Positions of doors on the right wall of the room.
        /// </summary>
        public IReadOnlyList<Point> RightDoors => _positionsList.RightPositions;

        /// <summary>
        /// Positions of doors on the bottom wall of the room.
        /// </summary>
        public IReadOnlyList<Point> BottomDoors => _positionsList.BottomPositions;

        /// <summary>
        /// Positions of doors on the bottom wall of the room.
        /// </summary>
        public IReadOnlyList<Point> LeftDoors => _positionsList.LeftPositions;

        /// <summary>
        /// The room that is having its doors tracked.
        /// </summary>
        public Rectangle Room => _positionsList.Rectangle.Expand(-1, -1);

        /// <summary>
        /// A rectangle including the outer walls of the room having its doors tracked.
        /// </summary>
        public Rectangle RoomWithOuterWalls => _positionsList.Rectangle;

        /// <summary>
        /// Positions all doors in all walls of the room, with no duplicate locations.
        /// </summary>
        public IEnumerable<Point> Doors => _positionsList.Positions;

        /// <summary>
        /// Retrieves a read-only list of doors on the given side.  Direction specified must be a cardinal.
        /// </summary>
        /// <param name="side">Side to get doors for.</param>
        /// <returns>A read-only list of doors on the given side.</returns>
        public IReadOnlyList<Point> this[Direction side] => _positionsList[side];

        /// <summary>
        /// Adds the given position to the appropriate lists of doors.
        /// </summary>
        /// <param name="doorPosition">Position to add.</param>
        public void AddDoor(Point doorPosition) => _positionsList.AddPosition(doorPosition);

        /// <summary>
        /// Adds the given positions to the appropriate lists of doors.
        /// </summary>
        /// <param name="doorPositions">Positions to add.</param>
        public void AddDoors(params Point[] doorPositions) => _positionsList.AddPositions(doorPositions);

        /// <summary>
        /// Adds the given positions to the appropriate lists of doors.
        /// </summary>
        /// <param name="doorPositions">Positions to add.</param>
        public void AddDoors(IEnumerable<Point> doorPositions) => _positionsList.AddPositions(doorPositions);
    }

    /// <summary>
    /// A list of rooms and entry/exit points of those rooms, generated/added by map generation components, that tracks what
    /// generation step
    /// created/recorded which opening.
    /// </summary>
    [PublicAPI]
    public class DoorList
    {
        private readonly Dictionary<Rectangle, RoomDoors> _doorsPerRoom;

        private readonly Dictionary<Point, string> _doorToStepMapping;

        /// <summary>
        /// Creates a new door manager context component.
        /// </summary>
        public DoorList()
        {
            _doorsPerRoom = new Dictionary<Rectangle, RoomDoors>();
            _doorToStepMapping = new Dictionary<Point, string>();
        }

        /// <summary>
        /// A dictionary associating rooms to their lists of doors.
        /// </summary>
        public IReadOnlyDictionary<Rectangle, RoomDoors> DoorsPerRoom => _doorsPerRoom.AsReadOnly();

        /// <summary>
        /// A dictionary associating rooms with the generation step that recorded/created them.
        /// </summary>
        public IReadOnlyDictionary<Point, string> DoorToStepMapping => _doorToStepMapping.AsReadOnly();

        /// <summary>
        /// Records a new opening in the given room at the given position.
        /// </summary>
        /// <param name="generationStepName">The name of the generation step recording the door position.</param>
        /// <param name="room">The room the door is a part of.</param>
        /// <param name="doorPosition">The location of the door to add.</param>
        public void AddDoor(string generationStepName, Rectangle room, Point doorPosition)
            => AddDoors(generationStepName, room, doorPosition);

        /// <summary>
        /// Records new openings in the given room at the given positions.
        /// </summary>
        /// <param name="generationStepName">The name of the generation step recording the door positions.</param>
        /// <param name="room">The room the doors are part of.</param>
        /// <param name="doorPositions">The locations of the door to add.</param>
        public void AddDoors(string generationStepName, Rectangle room, params Point[] doorPositions)
            => AddDoors(generationStepName, room, (IEnumerable<Point>)doorPositions);

        /// <summary>
        /// Records new openings in the given room at the given positions.
        /// </summary>
        /// <param name="generationStepName">The name of the generation step recording the door positions.</param>
        /// <param name="room">The room the doors are part of.</param>
        /// <param name="doorPositions">The locations of the door to add.</param>
        public void AddDoors(string generationStepName, Rectangle room, IEnumerable<Point> doorPositions)
        {
            foreach (var door in doorPositions)
            {
                if (!_doorsPerRoom.ContainsKey(room))
                    _doorsPerRoom.Add(room, new RoomDoors(room));

                // Add door to list
                _doorsPerRoom[room].AddDoor(door);

                // Add step that recorded the door
                _doorToStepMapping[door] = generationStepName;
            }
        }
    }
}
