using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class CoordTests
	{
		[TestMethod]
		public void TestNullEquality()
		{
			Coord c1 = (1, 6);
			Coord c2 = Coord.NONE;

			bool x = c1 == c2;
			Assert.AreEqual(false, x);
			x = c1 != c2;
			Assert.AreEqual(true, x);

			x = c2 == c1;
			Assert.AreEqual(false, x);
			x = c2 != c1;
			Assert.AreEqual(true, x);

			c1 = Coord.NONE;
			x = c1 == c2;
			Assert.AreEqual(true, x);

			x = c1 != c2;
			Assert.AreEqual(false, x);
		}
	}
}