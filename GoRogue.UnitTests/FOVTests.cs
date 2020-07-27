﻿using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit;
using Xunit.Abstractions;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests
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
        private static readonly IMapView<bool> _losMap = MockMaps.Rectangle(_width, _height);

        // LOS map with double-thick walls
        private static readonly IMapView<bool> _losMapDoubleThickWalls =
            MockMaps.DoubleThickRectangle(_width, _height);

        // Radius shapes to test
        public static readonly Radius[] Radii = TestUtils.GetEnumValues<Radius.Types>().Select(i => (Radius)i).ToArray();

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void OpenMapEqualToRadius(Radius shape)
        {
            var los = new FOV(_losMap);
            los.Calculate(_center.X, _center.Y, _radius, shape);

            var radArea = shape.PositionsInRadius(_center, _radius).ToHashSet();
            var losArea = los.Positions().Where(pos => los[pos] > 0.0).ToHashSet();

            Assert.Equal(radArea, losArea);
        }

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void VisibilityStoppedByWalls(Radius shape)
        {
            // FOV over open map
            var fov = new FOV(_losMapDoubleThickWalls);

            // Calculate LOS with infinite radius
            fov.Calculate(_center, double.MaxValue, shape);

            // Verify that the only non-lit positions are the outer walls (which are blocked
            // by the inner ones)
            var outerPositions = fov.Bounds().PerimeterPositions().ToHashSet();
            foreach (var pos in fov.Positions())
                Assert.NotEqual(outerPositions.Contains(pos), fov[pos] > 0.0);
        }

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void BooleanOutput(Radius shape)
        {
            var fov = new FOV(_losMap);
            fov.Calculate(_center, _radius, shape);

            _output.WriteLine("FOV for reference:");
            _output.WriteLine(fov.ToString(2));

            foreach (var pos in fov.Positions())
            {
                var inFOV = Math.Abs(fov[pos]) > 0.0000000001;
                Assert.Equal(inFOV, fov.BooleanFOV[pos]);
            }
        }

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void CurrentHash(Radius shape)
        {
            var fov = new FOV(_losMap);

            fov.Calculate(_center, _radius, shape);

            // Inefficient copy but fine for testing
            var currentFov = new HashSet<Point>(fov.CurrentFOV);

            foreach (var pos in _losMap.Positions())
                Assert.Equal(fov[pos] > 0.0, currentFov.Contains(pos));
        }

        [Theory]
        [MemberDataEnumerable(nameof(Radii))]
        public void NewlySeenUnseen(Radius shape)
        {
            var fov = new FOV(_losMap);

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
            var fov = new FOV(_losMap);
            foreach (var pos in fov.Positions())
                Assert.Equal(0.0, fov[pos]);
        }
    }
}