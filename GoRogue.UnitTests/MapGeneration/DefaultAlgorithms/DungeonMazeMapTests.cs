using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Troschuetz.Random.Generators;
using Xunit;
using Xunit.Abstractions;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.MapGeneration.DefaultAlgorithms
{
    public class DungeonMazeMapTests
    {
        private readonly ITestOutputHelper _output;

        #region Test Data

        // Seeds known to cause corner cases that, if connection algorithm doesn't choose true closest points,
        // will result in tunnels cutting through rooms (thus creating additional "doors")
        public static uint[] TrueClosestEdgeCaseSeeds = { 737318414, 3851955825, 3366057365 };
        #endregion

        public DungeonMazeMapTests(ITestOutputHelper output) => _output = output;

        /// <summary>
        /// Tests that all doors that are made in rooms are in the door structure, with known seeds that cause
        /// issues if area merges are incorrectly recorded/points are incorrectly selected.
        /// </summary>
        [Theory]
        [MemberDataEnumerable(nameof(TrueClosestEdgeCaseSeeds))]
        public void TunnelsConnectedToTrueClosest(uint seed)
        {
            // Use a generator with the seed known to cause an issue in a minimalistic case
            var rng = new XorShift128Generator(seed);

            // Generate a map
            var generator = new Generator(40, 40);
            generator
                .ConfigAndGenerateSafe((gen) =>
                {
                    gen.AddSteps(GoRogue.MapGeneration.DefaultAlgorithms.DungeonMazeMapSteps(
                        minRooms: 1, maxRooms: 1, roomMinSize: 5, roomMaxSize: 11, saveDeadEndChance: 0, rng: rng));
                });

            // Find components we're expecting
            var rooms = generator.Context.GetFirst<ItemList<Rectangle>>("Rooms");
            var wallFloor = generator.Context.GetFirst<IGridView<bool>>("WallFloor");
            var doors = generator.Context.GetFirst<DoorList>("Doors");

            // Print map representation to assist in debugging if test fails
            _output.WriteLine("Map: ");
            _output.WriteLine(wallFloor.ExtendToString(elementStringifier: i => i ? "." : "#"));

            // Go through each room and verify the correct items are there.
            foreach (var (room, _) in rooms)
            {
                _output.WriteLine($"Performing check on room: {room}");
                // Find all doors with a naive algorithm
                var holes = new HashSet<Point>();
                foreach (var edgePoint in room.Expand(1, 1).PerimeterPositions())
                    if (wallFloor[edgePoint])
                        holes.Add(edgePoint);

                // Find list of rooms in door list for the room we're working with, and make sure it contains the same
                // doors as are actually there.
                var doorListForCurrentRoom = doors.DoorsPerRoom[room];
                Assert.Equal(holes, new HashSet<Point>(doorListForCurrentRoom.Doors));
                Assert.Equal(holes, doorListForCurrentRoom.Select(pair => pair.Item).ToHashSet());
            }
        }

        /// <summary>
        /// Checks using an array of random seeds to ensure that DungeonMazeMap generation does not produce unexpected
        /// doors.
        /// </summary>
        [Fact]
        public void AllDoorsRecorded()
        {
            for (int i = 0; i < 500; i++)
            {
                // Use a particular generator so that we know the seed and thus have a replicable procedure if there
                // is an issue
                var rng = new XorShift128Generator();
                _output.WriteLine($"Using generator with seed: {rng.Seed}.");

                // Generate a map
                var generator = new Generator(40, 40);
                generator
                    .ConfigAndGenerateSafe(gen =>
                    {
                        gen.AddSteps(GoRogue.MapGeneration.DefaultAlgorithms.DungeonMazeMapSteps(
                            minRooms: 1, maxRooms: 1, roomMinSize: 5, roomMaxSize: 11, saveDeadEndChance: 0, rng: rng));
                    });

                // Find components we're expecting
                var rooms = generator.Context.GetFirst<ItemList<Rectangle>>("Rooms");
                var wallFloor = generator.Context.GetFirst<IGridView<bool>>("WallFloor");
                var doors = generator.Context.GetFirst<DoorList>("Doors");

                // Go through each room and verify the correct items are there.
                foreach (var (room, _) in rooms)
                {
                    // Find all doors with a naive algorithm
                    var holes = new HashSet<Point>();
                    foreach (var edgePoint in room.Expand(1, 1).PerimeterPositions())
                        if (wallFloor[edgePoint])
                            holes.Add(edgePoint);

                    // Find list of rooms in door list for the room we're working with, and make sure it contains the same
                    // doors as are actually there.
                    var doorListForCurrentRoom = doors.DoorsPerRoom[room];
                    Assert.Equal(holes, new HashSet<Point>(doorListForCurrentRoom.Doors));
                    Assert.Equal(holes, doorListForCurrentRoom.Select(pair => pair.Item).ToHashSet());
                }
            }
        }
    }
}
