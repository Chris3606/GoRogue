using GoRogue;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class GameFrameworkTests
	{
		[TestMethod]
		public void ApplyTerrainOverlay()
		{
			var grMap = new ArrayMap<bool>(10, 10);
			QuickGenerators.GenerateRectangleMap(grMap);

			var translationMap = new LambdaTranslationMap<bool, IGameObject>(grMap, (pos, val) =>
				val ? new GameObject(pos, 0, null, true, true, true) : new GameObject(pos, 0, null, true, false, false));
			var map = new Map(grMap.Width, grMap.Height, 1, Distance.CHEBYSHEV);

			// Normally you shouldn't need tempMap, could just use translationMap directly.  But we want ref equality comparison
			// capability for testing
			var tempMap = new ArrayMap<IGameObject>(grMap.Width, grMap.Height);
			tempMap.ApplyOverlay(translationMap);
			map.ApplyTerrainOverlay(tempMap);

			Assert.AreEqual(grMap.Width, map.Width);
			Assert.AreEqual(grMap.Height, map.Height);
			foreach (var pos in map.Positions())
				Assert.AreEqual(tempMap[pos], map.GetTerrain(pos));
		}

		[TestMethod]
		public void ApplyTerrainOverlayTranslation()
		{
			var grMap = new ArrayMap<bool>(10, 10);
			QuickGenerators.GenerateRectangleMap(grMap);

			var map = new Map(grMap.Width, grMap.Height, 1, Distance.CHEBYSHEV);
			map.ApplyTerrainOverlay(grMap, (pos, b) => b ? new GameObject(pos, 0, null, true, true, true) : new GameObject(pos, 0, null, true, false, false));

			// If any value is null this fails due to NullReferenceException: otherwise, we assert the right value got set
			foreach (var pos in grMap.Positions())
				if (grMap[pos])
					Assert.IsTrue(map.GetTerrain(pos).IsWalkable == true);
				else
					Assert.IsTrue(map.GetTerrain(pos).IsWalkable == false);
		}

		[TestMethod]
		public void OutOfBoundsTerrainAdd()
		{
			var map = new Map(10, 10, 1, Distance.CHEBYSHEV);
			var obj = new GameObject((-1, -1), 0, null, true, true, true);

			Assert.ThrowsException<ArgumentException>(() => map.SetTerrain(obj));
		}

		[TestMethod]
		public void OutOfBoundsEntityAdd()
		{
			var map = new Map(10, 10, 1, Distance.CHEBYSHEV);
			var obj = new GameObject((-1, -1), 1, null, false, true, true);

			bool added = map.AddEntity(obj);
			Assert.AreEqual(false, added);
		}

		[TestMethod]
		public void OutOfBoundsMove()
		{
			var map = new Map(10, 10, 1, Distance.CHEBYSHEV);
			var obj = new GameObject((1, 1), 1, null, false, true, true);

			bool added = map.AddEntity(obj);
			Assert.AreEqual(true, added);

			var oldPos = obj.Position;
			obj.Position = (-1, -1);
			Assert.AreEqual(oldPos, obj.Position);
		}

		[TestMethod]
		public void ValidEntityAdd()
		{
			var map = new Map(10, 10, 1, Distance.CHEBYSHEV);
			var obj = new GameObject((1, 1), 1, null, false, true, true);

			bool added = map.AddEntity(obj);
			Assert.AreEqual(true, added);
		}

		[TestMethod]
		public void ValidEntityMove()
		{
			var map = new Map(10, 10, 1, Distance.CHEBYSHEV);
			var obj = new GameObject((1, 1), 1, null, false, true, true);

			bool added = map.AddEntity(obj);
			Assert.AreEqual(true, added);

			obj.Position = (5, 5);
			Assert.AreEqual(new Coord(5, 5), obj.Position);
		}
	}
}
