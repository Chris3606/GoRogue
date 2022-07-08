using System.Collections.Generic;
using System.Linq;
using GoRogue.FOV;
using GoRogue.Random;
using GoRogue.SenseMapping;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.FOV
{
    public class FOVSenseMapEquivalenceTests
    {
        // Properties of the test map
        private const int Height = 30;
        private const int Width = 30;
        private const int Radius = 10;

        // Radius shapes to test
        private static readonly Radius[] s_radii = TestUtils.GetEnumValues<Radius.Types>().Select(i => (Radius)i).ToArray();

        // Basic rectangle boolean/resistance maps
        private static readonly IGridView<bool> s_losMap = MockMaps.Rectangle(Width, Height);
        private static readonly IGridView<double> s_resMap = MockMaps.RectangleResMap(Width, Height);

        // Positions to test on _losMap
        private static readonly Point[] s_testPositions =
            Enumerable.Range(0, 100).Select(i => GlobalRandom.DefaultRNG.RandomPosition(s_losMap, true)).ToArray();

        // Positions paired with radius shapes
        public static IEnumerable<(Point, Radius)> TestPositionsAndRadii => s_testPositions.Combinate(s_radii);
        private static IEnumerable<(Point, Radius)> ShortTestPositionsAndRadii => s_testPositions.Take(1).Combinate(s_radii);

        // Directions and spans to use for angle-based testing
        private static readonly int[] s_validDegrees =
        {
            // 45-degree increments
            0, 45, 90, 135, 180, 220, 270,
            // Some various odd-angle values
            32, 84, 97, 123, 299
        };
        private static readonly int[] s_validSpans =
        {
            // 45-degree increments
            45, 90, 135, 180, 220, 270, 360,
            // Some various odd-angle cases
            1, 84, 98, 19, 271
        };

        // Each position and radius shape paired with each angle test possibility
        public static IEnumerable<(Point, Radius, int, int)> AngleRestrictedShortTestPositionsAndRadii =
            ShortTestPositionsAndRadii.Combinate(s_validDegrees).Combinate(s_validSpans);


        [Theory]
        [MemberDataTuple(nameof(TestPositionsAndRadii))]
        public void SenseMapEquivalence(Point source, Radius shape)
        {
            var fov = new RecursiveShadowcastingFOV(s_losMap);
            var senseMap = new SenseMap(s_resMap);

            // Set up sense source (using shadow-casting to match LOS)
            var lightSource = new SenseSource(SourceType.Shadow, source, Radius, shape);
            senseMap.AddSenseSource(lightSource);

            // Calculate LOS and sense map
            fov.Calculate(source, Radius, shape);
            senseMap.Calculate();

            // Verify equivalence of LOS and SenseMap
            Assert.Equal(Width, fov.DoubleResultView.Width);
            Assert.Equal(Width, senseMap.Width);
            Assert.Equal(Height, fov.DoubleResultView.Height);
            Assert.Equal(Height, senseMap.Height);

            foreach (var pos in fov.DoubleResultView.Positions())
                Assert.Equal(fov.DoubleResultView[pos], senseMap[pos]);
        }


        [Theory]
        [MemberDataTuple(nameof(AngleRestrictedShortTestPositionsAndRadii))]
        public void SenseMapAngleEquivalence(Point source, Radius shape, int angle, int span)
        {
            var fov = new RecursiveShadowcastingFOV(s_losMap);
            var senseMap = new SenseMap(s_resMap);

            // Set up sense source (using shadow-casting to match LOS)
            var lightSource =
                new SenseSource(SourceType.Shadow, source, Radius, shape)
                {
                    IsAngleRestricted = true,
                    Angle = angle,
                    Span = span
                };
            senseMap.AddSenseSource(lightSource);

            // Calculate LOS and sense map
            fov.Calculate(source, Radius, shape, angle, span);
            senseMap.Calculate();

            // Verify equivalence of LOS and SenseMap
            Assert.Equal(Width, fov.DoubleResultView.Width);
            Assert.Equal(Width, senseMap.Width);
            Assert.Equal(Height, fov.DoubleResultView.Height);
            Assert.Equal(Height, senseMap.Height);

            foreach (var pos in fov.DoubleResultView.Positions())
                Assert.Equal(fov.DoubleResultView[pos], senseMap[pos]);
        }
    }
}
