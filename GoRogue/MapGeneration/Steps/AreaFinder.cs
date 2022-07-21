using System.Collections.Generic;
using GoRogue.MapGeneration.ContextComponents;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Finds the distinct areas in the boolean grid view specified, and adds them to the item list with the tag
    /// specified.
    /// </summary>
    [PublicAPI]
    public class AreaFinder : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the grid view used to find areas.
        /// </summary>
        public readonly string? GridViewComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component used to store areas found by this algorithm.
        /// </summary>
        public readonly string? AreasComponentTag;

        /// <summary>
        /// The adjacency method to use for determining whether two locations are in the same area.
        /// </summary>
        public AdjacencyRule AdjacencyMethod = AdjacencyRule.Cardinals;

        /// <summary>
        /// Creates a new AreaFinder generation step.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="AreaFinder"/></param>
        /// <param name="gridViewComponentTag">
        /// Optional tag that must be associated with the grid view used to find areas.
        /// Defaults to "WallFloor".
        /// </param>
        /// <param name="areasComponentTag">
        /// Optional tag that must be associated with the component used to store areas found by this algorithm.
        /// Defaults to "Areas".
        /// </param>
        public AreaFinder(string? name = null, string? gridViewComponentTag = "WallFloor",
                          string? areasComponentTag = "Areas")
            : base(name, (typeof(IGridView<bool>), gridViewComponentTag))
        {
            AreasComponentTag = areasComponentTag;
            GridViewComponentTag = gridViewComponentTag;
        }

        /// <inheritdoc/>
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Get/create required components
            var gridView = context.GetFirst<IGridView<bool>>(GridViewComponentTag); // Known to succeed because required
            var areas = context.GetFirstOrNew(() => new ItemList<Area>(), AreasComponentTag);

            // Use MapAreaFinder to find unique areas and record them in the correct component
            areas.AddRange(MapAreaFinder.MapAreasFor(gridView, AdjacencyMethod), Name);

            yield break;
        }
    }
}
