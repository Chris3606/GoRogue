using GoRogue;
using GoRogue.SenseMapping;
using GoRogue.MapGeneration.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class LightingTest
    {
        [TestMethod]
        public void FOVCurrentHash()
        {
            var map = new BoxResMap(50, 50);
            var fov = new FOV(map);

            fov.Calculate(20, 20, 10);

            // Inefficient copy but fine for testing
            HashSet<Coord> currentFov = new HashSet<Coord>(fov.CurrentFOV);

            foreach (var item in currentFov)
                Console.Write(item + ", ");

            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                {
                    if (fov[x, y] > 0.0)
                    {
                        Console.WriteLine("Checking " + Coord.Get(x, y));
                        Assert.AreEqual(true, currentFov.Contains(Coord.Get(x, y)));
                    }
                    else
                        Assert.AreEqual(false, currentFov.Contains(Coord.Get(x, y)));
                }
        }

        [TestMethod]
        public void FOVNewlySeenUnseen()
        {
            var map = new BoxResMap(50, 50);
            var fov = new FOV(map);

            fov.Calculate(20, 20, 10, Radius.SQUARE);
            var prevFov = new HashSet<Coord>(fov.CurrentFOV);

            fov.Calculate(19, 19, 10, Radius.SQUARE);
            var curFov = new HashSet<Coord>(fov.CurrentFOV);
            var newlySeen = new HashSet<Coord>(fov.NewlySeen);
            var newlyUnseen = new HashSet<Coord>(fov.NewlyUnseen);

            foreach (var pos in prevFov)
            {
                if (!curFov.Contains(pos))
                    Assert.AreEqual(true, newlyUnseen.Contains(pos));
                else
                    Assert.AreEqual(false, newlyUnseen.Contains(pos));
            }

            foreach (var pos in curFov)
            {
                if (!prevFov.Contains(pos))
                    Assert.AreEqual(true, newlySeen.Contains(pos));
                else
                    Assert.AreEqual(false, newlySeen.Contains(pos));
            }
        }

        [TestMethod]
        public void CircleRadius()
        {
            var testResMap = new EmptyResMap(17, 17);
            var myFov = new FOV(testResMap);
            var myLighting = new SenseMap(testResMap);
            // Circle at 8, 8; radius 7

            myLighting.AddSenseSource(new SenseSource(SourceType.SHADOW, Coord.Get(8, 8), 7, Radius.CIRCLE));
            myFov.Calculate(8, 8, 7, Radius.CIRCLE);
            myLighting.Calculate();
            for (int x = 0; x < testResMap.Width; x++)
            {
                for (int y = 0; y < testResMap.Height; y++)
                {
                    Console.Write(myLighting[x, y].ToString("0.00") + " ");
                    Assert.AreEqual(myFov[x, y], myLighting[x, y]); // Both got the same results
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void RippleCircleValue()
        {
            int MAP_SIZE = 30;
            int RADIUS = 10;
            Radius RAD_TYPE = Radius.CIRCLE;
            Coord SOURCE_POS = Coord.Get(15, 15);

            BoxResMap resMap = new BoxResMap(MAP_SIZE, MAP_SIZE);
            SenseMap senseMap = new SenseMap(resMap);

            var source = new SenseSource(SourceType.RIPPLE, SOURCE_POS, RADIUS, RAD_TYPE);
            senseMap.AddSenseSource(source);

            senseMap.Calculate();

            Console.WriteLine("Map on 10x10, light source at (2, 3), 3 circle radius, using ripple is: ");
            for (int x = 0; x < MAP_SIZE; x++)
            {
                for (int y = 0; y < MAP_SIZE; y++)
                {
                    Console.Write($"{senseMap[x, y].ToString("N4")} ");
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void EqualLargeMap()
        {
            var testResMap = new TestResMap(17, 17);
            testResMap[8, 8] = 0.0; // Make sure start is free
            var myFov = new FOV(testResMap);
            var myLighting = new SenseMap(testResMap);
            // Circle at 8, 8; radius 7
            myLighting.AddSenseSource(new SenseSource(SourceType.SHADOW, Coord.Get(8, 8), 7, Radius.CIRCLE));
            myFov.Calculate(8, 8, 7, Radius.CIRCLE);
            myLighting.Calculate();
            Console.WriteLine("LOS: ");
            for (int x = 0; x < testResMap.Width; x++)
            {
                for (int y = 0; y < testResMap.Height; y++)
                {
                    Console.Write($"{myFov[x, y].ToString("N2")}\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine("\nLighting:");
            for (int x = 0; x < testResMap.Width; x++)
            {
                for (int y = 0; y < testResMap.Height; y++)
                {
                    Console.Write($"{myLighting[x, y].ToString("N2")}\t");
                }
                Console.WriteLine();
            }
            for (int x = 0; x < testResMap.Width; x++)
                for (int y = 0; y < testResMap.Height; y++)
                {
                    System.Console.WriteLine($"We have ({x},{y}) fov {myFov[x, y]}, lighting {myLighting[Coord.Get(x, y)]}");
                    Assert.AreEqual(myFov[x, y], myLighting[x, y]); // Both got the same results
                }
        }
    }

    internal class TestResMap : ArrayMap<double>
    {
        public TestResMap(int width, int height)
            : base(width, height)
        {
            Random rng = new Random();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    this[x, y] = rng.NextDouble();
                }
        }
    }

    internal class EmptyResMap : IMapView<double>
    {
        public int Width { get; private set; }

        public int Height { get; private set; }

        public double this[int x, int y] { get => 0.0; }
        public double this[Coord c] { get => 0.0; }

        public EmptyResMap(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    internal class BoxResMap : ArrayMap<double>
    {
        public BoxResMap(int width, int height)
            : base(width, height)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                        this[x, y] = 1.0;
                    else
                        this[x, y] = 0.0;
                }
        }
    }
}