using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoRogue;
using GoRogue.MapGeneration.Generators;
using System;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class ToStringTests
    {
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
