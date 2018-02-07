using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class SpatialMapTests
    {
        [TestMethod]
        public void SpatialMapAdd()
        {
            var mySpatialMap = new SpatialMap<MyIDImpl>();

            var myId1 = new MyIDImpl(0);
            var myId2 = new MyIDImpl(1);
            mySpatialMap.Add(myId1, Coord.Get(1, 2));
            Assert.AreEqual(1, mySpatialMap.Count);

            bool retVal = mySpatialMap.Contains(Coord.Get(1, 2));
            Assert.AreEqual(true, retVal);

            retVal = mySpatialMap.Contains(myId1);
            Assert.AreEqual(true, retVal);

            retVal = mySpatialMap.Contains(Coord.Get(2, 3));
            Assert.AreEqual(false, retVal);

            retVal = mySpatialMap.Contains(myId2);
            Assert.AreEqual(false, retVal);

            int count = 0;
            foreach (var item in mySpatialMap.GetItems(Coord.Get(1, 2)))
                count++;
            Assert.AreEqual(1, count);

            count = 0;
            foreach (var item in mySpatialMap.GetItems(Coord.Get(2, 3)))
                count++;
            Assert.AreEqual(0, count);

            retVal = mySpatialMap.Add(myId2, Coord.Get(1, 2));
            Assert.AreEqual(false, retVal);

            count = 0;
            foreach (var item in mySpatialMap.GetItems(Coord.Get(1, 2)))
                count++;
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void SpatialMapCreate()
        {
            var mySpatialMap = new SpatialMap<MyIDImpl>();
            Assert.AreEqual(0, mySpatialMap.Count);
            bool retVal = mySpatialMap.Remove(new MyIDImpl(0));
            Assert.AreEqual(false, retVal);

            retVal = false;
            foreach (var item in mySpatialMap.Remove(Coord.Get(1, 2)))
                retVal = true;
            Assert.AreEqual(false, retVal);

            retVal = false;
            foreach (var item in mySpatialMap.Items)
                retVal = true;
            Assert.AreEqual(false, retVal);

            retVal = mySpatialMap.Move(new MyIDImpl(0), Coord.Get(5, 6));
            Assert.AreEqual(false, retVal);

            retVal = false;
            foreach (var item in mySpatialMap.Move(Coord.Get(1, 2), Coord.Get(5, 6)))
                retVal = true;
            Assert.AreEqual(false, retVal);
        }

        [TestMethod]
        public void SpatialMapMove()
        {
            var mySpatialMap = new SpatialMap<MyIDImpl>();

            var myId1 = new MyIDImpl(0);
            var myId2 = new MyIDImpl(1);
            mySpatialMap.Add(myId1, Coord.Get(1, 2));
            mySpatialMap.Add(myId2, Coord.Get(2, 3));

            bool retVal = mySpatialMap.Move(myId1, Coord.Get(5, 6));
            Assert.AreEqual(true, retVal);
            Assert.AreEqual(Coord.Get(5, 6), mySpatialMap.GetPosition(myId1));

            retVal = mySpatialMap.Contains(Coord.Get(5, 6));
            Assert.AreEqual(true, retVal);

            retVal = mySpatialMap.Contains(Coord.Get(1, 2));
            Assert.AreEqual(false, retVal);

            retVal = mySpatialMap.Contains(Coord.Get(2, 3));
            Assert.AreEqual(true, retVal);

            retVal = mySpatialMap.Move(myId2, Coord.Get(5, 6));
            Assert.AreEqual(false, retVal);

            Assert.AreEqual(true, mySpatialMap.Contains(Coord.Get(2, 3)));
            Assert.AreEqual(true, mySpatialMap.Contains(Coord.Get(5, 6)));
        }

        [TestMethod]
        public void SpatialMapRemove()
        {
            var mySpatialMap = new SpatialMap<MyIDImpl>();

            var myId1 = new MyIDImpl(0);
            var myId2 = new MyIDImpl(1);
            var myId3 = new MyIDImpl(2);

            mySpatialMap.Add(myId1, Coord.Get(1, 2));
            mySpatialMap.Add(myId2, Coord.Get(2, 3));
            mySpatialMap.Add(myId3, Coord.Get(3, 4));

            bool retVal = mySpatialMap.Remove(myId1);
            Assert.AreEqual(true, retVal);

            retVal = mySpatialMap.Contains(myId1);
            Assert.AreEqual(false, retVal);

            retVal = mySpatialMap.Contains(Coord.Get(1, 2));
            Assert.AreEqual(false, retVal);

            int count = 0;
            foreach (var i in mySpatialMap.Remove(Coord.Get(2, 3)))
                count++;

            Assert.AreEqual(1, count);
            Assert.AreEqual(false, mySpatialMap.Contains(Coord.Get(2, 3)));
            Assert.AreEqual(false, mySpatialMap.Contains(myId2));

            retVal = mySpatialMap.Remove(myId1);
            Assert.AreEqual(false, retVal);

            count = 0;
            foreach (var i in mySpatialMap.Remove(Coord.Get(5, 6)))
                count++;
            Assert.AreEqual(0, count);
        }
    }

    internal class MyIDImpl : IHasID
    {
        private static IDGenerator idGen = new IDGenerator();

        public MyIDImpl(int myInt)
        {
            ID = idGen.UseID();
            MyInt = myInt;
        }

        public uint ID { get; private set; }
        public int MyInt { get; private set; }
    }
}