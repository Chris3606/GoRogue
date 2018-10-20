using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class DirectionTests
	{
		[TestMethod]
		public void TestGetCardinalDirection()
		{
			int yMin = -30;
			int yMax = 30;
			int xMin = -30;
			int xMax = 30;

			for (int x = xMin; x <= xMax; x++)
				for (int y = yMin; y <= yMax; y++)
				{
					Direction d = Direction.GetCardinalDirection(x, y); // Assume around 0
					bool result = (d == Direction.UP || d == Direction.RIGHT || d == Direction.DOWN || d == Direction.LEFT);

					if (x == y && y == 0)
						result = (d == Direction.NONE);

					if (!result)
						Console.WriteLine($"Failed: We returned: {d} for (x, y) {Coord.Get(x, y)}");
					Assert.AreEqual(true, result);
				}
		}

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