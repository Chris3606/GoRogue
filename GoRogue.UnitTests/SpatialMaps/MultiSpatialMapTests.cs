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

            Assert.Throws<InvalidOperationException>(() => mySpatialMap.Remove(myId1));


            Assert.Empty(mySpatialMap.Remove((5, 6)));
        }

        // TODO: tests for MoveValid, etc.  Split into different test cases like LayeredSpatialMapTests
    }
}
