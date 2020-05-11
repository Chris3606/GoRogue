using System;
using GoRogue.Random;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Carves out non-overlapping rooms in a map.  Rooms generated will not overlap with themselves, or any existing open areas in the map.
    ///
    /// Context Components Required:
    ///     - None
    ///
    /// Context Components Added:
    ///     - <see cref="ContextComponents.RoomsList"/>
    ///     - <see cref="ContextComponents.WallFloor"/> (if none is present -- existing one is used if one already exists)
    /// </summary>
    /// <remarks>
    /// This generation step generates rooms, and adds a <see cref="ContextComponents.RoomsList"/> context component that contains those rooms to the <see cref="GenerationContext"/>.
    /// It also sets the interior positions to true in the map context's <see cref="ContextComponents.WallFloor"/> context component.  If the GenerationContext has an existing WallFloor context
    /// component, that component is used.  If not, a WallFloor component is created and added to the map context, with it's <see cref="ContextComponents.WallFloor.View"/> property being an
    /// <see cref="MapViews.ArrayMap{T}"/> whose width/height match <see cref="GenerationContext.Width"/>/<see cref="GenerationContext.Height"/>.
    /// </remarks>
    public class RoomsGenerationStep : GenerationStep
    {
        /// <summary>
        /// RNG to use for room creation/placement.
        /// </summary>
        public IGenerator? RNG = null;

        /// <summary>
        /// Minimum amount of rooms to generate.  Defaults to 4.
        /// </summary>
        public int MinRooms = 4;

        /// <summary>
        /// Maximum amount of rooms to generate.  Defaults to 10.
        /// </summary>
        public int MaxRooms = 10;

        /// <summary>
        /// The minimum size allowed for rooms.  Rounded up to an odd number.  Defaults to 3.
        /// </summary>
        public int RoomMinSize = 3;

        /// <summary>
        /// The maximum size allowed for rooms.  Rounded up to an odd number.  Defaults to 7.
        /// </summary>
        public int RoomMaxSize = 7;

        /// <summary>
        /// The ratio of the room width to the height. Defaults to 1.0.
        /// </summary>
        public float RoomSizeRatioX = 1f;

        /// <summary>
        /// The ratio of the room height to the width. Defaults to 1.0.
        /// </summary>
        public float RoomSizeRatioY = 1f;

        /// <summary>
        /// The maximum times to re-generate a room that fails to place in a valid location before giving up on generating that room entirely.  Defaults to 10.
        /// </summary>
        public int MaxCreationAttempts = 10;

        /// <summary>
        /// The maximum times to attempt to place a room in a map without intersection, before giving up
        /// and re-generating that room. Defaults to 10.
        /// </summary>
        public int MaxPlacementAttempts = 10;

        /// <inheritdoc/>
        protected override void OnPerform(GenerationContext context)
        {
            // Use proper RNG
            if (RNG == null)
                RNG = SingletonRandom.DefaultRNG;

            // Validate configuration
            if (MinRooms > MaxRooms)
                throw new Exception("The minimum amount of rooms must be less than or equal to the maximum amount of rooms.");

            if (RoomMinSize > RoomMaxSize)
                throw new Exception("The minimum size of a room must be less than or equal to the maximum size of a room.");

            if (RoomSizeRatioX <= 0f)
                throw new Exception("X-value room size ratio must be greater than 0.");

            if (RoomSizeRatioY <= 0f)
                throw new Exception("Y-value room size ratio must be greater than 0.");

            // Add wall/floor component if one doesn't exist and retrieve it
            var wallFloorContext = context.GetComponent<ContextComponents.WallFloor>();
            if (wallFloorContext == null)
            {
                wallFloorContext = new ContextComponents.WallFloor(context);
                context.AddComponent(wallFloorContext);
            }
            
            // Determine how many rooms to generate
            var roomCounter = RNG.Next(MinRooms, MaxRooms + 1);

            // Create rooms context
            var roomsContext = new ContextComponents.RoomsList(roomCounter);

            // Try to place all the rooms
            while (roomCounter != 0)
            {
                int tryCounterCreate = MaxCreationAttempts;
                bool placed = false;

                // Attempt to create the room until either we reach max attempts or we create and place a room in a valid location
                while (tryCounterCreate != 0)
                {
                    int roomSize = RNG.Next(RoomMinSize, RoomMaxSize + 1);
                    int width = (int)(roomSize * RoomSizeRatioX);  // This helps with non square fonts. So rooms dont look odd
                    int height = (int)(roomSize * RoomSizeRatioY);

                    // When accounting for font ratios, these adjustments help prevent all rooms
                    // having the same looking square format
                    int adjustmentBase = roomSize / 4;

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

                    int tryCounterPlace = MaxPlacementAttempts;

                    // Try to place the room we've created until either it doesn't intersect any other rooms, or we reach max retries (in which case, we will scrap the room entirely and try again)
                    while (tryCounterPlace != 0)
                    {
                        int xPos = 0, yPos = 0;

                        // Generate the rooms at odd positions, to make door/tunnel placement easier
                        while (xPos % 2 == 0)
                            xPos = RNG.Next(3, wallFloorContext.View.Width - roomInnerRect.Width - 3);
                        while (yPos % 2 == 0)
                            yPos = RNG.Next(3, wallFloorContext.View.Height - roomInnerRect.Height - 3);

                        // Record a rectangle for the inner and outer bounds of the room we've created
                        roomInnerRect = roomInnerRect.WithPosition(new Point(xPos, yPos));
                        Rectangle roomBounds = roomInnerRect.Expand(3, 3);

                        // Check if the room intersects with any floor tile on the map already.  We do it this way instead of checking against only the rooms list
                        // to ensure that if some other map generation step placed things before we did, we don't intersect those.
                        bool intersected = false;
                        foreach (var point in roomBounds.Positions())
                        {
                            if (wallFloorContext.View[point])
                            {
                                intersected = true;
                                break;
                            }
                        }

                        // If we intersected floor tiles, try to place the room again
                        if (intersected)
                        {
                            tryCounterPlace--;
                            continue;
                        }

                        // Once we place it in a valid location, update the wall/floor context, and add the room to the list of rooms.
                        foreach (var point in roomInnerRect.Positions())
                            wallFloorContext.View[point] = true;

                        placed = true;
                        roomsContext.Rooms.Add(roomInnerRect);
                        break;
                    }

                    if (placed)
                        break;

                    tryCounterCreate--;
                }

                roomCounter--;
            }

            // Add RoomsList context to the map context
            context.AddComponent(roomsContext);
        }
    }
}
