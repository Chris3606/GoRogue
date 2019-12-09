using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class PointTests
	{
		[TestMethod]
		public void TestNullEquality()
		{
			Point c1 = (1, 6);
			Point c2 = Point.NONE;

			bool x = c1 == c2;
			Assert.AreEqual(false, x);
			x = c1 != c2;
			Assert.AreEqual(true, x);

			x = c2 == c1;
			Assert.AreEqual(false, x);
			x = c2 != c1;
			Assert.AreEqual(true, x);

			c1 = Point.NONE;
			x = c1 == c2;
			Assert.AreEqual(true, x);

			x = c1 != c2;
			Assert.AreEqual(false, x);
		}
	}
}
