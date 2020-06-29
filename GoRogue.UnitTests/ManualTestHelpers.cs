using System;
using System.Collections.Generic;
using GoRogue.DiceNotation;
using GoRogue.MapViews;
using GoRogue.Pathing;
using GoRogue.SenseMapping;
using GoRogue.SpatialMaps;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit; //using System.Drawing;

namespace GoRogue.UnitTests
{
    public class ManualTestHelpers
    {
        private const int width = 50;
        private const int height = 50;

        [Fact]
        public void ManualPrint2DArray()
        {
            var array = new int[10, 10];
            var i = 0;

            for (var x = 0; x < 10; x++)
            for (var y = 0; y < 10; y++)
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

        [Fact]
        public void ManualPrintAdjacencyRule()
        {
            Console.WriteLine(AdjacencyRule.Cardinals);
            Console.WriteLine(AdjacencyRule.EightWay);
            Console.WriteLine(AdjacencyRule.Diagonals);
        }

        [Fact]
        public void ManualPrintDiceExpression()
        {
            var expr = Dice.Parse("1d(1d12+4)+5*3");

            Console.WriteLine(expr);
        }

        [Fact]
        public void ManualPrintDictionary()
        {
            var myDict = new Dictionary<int, string>();

            myDict[1] = "On";
            myDict[3] = "Three";
            myDict[1] = "One";
            myDict[2] = "Two";

            Console.WriteLine(myDict.ExtendToString());
        }

        [Fact]
        public void ManualPrintDisjointSet()
        {
            var s = new DisjointSet(5);

            s.MakeUnion(1, 3);
            s.MakeUnion(2, 4);

            Console.WriteLine(s);
        }

        [Fact]
        public void ManualPrintFOV()
        {
            var map = new ArrayMap<bool>(10, 10);
            map = (ArrayMap<bool>)MockFactory.Rectangle(width, height);
            var myFov = new FOV(map);
            myFov.Calculate(5, 5, 3);

            Console.WriteLine(myFov);
            Console.WriteLine();
            Console.WriteLine(myFov.ToString(3));
        }

        [Fact]
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

        [Fact]
        public void ManualPrintMockMap()
        {
            ISettableMapView<bool> map = (ArrayMap<bool>)MockFactory.Rectangle(width, height);
            Console.Write(map.ToString());
        }

        [Fact]
        public void ManualPrintMultiSpatialMap()
        {
            var sm = new MultiSpatialMap<MyIDImpl>();

            sm.Add(new MyIDImpl(1), 1, 2);
            sm.Add(new MyIDImpl(2), 1, 2);

            sm.Add(new MyIDImpl(3), 4, 5);

            Console.WriteLine(sm);
        }

        [Fact]
        public void ManualPrintPath()
        {
            var map = new ArrayMap<bool>(30, 30);
            map = (ArrayMap<bool>)MockFactory.Rectangle(width, height);
            var pather = new AStar(map, Distance.Manhattan);
            var path = pather.ShortestPath(1, 2, 5, 6);

            Console.WriteLine(path);
        }

        [Fact]
        public void ManualPrintRadius()
        {
            Console.WriteLine(Radius.Circle);
            Console.WriteLine(Radius.Square);
            Console.WriteLine(Radius.Diamond);
        }

        [Fact]
        public void ManualPrintRadiusLocationContext()
        {
            var boundlessAreaProv = new RadiusLocationContext((5, 4), 10);
            var boundedAreaProv = new RadiusLocationContext((5, 4), 10, new Rectangle(0, 0, 10, 10));

            Console.WriteLine(boundlessAreaProv + " is boundless!");
            Console.WriteLine(boundedAreaProv + " is bounded...");
        }

        [Fact]
        public void ManualPrintRectangle()
        {
            var rect = new Rectangle(0, 0, 10, 10);
            Console.WriteLine(rect);
        }

        [Fact]
        public void ManualPrintSenseMap()
        {
            var map = new ArrayMap<bool>(30, 30);
            map = (ArrayMap<bool>)MockFactory.Rectangle(width, height);

            var resMap = new ResMap(map);
            var senseMap = new SenseMap(resMap);

            var source = new SenseSource(SourceType.Shadow, 12, 15, 10, Radius.Circle);
            var source2 = new SenseSource(SourceType.Shadow, 18, 15, 10, Radius.Circle);
            senseMap.AddSenseSource(source);
            senseMap.AddSenseSource(source2);

            senseMap.Calculate();

            Console.WriteLine(senseMap);
            Console.WriteLine();
            Console.WriteLine(senseMap.ToString(3));
            Console.WriteLine();
            Console.WriteLine(source);
        }

        [Fact]
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

        [Fact]
        public void ManualPrintSpatialMap()
        {
            var sm = new SpatialMap<MyIDImpl>();

            sm.Add(new MyIDImpl(1), 1, 2);
            sm.Add(new MyIDImpl(2), 1, 3);
            sm.Add(new MyIDImpl(3), 4, 5);

            Console.WriteLine(sm);
        }
    }
}
