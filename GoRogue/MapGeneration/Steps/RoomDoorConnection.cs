using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapViews;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Selects and opens walls of rectangular rooms to connect them to adjacent open spaces (typically mazes/tunnels).
    /// Components Required:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Component</term>
    ///         <description>Default Tag</description>
    ///     </listheader>
    ///     <item>
    ///         <term>
    ///             <see cref="ItemList{TItem}" />
    ///         </term>
    ///         <description>"Rooms"</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ISettableGridView{T}" /> where T is bool</term>
    ///         <description>"WallFloor"</description>
    ///     </item>
    /// </list>
    /// Components Added/Used:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Component</term>
    ///         <description>Default Tag</description>
    ///     </listheader>
    ///     <item>
    ///         <term>
    ///             <see cref="DoorList" />
    ///         </term>
    ///         <description>"Doors"</description>
    ///     </item>
    /// </list>
    /// In the case of the DoorsList component, an existing component is used if an appropriate one is present; a new one is
    /// added if not.
    /// </summary>
    /// <remarks>
    /// This algorithm goes through each room specified in the <see cref="ItemList{Rectangle}" /> context component, and
    /// selects a random number of sides
    /// to place connections on (within the parameters specified).  For each side, it then selects randomly from the valid
    /// connection points on that side, and
    /// carves out the selected location by setting its value to true in the "WallFloor" map view, and adding it to the list of
    /// doors associated with the
    /// appropriate room in the <see cref="DoorList" /> context component.  It continues to select connection points on a side
    /// until the <see cref="CancelConnectionPlacementChance" />
    /// succeeds.  <see cref="CancelConnectionPlacementChance" /> is increased by
    /// <see cref="CancelConnectionPlacementChanceIncrease" /> each time a point is selected.
    /// The algorithm will never select two adjacent points on a side as connection points.  Similarly, it will never break
    /// through the edges of the map. If an existing
    /// <see cref="DoorList" /> component exists on the map context with the proper tag, that component is used to record the
    /// doors generated; if not, a new one is created.
    /// </remarks>
    [PublicAPI]
    public class RoomDoorConnection : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the component created/used to record the locations of doors created by this
        /// algorithm.
        /// </summary>
        public readonly string? DoorsListComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component that contains the rooms being connected by this algorithm.
        /// </summary>
        public readonly string? RoomsComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component used to set wall/floor status of tiles changed by this
        /// algorithm.
        /// </summary>
        public readonly string? WallFloorComponentTag;

        /// <summary>
        /// A chance out of 100 to cancel placing a door on a side after one has been placed (per side). Defaults to 70.
        /// </summary>
        public ushort CancelConnectionPlacementChance = 70;

        /// <summary>
        /// Increase the <see cref="CancelConnectionPlacementChance" /> value by this amount each time a door is placed (per side).
        /// Defaults to 10.
        /// </summary>
        public ushort CancelConnectionPlacementChanceIncrease = 10;

        /// <summary>
        /// A chance out of 100 to cancel selecting sides to process (per room). Defaults to 50.
        /// </summary>
        public ushort CancelSideConnectionSelectChance = 50;

        /// <summary>
        /// Maximum sides of each room to process. Defaults to 4.
        /// </summary>
        public ushort MaxSidesToConnect = 4;

        /// <summary>
        /// Minimum sides of each room to process.  Defaults to 1.
        /// </summary>
        public ushort MinSidesToConnect = 1;

        /// <summary>
        /// The RNG to use for connections.
        /// </summary>
        public IGenerator RNG = GlobalRandom.DefaultRNG;

        /// <summary>
        /// Creates a new maze generation step.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="RoomDoorConnection" />.</param>
        /// <param name="roomsComponentTag">
        /// Optional tag that must be associated with the component that contains the rooms being
        /// connected by this algorithm.  Defaults to "Rooms".
        /// </param>
        /// <param name="wallFloorComponentTag">
        /// Optional tag that must be associated with the component used to set wall/floor
        /// status of tiles changed by this algorithm.  Defaults to "WallFloor".
        /// </param>
        /// <param name="doorsListComponentTag">
        /// Optional tag that must be associated with the component created/used to record the
        /// locations of doors created by this algorithm.  Defaults to "Doors".
        /// </param>
        public RoomDoorConnection(string? name = null, string? roomsComponentTag = "Rooms",
                                  string? wallFloorComponentTag = "WallFloor", string? doorsListComponentTag = "Doors")
            : base(name, (typeof(ItemList<Rectangle>), roomsComponentTag),
                (typeof(ISettableGridView<bool>), wallFloorComponentTag))
        {
            RoomsComponentTag = roomsComponentTag;
            WallFloorComponentTag = wallFloorComponentTag;
            DoorsListComponentTag = doorsListComponentTag;
        }

        /// <inheritdoc />
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Validate configuration
            if (MaxSidesToConnect > 4 || MaxSidesToConnect <= 0)
                throw new InvalidConfigurationException(this, nameof(MaxSidesToConnect),
                    "The value must be in range [1, 4].");

            if (MinSidesToConnect > MaxSidesToConnect)
                throw new InvalidConfigurationException(this, nameof(MinSidesToConnect),
                    $"The value must be less than or equal to {nameof(MaxSidesToConnect)}.");

            if (CancelSideConnectionSelectChance > 100)
                throw new InvalidConfigurationException(this, nameof(CancelSideConnectionSelectChance),
                    "The value must be a valid percent (between 0 and 100).");

            if (CancelConnectionPlacementChance > 100)
                throw new InvalidConfigurationException(this, nameof(CancelConnectionPlacementChance),
                    "The value must be a valid percent (between 0 and 100).");

            if (CancelConnectionPlacementChanceIncrease > 100)
                throw new InvalidConfigurationException(this, nameof(CancelConnectionPlacementChanceIncrease),
                    "The value must be a valid percent (between 0 and 100).");

            // Get required components; guaranteed to exist because enforced by required components list
            var rooms = context.GetFirst<ItemList<Rectangle>>(RoomsComponentTag);
            var wallFloor = context.GetFirst<ISettableGridView<bool>>(WallFloorComponentTag);

            // Get rectangle of inner map bounds (the entire map except for the outer box that must remain all walls
            var innerMap = wallFloor.Bounds().Expand(-1, -1);

            // Get/create doors list component.
            var doorsList = context.GetFirstOrNew(() => new DoorList(), DoorsListComponentTag);

            /*
			- Get all valid points along a side
			- if point count for side is > 0
			  - mark side for placement
			- if total sides marked > max
			  - loop total sides > max
				- randomly remove side
			- if total sides marked > min
			  - loop sides
				- CHECK side placement cancel check OK
				  - un-mark side
				- if total sides marked == min
				  -break loop
			- Loop sides
			  - Loop points
				- If point passes availability (no already chosen point next to point)
				  - CHECK point placement OK
					- Add point to list
			*/

            foreach (var room in rooms.Items)
            {
                // Holds positions that are valid options to carve out as doors to this room.
                // We're recording wall positions, and the room rectangle is only interior, so
                // we expand by one so we can store positions that are walls
                var validPositions = new RectangleEdgePositionsList(room.Expand(1, 1));

                // For each side, add any valid carving positions
                foreach (var side in AdjacencyRule.Cardinals.DirectionsOfNeighbors())
                    foreach (var sidePosition in room.PositionsOnSide(side))
                    {
                        var wallPoint = sidePosition + side; // Calculate point of wall next to the current position
                        var testPoint =
                            wallPoint + side; // Keep going in that direction to see where an opening here would lead

                        // If this opening hasn't been carved out already, wouldn't lead to the edge of the map, and WOULD lead to a walkable tile,
                        // then it's a valid location for us to choose to carve a door
                        if (!wallFloor[wallPoint] && innerMap.Contains(testPoint) && wallFloor[testPoint])
                            validPositions.Add(wallPoint);
                    }

                // Any side with at least one valid carving position is a valid side to select to start
                var validSides = AdjacencyRule.Cardinals.DirectionsOfNeighbors()
                    .Where(side => validPositions[side].Count > 0).ToList();

                // If the total sides we can select from is greater than the maximum amount of sides we are allowed to select per room,
                // then we must randomly remove sides until we are within the max parameter
                while (validSides.Count > MaxSidesToConnect)
                    validSides.RemoveAt(validSides.RandomIndex(RNG));

                // If there are some extra sides that we could remove and still stay within the minimum sides parameter,
                // then check the side cancellation chance and remove if needed.
                if (validSides.Count > MinSidesToConnect)
                {
                    var sidesRemoved = 0;
                    for (var i = 0; i < validSides.Count; i++)
                    {
                        if (RNG.PercentageCheck(CancelSideConnectionSelectChance))
                        {
                            // Since None couldn't be a valid side to begin with, we just use it as a marker for deletion to avoid modifying while iterating
                            validSides[i] = Direction.None;
                            sidesRemoved++;
                        }

                        // We can't remove any more sides without violating minimum parameter, so stop checking sides for cancellation
                        if (validSides.Count - sidesRemoved == MinSidesToConnect)
                            break;
                    }

                    validSides.RemoveAll(side => side == Direction.None);
                }

                foreach (var side in validSides)
                {
                    var currentCancelPlacementChance = CancelConnectionPlacementChance;
                    var selectedAPoint = false;
                    // While there are still points to connect
                    while (validPositions[side].Count > 0)
                    {
                        // Select a position from the list
                        var newConnectionPoint = validPositions[side].RandomItem(RNG);
                        validPositions.Remove(newConnectionPoint);

                        // If point is by two valid walls, we'll carve it.  This might not be the case if we happened to select the point next to it
                        // previously
                        if (AdjacencyRule.Cardinals.Neighbors(newConnectionPoint)
                            .Count(pos => wallFloor.Contains(pos) && !wallFloor[pos]) >= 2)
                        {
                            doorsList.AddDoor(Name, room, newConnectionPoint);
                            wallFloor[newConnectionPoint] = true;
                            selectedAPoint = true;
                        }

                        // In either case, as long as we have at least one point selected on this side, we'll run the cancel chance to see if we're
                        // cancelling connection placement, and increase the chance of cancelling next iteration as needed
                        if (selectedAPoint)
                        {
                            if (RNG.PercentageCheck(currentCancelPlacementChance))
                                break;

                            currentCancelPlacementChance += CancelConnectionPlacementChanceIncrease;
                        }

                        yield return null;
                    }
                }
            }
        }
    }
}
