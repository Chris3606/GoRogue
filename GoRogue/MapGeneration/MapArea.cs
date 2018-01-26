using System.Collections.Generic;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Represents an arbitrarily-shaped area of a map.  Stores and provides access to a list of each unique
    /// position considered connected.
    /// </summary>
    public class MapArea
    {
        /// <summary>
        /// Number of (unique) positions in the currently stored list.
        /// </summary>
        public int Count { get { return _positions.Count; } }

        private int left, top, bottom, right;

        /// <summary>
        /// Smallest possible rectangle that encompasses every position in the area.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                if (right < left)
                    return Rectangle.EMPTY;

                return new Rectangle(left, top, right - left + 1, bottom - top + 1);
            }
        }

        private List<Coord> _positions;

        /// <summary>
        /// List of all (unique) positions in the list.
        /// </summary>
        public IList<Coord> Positions { get { return _positions.AsReadOnly(); } }

        private HashSet<Coord> positionsSet;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MapArea()
        {
            left = int.MaxValue;
            top = int.MaxValue;

            right = 0;
            bottom = 0;

            _positions = new List<Coord>();
            positionsSet = new HashSet<Coord>();
        }

        /// <summary>
        /// Determines whether or not the given position is considered within the area or not.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the specified position is within the area, false otherwise.</returns>
        public bool Contains(Coord position)
        {
            return positionsSet.Contains(position);
        }

        /// <summary>
        /// Determines whether or not the given position is considered within the area or not.
        /// </summary>
        /// <param name="x">X-coordinate of the position to check.</param>
        /// <param name="y">Y-coordinate of the position to check.</param>
        /// <returns>True if the specified position is within the area, false otherwise.</returns>
        public bool Contains(int x, int y)
        {
            return positionsSet.Contains(Coord.Get(x, y));
        }

        /// <summary>
        /// Adds the given position to the list of points within the area if it is not already in the list,
        /// or does nothing otherwise.  Because the class uses a hash set internally to determine what points
        /// have already been added, this is an average case O(1) operation.
        /// </summary>
        /// <param name="position">The position to add.</param>
        public void Add(Coord position)
        {
            if (positionsSet.Add(position))
            {
                _positions.Add(position);

                // Update bounds
                if (position.X > right) right = position.X;
                if (position.X < left) left = position.X;
                if (position.Y > bottom) bottom = position.Y;
                if (position.Y < top) top = position.Y;
            }
        }

        /// <summary>
        /// Adds the given position to the list of points within the area if it is not already in the list,
        /// or does nothing otherwise.  Because the class uses a hash set internally to determine what points
        /// have already been added, this is an average case O(1) operation.
        /// </summary>
        /// <param name="x">X-coordinate of the position to add.</param>
        /// <param name="y">Y-coordinate of the position to add.</param>
        public void Add(int x, int y)
        {
            Add(Coord.Get(x, y));
        }
    }
}