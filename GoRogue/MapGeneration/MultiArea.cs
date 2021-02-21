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
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public int Count => _subAreas.Sum(area => area.Count);

        /// <inheritdoc/>
        public Point this[int index]
        {
            get
            {
                int sum = 0;
                foreach (var area in _subAreas)
                {
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

        /// <inheritdoc/>
        public bool Contains(IReadOnlyArea area)
        {
            foreach (var subArea in _subAreas)
                if (subArea.Contains(area))
                    return true;

            return false;
        }

        /// <inheritdoc/>
        public bool Contains(Point position)
        {
            foreach (var subArea in _subAreas)
                if (subArea.Contains(position))
                    return true;

            return false;
        }

        /// <inheritdoc/>
        public bool Contains(int positionX, int positionY)
        {
            foreach (var subArea in _subAreas)
                if (subArea.Contains(positionX, positionY))
                    return true;

            return false;
        }

        /// <inheritdoc/>
        public bool Intersects(IReadOnlyArea area)
        {
            foreach (var subArea in _subAreas)
                if (subArea.Intersects(area))
                    return true;

            return false;
        }
    }
}
