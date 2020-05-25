using GoRogue.MapGeneration;
using GoRogue.MapViews;
using Xunit;
using Xunit.Abstractions;

namespace GoRogue.UnitTests.MapGeneration
{
    public class TempDefaultAlgorithmsTests
    {
        private readonly ITestOutputHelper _output;

        public TempDefaultAlgorithmsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void PrintDungeonMazeMap()
        {
            var rng = new Troschuetz.Random.Generators.XorShift128Generator(12345);

            var generator = new Generator(40, 50);
            generator.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps(rng: rng, saveDeadEndChance: 10));
            generator.Generate();

            var wallFloorMap = generator.Context.GetComponent<ISettableMapView<bool>>("WallFloor");
            Assert.NotNull(wallFloorMap);

            _output.WriteLine("Generated map: ");
            _output.WriteLine(wallFloorMap!.ExtendToString(elementStringifier: val => val ? "." : "#"));
        }
    }
}
