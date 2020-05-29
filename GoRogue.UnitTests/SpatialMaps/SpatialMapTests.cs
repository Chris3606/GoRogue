using GoRogue;
using GoRogue.SpatialMaps;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.SpatialMaps
{
    //public class SpatialMapTests
    //{
    //    private Point newPos;

    //    // Used to test events
    //    private Point oldPos;
    //    [Fact]
    //    public void SpatialMapAdd()
    //    {
    //        var mySpatialMap = new SpatialMap<MyIDImpl>();

    //        var myId1 = new MyIDImpl(0);
    //        var myId2 = new MyIDImpl(1);
    //        mySpatialMap.Add(myId1, (1, 2));
    //        Assert.Equal(1, mySpatialMap.Count);

    //        bool retVal = mySpatialMap.Contains((1, 2));
    //        Assert.Equal(true, retVal);

    //        retVal = mySpatialMap.Contains(myId1);
    //        Assert.Equal(true, retVal);

    //        retVal = mySpatialMap.Contains((2, 3));
    //        Assert.Equal(false, retVal);

    //        retVal = mySpatialMap.Contains(myId2);
    //        Assert.Equal(false, retVal);

    //        var count = 0;
    //        foreach (var item in mySpatialMap.GetItemsAt((1, 2)))
    //            count++;
    //        Assert.Equal(1, count);

    //        count = 0;
    //        foreach (var item in mySpatialMap.GetItemsAt((2, 3)))
    //            count++;
    //        Assert.Equal(0, count);

    //        retVal = mySpatialMap.Add(myId2, (1, 2));
    //        Assert.Equal(false, retVal);

    //        count = 0;
    //        foreach (var item in mySpatialMap.GetItemsAt((1, 2)))
    //            count++;
    //        Assert.Equal(1, count);
    //    }

    //    [Fact]
    //    public void SpatialMapCreate()
    //    {
    //        var mySpatialMap = new SpatialMap<MyIDImpl>();
    //        Assert.Equal(0, mySpatialMap.Count);
    //        bool retVal = mySpatialMap.Remove(new MyIDImpl(0));
    //        Assert.Equal(false, retVal);

    //        retVal = false;
    //        foreach (var item in mySpatialMap.Remove((1, 2)))
    //            retVal = true;
    //        Assert.Equal(false, retVal);

    //        retVal = false;
    //        foreach (var item in mySpatialMap.Items)
    //            retVal = true;
    //        Assert.Equal(false, retVal);

    //        retVal = mySpatialMap.Move(new MyIDImpl(0), (5, 6));
    //        Assert.Equal(false, retVal);

    //        retVal = false;
    //        foreach (var item in mySpatialMap.Move((1, 2), (5, 6)))
    //            retVal = true;
    //        Assert.Equal(false, retVal);
    //    }

    //    [Fact]
    //    public void SpatialMapMove()
    //    {
    //        var mySpatialMap = new SpatialMap<MyIDImpl>();

    //        var myId1 = new MyIDImpl(0);
    //        var myId2 = new MyIDImpl(1);
    //        mySpatialMap.Add(myId1, (1, 2));
    //        mySpatialMap.Add(myId2, (2, 3));

    //        bool retVal = mySpatialMap.Move(myId1, (5, 6));
    //        Assert.Equal(true, retVal);
    //        Assert.Equal(new Coord(5, 6), mySpatialMap.GetPosition(myId1));

    //        retVal = mySpatialMap.Contains((5, 6));
    //        Assert.Equal(true, retVal);

    //        retVal = mySpatialMap.Contains((1, 2));
    //        Assert.Equal(false, retVal);

    //        retVal = mySpatialMap.Contains((2, 3));
    //        Assert.Equal(true, retVal);

    //        retVal = mySpatialMap.Move(myId2, (5, 6));
    //        Assert.Equal(false, retVal);

    //        Assert.Equal(true, mySpatialMap.Contains((2, 3)));
    //        Assert.Equal(true, mySpatialMap.Contains((5, 6)));
    //    }

    //    [Fact]
    //    public void SpatialMapRemove()
    //    {
    //        var mySpatialMap = new SpatialMap<MyIDImpl>();

    //        var myId1 = new MyIDImpl(0);
    //        var myId2 = new MyIDImpl(1);
    //        var myId3 = new MyIDImpl(2);

    //        mySpatialMap.Add(myId1, (1, 2));
    //        mySpatialMap.Add(myId2, (2, 3));
    //        mySpatialMap.Add(myId3, (3, 4));

    //        bool retVal = mySpatialMap.Remove(myId1);
    //        Assert.Equal(true, retVal);

    //        retVal = mySpatialMap.Contains(myId1);
    //        Assert.Equal(false, retVal);

    //        retVal = mySpatialMap.Contains((1, 2));
    //        Assert.Equal(false, retVal);

    //        var count = 0;
    //        foreach (var i in mySpatialMap.Remove((2, 3)))
    //            count++;

    //        Assert.Equal(1, count);
    //        Assert.Equal(false, mySpatialMap.Contains((2, 3)));
    //        Assert.Equal(false, mySpatialMap.Contains(myId2));

    //        retVal = mySpatialMap.Remove(myId1);
    //        Assert.Equal(false, retVal);

    //        count = 0;
    //        foreach (var i in mySpatialMap.Remove((5, 6)))
    //            count++;
    //        Assert.Equal(0, count);
    //    }

    //    [Fact]
    //    public void SpatialMapMoveEvent()
    //    {
    //        var mySpatialMap = new SpatialMap<MyIDImpl>();

    //        var myId1 = new MyIDImpl(0);
    //        var myId2 = new MyIDImpl(1);
    //        var myId3 = new MyIDImpl(2);

    //        mySpatialMap.Add(myId1, (1, 2));
    //        mySpatialMap.Add(myId2, (2, 3));
    //        mySpatialMap.Add(myId3, (3, 4));

    //        mySpatialMap.ItemMoved += onItemMoved;
    //        oldPos = (1, 2);
    //        newPos = (5, 6);
    //        mySpatialMap.Move(myId1, (5, 6));
    //        mySpatialMap.ItemMoved -= onItemMoved;
    //    }

    //    private void onItemMoved(object s, ItemMovedEventArgs<MyIDImpl> e)
    //    {
    //        Assert.Equal(oldPos, e.OldPosition);
    //        Assert.Equal(newPos, e.NewPosition);
    //    }
    //}
}
