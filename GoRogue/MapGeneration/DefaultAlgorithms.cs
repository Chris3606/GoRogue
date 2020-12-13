using System.Collections.Generic;
using GoRogue.MapGeneration.ConnectionPointSelectors;
using GoRogue.MapGeneration.Steps;
using GoRogue.MapGeneration.Steps.Translation;
using GoRogue.MapGeneration.TunnelCreators;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A collection of functions that return pre-defined series of generation steps that generate particular types
    /// of maps.  For more customizable map generation, see documentation for the individual steps in <see cref="Steps" /> and
    /// use AddStep
    /// to add them to a generator.
    /// </summary>
    /// <remarks>
    /// These algorithms serve as a quick way to generate a map and demonstrate how generation steps may be used together.
    /// Feel free to look at the source, and copy onr or more of the generation steps into a custom generator.
    /// </remarks>
    [PublicAPI]
    public static class DefaultAlgorithms
    {
        /// <summary>
        /// Generates a basic dungeon by randomly sizing and placing rooms on the map, then connecting them with basic
        /// tunnels.
        /// </summary>
        /// <param name="rng">The RNG to use for map generation.  Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <param name="minRooms">Minimum amount of rooms to generate on the map.  Defaults to 4.</param>
        /// <param name="maxRooms">Maximum amount of rooms to generate on the map.  Defaults to 10.</param>
        /// <param name="roomMinSize">The minimum size allowed for generated rooms.  Rounded up to an odd number.  Defaults to 3.</param>
        /// <param name="roomMaxSize">The maximum size allowed for generated rooms.  Rounded up to an odd number.  Defaults to 7.</param>
        /// <param name="roomSizeRatioX">The ratio of the room width to the height for generated rooms. Defaults to 1.0.</param>
        /// <param name="roomSizeRatioY">The ratio of the room height to the width for generated rooms. Defaults to 1.0.</param>
        /// <param name="maxCreationAttempts">
        /// The maximum times to re-generate a room that fails to place in a valid location
        /// before giving up on generating that room entirely.  Defaults to 10.
        /// </param>
        /// <param name="maxPlacementAttempts">
        /// The maximum times to attempt to place a room in a map without intersection, before
        /// giving up and re-generating that room. Defaults to 10.
        /// </param>
        /// <param name="connectionPointSelector">
        /// The area connection strategy to use. Defaults to a <see cref="CenterBoundsConnectionPointSelector"/>.</param>
        /// <param name="tunnelCreator">
        /// The tunnel creation strategy to use. Defaults to <see cref="HorizontalVerticalTunnelCreator"/> using
        /// the given RNG.</param>
        /// <returns>A set of map generation steps that generate a map with rectangular rooms connected to each other.</returns>
        public static IEnumerable<GenerationStep> BasicRandomRoomsMapSteps(IGenerator? rng = null, int minRooms = 4,
                                                                      int maxRooms = 10, int roomMinSize = 3,
                                                                      int roomMaxSize = 7, float roomSizeRatioX = 1f,
                                                                      float roomSizeRatioY = 1f,
                                                                      int maxCreationAttempts = 10,
                                                                      int maxPlacementAttempts = 10,
                                                                      IConnectionPointSelector? connectionPointSelector = null,
                                                                      ITunnelCreator? tunnelCreator = null
                                                                      )
        {
            rng ??= GlobalRandom.DefaultRNG;
            tunnelCreator ??= new HorizontalVerticalTunnelCreator(rng);
            connectionPointSelector ??= new CenterBoundsConnectionPointSelector();

            // 1. Generate rectangular rooms
            yield return new RoomsGeneration
            {
                RNG = rng,
                MinRooms = minRooms,
                MaxRooms = maxRooms,
                RoomMinSize = roomMinSize,
                RoomMaxSize = roomMaxSize,
                RoomSizeRatioX = roomSizeRatioX,
                RoomSizeRatioY = roomSizeRatioY,
                MaxCreationAttempts = maxCreationAttempts,
                MaxPlacementAttempts = maxPlacementAttempts
            };

            // 2. Convert the rectangles for rooms given by the last step to areas so we can run
            // the connection algorithm
            yield return new RectanglesToAreas("Rooms", "RoomAreas");

            // 3. Connect rooms together randomly using the specified tunnel creation method/connection point
            // selector
            yield return new OrderedMapAreaConnection(areasComponentTag: "RoomAreas")
            {
                ConnectionPointSelector = connectionPointSelector,
                RandomizeOrder = true,
                RNG = rng,
                TunnelCreator = tunnelCreator
            };

            // 4. Find and record the locations that doors should be placed in the generated rooms.
            yield return new DoorFinder();
        }

        /// <summary>
        /// Generates a dungeon map based on the process outlined here:
        /// http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/.
        /// </summary>
        /// <param name="rng">The RNG to use for map generation.  Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <param name="minRooms">Minimum amount of rooms to generate on the map.  Defaults to 4.</param>
        /// <param name="maxRooms">Maximum amount of rooms to generate on the map.  Defaults to 10.</param>
        /// <param name="roomMinSize">The minimum size allowed for generated rooms.  Rounded up to an odd number.  Defaults to 3.</param>
        /// <param name="roomMaxSize">The maximum size allowed for generated rooms.  Rounded up to an odd number.  Defaults to 7.</param>
        /// <param name="roomSizeRatioX">The ratio of the room width to the height for generated rooms. Defaults to 1.0.</param>
        /// <param name="roomSizeRatioY">The ratio of the room height to the width for generated rooms. Defaults to 1.0.</param>
        /// <param name="maxCreationAttempts">
        /// The maximum times to re-generate a room that fails to place in a valid location
        /// before giving up on generating that room entirely.  Defaults to 10.
        /// </param>
        /// <param name="maxPlacementAttempts">
        /// The maximum times to attempt to place a room in a map without intersection, before
        /// giving up and re-generating that room. Defaults to 10.
        /// </param>
        /// <param name="crawlerChangeDirectionImprovement">
        /// Out of 100, how much to increase the chance of a crawler changing
        /// direction each step during maze generation.  Once it changes direction, it resets to 0 and increases by this amount.
        /// Defaults to 10.
        /// </param>
        /// <param name="minSidesToConnect">Minimum sides of each room to connect to the maze.  Defaults to 1.</param>
        /// <param name="maxSidesToConnect">Maximum sides of each room to connect to the maze. Defaults to 4.</param>
        /// <param name="cancelSideConnectionSelectChance">
        /// A chance out of 100 to cancel selecting sides to connect to the maze
        /// (per room). Defaults to 50.
        /// </param>
        /// <param name="cancelConnectionPlacementChance">
        /// A chance out of 100 to cancel placing a door on a given side of a room
        /// after one has been placed on that side. Defaults to 70.
        /// </param>
        /// <param name="cancelConnectionPlacementChanceIncrease">
        /// The <paramref name="cancelConnectionPlacementChance" /> value is
        /// increased by this amount each time a door is placed on a given side of a room. Defaults to 10.
        /// </param>
        /// <param name="saveDeadEndChance">
        /// The chance out of 100 that a dead end is left alone during dead end trimming.  Defaults
        /// to 40.
        /// </param>
        /// <param name="maxTrimIterations">
        /// Maximum number of passes to make looking for dead ends per area during dead end
        /// trimming.  Defaults to infinity.
        /// </param>
        /// <returns>A set of map generation steps that generate a map with rectangular rooms connected by a maze of tunnels.</returns>
        public static IEnumerable<GenerationStep> DungeonMazeMapSteps(IGenerator? rng = null, int minRooms = 4,
                                                                      int maxRooms = 10, int roomMinSize = 3,
                                                                      int roomMaxSize = 7, float roomSizeRatioX = 1f,
                                                                      float roomSizeRatioY = 1f,
                                                                      int maxCreationAttempts = 10,
                                                                      int maxPlacementAttempts = 10,
                                                                      ushort crawlerChangeDirectionImprovement = 10,
                                                                      ushort minSidesToConnect = 1,
                                                                      ushort maxSidesToConnect = 4,
                                                                      ushort cancelSideConnectionSelectChance = 50,
                                                                      ushort cancelConnectionPlacementChance = 70,
                                                                      ushort cancelConnectionPlacementChanceIncrease =
                                                                          10, ushort saveDeadEndChance = 40,
                                                                      int maxTrimIterations = -1)
        {
            rng ??= GlobalRandom.DefaultRNG;

            // 1. Generate rectangular rooms
            yield return new RoomsGeneration
            {
                RNG = rng,
                MinRooms = minRooms,
                MaxRooms = maxRooms,
                RoomMinSize = roomMinSize,
                RoomMaxSize = roomMaxSize,
                RoomSizeRatioX = roomSizeRatioX,
                RoomSizeRatioY = roomSizeRatioY,
                MaxCreationAttempts = maxCreationAttempts,
                MaxPlacementAttempts = maxPlacementAttempts
            };

            // 2. Generate mazes in the space between the rooms
            yield return new MazeGeneration
            {
                RNG = rng, CrawlerChangeDirectionImprovement = crawlerChangeDirectionImprovement
            };

            // 3. Make sure all the mazes are connected into a single maze.
            //
            // This connection steps would default to connecting areas in the list with the tag "Areas", but since we want to
            // connect the tunnels left by the maze generation step (which added its areas to a "Tunnels" component, we change
            // this tag appropriately.  Similarly, ClosestMapAreaConnection can't store its results in the same area as it takes from,
            // so we give it a different tag.
            yield return new
                ClosestMapAreaConnection(areasComponentTag: "Tunnels", tunnelsComponentTag: "MazeConnections")
                {
                    ConnectionPointSelector = new ClosestConnectionPointSelector(Distance.Manhattan),
                    TunnelCreator = new HorizontalVerticalTunnelCreator(rng)
                };

            // 4. So that the tunnels are all in one component, add the MazeConnections to the tunnels, minus any overlapping points
            yield return new RemoveDuplicatePoints("Tunnels", "MazeConnections");
            yield return new AppendItemLists<Area>("Tunnels", "MazeConnections") { RemoveAppendedComponent = true };

            // 5. Open up walls of rooms to connect them to the maze
            yield return new RoomDoorConnection
            {
                RNG = rng,
                MinSidesToConnect = minSidesToConnect,
                MaxSidesToConnect = maxSidesToConnect,
                CancelSideConnectionSelectChance = cancelSideConnectionSelectChance,
                CancelConnectionPlacementChance = cancelConnectionPlacementChance,
                CancelConnectionPlacementChanceIncrease = cancelConnectionPlacementChanceIncrease
            };

            // 6. Trim back dead ends in the maze to reduce the maze density
            yield return new TunnelDeadEndTrimming
            {
                RNG = rng, SaveDeadEndChance = saveDeadEndChance, MaxTrimIterations = maxTrimIterations
            };
        }

        /// <summary>
        /// Generates a cave-like map using the cellular automata algorithm here:
        /// http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels.
        /// </summary>
        /// <param name="rng">The RNG to use.  Defaults to <see cref="GlobalRandom.DefaultRNG"/>.</param>
        /// <param name="fillProbability">
        /// Represents the percent chance that a given cell will be a floor cell when the map is initially randomly
        /// filled. Recommended to be in range [40, 60].
        /// </param>
        /// <param name="totalIterations">
        /// Total number of times the cellular automata-based smoothing algorithm is executed.  Recommended to be in
        /// range [2, 10].
        /// </param>
        /// <param name="cutoffBigAreaFill">
        /// Total number of times the cellular automata smoothing variation that is more likely to
        /// result in "breaking up" large areas will be run before switching to the more standard
        /// nearest neighbors version. Must be less than or equal to <paramref name="totalIterations"/>.
        /// Recommended to be in range [2, 7].
        /// </param>
        /// <param name="distanceCalculation">
        /// The distance calculation that determines distance/neighbors for determining unique areas and connecting
        /// them.  Defaults to <see cref="Distance.Manhattan"/>.
        /// </param>
        /// <param name="connectionPointSelector">
        /// The area connection strategy to use. Not all methods function on maps with concave areas
        /// -- see respective class documentation for details.  Defaults to using a
        /// <see cref="RandomConnectionPointSelector"/>.
        /// </param>
        /// <param name="tunnelCreationMethod">
        /// The tunnel creation strategy to use. Defaults to <see cref="DirectLineTunnelCreator" /> with
        /// given distance's adjacency rules.
        /// </param>
        /// <returns>A set of map generation steps that generate a cave-like map using the cellular automata algorithm.</returns>
        public static IEnumerable<GenerationStep> CellularAutomataGenerationSteps(IGenerator? rng = null,
                                                                                  ushort fillProbability = 40,
                                                                                  int totalIterations = 7,
                                                                                  int cutoffBigAreaFill = 4,
                                                                                  Distance? distanceCalculation = null,
                                                                                  IConnectionPointSelector? connectionPointSelector = null,
                                                                                  ITunnelCreator? tunnelCreationMethod = null)
        {
            rng ??= GlobalRandom.DefaultRNG;
            Distance dist = distanceCalculation ?? Distance.Manhattan;
            connectionPointSelector ??= new RandomConnectionPointSelector(rng);
            tunnelCreationMethod ??= new DirectLineTunnelCreator(dist);

            // 1. Randomly fill the map with walls/floors
            yield return new RandomViewFill
            {
                FillProbability = fillProbability,
                RNG = rng
            };

            // 2. Smooth the map into areas with the cellular automata algorithm
            yield return new CellularAutomataAreaGeneration
            {
                AreaAdjacencyRule = dist,
                TotalIterations = totalIterations,
                CutoffBigAreaFill = cutoffBigAreaFill,
            };

            // 3. Find all unique areas generated by the smoothing and record them
            yield return new AreaFinder
            {
                AdjacencyMethod = dist
            };

            // 4. Connect areas generated by this algorithm by connecting each area to its closest neighbor
            yield return new ClosestMapAreaConnection
            {
                ConnectionPointSelector = connectionPointSelector,
                DistanceCalc = dist,
                TunnelCreator = tunnelCreationMethod
            };
        }

        /// <summary>
        /// Generates a simple map that is a hollowed-out rectangle surrounded by walls.
        /// </summary>
        /// <returns>A set of map generation steps that generate a simple map composed of a hollowed out rectangle
        /// surrounded by walls.</returns>
        public static IEnumerable<GenerationStep> RectangleMapSteps()
        {
            yield return new RectangleGenerator();
        }
    }
}
