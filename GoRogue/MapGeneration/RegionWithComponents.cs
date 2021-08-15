using GoRogue.Components;
using GoRogue.Components.ParentAware;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A Class which contains a Region, and a Component Collection.
    /// </summary>
    public class RegionWithComponents : IObjectWithComponents
    {
        /// <summary>
        /// The region containing the components
        /// </summary>
        public Region Region;

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
    }
}
