using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GoRogue.MapGeneration.ContextComponents;
using JetBrains.Annotations;
using SadRogue.Primitives.SerializedTypes;

namespace GoRogue.SerializedTypes.MapGeneration.ContextComponents
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="RoomDoors"/> instance.
    /// </summary>
    [PublicAPI]
    [DataContract]
    public struct RoomDoorsSerialized
    {
        /// <summary>
        /// The room whose doors are being stored.
        /// </summary>
        [DataMember] public RectangleSerialized Room;

        /// <summary>
        /// Positions of doors.
        /// </summary>
        [DataMember] public List<ItemStepPair<PointSerialized>> Doors;

        /// <summary>
        /// Converts <see cref="RoomDoors"/> to <see cref="RoomDoorsSerialized"/>.
        /// </summary>
        /// <param name="doorsList"/>
        /// <returns/>
        public static implicit operator RoomDoorsSerialized(RoomDoors doorsList)
            => FromRoomDoors(doorsList);

        /// <summary>
        /// Converts <see cref="RoomDoorsSerialized"/> to <see cref="RoomDoors"/>.
        /// </summary>
        /// <param name="doorsList"/>
        /// <returns/>
        public static implicit operator RoomDoors(RoomDoorsSerialized doorsList)
            => doorsList.ToRoomDoors();

        /// <summary>
        /// Converts <see cref="RoomDoors"/> to <see cref="RoomDoorsSerialized"/>.
        /// </summary>
        /// <param name="doorsList"/>
        /// <returns/>
        public static RoomDoorsSerialized FromRoomDoors(RoomDoors doorsList)
            => new RoomDoorsSerialized()
            {
                Room = doorsList.Room,
                Doors = doorsList.Select(pair => new ItemStepPair<PointSerialized>(pair.Item, pair.Step)).ToList()
            };

        /// <summary>
        /// Converts <see cref="RoomDoorsSerialized"/> to <see cref="RoomDoors"/>.
        /// </summary>
        /// <returns/>
        public RoomDoors ToRoomDoors()
        {
            var result = new RoomDoors(Room);
            foreach (var (item, step) in Doors)
                result.AddDoor(step, item);

            return result;
        }
    }
}
