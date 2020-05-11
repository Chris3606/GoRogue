using SadRogue.Primitives;

namespace GoRogue.MapGeneration.ContextComponents
{
    /// <summary>
    /// Context component for map generation containing a list of rooms.  Used by map generation steps that generate/place rectangular rooms
    /// on the map.  Each <see cref="Rectangle"/> represents the interior of a single room (not including walls).
    /// </summary>
    public class RoomsList : ItemList<Rectangle>
    {
        /// <summary>
        /// Creates an empty list of rooms.
        /// </summary>
        public RoomsList()
            : base() { }

        /// <summary>
        /// Creates an empty list of rooms with the initial capacity specified.
        /// </summary>
        /// <param name="initialItemCapacity">The initial capacity for rooms.</param>
        public RoomsList(int initialItemCapacity)
            : base(initialItemCapacity) { }
    }
}
