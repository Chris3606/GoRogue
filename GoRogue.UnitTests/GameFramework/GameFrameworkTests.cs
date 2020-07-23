using System;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests.GameFramework
{
    public class GameFrameworkTests
    {
        [Fact]
        public void ApplyTerrainOverlay()
        {
            var grMap = new Generator(10, 10)
                .AddSteps(DefaultAlgorithms.RectangleMapSteps())
                .Generate()
                .Context.GetFirstOrDefault<ISettableMapView<bool>>();

            TestUtils.NotNull(grMap);

            // Normally, in this situation, you would just use the ApplyTerrainOverlay function overload that takes
            // a translation function, instead of creating tempMap and translationMap.  But we want to test the other
            // overload so we do it this way only for testing
            var translationMap = new LambdaTranslationMap<bool, IGameObject>(grMap,
                (pos, val) =>
                    val ?
                        new GameObject(pos, 0, null)
                        : new GameObject(pos, 0, null, true, false));

            // Create map
            var map = new Map(grMap.Width, grMap.Height, 1, Distance.Chebyshev);

            // Create temporary map to record what values are supposed to be
            var tempMap = new ArrayMap<IGameObject>(grMap.Width, grMap.Height);
            tempMap.ApplyOverlay(translationMap);

            // Apply overlay
            map.ApplyTerrainOverlay(tempMap);

            // Verify tiles match
            Assert.Equal(grMap.Width, map.Width);
            Assert.Equal(grMap.Height, map.Height);
            foreach (var pos in map.Positions())
                Assert.Equal(tempMap[pos], map.GetTerrainAt(pos));
        }

        [Fact]
        public void ApplyTerrainOverlayTranslation()
        {
            var grMap = new Generator(10, 10)
                .AddSteps(DefaultAlgorithms.RectangleMapSteps())
                .Generate()
                .Context.GetFirstOrDefault<ISettableMapView<bool>>();

            TestUtils.NotNull(grMap);

            // Create map and apply overlay with a translation function
            var map = new Map(grMap.Width, grMap.Height, 1, Distance.Chebyshev);
            map.ApplyTerrainOverlay(grMap,
                (pos, b) =>
                    b ?
                        new GameObject(pos, 0, null)
                        : new GameObject(pos, 0, null, false, false));

            foreach (var pos in grMap.Positions())
            {
                var terrain = map.GetTerrainAt(pos);
                TestUtils.NotNull(terrain);
                Assert.Equal(grMap[pos], terrain.IsWalkable);
            }
        }

        [Fact]
        public void ApplyTerrainOverlayPositions()
        {
            var grMap = new Generator(10, 10)
                .AddSteps(DefaultAlgorithms.RectangleMapSteps())
                .Generate()
                .Context.GetFirstOrDefault<ISettableMapView<bool>>();

            TestUtils.NotNull(grMap);

            // Create map and apply overlay with a translation function
            var map = new Map(grMap.Width, grMap.Height, 1, Distance.Chebyshev);
            map.ApplyTerrainOverlay(grMap,
                (pos, b) =>
                    b ?
                        new GameObject(pos, 0, null)
                        : new GameObject(pos, 0, null, true, false));

            // Assert all objects are at right position to start with
            foreach (var pos in grMap.Positions())
            {
                var terrain = map.GetTerrainAt(pos);
                TestUtils.NotNull(terrain);

                Assert.Equal(pos, terrain.Position);
            }

            // Rearrange positions by mirror-imaging in X/Y
            var terrainMap2 = new ArrayMap<IGameObject>(map.Width, map.Height);
            foreach (var (x, y) in map.Positions())
            {
                var terrain = map.GetTerrainAt(x, y);
                TestUtils.NotNull(terrain);
                terrainMap2[map.Width - 1 - x, map.Height - 1 - y] = terrain;
            }

            // Apply overlay, hopefully adapting positions properly
            map.ApplyTerrainOverlay(terrainMap2);

            foreach (var pos in grMap.Positions())
            {
                var terrain = map.GetTerrainAt(pos);
                TestUtils.NotNull(terrain);

                Assert.Equal(pos, terrain.Position);
            }
        }

        [Fact]
        public void ApplyTerrainOverlayRemoveFromOldMap()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var map2 = new Map(map.Width, map.Height, map.Entities.NumberOfLayers, map.DistanceMeasurement);
            var gameObject = new GameObject((1, 2), 0, null);

            map.SetTerrain(gameObject);
            Assert.Equal(map, gameObject.CurrentMap);

            // Create an overlay with our object
            var terrainOverlay = new ArrayMap<IGameObject>(map.Width, map.Height) { [5, 6] = gameObject };

            // Apply overlay
            map2.ApplyTerrainOverlay(terrainOverlay);

            // Verify there is nothing in original map
            foreach (var pos in map.Positions())
                Assert.Null(map.Terrain[pos]);

            // Verify the object is in the new map precisely once at the position it was in in the overlay
            foreach (var pos in map2.Positions())
                Assert.NotEqual(pos == (5, 6), map2.Terrain[pos] == null);
        }

        [Fact]
        public void OutOfBoundsEntityAdd()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((-1, -1), 1, null);

            Assert.Throws<InvalidOperationException>(() => map.AddEntity(obj));
            Assert.Empty(map.Entities);
        }

        [Fact]
        public void OutOfBoundsMove()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1, null);

            map.AddEntity(obj);

            var oldPos = obj.Position;
            Assert.Throws<InvalidOperationException>(() => obj.Position = (-1, -1));
            Assert.Equal(oldPos, obj.Position);
        }

        [Fact]
        public void OutOfBoundsTerrainAdd()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((-1, -1), 0, null);

            Assert.Throws<ArgumentException>(() => map.SetTerrain(obj));
        }

        [Fact]
        public void ValidEntityAdd()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1, null);

            map.AddEntity(obj);
            Assert.Single(map.Entities);
        }

        [Fact]
        public void ValidEntityMove()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1, null);
            var five = new Point(5, 5);
            map.AddEntity(obj);

            obj.Position = five;
            Assert.Equal(five, obj.Position);
        }
    }
}
