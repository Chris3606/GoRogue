using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.FOV;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Xunit;
using Xunit.Abstractions;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.FOV
{
    public class FOVTests
    {
        public FOVTests(ITestOutputHelper output) => _output = output;

        private readonly ITestOutputHelper _output;

        // Properties of the test map
        private const int _height = 30;
        private const int _width = 30;
        private static readonly Point _center = (_width / 2, _height / 2);
        private const int _radius = 10;

        // Basic rectangle LOS map
        private static readonly IGridView<bool> _losMap = MockMaps.Rectangle(_width, _height);

        // LOS map with double-thick walls
        private static readonly IGridView<bool> _losMapDoubleThickWalls =
            MockMaps.DoubleThickRectangle(_width, _height);

        // Radius shapes to test
        public static readonly Radius[] Radii = TestUtils.GetEnumValues<Radius.Types>().Select(i => (Radius)i).ToArray();

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void OpenMapEqualToRadius(Radius shape)
        {
            var los = new RecursiveShadowcastingFOV(_losMap);
            los.Calculate(_center.X, _center.Y, _radius, shape);

            var radArea = shape.PositionsInRadius(_center, _radius).ToHashSet();
            var losArea = los.DoubleResultView.Positions().Where(pos => los.DoubleResultView[pos] > 0.0).ToHashSet();

            Assert.Equal(radArea, losArea);
        }

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void VisibilityStoppedByWalls(Radius shape)
        {
            // FOV over open map
            var fov = new RecursiveShadowcastingFOV(_losMapDoubleThickWalls);

            // Calculate LOS with infinite radius
            fov.Calculate(_center, double.MaxValue, shape);

            // Verify that the only non-lit positions are the outer walls (which are blocked
            // by the inner ones)
            var outerPositions = fov.DoubleResultView.Bounds().PerimeterPositions().ToHashSet();
            foreach (var pos in fov.DoubleResultView.Positions())
                Assert.NotEqual(outerPositions.Contains(pos), fov.DoubleResultView[pos] > 0.0);
        }

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void BooleanOutput(Radius shape)
        {
            var fov = new RecursiveShadowcastingFOV(_losMap);
            fov.Calculate(_center, _radius, shape);

            _output.WriteLine("FOV for reference:");
            //_output.WriteLine(fov.ToString(2));
            _output.WriteLine(fov.ToString());

            foreach (var pos in fov.DoubleResultView.Positions())
            {
                var inFOV = Math.Abs(fov.DoubleResultView[pos]) > 0.0000000001;
                Assert.Equal(inFOV, fov.BooleanResultView[pos]);
            }
        }

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void CurrentHash(Radius shape)
        {
            var fov = new RecursiveShadowcastingFOV(_losMap);

            fov.Calculate(_center, _radius, shape);

            // Inefficient copy but fine for testing
            var currentFov = new HashSet<Point>(fov.CurrentFOV);

            foreach (var pos in _losMap.Positions())
                Assert.Equal(fov.DoubleResultView[pos] > 0.0, currentFov.Contains(pos));
        }

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void NewlySeenUnseen(Radius shape)
        {
            var fov = new RecursiveShadowcastingFOV(_losMap);

            fov.Calculate(_center, _radius, shape);
            var prevFov = new HashSet<Point>(fov.CurrentFOV);

            fov.Calculate(_center - 1, _radius, shape);
            var curFov = new HashSet<Point>(fov.CurrentFOV);
            var newlySeen = new HashSet<Point>(fov.NewlySeen);
            var newlyUnseen = new HashSet<Point>(fov.NewlyUnseen);

            foreach (var pos in prevFov)
                Assert.NotEqual(curFov.Contains(pos), newlyUnseen.Contains(pos));

            foreach (var pos in curFov)
                Assert.NotEqual(prevFov.Contains(pos), newlySeen.Contains(pos));
        }

        [Fact]
        public void AccessibleBeforeCalculate()
        {
            var fov = new RecursiveShadowcastingFOV(_losMap);
            foreach (var pos in fov.DoubleResultView.Positions())
                Assert.Equal(0.0, fov.DoubleResultView[pos]);
        }

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void MultipleFOVDistanceOverlap(Radius shape)
        {
            var fov = new RecursiveShadowcastingFOV(_losMap);
            var decay = 1.0 / (_radius + 1);
            Distance dist = shape;

            // Calculate two overlapping FOVs
            var point2 = _center + 5;
            fov.Calculate(_center, _radius, shape);
            fov.CalculateAppend(point2, _radius, shape);

            // Print result, for reference
            _output.WriteLine("Resulting FOV (For Reference):");
            _output.WriteLine(fov.ToString());

            // Make sure the distance portion of the FOV always reflects the closest source (to ensure overlapping
            // radii were handled properly)
            foreach (var pos in fov.DoubleResultView.Positions())
            {
                double minDist = Math.Min(dist.Calculate(pos, _center), dist.Calculate(pos, point2));

                double expectedVal = minDist > _radius ? 0 : 1.0 - (decay * minDist);
                Assert.Equal(expectedVal, fov.DoubleResultView[pos]);
            }
        }
    }
}
