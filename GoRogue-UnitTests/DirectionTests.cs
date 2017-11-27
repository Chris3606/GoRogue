using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class DirectionTests
    {
        [TestMethod]
        public void TestYIncDownGetDirection()
        {
            Direction.YIncreasesUpward = false;
            Direction d = Direction.GetDirection(1, 1); // DOWN_RIGHT
            Console.WriteLine("D is: " + d);
            Assert.AreEqual(Direction.DOWN_RIGHT, d);
        }

        [TestMethod]
        public void TestYIncUpGetDirection()
        {
            Direction.YIncreasesUpward = true;
            Direction d = Direction.GetDirection(1, 1); // UP_RIGHT
            Console.WriteLine(d);
            Assert.AreEqual(Direction.UP_RIGHT, d);
        }
    }
}