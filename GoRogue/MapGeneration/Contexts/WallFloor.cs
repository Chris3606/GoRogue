using GoRogue.MapViews;

namespace GoRogue.MapGeneration.Contexts
{
    /// <summary>
    /// Context component for map generation containing a basic map of walls/floors, where false indicates wall, and true indicates floor.
    /// </summary>
    public class WallFloor
    {
        /// <summary>
        /// Wall/floor status for the map.
        /// </summary>
        public readonly ISettableMapView<bool> View;

        /// <summary>
        /// Creates a <see cref="WallFloor"/> with the given map view as <see cref="View"/>.
        /// </summary>
        /// <param name="wallFloorView">Map view indicating walls and floors.</param>
        public WallFloor(ISettableMapView<bool> wallFloorView)
        {
            View = wallFloorView;
        }

        /// <summary>
        /// Creates a <see cref="WallFloor"/> wherein the <see cref="View"/> will be an array map that as a size based on the
        /// <see cref="GenerationContext.Width"/> and <see cref="GenerationContext.Height"/> properties.
        /// </summary>
        /// <param name="context">Context this component is being added to.  Width and height are pulled from here.</param>
        public WallFloor(GenerationContext context)
        {
            View = new ArrayMap<bool>(context.Width, context.Height);
        }
    }
}
