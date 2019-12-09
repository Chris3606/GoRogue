using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class RectangleTests
	{
		[TestMethod]
		public void RectangleConstruction()
		{
			var rect = new Rectangle(1, 1, 10, 10);
			TestRect(rect, (1, 1), (10, 10));

			rect = new Rectangle((2, 2), (10, 10));
			TestRect(rect, (2, 2), (10, 10));

			rect = new Rectangle((10, 5), 10, 5);
			TestRect(rect, (0, 0), (20, 10));
		}

		[TestMethod]
		public void RectangleContains()
		{
			Rectangle myRect = new Rectangle(1, 1, 50, 25);
			TestRectContains(myRect, true, 50, 25);
			TestRectContains(myRect, true, 1, 1);
			TestRectContains(myRect, true, 10, 15);
			TestRectContains(myRect, false, 1, 0);
			TestRectContains(myRect, false, 0, 1);
			TestRectContains(myRect, false, 51, 25);
			TestRectContains(myRect, false, 50, 26);
			TestRectContains(myRect, false, 100, 150);

			Rectangle myRect2 = myRect; // Contained within each other
			Assert.AreEqual(true, myRect.Contains(myRect2));
			Assert.AreEqual(true, myRect2.Contains(myRect));

			myRect2 = new Rectangle(10, 10, 5, 5); // Contained within and smaller than myRect
			Assert.AreEqual(true, myRect.Contains(myRect2));
			Assert.AreEqual(false, myRect2.Contains(myRect));

			// Although myRect3 largely overlaps with myRect, myRect does NOT completely contain
			// myRect3, nor does myRect3 completely contain myRect.
			myRect2 = new Rectangle(-5, -5, 50, 50);
			Assert.AreEqual(false, myRect.Contains(myRect2));
			Assert.AreEqual(false, myRect2.Contains(myRect));
		}

		[TestMethod]
		public void RectangleSetSize()
		{
			Rectangle myRect = new Rectangle(1, 1, 50, 25);
			Assert.AreEqual(new Point(50, 25), myRect.Size);

			myRect = myRect.WithSize(20, 15);
			Assert.AreEqual(new Point(20, 15), myRect.Size);
			myRect = myRect.WithSize((21, 16));
			Assert.AreEqual(new Point(21, 16), myRect.Size);
		}

		private void TestRect(Rectangle rect, Point expectedMinExtent, Point expectedMaxExtent)
		{
			Assert.AreEqual(expectedMinExtent, rect.MinExtent);
			Assert.AreEqual(expectedMaxExtent, rect.MaxExtent);
		}

		private void TestRectContains(Rectangle rect, bool expectedValue, int containsX, int containsY)
		{
			Assert.AreEqual(expectedValue, rect.Contains(containsX, containsY));
			Assert.AreEqual(expectedValue, rect.Contains((containsX, containsY)));
		}
	}
}
