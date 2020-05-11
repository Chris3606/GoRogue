using SadRogue.Primitives;

namespace GoRogue.MapGeneration.ContextComponents
{
    /// <summary>
    /// Context component for map generation containing a list of rooms.  Used by map generation steps that create tunnels, for example to connect rooms
    /// or to create a maze.
    /// </summary>
    public class TunnelsList : ItemList<Area>
    {
        /// <summary>
        /// Creates an empty list of tunnels.
        /// </summary>
        public TunnelsList()
            : base() { }

        /// <summary>
        /// Creates an empty list of tunnels with the initial capacity specified.
        /// </summary>
        /// <param name="initialItemCapacity">The initial capacity for tunnels.</param>
        public TunnelsList(int initialItemCapacity)
            : base(initialItemCapacity) { }
    }
}
