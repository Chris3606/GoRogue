using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Carves a simple spiral tunnel through un-walkable terrain.
    /// </summary>
    /// <remarks>
    /// Best used as a part of a more complex generation algorithm.
    /// </remarks>
    [PublicAPI]
    public class SpiralGeneration : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the component used to set wall/floor status of tiles changed by this
        /// algorithm.
        /// </summary>
        public readonly string? WallFloorComponentTag;

        /// <summary>
        /// RNG to use for maze generation.
        /// </summary>
        public IGenerator RNG = GlobalRandom.DefaultRNG;

        /// <summary>
        /// Creates a new grid of parallelograms.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="ParallelogramGeneration" />.</param>
        /// <param name="wallFloorComponentTag">
        /// Optional tag that must be associated with the map view component used to store/set
        /// floor/wall status.  Defaults to "WallFloor".
        /// </param>
        public SpiralGeneration(string? name = null, string? wallFloorComponentTag = "WallFloor") : base(name)
        {
            WallFloorComponentTag = wallFloorComponentTag;
        }

        /// <inheritdoc />
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Get or create/add a wall-floor context component
            var map = context.GetFirstOrNew<ISettableGridView<bool>>(
                () => new ArrayView<bool>(context.Width, context.Height),
                WallFloorComponentTag
            );

            Point origin = (RNG.Next(0, map.Width), RNG.Next(0, map.Height));

            double increment = 0.01;
            for (double i = 0; i < map.Width * 3; i += increment)
            {
                Point here = origin + new PolarCoordinate(i/3, i);
                if (map.Contains(here))
                {
                    map[here] = true;
                }
            }

            yield return null;
        }
    }
}
