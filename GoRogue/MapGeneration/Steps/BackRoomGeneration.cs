﻿using System.Collections.Generic;
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
    /// Creates a series of irregularly-sized rectangles.
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    [PublicAPI]
    public class BackRoomGeneration : GenerationStep
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
        /// RNG to use for maze generation.
        /// </summary>
        public IGenerator RNG = GlobalRandom.DefaultRNG;

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
        public BackRoomGeneration(string? name = null, string? roomsComponentTag = "Parallelograms",
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
                () => new ItemList<Rectangle>(),
                RoomsComponentTag
            );

            var largeRooms = new List<Rectangle>();

            int thirdWidth = wallFloorContext.Width / 3;
            int halfWidth = wallFloorContext.Width / 2;
            int thirdHeight = wallFloorContext.Height / 3;
            int halfHeight = wallFloorContext.Height / 2;

            if (RNG.Next(0, 2) % 2 == 0)
            {
                if (RNG.Next(0, 2) % 2 == 0)
                {
                    largeRooms.Add(new Rectangle(halfWidth - 1, 0, halfWidth, wallFloorContext.Height));
                    largeRooms.Add(new Rectangle(0, 0, halfWidth, thirdHeight));
                    largeRooms.Add(new Rectangle(0, thirdHeight - 1, halfWidth, thirdHeight));
                    largeRooms.Add(new Rectangle(0, thirdHeight * 2 - 1, halfWidth, thirdHeight));
                }
                else
                {
                    largeRooms.Add(new Rectangle(0, 0, halfWidth, wallFloorContext.Height));
                    largeRooms.Add(new Rectangle(halfWidth - 1, 0, halfWidth, thirdHeight));
                    largeRooms.Add(new Rectangle(halfWidth - 1, thirdHeight - 1, halfWidth, thirdHeight));
                    largeRooms.Add(new Rectangle(halfWidth - 1, thirdHeight * 2 - 1, halfWidth, thirdHeight));
                }
            }
            else
            {
                if (RNG.Next(0, 2) % 2 == 0)
                {
                    largeRooms.Add(new Rectangle(0,0, wallFloorContext.Width, halfHeight));
                    largeRooms.Add(new Rectangle(0, halfHeight - 1, thirdWidth, halfHeight));
                    largeRooms.Add(new Rectangle(thirdWidth - 1, halfHeight - 1, thirdWidth, halfHeight));
                    largeRooms.Add(new Rectangle(thirdWidth * 2 - 1, halfHeight - 1, thirdWidth, halfHeight));
                }
                else
                {
                    largeRooms.Add(new Rectangle(0,halfHeight - 1, wallFloorContext.Width, halfHeight));
                    largeRooms.Add(new Rectangle(0, 0, thirdWidth, halfHeight));
                    largeRooms.Add(new Rectangle(thirdWidth - 1, 0, thirdWidth, halfHeight));
                    largeRooms.Add(new Rectangle(thirdWidth * 2 - 1, 0, thirdWidth, halfHeight));
                }
            }

            foreach(var rectangle in largeRooms)
                roomsContext.AddRange(rectangle.BisectRecursive(RNG.Next(3,9)), Name);

            foreach (var room in roomsContext)
            {
                foreach (var point in room.Item.Positions())
                {
                    if (point.X == room.Item.MinExtentX || point.Y == room.Item.MaxExtentY)
                        wallFloorContext[point] = false;
                    else
                        wallFloorContext[point] = true;
                }

                yield return null;
            }

            yield return null;
        }
    }
}