using System.Collections;
using System.Collections.Generic;
using GoRogue.Components;
using GoRogue.Components.ParentAware;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A Class which contains a Region, and a Component Collection.
    /// </summary>
    public class RegionWithComponents : IObjectWithComponents, IReadOnlyArea
    {
        /// <summary>
        /// The region containing the components
        /// </summary>
        public Region Region { get; private set; }

        /// <summary>
        /// The GoRogueComponents on this region
        /// </summary>
        public IComponentCollection GoRogueComponents { get; set; }

        /// <summary>
        /// Creates a new Region with Components
        /// </summary>
        /// <param name="northWestCorner"></param>
        /// <param name="northEastCorner"></param>
        /// <param name="southEastCorner"></param>
        /// <param name="southWestCorner"></param>
        /// <param name="components">The Component Collection</param>
        public RegionWithComponents(Point northWestCorner, Point northEastCorner, Point southEastCorner, Point southWestCorner, IComponentCollection? components = null)
        {
            Region = new Region(northWestCorner, northEastCorner, southEastCorner, southWestCorner);
            GoRogueComponents = components ?? new ComponentCollection();
        }

        /// <summary>
        /// Creates a new Region with Components
        /// </summary>
        /// <param name="region">The region</param>
        /// <param name="components">The components</param>
        public RegionWithComponents(Region region, IComponentCollection? components = null)
        {
            Region = region;
            GoRogueComponents = components ?? new ComponentCollection();
        }

        /// <inheritdoc/>
        public bool Matches(IReadOnlyArea other) => Region.Matches(other);

        /// <inheritdoc/>
        public IEnumerator<Point> GetEnumerator() => Region.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public bool Contains(IReadOnlyArea area) => Region.Contains(area);

        /// <inheritdoc/>
        public bool Contains(Point position) => Region.Contains(position);

        /// <inheritdoc/>
        public bool Contains(int positionX, int positionY) => Region.Contains(positionX, positionY);

        /// <inheritdoc/>
        public bool Intersects(IReadOnlyArea area) => Region.Intersects(area);

        /// <inheritdoc/>
        public Rectangle Bounds => Region.Bounds;

        /// <inheritdoc/>
        public int Count => Region.Count;

        /// <inheritdoc/>
        public Point this[int index] => Region[index];

        /// <summary>
        /// Shifts the region by a selected amount
        /// </summary>
        /// <param name="xy">The amount by which to shift</param>
        public void Translate(Point xy)
            => Region = Region.Translate(xy);

        /// <summary>
        /// Inverts the X/Y of the region, with respect to a diagonal line that passes through Point xy
        /// </summary>
        /// <param name="xy">A point intersecting the line around which to transpose</param>
        public void Transpose(Point xy)
            => Region = Region.Transpose(xy);

        /// <summary>
        /// Flips the region around an X-axis
        /// </summary>
        /// <param name="x">The line around which to flip</param>
        public void FlipHorizontal(int x)
            => Region = Region.FlipHorizontal(x);

        /// <summary>
        /// Flips the region around a Y-axis
        /// </summary>
        /// <param name="y">The line around which to flip</param>
        public void FlipVertical(int y)
            => Region = Region.FlipVertical(y);

        /// <summary>
        /// Rotates the Region
        /// </summary>
        /// <param name="degrees">Degree of rotation</param>
        /// <param name="origin">Point around which to rotate</param>
        public void Rotate(double degrees, Point origin)
            => Region = Region.Rotate(degrees, origin);
    }
}
