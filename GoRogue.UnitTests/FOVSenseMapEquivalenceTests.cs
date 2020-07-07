using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using GoRogue.SenseMapping;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests
{
    public class FOVSenseMapEquivalenceTests
    {
        // Properties of the test map
        private const int _height = 30;
        private const int _width = 30;
        private const int _radius = 10;

        // Radius shapes to test
        private static readonly Radius[] _radii = TestUtils.GetEnumValues<Radius.Types>().Select(i => (Radius)i).ToArray();

        // Basic rectangle boolean/resistance maps
        private static readonly IMapView<bool> _losMap = MockFactory.Rectangle(_width, _height);
        private static readonly IMapView<double> _resMap = MockFactory.RectangleResMap(_width, _height);

        // Positions to test on _losMap
        private static readonly Point[] _testPositions =
            Enumerable.Range(0, 100).Select(i => _losMap.RandomPosition(true)).ToArray();

        // Positions paired with radius shapes
        public static IEnumerable<(Point, Radius)> TestPositionsAndRadii => _testPositions.Combinate(_radii);
        private static IEnumerable<(Point, Radius)> ShortTestPositionsAndRadii => _testPositions.Take(1).Combinate(_radii);

        // Directions and spans to use for angle-based testing
        private static readonly int[] _validDegrees =
        {
            // 45-degree increments
            0, 45, 90, 135, 180, 220, 270,
            // Some various odd-angle values
            32, 84, 97, 123, 299
        };
        private static readonly int[] _validSpans =
        {
            // 45-degree increments
            45, 90, 135, 180, 220, 270, 360,
            // Some various odd-angle cases
            1, 84, 98, 19, 271
        };

        // Each position and radius shape paired with each angle test possibility
        public static IEnumerable<(Point, Radius, int, int)> AngleRestrictedShortTestPositionsAndRadii =
            ShortTestPositionsAndRadii.Combinate(_validDegrees).Combinate(_validSpans);


        [Theory]
        [MemberDataTuple(nameof(TestPositionsAndRadii))]
        public void SenseMapEquivalence(Point source, Radius shape)
        {
            var fov = new FOV(_losMap);
            var senseMap = new SenseMap(_resMap);

            // Set up sense source (using shadow-casting to match LOS)
            var lightSource = new SenseSource(SourceType.Shadow, source, _radius, shape);
            senseMap.AddSenseSource(lightSource);

            // Calculate LOS and sense map
            fov.Calculate(source, _radius, shape);
            senseMap.Calculate();

            // Verify equivalence of LOS and SenseMap
            Assert.Equal(_width, fov.Width);
            Assert.Equal(_width, senseMap.Width);
            Assert.Equal(_height, fov.Height);
            Assert.Equal(_height, senseMap.Height);

            foreach (var pos in fov.Positions())
                Assert.Equal(fov[pos], senseMap[pos]);
        }


        [Theory]
        [MemberDataTuple(nameof(AngleRestrictedShortTestPositionsAndRadii))]
        public void SenseMapAngleEquivalence(Point source, Radius shape, int angle, int span)
        {
            var fov = new FOV(_losMap);
            var senseMap = new SenseMap(_resMap);

            // Set up sense source (using shadow-casting to match LOS)
            var lightSource =
                new SenseSource(SourceType.Shadow, source, _radius, shape)
                {
                    IsAngleRestricted = true,
                    Angle = angle,
                    Span = span
                };
            senseMap.AddSenseSource(lightSource);

            // Calculate LOS and sense map
            fov.Calculate(source, _radius, shape, angle, span);
            senseMap.Calculate();

            // Verify equivalence of LOS and SenseMap
            Assert.Equal(_width, fov.Width);
            Assert.Equal(_width, senseMap.Width);
            Assert.Equal(_height, fov.Height);
            Assert.Equal(_height, senseMap.Height);

            foreach (var pos in fov.Positions())
                Assert.Equal(fov[pos], senseMap[pos]);
        }
    }
}
