using GoRogue.MapGeneration.Generators;
using GoRogue.MapViews;
using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class MapViewTests
    {
        [TestMethod]
        public void LambaMapViewTest()
        {
            ArrayMap<bool> map = new ArrayMap<bool>(10, 10);
            RectangleMapGenerator.Generate(map);

            IMapView<double> lambdaMapView = new LambdaMapView<double>(map.Width, map.Height, c => map[c] ? 1.0 : 0.0);

            checkMaps(map, lambdaMapView);
        }

        [TestMethod]
        public void LambdaSettableMapViewTest()
        {
            ArrayMap<double> map = new ArrayMap<double>(10, 10);

            ArrayMap<bool> controlMap = new ArrayMap<bool>(10, 10);
            RectangleMapGenerator.Generate(controlMap);

            var settable = new LambdaSettableMapView<bool>(map.Width, map.Height, c => map[c] > 0.0, (c, b) => map[c] = b ? 1.0 : 0.0);
            RectangleMapGenerator.Generate(settable);

            checkMaps(controlMap, map);
        }

        [TestMethod]
        public void LambdaSettableTranslationMapTest()
        {
            ArrayMap<double> map = new ArrayMap<double>(10, 10);

            ArrayMap<bool> controlMap = new ArrayMap<bool>(10, 10);
            RectangleMapGenerator.Generate(controlMap);

            var settable = new LambdaSettableTranslationMap<double, bool>(map, d => d > 0.0, b => b ? 1.0 : 0.0);
            RectangleMapGenerator.Generate(settable);

            checkMaps(controlMap, map);
        }

        [TestMethod]
        public void LambdaTranslationMapTest()
        {
            ArrayMap<bool> map = new ArrayMap<bool>(10, 10);
            RectangleMapGenerator.Generate(map);

            var lambdaMap = new LambdaTranslationMap<bool, double>(map, b => b ? 1.0 : 0.0);

            checkMaps(map, lambdaMap);
        }

        [TestMethod]
        public void ViewportBoundingRectangleTest()
        {
            var arrayMap = new ArrayMap<bool>(100, 100);
            RectangleMapGenerator.Generate(arrayMap);

            var viewport = new Viewport<bool>(arrayMap, new Rectangle(0, 0, 10, 10));
            checkViewportBounds(viewport, Coord.Get(0, 0), Coord.Get(9, 9));

            viewport.ViewArea = viewport.ViewArea.NewWithMinCorner(Coord.Get(-1, 0)); // Should end up being 0, 0 thanks to bounding
            checkViewportBounds(viewport, Coord.Get(0, 0), Coord.Get(9, 9));

            viewport.ViewArea = viewport.ViewArea.NewWithMinCorner(Coord.Get(5, 5));
            checkViewportBounds(viewport, Coord.Get(5, 5), Coord.Get(14, 14)); 

            viewport.ViewArea = viewport.ViewArea.NewWithMinCorner(Coord.Get(98, 98));
            checkViewportBounds(viewport, Coord.Get(90, 90), Coord.Get(99, 99)); 
        }


        private static void checkViewportBounds(Viewport<bool> viewport, Coord expectedMinCorner, Coord expectedMaxCorner)
        {
            Assert.AreEqual(viewport.ViewArea.MinCorner, expectedMinCorner);
            Assert.AreEqual(viewport.ViewArea.MaxCorner, expectedMaxCorner);

            Assert.AreEqual(true, viewport.ViewArea.X >= 0);
            Assert.AreEqual(true, viewport.ViewArea.Y >= 0);
            Assert.AreEqual(true, viewport.ViewArea.X < viewport.MapView.Width);
            Assert.AreEqual(true, viewport.ViewArea.Y < viewport.MapView.Height);


            foreach (var pos in viewport.ViewArea.Positions())
            {
                Assert.AreEqual(true, pos.X >= viewport.ViewArea.X);
                Assert.AreEqual(true, pos.Y >= viewport.ViewArea.Y);

                Assert.AreEqual(true, pos.X <= viewport.ViewArea.MaxX);
                Assert.AreEqual(true, pos.Y <= viewport.ViewArea.MaxY);

                Assert.AreEqual(true, pos.X >= 0);
                Assert.AreEqual(true, pos.Y >= 0);
                Assert.AreEqual(true, pos.X < viewport.MapView.Width);
                Assert.AreEqual(true, pos.Y < viewport.MapView.Height);

                // Utterly stupid way to access things via viewport, but verifies that the coordinate translation is working properly.
                if (pos.X == 0 || pos.Y == 0 || pos.X == viewport.MapView.Width - 1 || pos.Y == viewport.MapView.Height - 1)
                    Assert.AreEqual(false, viewport[pos - viewport.ViewArea.MinCorner]);
                else
                    Assert.AreEqual(true, viewport[pos - viewport.ViewArea.MinCorner]);
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
    }
}