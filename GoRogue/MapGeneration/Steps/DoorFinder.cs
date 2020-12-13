using System.Collections.Generic;
using GoRogue.MapGeneration.ContextComponents;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Finds the locations of open walls in rectangular rooms that constitute doorways.
    /// </summary>
    [PublicAPI]
    public class DoorFinder : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the grid view used to find openings in room walls.
        /// </summary>
        public readonly string? WallFloorComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component used to store rectangular rooms that it generates
        /// door locations for.
        /// </summary>
        public readonly string? RoomsComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component created/used to record the door locations found by
        /// this algorithm.
        /// </summary>
        public readonly string? DoorsListComponentTag;

        /// <summary>
        /// Creates a door finder generation step.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="DoorFinder"/></param>
        /// <param name="wallFloorComponentTag">
        /// Optional tag that must be associated with the grid view used to find whether room walls are open.
        /// Defaults to "WallFloor".
        /// </param>
        /// <param name="roomsComponentTag">
        /// Optional tag that must be associated with the component used to store the rectangular rooms this algorithm
        /// finds openings for.  Defaults to "Rooms".
        /// </param>
        /// <param name="doorsListComponentTag">
        /// Optional tag that must be associated with the component created/used to record the door locations found
        /// by this algorithm.  Defaults to "Doors".
        /// </param>
        public DoorFinder(string? name = null, string? wallFloorComponentTag = "WallFloor",
                          string? roomsComponentTag = "Rooms", string? doorsListComponentTag = "Doors")
            : base(name,
                (typeof(IGridView<bool>), wallFloorComponentTag),
                                      (typeof(ItemList<Rectangle>), roomsComponentTag))
        {
            WallFloorComponentTag = wallFloorComponentTag;
            RoomsComponentTag = roomsComponentTag;
            DoorsListComponentTag = doorsListComponentTag;
        }

        /// <inheritdoc/>
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Get required components; guaranteed to exist because enforced by required components list
            var wallFloor = context.GetFirst<IGridView<bool>>(WallFloorComponentTag);
            var roomsList = context.GetFirst<ItemList<Rectangle>>(RoomsComponentTag);

            // Get/create doors component
            var doorsList = context.GetFirstOrNew(() => new DoorList(), DoorsListComponentTag);

            // Go through each room and add door locations for it
            foreach (var room in roomsList.Items)
            {
                foreach (var perimeterPos in room.Expand(1, 1).PerimeterPositions())
                    if (wallFloor[perimeterPos])
                        doorsList.AddDoor(Name, room, perimeterPos);

                yield return null;
            }
        }
    }
}
