using System.Collections.Generic;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives.GridViews;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Randomly fills a boolean <see cref="SadRogue.Primitives.GridViews.IGridView{T}"/> with true/false values.  Creates a grid view
    /// with the given tag if none is present.
    /// </summary>
    [PublicAPI]
    public class RandomViewFill : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the grid view that random values are set to.
        /// </summary>
        public readonly string? GridViewComponentTag;

        /// <summary>
        /// The RNG to use for filling the view.
        /// </summary>
        public IEnhancedRandom RNG = GlobalRandom.DefaultRNG;

        /// <summary>
        /// Represents the percent chance that a given cell will be a floor cell when the map is
        /// initially randomly filled.
        /// </summary>
        public ushort FillProbability = 40;

        /// <summary>
        /// Whether or not to exclude the perimeter points from the random fill.
        /// </summary>
        public bool ExcludePerimeterPoints = true;

        /// <summary>
        /// How many squares to fill before yield returning for a pause.  Defaults to no pauses (0).
        /// </summary>
        public uint FillsBetweenPauses;

        /// <summary>
        /// Creates a new step for applying random values to a map view.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="RandomViewFill" />.</param>
        /// <param name="gridViewComponentTag">
        /// Optional tag that must be associated with the grid view that random values are set to.  Defaults to
        /// "WallFloor".
        /// </param>
        public RandomViewFill(string? name = null, string? gridViewComponentTag = "WallFloor")
            : base(name)
        {
            GridViewComponentTag = gridViewComponentTag;
        }

        /// <inheritdoc/>
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Validate configuration
            if (FillProbability > 100)
                throw new InvalidConfigurationException(this, nameof(FillProbability),
                    "The value must be a valid percent (between 0 and 100).");

            // Get or create/add a grid view context component to fill
            var gridViewContext = context.GetFirstOrNew<ISettableGridView<bool>>(
                () => new ArrayView<bool>(context.Width, context.Height),
                GridViewComponentTag);

            // Determine positions to fill based on exclusion settings
            var positionsRect = ExcludePerimeterPoints
                ? gridViewContext.Bounds().Expand(-1, -1)
                : gridViewContext.Bounds();

            // Fill each position with a random value
            uint squares = 0;
            foreach (var position in positionsRect.Positions())
            {
                gridViewContext[position] = RNG.PercentageCheck(FillProbability);
                squares++;
                if (FillsBetweenPauses != 0 && squares == FillsBetweenPauses)
                {
                    squares = 0;
                    yield return null;
                }
            }

            // Pause one last time if we need
            if (FillsBetweenPauses != 0 && squares != 0)
                yield return null;
        }
    }
}
