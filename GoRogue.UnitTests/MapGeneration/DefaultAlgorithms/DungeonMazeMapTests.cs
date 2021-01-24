using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.Random;
using GoRogue.UnitTests.Mocks;
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
        private ITestOutputHelper _output;

        public DungeonMazeMapTests(ITestOutputHelper output) => _output = output;

        /// <summary>
        /// Tests that all doors that are made in rooms are in the door structure.
        /// </summary>
        [Fact]
        public void AllDoorsRecorded()
        {
            // Use a generator with an arbitrary seed (known to cause an issue) in a minimalistic case
            var rng = new XorShift128Generator(737318414);

            // Generate a map
            var generator = new Generator(40, 40);
            generator
                .AddSteps(GoRogue.MapGeneration.DefaultAlgorithms.DungeonMazeMapSteps(
                    minRooms: 1, maxRooms: 1, roomMinSize:5, roomMaxSize: 11, saveDeadEndChance: 0, rng: rng))
                .Generate();

            // Find components we're expecting
            var rooms = generator.Context.GetFirst<ItemList<Rectangle>>("Rooms");
            var wallFloor = generator.Context.GetFirst<IGridView<bool>>("WallFloor");
            var doors = generator.Context.GetFirst<DoorList>("Doors");

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
    }
}
