using GoRogue;
using GoRogue.MapGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class SerializationTests
	{
		[TestMethod]
		public void SerializeAdjacencyRule() => TestSerialization(AdjacencyRule.EIGHT_WAY);

		[TestMethod]
		public void SerializeBoundedRectangle() => TestSerialization(new BoundedRectangle((1, 2, 3, 4), (10, 10, 10, 10)));

		[TestMethod]
		public void SerializeCoord() => TestSerialization<Coord>((1, 2));

		[TestMethod]
		public void SerializeDirection() => TestSerialization(Direction.DOWN_RIGHT);

		[TestMethod]
		public void SerializeDisjointSet()
		{
			var dj = new DisjointSet(10);
			dj.MakeUnion(1, 2);
			dj.MakeUnion(2, 6);

			TestSerialization(dj, DisjointSetEquality);
		}

		[TestMethod]
		public void TestDistance() => TestSerialization(Distance.CHEBYSHEV);

		[TestMethod]
		public void TestMapArea()
		{
			var area = new MapArea();
			area.Add((1, 2, 3, 4));

			TestSerialization(area);
		}

		[TestMethod]
		public void TestRadius() => TestSerialization(Radius.CIRCLE);

		[TestMethod]
		public void TestRadiusAreaProvider()
		{
			var rap = new RadiusAreaProvider((2, 3), 5, Distance.CHEBYSHEV);
			rap.CalculatePositions();

			TestSerialization(rap);
		}

		[TestMethod]
		public void TestRectangle() => TestSerialization(new Rectangle(2, 1, 5, 8));

		private bool DisjointSetEquality(DisjointSet t1, DisjointSet t2)
		{
			if (t1.Count != t2.Count)
				return false;

			for (int i = 0; i < t1.Count; i++)
				if (t1.Find(i) != t2.Find(i))
					return false;

			return true;
		}
		private void TestSerialization<T>(T typeToSerialize, System.Func<T, T, bool> equality = null)
		{
			if (equality == null)
				equality = (t1, t2) => t1.Equals(t2);

			string name = $"{typeof(T)}.bin";

			var formatter = new BinaryFormatter();
			using (var stream = new FileStream(name, FileMode.Create, FileAccess.Write))
				formatter.Serialize(stream, typeToSerialize);

			T reSerialized = default;
			using (var stream = new FileStream(name, FileMode.Open, FileAccess.Read))
				reSerialized = (T)formatter.Deserialize(stream);

			Assert.AreEqual(true, equality(typeToSerialize, reSerialized));
		}
	}
}
