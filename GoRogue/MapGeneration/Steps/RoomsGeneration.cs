﻿using System;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapViews;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Steps
{
    // TODO: Fix docs on ratios
    /// <summary>
    /// Carves out non-overlapping rooms in a map.  Rooms generated will not overlap with themselves, or any existing open
    /// areas in the map.
    /// Context Components Required:
    /// - None
    /// Context Components Added/Used:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Component</term>
    ///         <description>Default Tag</description>
    ///     </listheader>
    ///     <item>
    ///         <term>
    ///             <see cref="ContextComponents.ItemList{Rectangle}" />
    ///         </term>
    ///         <description>"Rooms"</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ISettableMapView{T}" /> where T is bool</term>
    ///         <description>"WallFloor"</description>
    ///     </item>
    /// </list>
    /// In the case of both components, existing components are used if they are present; new ones are added if not.
    /// </summary>
    /// <remarks>
    /// This generation step generates rooms, and adds the rooms generated to the
    /// <see cref="ContextComponents.ItemList{Rectangle}" /> context component with the given tag
    /// in the <see cref="GenerationContext" />.  If such a component does not exist, a new one is created.  It also sets the
    /// interior positions to true in the map context's
    /// map view with the given tag.  If the GenerationContext has an existing map view context component, that component is
    /// used.  If not, an <see cref="MapViews.ArrayMap{T}" />
    /// where T is bool is created and added to the map context, whose width/height match
    /// <see cref="GenerationContext.Width" />/<see cref="GenerationContext.Height" />.
    /// </remarks>
    [PublicAPI]
    public class RoomsGeneration : GenerationStep
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
        /// The maximum times to re-generate a room that fails to place in a valid location before giving up on generating that
        /// room entirely.  Defaults to 10.
        /// </summary>
        public int MaxCreationAttempts = 10;

        /// <summary>
        /// The maximum times to attempt to place a room in a map without intersection, before giving up
        /// and re-generating that room. Defaults to 10.
        /// </summary>
        public int MaxPlacementAttempts = 10;

        /// <summary>
        /// Maximum amount of rooms to generate.  Defaults to 10.
        /// </summary>
        public int MaxRooms = 10;

        /// <summary>
        /// Minimum amount of rooms to generate.  Defaults to 4.
        /// </summary>
        public int MinRooms = 4;

        /// <summary>
        /// RNG to use for room creation/placement.
        /// </summary>
        public IGenerator RNG = GlobalRandom.DefaultRNG;

        /// <summary>
        /// The maximum size allowed for rooms.  Rounded up to an odd number.  Defaults to 7.
        /// </summary>
        public int RoomMaxSize = 7;

        /// <summary>
        /// The minimum size allowed for rooms.  Rounded up to an odd number.  Defaults to 3.
        /// </summary>
        public int RoomMinSize = 3;

        /// <summary>
        /// The ratio of the room width to the height. Defaults to 1.0.
        /// </summary>
        public float RoomSizeRatioX = 1f;

        /// <summary>
        /// The ratio of the room height to the width. Defaults to 1.0.
        /// </summary>
        public float RoomSizeRatioY = 1f;


        /// <summary>
        /// Creates a new rooms generation step.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="RoomsGeneration" />.</param>
        /// <param name="roomsComponentTag">
        /// Optional tag that must be associated with the component used to store rooms.  Defaults
        /// to "Rooms".
        /// </param>
        /// <param name="wallFloorComponentTag">
        /// Optional tag that must be associated with the map view component used to store/set
        /// floor/wall status.  Defaults to "WallFloor".
        /// </param>
        public RoomsGeneration(string? name = null, string? roomsComponentTag = "Rooms",
                               string? wallFloorComponentTag = "WallFloor")
            : base(name)
        {
            RoomsComponentTag = roomsComponentTag;
            WallFloorComponentTag = wallFloorComponentTag;
        }

        /// <inheritdoc />
        protected override void OnPerform(GenerationContext context)
        {
            // Validate configuration
            if (MinRooms > MaxRooms)
                throw new InvalidConfigurationException(this, nameof(MinRooms),
                    $"The value must be less than or equal to the value of {nameof(MaxRooms)}.");

            if (RoomMinSize > RoomMaxSize)
                throw new InvalidConfigurationException(this, nameof(RoomMinSize),
                    $"The value must be less than or equal to the value of ${nameof(RoomMaxSize)}.");

            if (RoomSizeRatioX <= 0f)
                throw new InvalidConfigurationException(this, nameof(RoomSizeRatioX),
                    "The value must be greater than 0.");

            if (RoomSizeRatioY <= 0f)
                throw new InvalidConfigurationException(this, nameof(RoomSizeRatioY),
                    "The value must be greater than 0.");

            // Get or create/add a wall-floor context component
            var wallFloorContext = context.GetFirstOrNew<ISettableMapView<bool>>(
                () => new ArrayMap<bool>(context.Width, context.Height),
                WallFloorComponentTag
            );

            // Determine how many rooms to generate
            var roomCounter = RNG.Next(MinRooms, MaxRooms + 1);

            // Get or create/add a rooms context component
            var roomsContext = context.GetFirstOrNew(
                () => new ItemList<Rectangle>(roomCounter),
                RoomsComponentTag
            );

            // Try to place all the rooms
            while (roomCounter != 0)
            {
                var tryCounterCreate = MaxCreationAttempts;
                var placed = false;

                // Attempt to create the room until either we reach max attempts or we create and place a room in a valid location
                while (tryCounterCreate != 0)
                {
                    var roomSize = RNG.Next(RoomMinSize, RoomMaxSize + 1);
                    var width =
                        (int)(roomSize * RoomSizeRatioX); // This helps with non square fonts. So rooms don't look odd
                    var height = (int)(roomSize * RoomSizeRatioY);

                    // When accounting for font ratios, these adjustments help prevent all rooms
                    // having the same looking square format
                    var adjustmentBase = roomSize / 4;

                    if (adjustmentBase != 0)
                    {
                        var adjustment = RNG.Next(-adjustmentBase, adjustmentBase + 1);
                        var adjustmentChance = RNG.Next(0, 2);

                        if (adjustmentChance == 0)
                            width += (int)(adjustment * RoomSizeRatioX);
                        else if (adjustmentChance == 1)
                            height += (int)(adjustment * RoomSizeRatioY);
                    }

                    width = Math.Max(RoomMinSize, width);
                    height = Math.Max(RoomMinSize, height);

                    // Keep room interior odd, helps with placement + tunnels around the outside.
                    if (width % 2 == 0)
                        width += 1;

                    if (height % 2 == 0)
                        height += 1;

                    var roomInnerRect = new Rectangle(0, 0, width, height);

                    var tryCounterPlace = MaxPlacementAttempts;

                    // Try to place the room we've created until either it doesn't intersect any other rooms, or we reach max retries (in which case, we will scrap the room entirely, create a new one, and try again)
                    while (tryCounterPlace != 0)
                    {
                        int xPos = 0, yPos = 0;

                        // Generate the rooms at odd positions, to make door/tunnel placement easier
                        while (xPos % 2 == 0)
                            xPos = RNG.Next(3, wallFloorContext.Width - roomInnerRect.Width - 3);
                        while (yPos % 2 == 0)
                            yPos = RNG.Next(3, wallFloorContext.Height - roomInnerRect.Height - 3);

                        // Record a rectangle for the inner and outer bounds of the room we've created
                        roomInnerRect = roomInnerRect.WithPosition(new Point(xPos, yPos));
                        var roomBounds = roomInnerRect.Expand(3, 3);

                        // Check if the room intersects with any floor tile on the map already.  We do it this way instead of checking against only the rooms list
                        // to ensure that if some other map generation step placed things before we did, we don't intersect those.
                        var intersected = false;
                        foreach (var point in roomBounds.Positions())
                            if (wallFloorContext[point])
                            {
                                intersected = true;
                                break;
                            }

                        // If we intersected floor tiles, try to place the room again
                        if (intersected)
                        {
                            tryCounterPlace--;
                            continue;
                        }

                        // Once we place it in a valid location, update the wall/floor context, and add the room to the list of rooms.
                        foreach (var point in roomInnerRect.Positions())
                            wallFloorContext[point] = true;

                        placed = true;
                        roomsContext.Add(roomInnerRect, Name);
                        break;
                    }

                    if (placed)
                        break;

                    tryCounterCreate--;
                }

                roomCounter--;
            }
        }
    }
}