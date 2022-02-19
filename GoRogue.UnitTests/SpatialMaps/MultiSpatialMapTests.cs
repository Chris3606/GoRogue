using System;
using System.Linq;
using GoRogue.SpatialMaps;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.SpatialMaps
{
    public class MultiSpatialMapTests
    {
        private MultiSpatialMap<MyIDImpl> _spatialMap;
        private MyIDImpl[] _mapObjects;

        public MultiSpatialMapTests()
        {
            _spatialMap = new MultiSpatialMap<MyIDImpl>();
            _mapObjects = new MyIDImpl[4];

            for (int i = 0; i < _mapObjects.Length; i++)
                _mapObjects[i] = new MyIDImpl(i);
        }

        private void AddNItems(int n)
        {
        }

        [Fact]
        public void MultiSpatialMapAdd()
        {
            var mySpatialMap = new MultiSpatialMap<MyIDImpl>();

            var myId1 = new MyIDImpl(0);
            var myId2 = new MyIDImpl(1);

            mySpatialMap.Add(myId1, (1, 2));
            Assert.Equal(1, mySpatialMap.Count);

            var retVal = mySpatialMap.Contains((1, 2));
            Assert.True(retVal);

            retVal = mySpatialMap.Contains(myId1);
            Assert.True(retVal);

            retVal = mySpatialMap.Contains((2, 3));
            Assert.False(retVal);

            retVal = mySpatialMap.Contains(myId2);
            Assert.False(retVal);

            var count = mySpatialMap.GetItemsAt((1, 2)).Count();
            Assert.Equal(1, count);

            count = mySpatialMap.GetItemsAt((2, 3)).Count();
            Assert.Equal(0, count);

            mySpatialMap.Add(myId2, (1, 2));

            count = mySpatialMap.GetItemsAt((1, 2)).Count();
            Assert.Equal(2, count);
        }

        [Fact]
        public void MultiSpatialMapCreate()
        {
            var mySpatialMap = new MultiSpatialMap<MyIDImpl>();
            Assert.Equal(0, mySpatialMap.Count);
            //mySpatialMap.Remove(new MyIDImpl(0));

            Assert.Empty(mySpatialMap.Items);

            mySpatialMap.Add(new MyIDImpl(0), (2, 2));
            Assert.Single(mySpatialMap.Items);

            Assert.Empty(mySpatialMap.Remove((1, 2)));
        }

        [Fact]
        public void MultiSpatialMapMove()
        {
            var mySpatialMap = new MultiSpatialMap<MyIDImpl>();

            var myId1 = new MyIDImpl(0);
            var myId2 = new MyIDImpl(1);
            mySpatialMap.Add(myId1, (1, 2));
            mySpatialMap.Add(myId2, (2, 3));

            mySpatialMap.Move(myId1, (5, 6));
            Assert.Equal(new Point(5, 6), mySpatialMap.GetPositionOf(myId1));

            var retVal = mySpatialMap.Contains((5, 6));
            Assert.True(retVal);

            retVal = mySpatialMap.Contains((1, 2));
            Assert.False(retVal);

            retVal = mySpatialMap.Contains((2, 3));
            Assert.True(retVal);

            mySpatialMap.Move(myId2, (5, 6));


            Assert.False(mySpatialMap.Contains((2, 3)));
            Assert.True(mySpatialMap.Contains((5, 6)));

            Assert.Equal(2, mySpatialMap.GetItemsAt((5, 6)).Count());
        }

        [Fact]
        public void MultiSpatialMapRemove()
        {
            var mySpatialMap = new MultiSpatialMap<MyIDImpl>();

            var myId1 = new MyIDImpl(0);
            var myId2 = new MyIDImpl(1);
            var myId3 = new MyIDImpl(2);
            var myId4 = new MyIDImpl(3);

            mySpatialMap.Add(myId1, (1, 2));
            mySpatialMap.Add(myId2, (2, 3));
            mySpatialMap.Add(myId3, (3, 4));
            mySpatialMap.Add(myId4, (1, 2));

            mySpatialMap.Remove(myId1);


            var retVal = mySpatialMap.Contains(myId1);
            Assert.False(retVal);

            retVal = mySpatialMap.Contains((1, 2));
            Assert.True(retVal);



            Assert.Single(mySpatialMap.Remove((2, 3)));

            Assert.False(mySpatialMap.Contains((2, 3)));
            Assert.False(mySpatialMap.Contains(myId2));

            Assert.Throws<ArgumentException>(() => mySpatialMap.Remove(myId1));


            Assert.Empty(mySpatialMap.Remove((5, 6)));
        }

        [Fact]
        public void MultiSpatialMapMoveAll()
        {
            var mySpatialMap = new MultiSpatialMap<MyIDImpl>();

            var myId1 = new MyIDImpl(0);
            var myId2 = new MyIDImpl(1);
            var myId3 = new MyIDImpl(2);
            var myId4 = new MyIDImpl(3);

            mySpatialMap.Add(myId1, (1, 2));
            mySpatialMap.Add(myId2, (2, 3));
            mySpatialMap.Add(myId3, (3, 4));
            mySpatialMap.Add(myId4, (1, 2));

            // Move the two items at (1, 2) to (3, 4) (already occupied location)
            mySpatialMap.MoveAll((1, 2), (3, 4));
            Assert.Equal(new Point(3, 4), mySpatialMap.GetPositionOf(myId1));
            Assert.Equal(new Point(3, 4), mySpatialMap.GetPositionOf(myId4));
            Assert.Equal(3, mySpatialMap.GetItemsAt((3, 4)).Count());

            var retVal = mySpatialMap.Contains((3, 4));
            Assert.True(retVal);

            retVal = mySpatialMap.Contains((1, 2));
            Assert.False(retVal);

            retVal = mySpatialMap.Contains((2, 3));
            Assert.True(retVal);

            // Move the one item at (2, 3) to (6, 7) (unoccupied location)
            mySpatialMap.MoveAll((2, 3), (6, 7));

            Assert.False(mySpatialMap.Contains((2, 3)));
            Assert.True(mySpatialMap.Contains((6, 7)));

            Assert.Single(mySpatialMap.GetItemsAt((6, 7)));
        }

        [Fact]
        public void MultiSpatialMapMoveValid()
        {
            var mySpatialMap = new MultiSpatialMap<MyIDImpl>();

            var myId1 = new MyIDImpl(0);
            var myId2 = new MyIDImpl(1);
            var myId3 = new MyIDImpl(2);
            var myId4 = new MyIDImpl(3);

            mySpatialMap.Add(myId1, (1, 2));
            mySpatialMap.Add(myId2, (2, 3));
            mySpatialMap.Add(myId3, (3, 4));
            mySpatialMap.Add(myId4, (1, 2));

            // Move the two items at (1, 2) to (3, 4) (already occupied location)
            var list = mySpatialMap.MoveValid((1, 2), (3, 4));
            Assert.Equal(2, list.Count);
            Assert.Contains(myId1, list);
            Assert.Contains(myId4, list);
            Assert.Equal(new Point(3, 4), mySpatialMap.GetPositionOf(myId1));
            Assert.Equal(new Point(3, 4), mySpatialMap.GetPositionOf(myId4));
            Assert.Equal(3, mySpatialMap.GetItemsAt((3, 4)).Count());

            var retVal = mySpatialMap.Contains((3, 4));
            Assert.True(retVal);

            retVal = mySpatialMap.Contains((1, 2));
            Assert.False(retVal);

            retVal = mySpatialMap.Contains((2, 3));
            Assert.True(retVal);

            // Move the one item at (2, 3) to (6, 7) (unoccupied location)
            list = mySpatialMap.MoveValid((2, 3), (6, 7));
            Assert.Contains(myId2, list);
            Assert.Single(list);
            Assert.False(mySpatialMap.Contains((2, 3)));
            Assert.True(mySpatialMap.Contains((6, 7)));

            Assert.Single(mySpatialMap.GetItemsAt((6, 7)));
        }
    }
}
