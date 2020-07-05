using GoRogue.MapGeneration;
using GoRogue.MapViews;
using Troschuetz.Random.Generators;
using Xunit;
using Xunit.Abstractions;

namespace GoRogue.UnitTests.MapGeneration
{
    public class TempDefaultAlgorithmsTests
    {
        public TempDefaultAlgorithmsTests(ITestOutputHelper output) => _output = output;

        private readonly ITestOutputHelper _output;

        [Fact]
        public void PrintDungeonMazeMap()
        {
            var rng = new XorShift128Generator(12345);

            var generator = new Generator(40, 30);
            generator.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps(rng, saveDeadEndChance: 10));
            generator.Generate();

            var wallFloorMap = generator.Context.GetComponent<ISettableMapView<bool>>("WallFloor");
            Assert.NotNull(wallFloorMap);

            _output.WriteLine("Generated map: ");
            _output.WriteLine(wallFloorMap!.ExtendToString(elementStringifier: val => val ? "." : "#"));
        }
    }
}
