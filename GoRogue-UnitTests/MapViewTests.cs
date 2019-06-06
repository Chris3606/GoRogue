﻿using GoRogue;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class MapViewTests
	{
		[TestMethod]
		public void ApplyOverlayTest()
		{
			var map = new ArrayMap<bool>(100, 100);
			QuickGenerators.GenerateCellularAutomataMap(map);

			var duplicateMap = new ArrayMap<bool>(map.Width, map.Height);

			duplicateMap.ApplyOverlay(map);

			foreach (var pos in map.Positions())
				Assert.AreEqual(map[pos], duplicateMap[pos]);
		}

		[TestMethod]
		public void LambaMapViewTest()
		{
			ArrayMap<bool> map = new ArrayMap<bool>(10, 10);
			QuickGenerators.GenerateRectangleMap(map);

			IMapView<double> lambdaMapView = new LambdaMapView<double>(map.Width, map.Height, c => map[c] ? 1.0 : 0.0);

			checkMaps(map, lambdaMapView);
		}

		[TestMethod]
		public void LambdaSettableMapViewTest()
		{
			ArrayMap<double> map = new ArrayMap<double>(10, 10);

			ArrayMap<bool> controlMap = new ArrayMap<bool>(10, 10);
			QuickGenerators.GenerateRectangleMap(controlMap);

			var settable = new LambdaSettableMapView<bool>(map.Width, map.Height, c => map[c] > 0.0, (c, b) => map[c] = b ? 1.0 : 0.0);
			QuickGenerators.GenerateRectangleMap(settable);

			checkMaps(controlMap, map);
		}

		[TestMethod]
		public void LambdaSettableTranslationMapTest()
		{
			ArrayMap<bool> map = new ArrayMap<bool>(10, 10);
			QuickGenerators.GenerateRectangleMap(map);

			var settable = new LambdaSettableTranslationMap<bool, double>(map, b => b ? 1.0 : 0.0, d => d > 0.0);
			checkMaps(map, settable);

			// Check other constructor.  Intentaionally "misusing" the position parameter, to make sure we ensure the position
			// parameter is correct without complicating our test case
			settable = new LambdaSettableTranslationMap<bool, double>(map, (pos, b) => map[pos] ? 1.0 : 0.0, (pos, d) => d > 0.0);
			checkMaps(map, settable);
		}

		[TestMethod]
		public void LambdaTranslationMapTest()
		{
			ArrayMap<bool> map = new ArrayMap<bool>(10, 10);
			QuickGenerators.GenerateRectangleMap(map);

			var lambdaMap = new LambdaTranslationMap<bool, double>(map, b => b ? 1.0 : 0.0);

			checkMaps(map, lambdaMap);

			// Check other constructor.  Intentaionally "misusing" the position parameter, to make sure we ensure the position
			// parameter is correct without complicating our test case
			lambdaMap = new LambdaTranslationMap<bool, double>(map, (pos, b) => map[pos] ? 1.0 : 0.0);
			checkMaps(map, lambdaMap);
		}

		[TestMethod]
		public void ViewportBoundingRectangleTest()
		{
			const int VIEWPORT_WIDTH = 1280 / 12;
			const int VIEWPORT_HEIGHT = 768 / 12;
			const int MAP_WIDTH = 250;
			const int MAP_HEIGHT = 250;

			var arrayMap = new ArrayMap<bool>(MAP_WIDTH, MAP_HEIGHT);
			QuickGenerators.GenerateRectangleMap(arrayMap);

			var viewport = new Viewport<bool>(arrayMap, new Rectangle(0, 0, VIEWPORT_WIDTH, VIEWPORT_HEIGHT));
			checkViewportBounds(viewport, (0, 0), (VIEWPORT_WIDTH - 1, VIEWPORT_HEIGHT - 1));

			viewport.ViewArea = viewport.ViewArea.WithPosition((-1, 0)); // Should end up being 0, 0 thanks to bounding
			checkViewportBounds(viewport, (0, 0), (VIEWPORT_WIDTH - 1, VIEWPORT_HEIGHT - 1));

			viewport.ViewArea = viewport.ViewArea.WithPosition((5, 5));
			checkViewportBounds(viewport, (5, 5), (VIEWPORT_WIDTH - 1 + 5, VIEWPORT_HEIGHT - 1 + 5));

			// Move outside x-bounds by 1
			Coord newCenter = (MAP_WIDTH - (VIEWPORT_WIDTH / 2) + 1, MAP_HEIGHT - (VIEWPORT_HEIGHT / 2) + 1);
			// viewport.ViewArea = viewport.ViewArea.NewWithMinCorner(Coord.Get(250, 100));
			viewport.ViewArea = viewport.ViewArea.WithCenter(newCenter);

			Coord minVal = (MAP_WIDTH - VIEWPORT_WIDTH, MAP_HEIGHT - VIEWPORT_HEIGHT);
			Coord maxVal = (MAP_WIDTH - 1, MAP_HEIGHT - 1);
			checkViewportBounds(viewport, minVal, maxVal);
		}

		[TestMethod]
		public void UnboundedViewportTest()
		{
			const int MAP_WIDTH = 100;
			const int MAP_HEIGHT = 100;
			var map = new ArrayMap<int>(MAP_WIDTH, MAP_HEIGHT);
			var unboundedViewport = new UnboundedViewport<int>(map, 1);

			foreach (var pos in map.Positions())
				Assert.AreEqual(0, unboundedViewport[pos]);

			unboundedViewport.ViewArea = unboundedViewport.ViewArea.Translate(5, 5);

			foreach (var pos in unboundedViewport.Positions())
			{
				if (pos.X < MAP_WIDTH - 5 && pos.Y < MAP_HEIGHT - 5)
					Assert.AreEqual(0, unboundedViewport[pos]);
				else
					Assert.AreEqual(1, unboundedViewport[pos]);
			}

			unboundedViewport.ViewArea = unboundedViewport.ViewArea.WithSize(5, 5);

			foreach (var pos in unboundedViewport.Positions())
				Assert.AreEqual(0, unboundedViewport[pos]);

			unboundedViewport.ViewArea = unboundedViewport.ViewArea.WithPosition(MAP_WIDTH - 1, MAP_HEIGHT - 1);

			foreach (var pos in unboundedViewport.Positions())
			{
				if (pos.X == 0 && pos.Y == 0)
					Assert.AreEqual(0, unboundedViewport[pos]);
				else
					Assert.AreEqual(1, unboundedViewport[pos]);
				}
		}

		private static void checkMaps(IMapView<bool> genMap, IMapView<double> fovMap)
		{
			for (int x = 0; x < genMap.Width; x++)
				for (int y = 0; y < genMap.Height; y++)
				{
					var properValue = genMap[x, y] ? 1.0 : 0.0;
					Assert.AreEqual(properValue, fovMap[x, y]);
				}
		}

		private static void checkViewportBounds(Viewport<bool> viewport, Coord expectedMinCorner, Coord expectedMaxCorner)
		{
			Assert.AreEqual(expectedMaxCorner, viewport.ViewArea.MaxExtent);
			Assert.AreEqual(expectedMinCorner, viewport.ViewArea.MinExtent);

			Assert.AreEqual(true, viewport.ViewArea.X >= 0);
			Assert.AreEqual(true, viewport.ViewArea.Y >= 0);
			Assert.AreEqual(true, viewport.ViewArea.X < viewport.MapView.Width);
			Assert.AreEqual(true, viewport.ViewArea.Y < viewport.MapView.Height);

			foreach (var pos in viewport.ViewArea.Positions())
			{
				Assert.AreEqual(true, pos.X >= viewport.ViewArea.X);
				Assert.AreEqual(true, pos.Y >= viewport.ViewArea.Y);

				Assert.AreEqual(true, pos.X <= viewport.ViewArea.MaxExtentX);
				Assert.AreEqual(true, pos.Y <= viewport.ViewArea.MaxExtentY);

				Assert.AreEqual(true, pos.X >= 0);
				Assert.AreEqual(true, pos.Y >= 0);
				Assert.AreEqual(true, pos.X < viewport.MapView.Width);
				Assert.AreEqual(true, pos.Y < viewport.MapView.Height);

				// Utterly stupid way to access things via viewport, but verifies that the coordinate
				// translation is working properly.
				if (pos.X == 0 || pos.Y == 0 || pos.X == viewport.MapView.Width - 1 || pos.Y == viewport.MapView.Height - 1)
					Assert.AreEqual(false, viewport[pos - viewport.ViewArea.MinExtent]);
				else
					Assert.AreEqual(true, viewport[pos - viewport.ViewArea.MinExtent]);
			}
		}
	}
}