using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoRogue;
using Microsoft.Xna.Framework;
using GoRect = GoRogue.Rectangle;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class MonoGameConversionTests
	{
		[TestMethod]
		public void CoordToPointConversion()
		{
			Coord c = Coord.Get(1, 2);
			Point p = c;

			Assert.AreEqual(c.X, p.X);
			Assert.AreEqual(c.Y, p.Y);
		}

		[TestMethod]
		public void PointToCoordConversion()
		{
			Point p = new Point(1, 2);
			Coord c = p;

			Assert.AreEqual(p.X, c.X);
			Assert.AreEqual(p.Y, c.Y);
		}

		[TestMethod]
		public void GoRogueRectToMonoGameRectConversion()
		{
			GoRect g = new GoRect(1, 2, 5, 6);
			XnaRect m = g;

			Assert.AreEqual(g.X, m.X);
			Assert.AreEqual(g.Y, m.Y);
			Assert.AreEqual(g.Width, m.Width);
			Assert.AreEqual(g.Height, m.Height);
		}

		[TestMethod]
		public void MonoGameRectToGoRogueRectConversion()
		{
			XnaRect m = new XnaRect(1, 2, 5, 6);
			GoRect g = m;

			Assert.AreEqual(m.X, g.X);
			Assert.AreEqual(m.Y, g.Y);
			Assert.AreEqual(m.Width, g.Width);
			Assert.AreEqual(m.Height, g.Height);
		}
	}
}
