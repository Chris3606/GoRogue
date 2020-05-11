using System;
using System.Collections.Generic;
using System.Text;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.ContextComponents
{
    /// <summary>
    /// Context component for map generation containing a list of rooms.  Used by map generation steps that generate/place rectangular rooms
    /// on the map.  Each <see cref="Rectangle"/> represents the interior of a single room (not including walls).
    /// </summary>
    public class RoomsList
    {
        /// <summary>
        /// List of all rooms placed by relevant generation steps.  Each <see cref="Rectangle"/> represents the interior of a single room (not including walls).
        /// </summary>
        public readonly List<Rectangle> Rooms;

        /// <summary>
        /// Creates an empty list of rooms.
        /// </summary>
        public RoomsList()
        {
            Rooms = new List<Rectangle>();
        }

        /// <summary>
        /// Creates an empty list of rooms with the initial capacity specified.
        /// </summary>
        /// <param name="initialListCapacity">The initial capacity for the <see cref="Rooms"/> list.</param>
        public RoomsList(int initialListCapacity)
        {
            Rooms = new List<Rectangle>(initialListCapacity);
        }
    }
}
