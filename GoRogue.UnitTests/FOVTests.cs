using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using GoRogue.SenseMapping;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests
{
    public class FOVTests
    {
        private static readonly int height = 30;
        private static readonly int width = 30;
        private static readonly Point Center = (width / 2, height / 2);
        private static readonly int radius = 10;

        private static bool equivalentArrays(bool[,] arr1, bool[,] arr2)
        {
            if (arr1.GetLength(0) != arr2.GetLength(0) || arr1.GetLength(1) != arr2.GetLength(1))
            {
                Console.WriteLine("Error: Arrays weren't equal sizes");
                return false;
            }

            for (var x = 0; x < arr1.GetLength(0); x++)
            for (var y = 0; y < arr1.GetLength(1); y++)
                if (arr1[x, y] != arr2[x, y])
                {
                    Console.WriteLine("Radiuses not equal");
                    return false;
                }

            return true;
        }

        private static void printArray(bool[,] arr)
        {
            for (var y = 0; y < arr.GetLength(1); y++)
            {
                for (var x = 0; x < arr.GetLength(0); x++)
                    if (arr[x, y])
                        Console.Write("1 ");
                    else
                        Console.Write("0 ");
                Console.WriteLine();
            }
        }

        private static ArrayMap<double> rectResMap(int mapWidth, int mapHeight)
        {
            var map = new ArrayMap<bool>(mapWidth, mapHeight);
            var resMap = new ArrayMap<double>(mapWidth, mapHeight);

            map = (ArrayMap<bool>)MockFactory.Rectangle(width, height);

            for (var x = 0; x < map.Width; x++)
            for (var y = 0; y < map.Height; y++)
                resMap[x, y] = map[x, y] ? 0.0 : 1.0;

            return resMap;
        }

        private bool testLOS(Radius shape)
        {
            var map = MockFactory.Rectangle(width, height);
            // Start out at false
            var radiusMap = new bool[width, height];
            var losMap = new bool[width, height];

            var los = new FOV(map);
            los.Calculate(Center.X, Center.Y, radius, shape);

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                if (los[x, y] > 0)
                    losMap[x, y] = true;

            var radArea = shape.PositionsInRadius(Center, radius).ToList();
            foreach (var pos in radArea)
                radiusMap[pos.X, pos.Y] = true;

            Console.WriteLine("Radius Shape: ");
            printArray(radiusMap);

            Console.WriteLine("LOS Shape: ");
            printArray(losMap);

            return equivalentArrays(radiusMap, losMap);
        }

        private bool testSenseMap(SourceType algorithm, Radius shape)
        {
            var map = rectResMap(width, height);

            // Start out at false
            var radiusMap = new bool[width, height];
            var losMap = new bool[width, height];

            var los = new SenseMap(map);
            var lightSource = new SenseSource(algorithm, Center, radius, shape);
            los.AddSenseSource(lightSource);
            los.Calculate();

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                if (los[x, y] > 0)
                    losMap[x, y] = true;

            var radArea = shape.PositionsInRadius(Center, radius).ToList();
            foreach (var pos in radArea)
                radiusMap[pos.X, pos.Y] = true;

            Console.WriteLine("Radius Shape: ");
            printArray(radiusMap);

            Console.WriteLine("LOS Shape: ");
            printArray(losMap);

            return equivalentArrays(radiusMap, losMap);
        }

        [Fact]
        public void CircleLosShape() => Assert.True(testLOS(Radius.Circle));

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
            for (var x = 0; x < testResMap.Width; x++)
            {
                for (var y = 0; y < testResMap.Height; y++)
                {
                    Console.Write(myLighting[x, y].ToString("0.00") + " ");
                    Assert.Equal(myFov[x, y], myLighting[x, y]); // Both got the same results
                }

                Console.WriteLine();
            }
        }

        [Fact]
        public void DiamondLosShape() => Assert.True(testLOS(Radius.Diamond));

        [Fact]
        public void EqualLargeMap()
        {
            var testResMap = MockFactory.TestResMap(17, 17);
            var testFOVMap = new LambdaTranslationMap<double, bool>(testResMap, d => d >= 1.0 ? false : true);
            testResMap[8, 8] = 0.0; // Make sure start is free
            var myFov = new FOV(testFOVMap);
            var myLighting = new SenseMap(testResMap);
            // Circle at 8, 8; radius 7
            myLighting.AddSenseSource(new SenseSource(SourceType.Shadow, (8, 8), 7, Radius.Circle));
            myFov.Calculate(8, 8, 7, Radius.Circle);
            myLighting.Calculate();
            Console.WriteLine("LOS: ");
            for (var x = 0; x < testResMap.Width; x++)
            {
                for (var y = 0; y < testResMap.Height; y++)
                    Console.Write($"{myFov[x, y].ToString("N2")}\t");
                Console.WriteLine();
            }

            Console.WriteLine("\nLighting:");
            for (var x = 0; x < testResMap.Width; x++)
            {
                for (var y = 0; y < testResMap.Height; y++)
                    Console.Write($"{myLighting[x, y].ToString("N2")}\t");
                Console.WriteLine();
            }

            for (var x = 0; x < testResMap.Width; x++)
            for (var y = 0; y < testResMap.Height; y++)
            {
                Console.WriteLine($"We have ({x},{y}) fov {myFov[x, y]}, lighting {myLighting[(x, y)]}");
                Assert.Equal(myFov[x, y], myLighting[x, y]); // Both got the same results
            }
        }

        [Fact]
        public void FOVBooleanOutput()
        {
            var map = new ArrayMap<bool>(10, 10);
            map = (ArrayMap<bool>)MockFactory.Rectangle(width, height);
            var fov = new FOV(map);
            fov.Calculate(5, 5, 3);

            Console.WriteLine("FOV for reference:");
            Console.WriteLine(fov.ToString(2));

            foreach (var pos in fov.Positions())
            {
                var inFOV = fov[pos] != 0.0;
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
                    Assert.True(currentFov.Contains((x, y)));
                else
                    Assert.False(currentFov.Contains((x, y))); //... I think?
        }

        [Fact]
        public void FOVNewlySeenUnseen()
        {
            var map = MockFactory.BoxResMap(50, 50);
            var fovMap = new LambdaTranslationMap<double, bool>(map, d => d >= 1.0 ? false : true);
            var fov = new FOV(fovMap);

            fov.Calculate(20, 20, 10, Radius.Square);
            var prevFov = new HashSet<Point>(fov.CurrentFOV);

            fov.Calculate(19, 19, 10, Radius.Square);
            var curFov = new HashSet<Point>(fov.CurrentFOV);
            var newlySeen = new HashSet<Point>(fov.NewlySeen);
            var newlyUnseen = new HashSet<Point>(fov.NewlyUnseen);

            foreach (var pos in prevFov)
                if (!curFov.Contains(pos))
                    Assert.True(newlyUnseen.Contains(pos));
                else
                    Assert.False(newlyUnseen.Contains(pos));

            foreach (var pos in curFov)
                if (!prevFov.Contains(pos))
                    Assert.True(newlySeen.Contains(pos));
                else
                    Assert.False(newlySeen.Contains(pos));
        }

        [Fact]
        public void FOVSenseMapEquivalency()
        {
            var map = (ArrayMap<bool>)MockFactory.Rectangle(width, height);
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
                    var success = fov.BooleanFOV[pos] == (senseMap[pos] > 0.0 ? true : false);

                    if (!success)
                    {
                        Console.WriteLine($"Failed on pos {pos} with source at {senseSource.Position}.");
                        Console.WriteLine($"FOV: {fov[pos]}, SenseMap: {senseMap[pos]}");
                        Console.WriteLine(
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
                    fov.Calculate(senseSource.Position, senseSource.Radius, senseSource.DistanceCalc, senseSource.Angle,
                        senseSource.Span);
                    senseMap.Calculate();

                    foreach (var pos in map.Positions())
                    {
                        var success = fov.BooleanFOV[pos] == (senseMap[pos] > 0.0 ? true : false);

                        if (!success)
                        {
                            Console.WriteLine(
                                $"Failed on pos {pos} with source at {senseSource.Position}, angle: {senseSource.Angle}, span: {senseSource.Span}.");
                            Console.WriteLine($"FOV: {fov[pos]}, SenseMap: {senseMap[pos]}");
                            Console.WriteLine(
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
            var MAP_SIZE = 30;
            var RADIUS = 10;
            var RAD_TYPE = Radius.Circle;
            Point SOURCE_POS = (15, 15);

            var resMap = MockFactory.BoxResMap(MAP_SIZE, MAP_SIZE);
            var senseMap = new SenseMap(resMap);

            var source = new SenseSource(SourceType.Ripple, SOURCE_POS, RADIUS, RAD_TYPE);
            senseMap.AddSenseSource(source);

            senseMap.Calculate();

            Console.WriteLine("Map on 10x10, light source at (2, 3), 3 circle radius, using ripple is: ");
            for (var x = 0; x < MAP_SIZE; x++)
            {
                for (var y = 0; y < MAP_SIZE; y++)
                    Console.Write($"{senseMap[x, y].ToString("N4")} ");
                Console.WriteLine();
            }
        }

        [Fact]
        public void RippleSenseMapCircleShape() => Assert.True(testSenseMap(SourceType.Ripple, Radius.Circle));

        [Fact]
        public void RippleSenseMapDiamondShape() => Assert.True(testSenseMap(SourceType.Ripple, Radius.Diamond));

        [Fact]
        public void RippleSenseMapSquareShape() => Assert.True(testSenseMap(SourceType.Ripple, Radius.Square));

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
                    Assert.True(currentSenseMap.Contains((x, y)));
                else
                    Assert.False(currentSenseMap.Contains((x, y)));
        }

        [Fact]
        public void ShadowSenseMapCircleShape() => Assert.True(testSenseMap(SourceType.Shadow, Radius.Circle));

        [Fact]
        public void ShadowSenseMapDiamondShape() => Assert.True(testSenseMap(SourceType.Shadow, Radius.Diamond));

        [Fact]
        public void ShadowSenseMapSquareShape() => Assert.True(testSenseMap(SourceType.Shadow, Radius.Square));

        [Fact]
        public void SquareLosShape() => Assert.True(testLOS(Radius.Square));

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
