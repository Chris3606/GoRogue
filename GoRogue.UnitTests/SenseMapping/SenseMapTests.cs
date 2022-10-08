using System.Collections.Generic;
using System.Linq;
using GoRogue.SenseMapping;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.SenseMapping
{
    public class SenseMapTests
    {
        // Properties of the test map
        private const int Height = 30;
        private const int Width = 30;
        private static readonly Point s_center = (Width / 2, Height / 2);
        private const int Radius = 10;

        // All implemented SenseSource algorithms
        private static readonly SourceType[] s_senseSourceAlgorithms = TestUtils.GetEnumValues<SourceType>();

        // Basic rectangle boolean/resistance map, one with double-tile-thick walls
        private static readonly IGridView<double> s_resMap = MockMaps.RectangleResMap(Width, Height);

        private static readonly IGridView<double> s_resMapDoubleThickWalls =
            MockMaps.RectangleDoubleThickResMap(Width, Height);

        // Radius shapes to test
        private static readonly Radius[] s_radii = TestUtils.GetEnumValues<Radius.Types>().Select(i => (Radius)i)
            .ToArray();

        // Radius shapes paired with each SenseSource algorithm
        public static IEnumerable<(Radius, SourceType)> RadiiWithSenseAlgorithms =
            s_radii.Combinate(s_senseSourceAlgorithms);

        [Theory]
        [MemberDataTuple(nameof(RadiiWithSenseAlgorithms))]
        public void SingleSourceOpenMapEqualToRadius(Radius shape, SourceType algorithm)
        {
            var senseMap = new SenseMap(s_resMap);
            var lightSource = AlgorithmFactory.CreateSenseSource(algorithm, s_center, Radius, shape);
            senseMap.AddSenseSource(lightSource);
            senseMap.Calculate();

            var losArea = senseMap.ResultView.Positions().ToEnumerable().Where(pos => senseMap.ResultView[pos] > 0.0).ToHashSet();
            var radArea = shape.PositionsInRadius(s_center, Radius).ToHashSet();

            Assert.Equal(radArea, losArea);
        }

        [Theory]
        [MemberDataTuple(nameof(RadiiWithSenseAlgorithms))]
        public void WallsStopSpread(Radius shape, SourceType algorithm)
        {
            // SenseMap over open map
            var senseMap = new SenseMap(s_resMapDoubleThickWalls);

            // Set up SenseSource for radius at least as big as map, to ensure spread isn't
            // limited by radius
            var senseSource = AlgorithmFactory.CreateSenseSource(algorithm, s_center, Width * Height, shape);
            senseMap.AddSenseSource(senseSource);

            // Calculate sense map
            senseMap.Calculate();

            // Get lists of outer walls (totally unlit) and inner walls (some to all of which will be lit,
            // depending on how tight ripple is)
            var outerPositions = senseMap.ResultView.Bounds().PerimeterPositions().ToHashSet();
            var innerPositions = senseMap.ResultView.Bounds().Expand(-1, -1).PerimeterPositions();

            // At least one of the inner walls must be lit (to prove that walls are appropriately lit)
            Assert.Contains(innerPositions, i => senseMap.ResultView[i] > 0.0);

            // All of the outer walls should be unlit (as they're blocked by the inner walls)
            foreach (var pos in outerPositions)
                Assert.Equal(0.0, senseMap.ResultView[pos]);
        }

        [Theory]
        [MemberDataTuple(nameof(RadiiWithSenseAlgorithms))]
        public void CurrentHash(Radius shape, SourceType algorithm)
        {
            var senseMap = new SenseMap(s_resMap);
            senseMap.AddSenseSource(AlgorithmFactory.CreateSenseSource(algorithm, s_center, Radius, shape));

            senseMap.Calculate();

            // Inefficient copy but fine for testing
            var currentSenseMap = new HashSet<Point>(senseMap.CurrentSenseMap);

            foreach (var pos in s_resMap.Positions())
                Assert.Equal(senseMap.ResultView[pos] > 0.0, currentSenseMap.Contains(pos));
        }

        [Theory]
        [MemberDataTuple(nameof(RadiiWithSenseAlgorithms))]
        public void NewlyInAndOutOfSenseMap(Radius shape, SourceType algorithm)
        {
            var senseMap = new SenseMap(s_resMap);
            var senseSource = AlgorithmFactory.CreateSenseSource(algorithm, s_center, Radius, shape);
            senseMap.AddSenseSource(senseSource);

            senseMap.Calculate();

            var prevSenseMap = new HashSet<Point>(senseMap.CurrentSenseMap);

            senseSource.Position -= 1;
            senseMap.Calculate();

            var curSenseMap = new HashSet<Point>(senseMap.CurrentSenseMap);
            var newlySeen = new HashSet<Point>(senseMap.NewlyInSenseMap);
            var newlyUnseen = new HashSet<Point>(senseMap.NewlyOutOfSenseMap);

            foreach (var pos in prevSenseMap)
                Assert.NotEqual(curSenseMap.Contains(pos), newlyUnseen.Contains(pos));

            foreach (var pos in curSenseMap)
                Assert.NotEqual(prevSenseMap.Contains(pos), newlySeen.Contains(pos));
        }
    }
}
