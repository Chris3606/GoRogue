using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.ContextComponents
{
    /// <summary>
    /// A list of rooms and entry/exit points of those rooms, generated/added by map generation components, that tracks
    /// what generation step created/recorded which opening.
    /// </summary>
    [PublicAPI]
    public class DoorList
    {
        private readonly Dictionary<Rectangle, RoomDoors> _doorsPerRoom;

        /// <summary>
        /// Creates a new door manager context component.
        /// </summary>
        public DoorList()
        {
            _doorsPerRoom = new Dictionary<Rectangle, RoomDoors>();
        }

        /// <summary>
        /// A dictionary associating rooms to their lists of doors.
        /// </summary>
        public IReadOnlyDictionary<Rectangle, RoomDoors> DoorsPerRoom => _doorsPerRoom.AsReadOnly();

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

                // Add door to list with the generation step that created it
                _doorsPerRoom[room].AddDoor(generationStepName, door);
            }
        }
    }
}
