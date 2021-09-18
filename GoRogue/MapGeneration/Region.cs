using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoRogue.Components;
using GoRogue.Components.ParentAware;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A region of the map with four sides of arbitrary shape and size
    /// </summary>
    [PublicAPI]
    public class Region : IObjectWithComponents, IReadOnlyArea
    {
        /// <summary>
        /// The Area of this region
        /// </summary>
        public PolygonArea Area { get; set; }

        /// <inheritdoc/>
        public IComponentCollection GoRogueComponents { get; }

        /// <summary>
        /// Creates a new Region, with corners at the provided points
        /// </summary>
        /// <param name="corners">Each corner of the polygon, which is copied into a new list</param>
        /// <param name="algorithm">Which Line Algorithm to use</param>
        /// <param name="components"></param>
        public Region(IEnumerable<Point> corners, Lines.Algorithm algorithm = Lines.Algorithm.DDA, IComponentCollection? components = null)
            : this(corners.ToList(), algorithm, components) { }

        /// <summary>
        /// Creates a new Region, with corners at the provided points
        /// </summary>
        /// <param name="corners">The corners of this region</param>
        /// <param name="algorithm">Which Line Algorithm to use</param>
        /// <param name="components">A collection of components to add to this region</param>
        public Region(ref List<Point> corners, Lines.Algorithm algorithm = Lines.Algorithm.DDA, IComponentCollection? components = null)
            : this(corners, algorithm, components) { }

        /// <summary>
        /// Returns a new Region with corners at the provided points.
        /// </summary>
        /// <param name="algorithm">Which Line-drawing algorithm to use</param>
        /// <param name="corners">The points which are corners for this region</param>
        public Region(Lines.Algorithm algorithm, params Point[] corners)
            : this(corners, algorithm) { }

        /// <summary>
        /// Returns a new Region with corners at the provided points.
        /// </summary>
        /// <param name="components">A component collection to use for this region</param>
        /// <param name="corners">The points which are corners for this region</param>
        public Region(IComponentCollection? components, params Point[] corners)
            : this(corners, Lines.Algorithm.DDA, components) { }

        /// <summary>
        /// Returns a new Region with corners at the provided points.
        /// </summary>
        /// <param name="algorithm">Which Line-drawing algorithm to use</param>
        /// <param name="components">A component collection for this region</param>
        /// <param name="corners">The points which are corners for this region</param>
        public Region(Lines.Algorithm algorithm, IComponentCollection? components, params Point[] corners)
            : this(corners, algorithm, components) { }

        /// <summary>
        /// Returns a new Region with corners at the provided points, using the algorithm DDA to produce lines
        /// </summary>
        /// <param name="corners">The corners of the region</param>
        public Region(params Point[] corners) : this(corners, Lines.Algorithm.DDA) { }

        private Region(List<Point> corners, Lines.Algorithm algorithm, IComponentCollection? components)
        {
            Area = new PolygonArea(ref corners, algorithm);
            GoRogueComponents = components ?? new ComponentCollection();
            GoRogueComponents.ParentForAddedComponents = this;
        }

        #region Properties

        /// <summary>
        /// The left-most X-value of the region's four corners
        /// </summary>
        public int Left => Area.Left;

        /// <summary>
        /// The right-most X-value of the region's four corners
        /// </summary>
        public int Right => Area.Right;

        /// <summary>
        /// The top-most Y-value of the region's four corners
        /// </summary>
        public int Top => Area.Top;

        /// <summary>
        /// The bottom-most Y-value of the region's four corners
        /// </summary>
        public int Bottom => Area.Bottom;

        /// <summary>
        /// How Wide this region is
        /// </summary>
        public int Width => Area.Width;

        /// <summary>
        /// how tall this region is
        /// </summary>
        public int Height => Area.Height;

        /// <summary>
        /// The Center point of this region
        /// </summary>
        public Point Center => Area.Center;

        /// <inheritdoc />
        public Rectangle Bounds => Area.Bounds;

        /// <inheritdoc />
        public int Count => Area.Count;

        /// <inheritdoc />
        public Point this[int index] => Area[index];
        #endregion

        //Functions to access information about regions
        #region Data Access Functions

        /// <summary>
        /// Returns a string detailing the region's corner locations.
        /// </summary>
        public override string ToString()
        {
            var answer = new StringBuilder("Region with ");
            answer.Append($"{GoRogueComponents.Count} components and the following ");
            answer.Append(Area);
            return answer.ToString();
        }

        /// <summary>
        /// Is this Point one of the corners of the Region?
        /// </summary>
        /// <param name="position">the point to evaluate</param>
        public bool IsCorner(Point position) => Area.IsCorner(position);

        /// <summary>
        /// The value of the left-most Point in the region at elevation y
        /// </summary>
        /// <param name="y">The elevation to evaluate</param>
        /// <returns>The X-value of a Point</returns>
        public int LeftAt(int y) => Area.LeftAt(y);

        /// <summary>
        /// The value of the right-most Point in the region at elevation y
        /// </summary>
        /// <param name="y">The elevation to evaluate</param>
        /// <returns>The X-value of a Point</returns>
        public int RightAt(int y) => Area.RightAt(y);

        /// <summary>
        /// The value of the top-most Point in the region at longitude x
        /// </summary>
        /// <param name="x">The longitude to evaluate</param>
        /// <returns>The Y-value of a Point</returns>
        public int TopAt(int x) => Area.TopAt(x);

        /// <summary>
        /// The value of the bottom-most Point in the region at longitude x
        /// </summary>
        /// <param name="x">The longitude to evaluate</param>
        /// <returns>The Y-value of a Point</returns>
        public int BottomAt(int x) => Area.BottomAt(x);

        /// <inheritdoc />
        public bool Matches(IReadOnlyArea? area) => Area.Matches(area);

        /// <inheritdoc />
        public bool Contains(IReadOnlyArea area) => Area.Contains(area);

        /// <inheritdoc />
        public bool Contains(Point position) => Area.Contains(position);

        /// <inheritdoc />
        public bool Contains(int positionX, int positionY) => Contains((positionX, positionY));

        /// <inheritdoc />
        public bool Intersects(IReadOnlyArea area) => Area.Intersects(area);

        /// <inheritdoc />
        public IEnumerator<Point> GetEnumerator() => Area.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => Area.GetEnumerator();
        #endregion
    }
}
