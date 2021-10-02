using System;
using System.Text;
using GoRogue.Components;
using GoRogue.Components.ParentAware;
using JetBrains.Annotations;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Event fired when a region's Area is changed.
    /// </summary>
    [PublicAPI]
    public class RegionAreaChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The previous Area
        /// </summary>
        public readonly PolygonArea OldValue;

        /// <summary>
        /// The Area to which we update
        /// </summary>
        public readonly PolygonArea NewValue;

        /// <summary>
        /// The Event Arguments for when a region's area is changed
        /// </summary>
        /// <param name="oldValue">The former value of the Area</param>
        /// <param name="newValue">The new value of the Area</param>
        public RegionAreaChangedEventArgs(PolygonArea oldValue, PolygonArea newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
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
        public PolygonArea Area
        {
            get => _area;
            set
            {
                if (value == _area) return;

                var oldValue = _area;
                _area = value;
                AreaChanged?.Invoke(this, new RegionAreaChangedEventArgs(oldValue, value));
            }
        }
        private PolygonArea _area;

        /// <summary>
        /// Fired when the Area is changed.
        /// </summary>
        public EventHandler<RegionAreaChangedEventArgs>? AreaChanged;

        /// <inheritdoc/>
        public IComponentCollection GoRogueComponents { get; }

        /// <summary>
        /// Returns a new Region using the provided area
        /// </summary>
        /// <param name="area">This region's Area</param>
        /// <param name="components">A component collection for this region</param>
        public Region(PolygonArea area, IComponentCollection? components = null)
        {
            _area = area;
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
