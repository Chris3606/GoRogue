using GoRogue.MapViews;
using GoRogue.UnitTests.Mocks;
using JetBrains.Annotations;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.MapViews
{
    public class MapViewTests
    {
        private const int _width = 50;
        private const int _height = 50;

        [AssertionMethod]
        private static void CheckMaps(IMapView<bool> genMap, IMapView<double> resMap)
        {
            for (var x = 0; x < genMap.Width; x++)
                for (var y = 0; y < genMap.Height; y++)
                {
                    if (genMap[x, y])
                        Assert.True(resMap[x, y] > 0.0);
                    else
                        Assert.False(resMap[x, y] > 0.0);
                }
        }

        [AssertionMethod]
        private static void CheckViewportBounds(Viewport<bool> viewport, Point expectedMinCorner,
                                                Point expectedMaxCorner)
        {
            Assert.Equal(expectedMaxCorner, viewport.ViewArea.MaxExtent);
            Assert.Equal(expectedMinCorner, viewport.ViewArea.MinExtent);
            Assert.True(viewport.ViewArea.X >= 0);
            Assert.True(viewport.ViewArea.Y >= 0);
            Assert.True(viewport.ViewArea.X < viewport.MapView.Width);
            Assert.True(viewport.ViewArea.Y < viewport.MapView.Height);

            foreach (var pos in viewport.ViewArea.Positions())
            {
                Assert.True(pos.X >= viewport.ViewArea.X);
                Assert.True(pos.Y >= viewport.ViewArea.Y);
                Assert.True(pos.X <= viewport.ViewArea.MaxExtentX);
                Assert.True(pos.Y <= viewport.ViewArea.MaxExtentY);
                Assert.True(pos.X >= 0);
                Assert.True(pos.Y >= 0);
                Assert.True(pos.X < viewport.MapView.Width);
                Assert.True(pos.Y < viewport.MapView.Height);

                // Utterly stupid way to access things via viewport, but verifies that the coordinate
                // translation is working properly.
                if (pos.X == 0 || pos.Y == 0 || pos.X == viewport.MapView.Width - 1 ||
                    pos.Y == viewport.MapView.Height - 1)
                    Assert.False(viewport[pos - viewport.ViewArea.MinExtent]);
                else
                    Assert.True(viewport[pos - viewport.ViewArea.MinExtent]);
            }
        }

        [Fact]
        public void ApplyOverlayTest()
        {
            var map = MockMaps.Rectangle(_width, _height);

            var duplicateMap = new ArrayMap<bool>(map.Width, map.Height);

            duplicateMap.ApplyOverlay(map);

            foreach (var pos in map.Positions())
                Assert.Equal(map[pos], duplicateMap[pos]);
        }

        [Fact]
        public void IndexerAccessMapViewTest()
        {
            var map = new ArrayMap<bool>(_width, _height);
            ISettableMapView<bool> setMapView = map;
            IMapView<bool> mapView = map;
            bool[] array = map;

            // Set last entry via indexer syntax (via the ArrayMap, to prove implicit implementations
            // work at all levels)
            map[^1] = true;

            // Set second to last entry via settable map view
            setMapView[^2] = true;

            // Both of set should be true
            Assert.True(mapView[^2]);
            Assert.True(mapView[^1]);

            // All items should be false except for the last two
            for (int i = 0; i < array.Length - 2; i++)
                Assert.False(array[i]);

            Assert.True(array[^2]);
            Assert.True(array[^1]);
        }

        [Fact]
        public void LambdaMapViewTest()
        {
            var map = MockMaps.Rectangle(_width, _height);
            IMapView<double> lambdaMapView = new LambdaMapView<double>(map.Width, map.Height, c => map[c] ? 1.0 : 0.0);

            CheckMaps(map, lambdaMapView);
        }

        [Fact]
        public void LambdaSettableMapViewTest()
        {
            var map = MockMaps.TestResMap(10, 10);
            var lambdaSettable = new LambdaSettableMapView<bool>(map.Width, map.Height, c => map[c] > 0.0,
                (c, b) => map[c] = b ? 1.0 : 0.0);
            CheckMaps(lambdaSettable, map);

            // Set via lambda map, ensuring we actually change the value
            Assert.True(lambdaSettable[1, 2]);
            lambdaSettable[1, 2] = false;

            // Make sure maps still match
            CheckMaps(lambdaSettable, map);
        }

        [Fact]
        public void LambdaSettableTranslationMapTest()
        {
            var map = MockMaps.Rectangle(_width, _height);

            var settable = new LambdaSettableTranslationMap<bool, double>(map, b => b ? 1.0 : 0.0, d => d > 0.0);
            CheckMaps(map, settable);

            // Change the map via the settable, and re-check
            settable[0, 0] = 1.0;

            Assert.True(map[0, 0]);
            CheckMaps(map, settable);

            // Check other constructor.  Intentionally "misusing" the position parameter, to make sure we ensure the position
            // parameter is correct without complicating our test case
            settable = new LambdaSettableTranslationMap<bool, double>(map, (pos, b) => map[pos] ? 1.0 : 0.0,
                (pos, d) => d > 0.0);
            CheckMaps(map, settable);
        }

        [Fact]
        public void LambdaTranslationMapTest()
        {
            var map = MockMaps.Rectangle(_width, _height);
            var lambdaMap = new LambdaTranslationMap<bool, double>(map, b => b ? 1.0 : 0.0);

            CheckMaps(map, lambdaMap);

            // Check other constructor.  Intentionally "misusing" the position parameter, to make sure we ensure the position
            // parameter is correct without complicating our test case
            lambdaMap = new LambdaTranslationMap<bool, double>(map, (pos, b) => map[pos] ? 1.0 : 0.0);
            CheckMaps(map, lambdaMap);
        }

        [Fact]
        public void ViewportBoundingRectangleTest()
        {
            const int viewportWidth = 1280 / 12;
            const int viewportHeight = 768 / 12;
            const int mapWidth = 250;
            const int mapHeight = 250;

            var map = MockMaps.Rectangle(mapWidth, mapHeight);

            var viewport = new Viewport<bool>(map, new Rectangle(0, 0, viewportWidth, viewportHeight));
            CheckViewportBounds(viewport, (0, 0), (viewportWidth - 1, viewportHeight - 1));

            // Should end up being 0, 0 thanks to bounding
            viewport.SetViewArea(viewport.ViewArea.WithPosition((-1, 0)));
            CheckViewportBounds(viewport, (0, 0), (viewportWidth - 1, viewportHeight - 1));

            viewport.SetViewArea(viewport.ViewArea.WithPosition((5, 5)));
            CheckViewportBounds(viewport, (5, 5), (viewportWidth - 1 + 5, viewportHeight - 1 + 5));

            // Move outside x-bounds by 1
            Point newCenter = (mapWidth - viewportWidth / 2 + 1, mapHeight - viewportHeight / 2 + 1);
            viewport.SetViewArea(viewport.ViewArea.WithCenter(newCenter));

            Point minVal = (mapWidth - viewportWidth, mapHeight - viewportHeight);
            Point maxVal = (mapWidth - 1, mapHeight - 1);
            CheckViewportBounds(viewport, minVal, maxVal);
        }

        [Fact]
        public void UnboundedViewportTest()
        {
            const int mapWidth = 100;
            const int mapHeight = 100;
            var map = new ArrayMap<int>(mapWidth, mapHeight);
            var unboundedViewport = new UnboundedViewport<int>(map, 1);

            foreach (var pos in map.Positions())
                Assert.Equal(0, unboundedViewport[pos]);

            unboundedViewport.ViewArea = unboundedViewport.ViewArea.Translate((5, 5));

            foreach (var pos in unboundedViewport.Positions())
                if (pos.X < mapWidth - 5 && pos.Y < mapHeight - 5)
                    Assert.Equal(0, unboundedViewport[pos]);
                else
                    Assert.Equal(1, unboundedViewport[pos]);

            unboundedViewport.ViewArea = unboundedViewport.ViewArea.WithSize(5, 5);

            foreach (var pos in unboundedViewport.Positions())
                Assert.Equal(0, unboundedViewport[pos]);

            unboundedViewport.ViewArea = unboundedViewport.ViewArea.WithPosition((mapWidth - 1, mapHeight - 1));

            foreach (var pos in unboundedViewport.Positions())
                if (pos.X == 0 && pos.Y == 0)
                    Assert.Equal(0, unboundedViewport[pos]);
                else
                    Assert.Equal(1, unboundedViewport[pos]);
        }
    }
}
