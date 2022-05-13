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
        private const int _height = 30;
        private const int _width = 30;
        private static readonly Point _center = (_width / 2, _height / 2);
        private const int _radius = 10;

        // All implemented SenseSource algorithms
        private static readonly SourceType[] _senseSourceAlgorithms = TestUtils.GetEnumValues<SourceType>();

        // Basic rectangle boolean/resistance map, one with double-tile-thick walls
        private static readonly IGridView<double> _resMap = MockMaps.RectangleResMap(_width, _height);

        private static readonly IGridView<double> _resMapDoubleThickWalls =
            MockMaps.RectangleDoubleThickResMap(_width, _height);

        // Radius shapes to test
        private static readonly Radius[] _radii = TestUtils.GetEnumValues<Radius.Types>().Select(i => (Radius)i)
            .ToArray();

        // Radius shapes paired with each SenseSource algorithm
        public static IEnumerable<(Radius, SourceType)> RadiiWithSenseAlgorithms =
            _radii.Combinate(_senseSourceAlgorithms);

        [Theory]
        [MemberDataTuple(nameof(RadiiWithSenseAlgorithms))]
        public void SingleSourceOpenMapEqualToRadius(Radius shape, SourceType algorithm)
        {
            var senseMap = new SenseMap(_resMap);
            var lightSource = new SenseSource(algorithm, _center, _radius, shape);
            senseMap.AddSenseSource(lightSource);
            senseMap.Calculate();

            var losArea = senseMap.Positions().Where(pos => senseMap[pos] > 0.0).ToHashSet();
            var radArea = shape.PositionsInRadius(_center, _radius).ToHashSet();

            Assert.Equal(radArea, losArea);
        }

        [Theory]
        [MemberDataTuple(nameof(RadiiWithSenseAlgorithms))]
        public void WallsStopSpread(Radius shape, SourceType algorithm)
        {
            // SenseMap over open map
            var senseMap = new SenseMap(_resMapDoubleThickWalls);

            // Set up SenseSource for radius at least as big as map, to ensure spread isn't
            // limited by radius
            var senseSource = new SenseSource(algorithm, _center, _width * _height, shape);
            senseMap.AddSenseSource(senseSource);

            // Calculate sense map
            senseMap.Calculate();

            // Get lists of outer walls (totally unlit) and inner walls (some to all of which will be lit,
            // depending on how tight ripple is)
            var outerPositions = senseMap.Bounds().PerimeterPositions().ToHashSet();
            var innerPositions = senseMap.Bounds().Expand(-1, -1).PerimeterPositions();

            // At least one of the inner walls must be lit (to prove that walls are appropriately lit)
            Assert.Contains(innerPositions, i => senseMap[i] > 0.0);

            // All of the outer walls should be unlit (as they're blocked by the inner walls)
            foreach (var pos in outerPositions)
                Assert.Equal(0.0, senseMap[pos]);
        }

        [Theory]
        [MemberDataTuple(nameof(RadiiWithSenseAlgorithms))]
        public void CurrentHash(Radius shape, SourceType algorithm)
        {
            var senseMap = new SenseMap(_resMap);
            senseMap.AddSenseSource(new SenseSource(algorithm, _center, _radius, shape));

            senseMap.Calculate();

            // Inefficient copy but fine for testing
            var currentSenseMap = new HashSet<Point>(senseMap.CurrentSenseMap);

            foreach (var pos in _resMap.Positions())
                Assert.Equal(senseMap[pos] > 0.0, currentSenseMap.Contains(pos));
        }

        [Theory]
        [MemberDataTuple(nameof(RadiiWithSenseAlgorithms))]
        public void NewlyInAndOutOfSenseMap(Radius shape, SourceType algorithm)
        {
            var senseMap = new SenseMap(_resMap);
            var senseSource = new SenseSource(algorithm, _center, _radius, shape);
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
