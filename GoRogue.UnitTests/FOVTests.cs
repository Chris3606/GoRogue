using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using GoRogue.SenseMapping;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit;
using Xunit.Abstractions;

namespace GoRogue.UnitTests
{
    public class FOVTests
    {
        public FOVTests(ITestOutputHelper output) => _output = output;

        private const int _height = 30;
        private const int _width = 30;
        private static readonly Point s_center = (_width / 2, _height / 2);
        private const int _radius = 10;
        private readonly ITestOutputHelper _output;

        private bool EquivalentArrays(bool[,] arr1, bool[,] arr2)
        {
            if (arr1.GetLength(0) != arr2.GetLength(0) || arr1.GetLength(1) != arr2.GetLength(1))
            {
                _output.WriteLine("Error: Arrays weren't equal sizes");
                return false;
            }

            for (var x = 0; x < arr1.GetLength(0); x++)
                for (var y = 0; y < arr1.GetLength(1); y++)
                    if (arr1[x, y] != arr2[x, y])
                    {
                        _output.WriteLine("Arrays not equal");
                        return false;
                    }

            return true;
        }

        private void PrintArray(bool[,] arr)
            => _output.WriteLine(arr.ExtendToStringGrid(elementStringifier: v => v ? "1" : "0"));

        private static ArrayMap<double> RectResMap(int mapWidth, int mapHeight)
        {
            var map = MockFactory.Rectangle(_width, _height);
            var resMap = new ArrayMap<double>(mapWidth, mapHeight);

            for (var x = 0; x < map.Width; x++)
                for (var y = 0; y < map.Height; y++)
                    resMap[x, y] = map[x, y] ? 0.0 : 1.0;

            return resMap;
        }

        private bool TestLos(Radius shape)
        {
            var map = MockFactory.Rectangle(_width, _height);
            // Start out at false
            var radiusMap = new bool[_width, _height];
            var losMap = new bool[_width, _height];

            var los = new FOV(map);
            los.Calculate(s_center.X, s_center.Y, _radius, shape);

            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                    if (los[x, y] > 0)
                        losMap[x, y] = true;

            var radArea = shape.PositionsInRadius(s_center, _radius).ToList();
            foreach (var (x, y) in radArea)
                radiusMap[x, y] = true;

            _output.WriteLine("Radius Shape: ");
            PrintArray(radiusMap);

            _output.WriteLine("LOS Shape: ");
            PrintArray(losMap);

            return EquivalentArrays(radiusMap, losMap);
        }

        private bool TestSenseMap(SourceType algorithm, Radius shape)
        {
            var map = RectResMap(_width, _height);

            // Start out at false
            var radiusMap = new bool[_width, _height];
            var losMap = new bool[_width, _height];

            var los = new SenseMap(map);
            var lightSource = new SenseSource(algorithm, s_center, _radius, shape);
            los.AddSenseSource(lightSource);
            los.Calculate();

            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                    if (los[x, y] > 0)
                        losMap[x, y] = true;

            var radArea = shape.PositionsInRadius(s_center, _radius).ToList();
            foreach (var pos in radArea)
                radiusMap[pos.X, pos.Y] = true;

            _output.WriteLine("Radius Shape: ");
            PrintArray(radiusMap);

            _output.WriteLine("LOS Shape: ");
            PrintArray(losMap);

            return EquivalentArrays(radiusMap, losMap);
        }

        [Fact]
        public void CircleLosShape() => Assert.True(TestLos(Radius.Circle));

        [Fact]
        public void CircleRadius()
        {
            var testFOVMap = MockFactory.EmptyFOVMap(17, 17);
            var testResMap = new LambdaTranslationMap<bool, double>(testFOVMap, b => 0.0);
            var myFov = new FOV(testFOVMap);
            var myLighting = new SenseMap(testResMap);
            // Circle at 8, 8; radius 7

            myLighting.AddSenseSource(new SenseSource(SourceType.Shadow, (8, 8), 7, Radius.Circle));
            myFov.Calculate(8, 8, 7, Radius.Circle);
            myLighting.Calculate();
            _output.WriteLine(myLighting.ToString(2));
            for (var x = 0; x < testResMap.Width; x++)
                for (var y = 0; y < testResMap.Height; y++)
                    Assert.Equal(myFov[x, y], myLighting[x, y]); // Both got the same results
        }

        [Fact]
        public void DiamondLosShape() => Assert.True(TestLos(Radius.Diamond));

