using GoRogue.MapViews;
using JetBrains.Annotations;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Produces a very simple map that is entirely floor, with a single-thick outline of walls around the outside.
    ///
    /// Context Components Required:
    /// - None
    /// Context Components Added/Used:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Component</term>
    ///         <description>Default Tag</description>
    ///     </listheader>
    ///     <item>
    ///         <term><see cref="ISettableMapView{T}" /> where T is bool</term>
    ///         <description>"WallFloor"</description>
    ///     </item>
    /// </list>
    ///
    /// An existing wall-floor component used if one is present; if not, a new one is added.
    /// </summary>
    /// <remarks>
    /// This generation step simply turns the map into a giant rectangular room.  It sets the interior positions to
    /// true, and outer-edge points to false, in the map context's map view with the given tag.  If the
    /// GenerationContext has an existing map view context component, that component is used.  If not, an
    /// <see cref="MapViews.ArrayMap{T}" /> where T is bool is created and added to the map context, whose width/height
    /// match <see cref="GenerationContext.Width" />/<see cref="GenerationContext.Height" />.
    /// </remarks>
    [PublicAPI]
    public class RectangleGenerator : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the component used to set wall/floor status of tiles changed by this
        /// algorithm.
        /// </summary>
        public readonly string? WallFloorComponentTag;

        /// <summary>
        /// Creates a new rectangle map generation step.
        /// </summary>
        /// <param name="wallFloorComponentTag">
        /// Optional tag that must be associated with the map view component used to store/set
        /// floor/wall status.  Defaults to "WallFloor".
        /// </param>
        public RectangleGenerator(string? wallFloorComponentTag = "WallFloor")
        {
            WallFloorComponentTag = wallFloorComponentTag;
        }

        /// <inheritdoc/>
        protected override void OnPerform(GenerationContext context)
        {
            // Get or create/add a wall-floor context component
            var wallFloorContext = context.GetComponentOrNew<ISettableMapView<bool>>(
                () => new ArrayMap<bool>(context.Width, context.Height),
                WallFloorComponentTag
            );

            var innerBounds = wallFloorContext.Bounds().Expand(-1, -1);
            foreach (var position in wallFloorContext.Positions())
                wallFloorContext[position] = innerBounds.Contains(position);
        }
    }
}
