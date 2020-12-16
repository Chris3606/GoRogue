﻿using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Uses a cellular automata smoothing algorithm to smooth areas on the given map.
    /// </summary>
    [PublicAPI]
    public class CellularAutomataAreaGeneration : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the component used to set wall/floor status of tiles changed by this
        /// algorithm.
        /// </summary>
        public readonly string? WallFloorComponentTag;

        /// <summary>
        /// The adjacency rule to use to determine the unique areas generated by this algorithm.
        /// </summary>
        public AdjacencyRule AreaAdjacencyRule = AdjacencyRule.Cardinals;

        /// <summary>
        /// Total number of times the cellular automata-based smoothing algorithm is executed.
        /// Recommended to be in range [2, 10].
        /// </summary>
        public int TotalIterations = 7;

        /// <summary>
        /// Total number of times the cellular automata smoothing variation that is more likely to
        /// result in "breaking up" large areas will be run before switching to the more standard
        /// nearest neighbors version. Must be less than or equal to <see cref="TotalIterations"/>.
        /// Recommended to be in range [2, 7].
        /// </summary>
        public int CutoffBigAreaFill = 4;

        /// <summary>
        /// Creates a new cellular automata based area generation step.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="CellularAutomataAreaGeneration" />.</param>
        ///
        /// <param name="wallFloorComponentTag">
        /// Optional tag that must be associated with the map view component used to store/set
        /// floor/wall status.  Defaults to "WallFloor".
        /// </param>
        public CellularAutomataAreaGeneration(string? name = null, string? wallFloorComponentTag = "WallFloor")
            : base(name)
        {
            WallFloorComponentTag = wallFloorComponentTag;
        }

        /// <inheritdoc />
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Validate configuration
            if (CutoffBigAreaFill > TotalIterations)
                throw new InvalidConfigurationException(this, nameof(CutoffBigAreaFill),
                    $"The value must be less than or equal to the value of {nameof(TotalIterations)}.");

            // Get or create/add a wall-floor context component
            var wallFloorContext = context.GetFirstOrNew<ISettableGridView<bool>>(
                () => new ArrayView<bool>(context.Width, context.Height),
                WallFloorComponentTag);

            // Create a new array map to use in the smoothing algorithms to temporarily store old values.
            // Allocating it here instead of in the smoothing minimizes allocations.
            var oldMap = new ArrayView<bool>(wallFloorContext.Width, wallFloorContext.Height);

            // Iterate over the generated values, smoothing them with the appropriate algorithm
            for (int i = 0; i < TotalIterations; i++)
            {
                CellAutoSmoothingAlgo(wallFloorContext, oldMap, i < CutoffBigAreaFill);
                yield return null;
            }

            // Fill to a rectangle to ensure the resulting areas are enclosed
            foreach (var pos in wallFloorContext.Bounds().PerimeterPositions())
                wallFloorContext[pos] = false;
        }

        private static void CellAutoSmoothingAlgo(ISettableGridView<bool> map, ArrayView<bool> oldMap, bool bigAreaFill)
        {
            // Record current state of the map so we can compare to it to determine nearest walls
            oldMap.ApplyOverlay(map);

            // Iterate over inner square only to avoid messing with outer walls
            foreach (var pos in map.Bounds().Expand(-1, -1).Positions())
            {
                if (CountWallsNear(oldMap, pos, 1) >= 5 || bigAreaFill && CountWallsNear(oldMap, pos, 2) <= 2)
                    map[pos] = false;
                else
                    map[pos] = true;
            }
        }

        private static int CountWallsNear(ArrayView<bool> map, Point centerPos, int distance)
        {
            int count = 0;

            foreach (var pos in Radius.Square.PositionsInRadius(centerPos, distance))
                if (map.Contains(pos) && pos != centerPos && !map[pos])
                    count += 1;

            return count;
        }
    }
}