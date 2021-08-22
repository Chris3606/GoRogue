using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A class implementing the <see cref="IReadOnlyArea"/> interface, that derives its area from multiple
    /// "sub-areas".
    /// </summary>
    [PublicAPI]
    public class MultiArea : IReadOnlyArea
    {
        private readonly List<IReadOnlyArea> _subAreas;

        /// <summary>
        /// List of all sub-areas in the MultiArea.
        /// </summary>
        public IReadOnlyList<IReadOnlyArea> SubAreas => _subAreas.AsReadOnly();

        // TODO: Modify to be in ExpandToFit function in Rectangle
        /// <summary>
        /// Smallest possible rectangle that encompasses every position in every sub-area.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                if (_subAreas.Count == 0) return Rectangle.Empty;

                var firstBounds = _subAreas[0].Bounds;
                int minX = firstBounds.MinExtentX;
                int minY = firstBounds.MinExtentY;
                int maxX = firstBounds.MaxExtentX;
                int maxY = firstBounds.MaxExtentY;

                for (int i = 1; i < _subAreas.Count; i++)
                {
                    var currentBounds = _subAreas[i].Bounds;
                    if (minX > currentBounds.MinExtentX) minX = currentBounds.MinExtentX;
                    if (minY > currentBounds.MinExtentY) minY = currentBounds.MinExtentY;
                    if (maxX < currentBounds.MaxExtentX) maxX = currentBounds.MaxExtentX;
                    if (maxY < currentBounds.MaxExtentY) maxY = currentBounds.MaxExtentY;
                }

                return new Rectangle(new Point(minX, minY), new Point(maxX, maxY));
            }
        }

        /// <summary>
        /// Number of positions in all of this area's sub-areas combined.
        /// </summary>
        public int Count => _subAreas.Sum(area => area.Count);

        /// <summary>
        /// Returns positions from the area (via its sub-areas) in the same fashion you would via a list.
        /// </summary>
        /// <remarks>
        /// The indexing scheme considers index 0 to be index 0 in the first sub-area in <see cref="SubAreas"/>.
        /// The indices proceed in increasing order across all points in that sub-area, then roll over into the next
        ///  one.  Eg. index [SubAreas[0].Count] is actually index 0 in the second sub-area, ie. SubAreas[1][0].
        /// </remarks>
        /// <param name="index">Index of position to retrieve.</param>
        public Point this[int index]
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < _subAreas.Count; i++)
                {
                    var area = _subAreas[i];
                    if (sum + area.Count > index)
                        return area[index - sum];

                    sum += area.Count;
                }

                throw new ArgumentOutOfRangeException(nameof(index), "Index given is not valid.");
            }
        }

        /// <summary>
        /// Creates an area with no points/subareas.
        /// </summary>
        public MultiArea()
        {
            _subAreas = new List<IReadOnlyArea>();
        }

        /// <summary>
        /// Creates a MultiArea that has only the given sub-area.
        /// </summary>
        /// <param name="area">Sub-area to add.</param>
        public MultiArea(IReadOnlyArea area)
            : this(area.Yield())
        { }

        /// <summary>
        /// Creates a multi-area that is comprised of the given sub-areas.
        /// </summary>
        /// <param name="areas">Sub-areas to add.</param>
        public MultiArea(IEnumerable<IReadOnlyArea> areas) => _subAreas = new List<IReadOnlyArea>(areas);

        /// <summary>
        /// Adds the given sub-area to the MultiArea.
        /// </summary>
        /// <param name="subArea">The sub-area to add.</param>
        public void Add(IReadOnlyArea subArea) => _subAreas.Add(subArea);

        /// <summary>
        /// Adds the given sub-areas to the MultiArea.
        /// </summary>
        /// <param name="subAreas">The sub-areas to add.</param>
        public void AddRange(IEnumerable<IReadOnlyArea> subAreas) => _subAreas.AddRange(subAreas);

        /// <summary>
        /// Clears all sub-areas from the MultiArea.
        /// </summary>
        public void Clear() => _subAreas.Clear();

        /// <summary>
        /// Removes the given sub-area from the MultiArea.
        /// </summary>
        /// <param name="subArea">The sub-area to remove.</param>
        public void Remove(IReadOnlyArea subArea) => _subAreas.Remove(subArea);

        // TODO: Make this in the primitives lib an extension method or default interface method; it's a copy-paste of Area.
        /// <summary>
        /// Compares for equality. Returns true if the two areas contain exactly the same points.
        /// </summary>
        /// <param name="other"/>
        /// <returns>True if the areas contain exactly the same points, false otherwise.</returns>
        public bool Matches(IReadOnlyArea? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            // Quick checks that can short-circuit a function that would otherwise require looping over all points
            if (Count != other.Count)
                return false;

            if (Bounds != other.Bounds)
                return false;

            foreach (Point pos in this)
                if (!other.Contains(pos))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns an enumerator that iterates through all positions in all sub-areas.
        /// </summary>
        /// <returns>An enumerator that iterates through all positions in all sub-areas.</returns>
        public IEnumerator<Point> GetEnumerator()
        {
            foreach (var area in _subAreas)
            {
                foreach (var point in area)
                    yield return point;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through all positions in all sub-areas.
        /// </summary>
        /// <returns>An enumerator that iterates through all positions in all sub-areas.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns whether or not the given area is completely contained within the summation of this area's subareas.
        /// </summary>
        /// <param name="area">Area to check.</param>
        /// <returns>
        /// True if the all of the given area's points are contained within one or more subareas, false otherwise.
        /// </returns>
        public bool Contains(IReadOnlyArea area)
        {
            foreach (var pos in area)
            {
                // Try to find this point in one of this area's sub-areas
                bool found = false;
                for (var i = 0; i < _subAreas.Count; i++)
                {
                    var subarea = _subAreas[i];
                    if (subarea.Contains(pos))
                    {
                        found = true;
                        break;
                    }
                }

                // If we can't find this point in any sub-area, then the summation of the subareas does NOT contain
                // the area in question.
                if (!found)
                    return false;
            }

            // All points were found in at least one sub-area, so by definition, the summation of the subareas contains
            // the area in question.
            return true;
        }

        /// <summary>
        /// Determines whether or not the given position is considered within one of this area's subareas or not.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the specified position is within one of the subareas, false otherwise.</returns>
        public bool Contains(Point position)
        {
            for (int i = 0; i < _subAreas.Count; i++)
            {
                var subArea = _subAreas[i];
                if (subArea.Contains(position))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether or not the given position is considered within one of this area's subareas or not.
        /// </summary>
        /// <param name="positionX">X-value of the position to check.</param>
        /// <param name="positionY">X-value of the position to check.</param>
        /// <returns>True if the specified position is within one of the subareas, false otherwise.</returns>
        public bool Contains(int positionX, int positionY)
        {
            for (int i = 0; i < _subAreas.Count; i++)
            {
                var subArea = _subAreas[i];
                if (subArea.Contains(positionX, positionY))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether or not the given map area intersects any of this area's subareas. If you intend to
        /// determine/use the exact intersection based on this return value, it is best to instead
        /// call <see cref="Area.GetIntersection(IReadOnlyArea, IReadOnlyArea)"/>, and check the number
        /// of positions in the result (0 if no intersection).
        /// </summary>
        /// <param name="area">The area to check.</param>
        /// <returns>True if the given area intersects one of the current one's subareas, false otherwise.</returns>
        public bool Intersects(IReadOnlyArea area)
        {
            for (int i = 0; i < _subAreas.Count; i++)
            {
                var subArea = _subAreas[i];
                if (subArea.Intersects(area))
                    return true;
            }

            return false;
        }
    }
}