        [Fact]
        public void EqualLargeMap()
        {
            var testResMap = MockFactory.TestResMap(17, 17);
            var testFOVMap = new LambdaTranslationMap<double, bool>(testResMap, d => !(d >= 1.0));

            testResMap[8, 8] = 0.0; // Make sure start is free

            var myFov = new FOV(testFOVMap);
            var myLighting = new SenseMap(testResMap);

            // Circle at 8, 8; radius 7
            myLighting.AddSenseSource(new SenseSource(SourceType.Shadow, (8, 8), 7, Radius.Circle));
            myLighting.Calculate();

            myFov.Calculate(8, 8, 7, Radius.Circle);

            _output.WriteLine("LOS: ");
            _output.WriteLine(myFov.ToString(2));

            _output.WriteLine("\nLighting:");
            _output.WriteLine(myLighting.ToString(2));

            for (var x = 0; x < testResMap.Width; x++)
                for (var y = 0; y < testResMap.Height; y++)
                    Assert.Equal(myFov[x, y], myLighting[x, y]); // Both got the same results
        }

        [Fact]
        public void FOVBooleanOutput()
        {
            var map = MockFactory.Rectangle(_width, _height);
            var fov = new FOV(map);
            fov.Calculate(5, 5, 3);

            _output.WriteLine("FOV for reference:");
            _output.WriteLine(fov.ToString(2));

            foreach (var pos in fov.Positions())
            {
                var inFOV = Math.Abs(fov[pos]) > 0.0000000001;
                Assert.Equal(inFOV, fov.BooleanFOV[pos]);
            }
        }

        [Fact]
        public void FOVCurrentHash()
        {
            var map = MockFactory.BoxResMap(50, 50);
            var fovMap = new LambdaTranslationMap<double, bool>(map, d => d >= 1.0 ? false : true);
            var fov = new FOV(fovMap);

            fov.Calculate(20, 20, 10);

            // Inefficient copy but fine for testing
            var currentFov = new HashSet<Point>(fov.CurrentFOV);

            for (var x = 0; x < map.Width; x++)
                for (var y = 0; y < map.Height; y++)
                    if (fov[x, y] > 0.0)
                        Assert.Contains(new Point(x, y), currentFov);
                    else
                        Assert.DoesNotContain(new Point(x, y), currentFov);
        }

        [Fact]
        public void FOVNewlySeenUnseen()
        {
            var map = MockFactory.BoxResMap(50, 50);
            var fovMap = new LambdaTranslationMap<double, bool>(map, d => !(d >= 1.0));
            var fov = new FOV(fovMap);

            fov.Calculate(20, 20, 10, Radius.Square);
            var prevFov = new HashSet<Point>(fov.CurrentFOV);

            fov.Calculate(19, 19, 10, Radius.Square);
            var curFov = new HashSet<Point>(fov.CurrentFOV);
            var newlySeen = new HashSet<Point>(fov.NewlySeen);
            var newlyUnseen = new HashSet<Point>(fov.NewlyUnseen);

            foreach (var pos in prevFov)
                if (!curFov.Contains(pos))
                    Assert.Contains(pos, newlyUnseen);
                else
                    Assert.DoesNotContain(pos, newlyUnseen);

            foreach (var pos in curFov)
                if (!prevFov.Contains(pos))
                    Assert.Contains(pos, newlySeen);
                else
                    Assert.DoesNotContain(pos, newlySeen);
        }

