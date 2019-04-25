using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class MultiSpatialMapTests
	{
		[TestMethod]
		public void MultiSpatialMapAdd()
		{
			var mySpatialMap = new MultiSpatialMap<MyIDImpl>();

			var myId1 = new MyIDImpl(0);
			var myId2 = new MyIDImpl(1);
			var myId3 = new MyIDImpl(2);

			mySpatialMap.Add(myId1, (1, 2));
			Assert.AreEqual(1, mySpatialMap.Count);

			bool retVal = mySpatialMap.Contains((1, 2));
			Assert.AreEqual(true, retVal);

			retVal = mySpatialMap.Contains(myId1);
			Assert.AreEqual(true, retVal);

			retVal = mySpatialMap.Contains((2, 3));
			Assert.AreEqual(false, retVal);

			retVal = mySpatialMap.Contains(myId2);
			Assert.AreEqual(false, retVal);

			int count = 0;
			foreach (var item in mySpatialMap.GetItems((1, 2)))
				count++;
			Assert.AreEqual(1, count);

			count = 0;
			foreach (var item in mySpatialMap.GetItems((2, 3)))
				count++;
			Assert.AreEqual(0, count);

			retVal = mySpatialMap.Add(myId2, (1, 2));
			Assert.AreEqual(true, retVal);

			count = 0;
			foreach (var item in mySpatialMap.GetItems((1, 2)))
				count++;
			Assert.AreEqual(2, count);
		}

		[TestMethod]
		public void MultiSpatialMapCreate()
		{
			var mySpatialMap = new MultiSpatialMap<MyIDImpl>();
			Assert.AreEqual(0, mySpatialMap.Count);
			bool retVal = mySpatialMap.Remove(new MyIDImpl(0));
			Assert.AreEqual(false, retVal);

			retVal = false;
			foreach (var item in mySpatialMap.Remove((1, 2)))
				retVal = true;
			Assert.AreEqual(false, retVal);

			retVal = false;
			foreach (var item in mySpatialMap.Items)
				retVal = true;
			Assert.AreEqual(false, retVal);

			retVal = mySpatialMap.Move(new MyIDImpl(0), (5, 6));
			Assert.AreEqual(false, retVal);

			retVal = false;
			foreach (var item in mySpatialMap.Move((1, 2), (5, 6)))
				retVal = true;
			Assert.AreEqual(false, retVal);
		}

		[TestMethod]
		public void MultiSpatialMapMove()
		{
			var mySpatialMap = new MultiSpatialMap<MyIDImpl>();

			var myId1 = new MyIDImpl(0);
			var myId2 = new MyIDImpl(1);
			mySpatialMap.Add(myId1, (1, 2));
			mySpatialMap.Add(myId2, (2, 3));

			bool retVal = mySpatialMap.Move(myId1, (5, 6));
			Assert.AreEqual(true, retVal);
			Assert.AreEqual(new Coord(5, 6), mySpatialMap.GetPosition(myId1));

			retVal = mySpatialMap.Contains((5, 6));
			Assert.AreEqual(true, retVal);

			retVal = mySpatialMap.Contains((1, 2));
			Assert.AreEqual(false, retVal);

			retVal = mySpatialMap.Contains((2, 3));
			Assert.AreEqual(true, retVal);

			retVal = mySpatialMap.Move(myId2, (5, 6));
			Assert.AreEqual(true, retVal);

			Assert.AreEqual(false, mySpatialMap.Contains((2, 3)));
			Assert.AreEqual(true, mySpatialMap.Contains((5, 6)));

			int count = 0;
			foreach (var i in mySpatialMap.GetItems((5, 6)))
				count++;
			Assert.AreEqual(2, count);
		}

		[TestMethod]
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

			bool retVal = mySpatialMap.Remove(myId1);
			Assert.AreEqual(true, retVal);

			retVal = mySpatialMap.Contains(myId1);
			Assert.AreEqual(false, retVal);

			retVal = mySpatialMap.Contains((1, 2));
			Assert.AreEqual(true, retVal);

			int count = 0;
			foreach (var i in mySpatialMap.Remove((2, 3)))
				count++;

			Assert.AreEqual(1, count);

			Assert.AreEqual(false, mySpatialMap.Contains((2, 3)));
			Assert.AreEqual(false, mySpatialMap.Contains(myId2));

			retVal = mySpatialMap.Remove(myId1);
			Assert.AreEqual(false, retVal);

			count = 0;
			foreach (var i in mySpatialMap.Remove((5, 6)))
				count++;
			Assert.AreEqual(0, count);
		}
	}
}