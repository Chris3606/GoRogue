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

            var translationMap = new LambdaTranslationMap<bool, IGameObject>(grMap,
                (pos, val) =>
                    val ?
                        new GameObject(pos, 0, null, true)
                        : new GameObject(pos, 0, null, true, false, false));

            var map = new Map(grMap.Width, grMap.Height, 1, Distance.Chebyshev);

            // Normally you shouldn't need tempMap, as you would do what is done in ApplyTerrainOverlayTransition,
            // which is define the translation function the the ApplyTerrainOverlay function call.  But here, we want
            // to test the overload without the translation function, so we rely on a translation map instead.
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

            var map = new Map(grMap.Width, grMap.Height, 1, Distance.Chebyshev);
            map.ApplyTerrainOverlay(grMap,
                (pos, b) =>
                    b ?
                        new GameObject(pos, 0, null, true)
                        : new GameObject(pos, 0, null, true, false, false));

            // If any value is null this fails due to NullReferenceException: otherwise, we assert the right value got set
            foreach (var pos in grMap.Positions())
            {
                var terrain = map.GetTerrainAt(pos);
                TestUtils.NotNull(terrain);
                if (grMap[pos])
                    Assert.True(terrain.IsWalkable);
                else
                    Assert.False(terrain.IsWalkable);
            }
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
            var obj = new GameObject((-1, -1), 0, null, true);

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