        [Fact]
        public void FOVSenseMapEquivalency()
        {
            var map = (ArrayMap<bool>)MockFactory.Rectangle(_width, _height);
            var positions = Enumerable.Range(0, 100).Select(x => map.RandomPosition(true)).ToList();

            // Make 2-layer thick walls to verify wall-lighting is working properly
            foreach (var pos in map.Positions())
                if (pos.X == 1 || pos.Y == 1 || pos.X == map.Width - 2 || pos.Y == map.Height - 2)
                    map[pos] = false;

            var fov = new FOV(map);
            var senseMap = new SenseMap(new LambdaTranslationMap<bool, double>(map, x => x ? 0.0 : 1.0));
            var senseSource = new SenseSource(SourceType.Shadow, map.RandomPosition(true), 5, Distance.Euclidean);
            senseMap.AddSenseSource(senseSource);


            foreach (var curPos in positions)
            {
                if (!map[curPos])
                    continue;

                senseSource.Position = curPos;
                fov.Calculate(senseSource.Position, senseSource.Radius, senseSource.DistanceCalc);
                senseMap.Calculate();

                foreach (var pos in map.Positions())
                {
                    var success = fov.BooleanFOV[pos] == senseMap[pos] > 0.0;

                    if (!success)
                    {
                        _output.WriteLine($"Failed on pos {pos} with source at {senseSource.Position}.");
                        _output.WriteLine($"FOV: {fov[pos]}, SenseMap: {senseMap[pos]}");
                        _output.WriteLine(
                            $"Distance between source and fail point: {Distance.Euclidean.Calculate(senseSource.Position, pos)}, source radius: {senseSource.Radius}");
                    }

                    Assert.True(success);
                }
            }


            var degreesList = Enumerable.Range(0, 360).ToList();
            degreesList.FisherYatesShuffle();
            var spanList = Enumerable.Range(1, 359).ToList();
            spanList.FisherYatesShuffle();

            var degrees = degreesList.Take(30).ToList();
            var spans = spanList.Take(30).ToList();

            senseSource.IsAngleRestricted = true;
            // Test angle-based shadowcasting
            foreach (var curPos in positions.Take(1))
            {
                if (!map[curPos])
                    continue;

                foreach (var degree in degrees)
                    foreach (var span in spans)
                    {
                        senseSource.Angle = degree;
                        senseSource.Span = span;

                        senseSource.Position = curPos;
                        fov.Calculate(senseSource.Position, senseSource.Radius, senseSource.DistanceCalc,
                            senseSource.Angle,
                            senseSource.Span);
                        senseMap.Calculate();

                        foreach (var pos in map.Positions())
                        {
                            var success = fov.BooleanFOV[pos] == senseMap[pos] > 0.0;

                            if (!success)
                            {
                                _output.WriteLine(
                                    $"Failed on pos {pos} with source at {senseSource.Position}, angle: {senseSource.Angle}, span: {senseSource.Span}.");
                                _output.WriteLine($"FOV: {fov[pos]}, SenseMap: {senseMap[pos]}");
                                _output.WriteLine(
                                    $"Distance between source and fail point: {Distance.Euclidean.Calculate(senseSource.Position, pos)}, source radius: {senseSource.Radius}");
                            }

                            Assert.True(success);
                        }
                    }
            }
        }

        [Fact]
        public void RippleCircleValue()
        {
            const int mapSize = 30;
            const int radius = 10;
            var radType = Radius.Circle;
            Point sourcePos = (15, 15);

            var resMap = MockFactory.BoxResMap(mapSize, mapSize);
            var senseMap = new SenseMap(resMap);

            var source = new SenseSource(SourceType.Ripple, sourcePos, radius, radType);
            senseMap.AddSenseSource(source);

            senseMap.Calculate();

            _output.WriteLine("Map on 10x10, light source at (2, 3), 3 circle radius, using ripple is: ");
            _output.WriteLine(senseMap.ToString(4));
        }

        [Fact]
        public void RippleSenseMapCircleShape() => Assert.True(TestSenseMap(SourceType.Ripple, Radius.Circle));

        [Fact]
        public void RippleSenseMapDiamondShape() => Assert.True(TestSenseMap(SourceType.Ripple, Radius.Diamond));

        [Fact]
        public void RippleSenseMapSquareShape() => Assert.True(TestSenseMap(SourceType.Ripple, Radius.Square));

        [Fact]
        public void SenseMapCurrentHash()
        {
            var map = MockFactory.BoxResMap(50, 50);
            var senseMap = new SenseMap(map);
            senseMap.AddSenseSource(new SenseSource(SourceType.Ripple, (20, 20), 10, Radius.Circle));

            senseMap.Calculate();

            // Inefficient copy but fine for testing
            var currentSenseMap = new HashSet<Point>(senseMap.CurrentSenseMap);

            for (var x = 0; x < map.Width; x++)
                for (var y = 0; y < map.Height; y++)
                    if (senseMap[x, y] > 0.0)
                        Assert.Contains(new Point(x, y), currentSenseMap);
                    else
                        Assert.DoesNotContain(new Point(x, y), currentSenseMap);
        }

        [Fact]
        public void ShadowSenseMapCircleShape() => Assert.True(TestSenseMap(SourceType.Shadow, Radius.Circle));

        [Fact]
        public void ShadowSenseMapDiamondShape() => Assert.True(TestSenseMap(SourceType.Shadow, Radius.Diamond));

        [Fact]
        public void ShadowSenseMapSquareShape() => Assert.True(TestSenseMap(SourceType.Shadow, Radius.Square));

        [Fact]
        public void SquareLosShape() => Assert.True(TestLos(Radius.Square));

        [Fact]
        public void TestAccessBeforeCalculate()
        {
            var testFOVMap = MockFactory.EmptyFOVMap(17, 17);
            var myFOV = new FOV(testFOVMap);
            foreach (var pos in testFOVMap.Positions())
                Assert.Equal(0.0, myFOV[pos]);
        }
    }
}
