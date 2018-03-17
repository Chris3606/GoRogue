using GoRogue.MapViews;
using GoRogue.MapGeneration.Generators;
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
        public void LambdaTranslationMapTest()
        {
            ArrayMap<bool> map = new ArrayMap<bool>(10, 10);
            RectangleMapGenerator.Generate(map);

            var lambdaMap = new LambdaTranslationMap<bool, double>(map, b => b ? 1.0 : 0.0);

            checkMaps(map, lambdaMap);

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
