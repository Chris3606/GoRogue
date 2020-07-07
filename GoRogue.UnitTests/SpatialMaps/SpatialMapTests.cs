using System;
using GoRogue.SpatialMaps;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.SpatialMaps
{
    public class SpatialMapTests
    {
        private Point _newPos;

        // Used to test events
        private Point _oldPos;

        private void OnItemMoved(object? s, ItemMovedEventArgs<MyIDImpl> e)
        {
            Assert.Equal(_oldPos, e.OldPosition);
            Assert.Equal(_newPos, e.NewPosition);
        }

        [Fact]
        public void SpatialMapAdd()
        {
            var mySpatialMap = new SpatialMap<MyIDImpl>();

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

            Assert.Single(mySpatialMap.GetItemsAt((1, 2)));
            Assert.Empty(mySpatialMap.GetItemsAt((2, 3)));

            Assert.Throws<InvalidOperationException>(() => mySpatialMap.Add(myId2, (1, 2)));

            Assert.Single(mySpatialMap.GetItemsAt((1, 2)));
        }

        [Fact]
        public void SpatialMapCreate()
        {
            var mySpatialMap = new SpatialMap<MyIDImpl>();

            Assert.Equal(0, mySpatialMap.Count);
            Assert.Throws<InvalidOperationException>(() => mySpatialMap.Remove(new MyIDImpl(0)));
            Assert.Empty(mySpatialMap.Remove((1, 2)));
            Assert.Empty(mySpatialMap.Items);
        }

        [Fact]
        public void SpatialMapMove()
        {
            var mySpatialMap = new SpatialMap<MyIDImpl>();

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

            Assert.Throws<InvalidOperationException>(() => mySpatialMap.Move(myId2, (5, 6)));
            Assert.True(mySpatialMap.Contains((2, 3)));
            Assert.True(mySpatialMap.Contains((5, 6)));
        }

        [Fact]
        public void SpatialMapMoveEvent()
        {
            var mySpatialMap = new SpatialMap<MyIDImpl>();

            var myId1 = new MyIDImpl(0);
            var myId2 = new MyIDImpl(1);
            var myId3 = new MyIDImpl(2);

            mySpatialMap.Add(myId1, (1, 2));
            mySpatialMap.Add(myId2, (2, 3));
            mySpatialMap.Add(myId3, (3, 4));

            mySpatialMap.ItemMoved += OnItemMoved;
            _oldPos = (1, 2);
            _newPos = (5, 6);
            mySpatialMap.Move(myId1, (5, 6));
            mySpatialMap.ItemMoved -= OnItemMoved;
        }

        [Fact]
        public void SpatialMapRemove()
        {
            var mySpatialMap = new SpatialMap<MyIDImpl>();

            var myId1 = new MyIDImpl(0);
            var myId2 = new MyIDImpl(1);
            var myId3 = new MyIDImpl(2);

            mySpatialMap.Add(myId1, (1, 2));
            mySpatialMap.Add(myId2, (2, 3));
            mySpatialMap.Add(myId3, (3, 4));

            mySpatialMap.Remove(myId1);

            var retVal = mySpatialMap.Contains(myId1);
            Assert.False(retVal);

            retVal = mySpatialMap.Contains((1, 2));
            Assert.False(retVal);

            Assert.Single(mySpatialMap.Remove((2, 3)));

            Assert.False(mySpatialMap.Contains((2, 3)));
            Assert.False(mySpatialMap.Contains(myId2));

            Assert.Throws<InvalidOperationException>(() => mySpatialMap.Remove(myId1));
            Assert.Empty(mySpatialMap.Remove((5, 6)));
        }

        // TODO: tests for MoveValid, etc.  Split into different test cases like LayeredSpatialMapTests
    }
}
