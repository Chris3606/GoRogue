using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GoRogue.MapGeneration.ContextComponents;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.MapGeneration.ContextComponents
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="DoorList"/> instance.
    /// </summary>
    [PublicAPI]
    [DataContract]
    public struct DoorListSerialized
    {
        /// <summary>
        /// List of recorded rooms and associated doors.
        /// </summary>
        [DataMember] public List<RoomDoorsSerialized> RoomsAndDoors;

        /// <summary>
        /// Converts <see cref="DoorList"/> to <see cref="DoorListSerialized"/>.
        /// </summary>
        /// <param name="doorsList"/>
        /// <returns/>
        public static implicit operator DoorListSerialized(DoorList doorsList)
            => FromDoorList(doorsList);

        /// <summary>
        /// Converts <see cref="DoorListSerialized"/> to <see cref="DoorList"/>.
        /// </summary>
        /// <param name="doorsList"/>
        /// <returns/>
        public static implicit operator DoorList(DoorListSerialized doorsList)
            => doorsList.ToDoorList();

        /// <summary>
        /// Converts <see cref="DoorList"/> to <see cref="DoorListSerialized"/>.
        /// </summary>
        /// <param name="doorsList"/>
        /// <returns/>
        public static DoorListSerialized FromDoorList(DoorList doorsList)
            => new DoorListSerialized()
            {
                RoomsAndDoors = doorsList.DoorsPerRoom.Values.Select(roomDoor => (RoomDoorsSerialized)roomDoor).ToList()
            };

        /// <summary>
        /// Converts <see cref="DoorListSerialized"/> to <see cref="DoorList"/>.
        /// </summary>
        /// <returns/>
        public DoorList ToDoorList()
        {
            var result = new DoorList();

            foreach (var roomDoor in RoomsAndDoors)
            {
                foreach (var (point, step) in roomDoor.Doors)
                    result.AddDoor(step, roomDoor.Room, point);
            }

            return result;
        }
    }
}
