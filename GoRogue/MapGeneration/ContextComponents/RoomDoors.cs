using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.ContextComponents
{
    /// <summary>
    /// A list of openings in room walls, categorized by side they're on.  Typically created via a
    /// <see cref="DoorList" />.
    /// </summary>
    [PublicAPI]
    public class RoomDoors : IEnumerable<ItemStepPair<Point>>
    {
        private readonly RectangleEdgePositionsList _positionsList;
        private readonly Dictionary<Point, string> _doorToStepMapping;


        /// <summary>
        /// Creates a new list of doors for a given room.
        /// </summary>
        /// <param name="room">The room having its doors tracked.</param>
        public RoomDoors(Rectangle room)
        {
            _positionsList = new RectangleEdgePositionsList(room.Expand(1, 1));
            _doorToStepMapping = new Dictionary<Point, string>();
        }

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
        /// A dictionary associating doors with the generation step that recorded/created them.
        /// </summary>
        public IReadOnlyDictionary<Point, string> DoorToStepMapping => _doorToStepMapping.AsReadOnly();

        /// <summary>
        /// Retrieves a read-only list of doors on the given side.  Direction specified must be a cardinal.
        /// </summary>
        /// <param name="side">Side to get doors for.</param>
        /// <returns>A read-only list of doors on the given side.</returns>
        public IReadOnlyList<Point> this[Direction side] => _positionsList[side];

        /// <summary>
        /// Adds the given position to the appropriate lists of doors.
        /// </summary>
        /// <param name="generationStepName">The name of the generation step that is adding the door.</param>
        /// <param name="doorPosition">Position to add.</param>
        public void AddDoor(string generationStepName, Point doorPosition)
        {
            _positionsList.Add(doorPosition);
            _doorToStepMapping[doorPosition] = generationStepName;
        }

        /// <summary>
        /// Adds the given positions to the appropriate lists of doors.
        /// </summary>
        /// <param name="generationStepName">The name of the generation step that is adding the doors.</param>
        /// <param name="doorPositions">Positions to add.</param>
        public void AddDoors(string generationStepName, params Point[] doorPositions)
            => AddDoors(generationStepName, (IEnumerable<Point>)doorPositions);

        /// <summary>
        /// Adds the given positions to the appropriate lists of doors.
        /// </summary>
        /// <param name="generationStepName">The name of the generation step that is adding the doors.</param>
        /// <param name="doorPositions">Positions to add.</param>
        public void AddDoors(string generationStepName, IEnumerable<Point> doorPositions)
        {
            foreach (var pos in doorPositions)
            {
                _positionsList.Add(pos);
                _doorToStepMapping[pos] = generationStepName;
            }
        }

        /// <summary>
        /// Gets an enumerator for all doors recorded and the step that added them.
        /// </summary>
        /// <returns/>
        public IEnumerator<ItemStepPair<Point>> GetEnumerator()
        {
            foreach (var door in Doors)
                yield return new ItemStepPair<Point>(door, _doorToStepMapping[door]);
        }

        /// <summary>
        /// Gets an enumerator for all doors recorded and the step that added them.
        /// </summary>
        /// <returns/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
