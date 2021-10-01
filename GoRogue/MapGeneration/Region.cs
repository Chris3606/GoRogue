using System;
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
    /// Event fired when a region's Area is changed.
    /// </summary>
    [PublicAPI]
    public class RegionAreaChanged : EventArgs
    {
        /// <summary>
        /// The region whose area is changing
        /// </summary>
        public readonly Region Item;

        /// <summary>
        /// The previous Area
        /// </summary>
        public readonly Area Old;

        /// <summary>
        /// The Area to which we update
        /// </summary>
        public readonly Area New;

        /// <summary>
        /// The Event Arguments for when a region's area is changed
        /// </summary>
        /// <param name="item">The region whose area is changing</param>
        /// <param name="oldArea">The former value of the Area</param>
        /// <param name="newArea">The new value of the Area</param>
        public RegionAreaChanged(Region item, Area oldArea, Area newArea)
        {
            Item = item;
            Old = oldArea;
            New = newArea;
        }
    }
    /// <summary>
    /// A region of the map with four sides of arbitrary shape and size
    /// </summary>
    [PublicAPI]
    public class Region : IObjectWithComponents
    {
        /// <summary>
        /// The Area of this region
        /// </summary>
        public PolygonArea Area { get; set; }

        /// <summary>
        /// Fired when the Area is changed.
        /// </summary>
        public EventHandler<RegionAreaChanged>? AreaChanged;

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

        /// <summary>
        /// Returns a new Region using the provided area
        /// </summary>
        /// <param name="area">This region's Area</param>
        /// <param name="components">A component collection for this region</param>
        public Region(PolygonArea area, IComponentCollection? components = null)
        {
            Area = area;
            GoRogueComponents = components ?? new ComponentCollection();
            GoRogueComponents.ParentForAddedComponents = this;
        }

        private Region(List<Point> corners, Lines.Algorithm algorithm, IComponentCollection? components)
        {
            Area = new PolygonArea(ref corners, algorithm);
            GoRogueComponents = components ?? new ComponentCollection();
            GoRogueComponents.ParentForAddedComponents = this;
        }

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
    }
}
