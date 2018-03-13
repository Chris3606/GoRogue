using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoRogue;
using GoRogue.DiceNotation;
using GoRogue.MapGeneration.Generators;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using GoRogue.Pathing;
using GoRogue.SenseMapping;
using System;
using System.Collections.Generic;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class ToStringTests
    {
        [TestMethod]
        public void ManualPrintList()
        {
            var myList = new List<int>();

            myList.Add(1);
            myList.Add(1);
            myList.Add(3);
            myList.Add(2);

            Console.WriteLine(myList.ExtendToString());
            Console.WriteLine("\nWith bar separators:");
            Console.WriteLine(myList.ExtendToString(separator: " | "));
        }

        [TestMethod]
        public void ManualPrintSet()
        {
            var mySet = new HashSet<int>();

            mySet.Add(1);
            mySet.Add(1);
            mySet.Add(3);
            mySet.Add(2);

            Console.WriteLine(mySet.ExtendToString());
            Console.WriteLine("\nWith bar separators:");
            Console.WriteLine(mySet.ExtendToString(separator: " | "));
        }

        [TestMethod]
        public void ManualPrintDictionary()
        {
            var myDict = new Dictionary<int, string>();

            myDict[1] = "On";
            myDict[3] = "Three";
            myDict[1] = "One";
            myDict[2] = "Two";

            Console.WriteLine(myDict.ExtendToString());
        } 

        [TestMethod]
        public void ManualPrint2DArray()
        {
            int[,] array = new int[10, 10];
            int i = 0;

            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                {
                    array[x, y] = i;
                    i++;
                }

            Console.WriteLine(array.ExtendToString());
            Console.WriteLine("\nIn Grid:");
            Console.WriteLine(array.ExtendToStringGrid());
            Console.WriteLine("\nIn Grid with 5 field size:");
            Console.WriteLine(array.ExtendToStringGrid(5));
            Console.WriteLine("\nIn Grid with 5 field size left-align:");
            Console.WriteLine(array.ExtendToStringGrid(-5));


        }

        [TestMethod]
        public void ManualPrintDiceExpression()
        {
            var expr = Dice.Parse("1d(1d12+4)+5*3");

            Console.WriteLine(expr);

        }

        [TestMethod]
        public void ManualPrintMapArea()
        {
            var area = new MapArea();

            area.Add(1, 2);
            area.Add(1, 2);
            area.Add(2, 3);
            area.Add(2, 4);

            Console.WriteLine(area);
        }

        [TestMethod]
        public void ManualPrintPath()
        {
            var map = new ArrayMap<bool>(30, 30);
            RectangleMapGenerator.Generate(map);

            var pather = new AStar(map, Distance.MANHATTAN);
            var path = pather.ShortestPath(1, 2, 5, 6);

            Console.WriteLine(path);
        }

        [TestMethod]
        public void ManualPrintSenseMap()
        {
            var map = new ArrayMap<bool>(30, 30);
            RectangleMapGenerator.Generate(map);

            var resMap = new ResMap(map);
            var senseMap = new SenseMap(resMap);

            var source = new SenseSource(SourceType.SHADOW, 12, 15, 10, Radius.CIRCLE);
            var source2 = new SenseSource(SourceType.SHADOW, 18, 15, 10, Radius.CIRCLE);
            senseMap.AddSenseSource(source);
            senseMap.AddSenseSource(source2);

            senseMap.Calculate();

            Console.WriteLine(senseMap);
            Console.WriteLine();
            Console.WriteLine(senseMap.ToString(3));
            Console.WriteLine();
            Console.WriteLine(source);
        }
        
        [TestMethod]
        public void ManualPrintAdjacencyRule()
        {
            Console.WriteLine(AdjacencyRule.CARDINALS);
            Console.WriteLine(AdjacencyRule.EIGHT_WAY);
            Console.WriteLine(AdjacencyRule.DIAGONALS);
        }


        [TestMethod]
        public void ManualPrintDisjointSet()
        {
            DisjointSet s = new DisjointSet(5);

            s.MakeUnion(1, 3);
            s.MakeUnion(2, 4);

            Console.WriteLine(s);
        }

        [TestMethod]
        public void ManualPrintFOV()
        {
            var map = new ArrayMap<bool>(10, 10);
            RectangleMapGenerator.Generate(map);
            var resMap = new ResMap(map);
            

            FOV myFov = new FOV(resMap);
            myFov.Calculate(5, 5, 3);

            Console.WriteLine(myFov);
            Console.WriteLine();
            Console.WriteLine(myFov.ToString(3));
        }

        [TestMethod]
        public void ManualPrintMultiSpatialMap()
        {
            MultiSpatialMap<MyIDImpl> sm = new MultiSpatialMap<MyIDImpl>();

            sm.Add(new MyIDImpl(1), 1, 2);
            sm.Add(new MyIDImpl(2), 1, 2);

            sm.Add(new MyIDImpl(3), 4, 5);

            Console.WriteLine(sm);
        }

        [TestMethod]
        public void ManualPrintRadius()
        {
            Console.WriteLine(Radius.CIRCLE);
            Console.WriteLine(Radius.SQUARE);
            Console.WriteLine(Radius.DIAMOND);
            Console.WriteLine(Radius.SPHERE);
            Console.WriteLine(Radius.CUBE);
            Console.WriteLine(Radius.OCTAHEDRON);
        }

        [TestMethod]
        public void ManualPrintRadiusAreaProvider()
        {
            var boundlessAreaProv = new RadiusAreaProvider(5, 4, 10, Radius.CIRCLE);
            var boundedAreaProv = new RadiusAreaProvider(5, 4, 10, Radius.CIRCLE, new Rectangle(0, 0, 10, 10));

            Console.WriteLine(boundlessAreaProv + " is boundless!");
            Console.WriteLine(boundedAreaProv + " is bounded...");
        }

        [TestMethod]
        public void ManualPrintRectangle()
        {
            var rect = new Rectangle(0, 0, 10, 10);
            Console.WriteLine(rect);
        }

        [TestMethod]
        public void ManualPrintSpatialMap()
        {
            SpatialMap<MyIDImpl> sm = new SpatialMap<MyIDImpl>();

            sm.Add(new MyIDImpl(1), 1, 2);
            sm.Add(new MyIDImpl(2), 1, 3);
            sm.Add(new MyIDImpl(3), 4, 5);

            Console.WriteLine(sm);
        }
    }
}
