﻿using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration.ContextComponents;
using JetBrains.Annotations;
using SadRogue.Primitives.GridViews;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Carves out non-overlapping parallelograms in a map.
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    [PublicAPI]
    public class ParallelogramGeneration : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the component used to store rooms generated by this algorithm.
        /// </summary>
        public readonly string? RoomsComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component used to set wall/floor status of tiles changed by this
        /// algorithm.
        /// </summary>
        public readonly string? WallFloorComponentTag;

        /// <summary>
        /// The width of a side of the parallelogram
        /// </summary>
        public int RoomWidth = 7;

        /// <summary>
        /// Vertical Height of the Parallelogram
        /// </summary>
        public int RoomHeight = 4;

        /// <summary>
        /// Creates a new grid of parallelograms.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="ParallelogramGeneration" />.</param>
        /// <param name="roomsComponentTag">
        /// Optional tag that must be associated with the component used to store rooms.  Defaults
        /// to "Rooms".
        /// </param>
        /// <param name="wallFloorComponentTag">
        /// Optional tag that must be associated with the map view component used to store/set
        /// floor/wall status.  Defaults to "WallFloor".
        /// </param>
        public ParallelogramGeneration(string? name = null, string? roomsComponentTag = "Parallelograms",
                               string? wallFloorComponentTag = "WallFloor")
            : base(name)
        {
            RoomsComponentTag = roomsComponentTag;
            WallFloorComponentTag = wallFloorComponentTag;
        }

        /// <inheritdoc />
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Get or create/add a wall-floor context component
            var wallFloorContext = context.GetFirstOrNew<ISettableGridView<bool>>(
                () => new ArrayView<bool>(context.Width, context.Height),
                WallFloorComponentTag
            );

            // Get or create/add a rooms context component
            var roomsContext = context.GetFirstOrNew(
                () => new ItemList<Region>(),
                RoomsComponentTag
            );

            for (int i = -5; i < wallFloorContext.Width; i += RoomWidth)
            {
                for (int j = 0; j < wallFloorContext.Height; j += RoomHeight)
                {
                    var region = Region.RegularParallelogram("parallelogram", (i,j), RoomWidth, RoomHeight, 0);
                    roomsContext.Add(region, Name);

                    foreach (var point in region.InnerPoints.Positions.Where(p => wallFloorContext.Contains(p)))
                        wallFloorContext[point] = true;

                    foreach (var point in region.OuterPoints.Positions.Where(p => wallFloorContext.Contains(p)))
                        wallFloorContext[point] = false;

                }

                yield return null;
            }
        }
    }
}
