using System;
using System.Linq;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Xunit;

namespace GoRogue.UnitTests.GameFramework
{
    public class GameFrameworkTests
    {
        public static int MovedTimesCalled;

        [Fact]
        public void ApplyTerrainOverlay()
        {
            var grMap = new Generator(10, 10)
                .AddSteps(DefaultAlgorithms.RectangleMapSteps())
                .Generate()
                .Context.GetFirstOrDefault<ISettableGridView<bool>>();

            TestUtils.NotNull(grMap);

            // Normally, in this situation, you would just use the ApplyTerrainOverlay function overload that takes
            // a translation function, instead of creating tempMap and translationMap.  But we want to test the other
            // overload so we do it this way only for testing
            var translationMap = new LambdaTranslationGridView<bool, IGameObject>(grMap,
                (pos, val) =>
                    val ?
                        new GameObject(pos, 0)
                        : new GameObject(pos, 0, true, false));

            // Create map
            var map = new Map(grMap.Width, grMap.Height, 1, Distance.Chebyshev);

            // Create temporary map to record what values are supposed to be
            var tempMap = new ArrayView<IGameObject>(grMap.Width, grMap.Height);
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
                .Context.GetFirstOrDefault<ISettableGridView<bool>>();

            TestUtils.NotNull(grMap);

            // Create map and apply overlay with a translation function
            var map = new Map(grMap.Width, grMap.Height, 1, Distance.Chebyshev);
            map.ApplyTerrainOverlay(grMap,
                (pos, b) =>
                    b ?
                        new GameObject(pos, 0)
                        : new GameObject(pos, 0, false, false));

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
                .Context.GetFirstOrDefault<ISettableGridView<bool>>();

            TestUtils.NotNull(grMap);

            // Create map and apply overlay with a translation function
            var map = new Map(grMap.Width, grMap.Height, 1, Distance.Chebyshev);
            map.ApplyTerrainOverlay(grMap,
                (pos, b) =>
                    b ?
                        new GameObject(pos, 0)
                        : new GameObject(pos, 0, true, false));

            // Assert all objects are at right position to start with
            foreach (var pos in grMap.Positions())
            {
                var terrain = map.GetTerrainAt(pos);
                TestUtils.NotNull(terrain);

                Assert.Equal(pos, terrain.Position);
            }

            // Rearrange positions by mirror-imaging in X/Y
            var terrainMap2 = new ArrayView<IGameObject>(map.Width, map.Height);
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
            var gameObject = new GameObject((1, 2), 0);

            map.SetTerrain(gameObject);
            Assert.Equal(map, gameObject.CurrentMap);

            // Create an overlay with our object
            var terrainOverlay = new ArrayView<IGameObject>(map.Width, map.Height) { [5, 6] = gameObject };

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
        public void CreateMapHelperConstructorInitializesCurrentMap()
        {
            var grMap = new Generator(10, 10)
                .AddSteps(DefaultAlgorithms.RectangleMapSteps())
                .Generate()
                .Context.GetFirstOrDefault<ISettableGridView<bool>>();

            TestUtils.NotNull(grMap);

            // Create our own terrain layer and pass it to the map.
            //
            // Note: DO NOT actually write this functionality like this in production; use ApplyTerrainOverlay instead.
            // This is done via the CreateMap function/Map constructor taking a custom terrain view ONLY as a test case
            // for that map constructor
            var translator = new LambdaTranslationGridView<bool, GameObject>(grMap, (pos, b) => b
                    ? new GameObject(pos, 0)
                    : new GameObject(pos, 0, true, false));
            var terrain = new ArrayView<GameObject?>(grMap.Width, grMap.Height);
            terrain.ApplyOverlay(translator);
            var map = Map.CreateMap(terrain, 1, Distance.Chebyshev);
            TestUtils.NotNull(map);

            // Check each terrain's CurrentMap to ensure it was set properly and that the object wasn't changed out
            foreach (var pos in map.Positions())
            {
                Assert.Equal(terrain[pos], map.GetTerrainAt(pos));
                Assert.Equal(map, map.GetTerrainAt(pos)?.CurrentMap);
            }
        }

        [Fact]
        public void OutOfBoundsEntityAdd()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((-1, -1), 1);

            Assert.Throws<ArgumentException>(() => map.AddEntity(obj));
            Assert.Empty(map.Entities);
        }

        [Fact]
        public void OutOfBoundsMove()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1);

            map.AddEntity(obj);

            var oldPos = obj.Position;
            Assert.Throws<InvalidOperationException>(() => obj.Position = (-1, -1));
            Assert.Equal(oldPos, obj.Position);
        }

        [Fact]
        public void OutOfBoundsTerrainAdd()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((-1, -1), 0);

            Assert.Throws<ArgumentException>(() => map.SetTerrain(obj));
        }

        [Fact]
        public void EntityAddedAsTerrainError()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1);

            Assert.Throws<ArgumentException>(() => map.SetTerrain(obj));
        }

        [Fact]
        public void ValidEntityAdd()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1);

            map.AddEntity(obj);
            Assert.Single(map.Entities);
        }

        [Fact]
        public void CanAddEntityAt()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1, false, false);
            var obj2 = new GameObject((2, 2), 1, false, false);
            map.AddEntity(obj);
            Assert.Single(map.Entities);

            Assert.True(map.CanAddEntity(obj2));
            Assert.True(map.CanAddEntityAt(obj2, obj2.Position));

            Assert.False(map.CanAddEntityAt(obj2, obj.Position));
        }

        [Fact]
        public void TryAddEntityAt()
        {
            MovedTimesCalled = 0;

            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1, false, false);
            var obj2 = new GameObject((2, 2), 1, false, false);
            obj2.PositionChanged += (s, e) => ++MovedTimesCalled;

            map.AddEntity(obj);
            Assert.Single(map.Entities);

            Assert.False(map.TryAddEntityAt(obj2, obj.Position));
            Assert.Equal(new Point(2, 2), obj2.Position);
            Assert.Equal(0, MovedTimesCalled);

            Assert.True(map.TryAddEntityAt(obj2, (3, 3)));
            Assert.Equal(new Point(3, 3), obj2.Position);
            Assert.Equal(1, MovedTimesCalled);
        }

        [Fact]
        public void TerrainAddedAsEntityError()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 0);

            Assert.Throws<ArgumentException>(() => map.AddEntity(obj));
        }

        [Fact]
        public void EntityAddedToSingleItemLayerCollisionError()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev, entityLayersSupportingMultipleItems: 0);
            var obj = new GameObject((1, 1), 1);
            var obj2 = new GameObject((1, 1), 1);

            map.AddEntity(obj);
            Assert.Throws<ArgumentException>(() => map.AddEntity(obj2));
        }

        [Fact]
        public void CanAddEntityToSingleLayerExistingItem()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev, entityLayersSupportingMultipleItems: 0);
            var obj = new GameObject((1, 1), 1);
            var obj2 = new GameObject((1, 1), 1);

            map.AddEntity(obj);
            Assert.False(map.CanAddEntity(obj2));
        }

        [Fact]
        public void ValidEntityMove()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1);
            var five = new Point(5, 5);
            map.AddEntity(obj);

            obj.Position = five;
            Assert.Equal(five, obj.Position);
        }

        [Fact]
        public void ValidTerrainWalkabilityChange()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 0);
            map.SetTerrain(obj);

            Assert.True(map.WalkabilityView[obj.Position]);


            obj.IsWalkable = false;
            Assert.False(map.WalkabilityView[obj.Position]);

            obj.IsWalkable = true;
            Assert.True(map.WalkabilityView[obj.Position]);
        }

        [Fact]
        public void ValidEntityWalkabilityChange()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1);
            map.AddEntity(obj);

            Assert.True(map.WalkabilityView[obj.Position]);


            obj.IsWalkable = false;
            Assert.False(map.WalkabilityView[obj.Position]);

            obj.IsWalkable = true;
            Assert.True(map.WalkabilityView[obj.Position]);
        }
        [Fact]
        public void InvalidEntityWalkabilityChange()
        {
            var map = new Map(10, 10, 2, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1);
            map.AddEntity(obj);

            var obj2 = new GameObject((2, 2), 1, false);
            map.AddEntity(obj2);
            var obj3 = new GameObject((2, 1), 2, false);
            map.AddEntity(obj3);

            Assert.True(map.WalkabilityView[obj.Position]);
            Assert.False(map.WalkabilityView[obj2.Position]);
            Assert.False(map.WalkabilityView[obj3.Position]);

            obj.Position = obj2.Position;
            // obj2 is non-walkable and also at this location
            Assert.Throws<InvalidOperationException>(() => obj.IsWalkable = false);
            // Should have reset the IsWalkable to true so the error is recoverable
            Assert.True(obj.IsWalkable);

            obj.Position = obj3.Position;
            // obj3 is non-walkable and also at this location
            Assert.Throws<InvalidOperationException>(() => obj.IsWalkable = false);
            // Should have reset the IsWalkable to true so the error is recoverable
            Assert.True(obj.IsWalkable);
        }

        [Fact]
        public void WalkabilityViewUpdatesOnAdd()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            Assert.True(map.WalkabilityView[1, 1]);

            map.SetTerrain(new GameObject((1, 1), 0, false, isTransparent: true));
            Assert.False(map.WalkabilityView[1, 1]);

            map.AddEntity(new GameObject((1, 2), 1, true, false));
            Assert.True(map.WalkabilityView[1, 2]);
        }

        [Fact]
        public void WalkabilityViewUpdatesOnMove()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1, false, isTransparent: true);
            map.AddEntity(obj);

            var obj2 = new GameObject((1, 2), 1, false, isTransparent: true);
            map.AddEntity(obj2);

            Assert.False(map.WalkabilityView[obj.Position]);
            Assert.False(map.WalkabilityView[obj2.Position]);

            obj.Position = (2, 2);
            Assert.Equal(new Point(2, 2), obj.Position);
            Assert.True(map.WalkabilityView[1, 1]);
            Assert.False(map.WalkabilityView[obj.Position]);
            Assert.False(map.WalkabilityView[obj2.Position]);

            obj2.Position = (5, 6);
            Assert.Equal(new Point(5, 6), obj2.Position);
            Assert.True(map.WalkabilityView[1, 2]);
            Assert.False(map.WalkabilityView[obj2.Position]);
            Assert.False(map.WalkabilityView[obj.Position]);
        }

        [Fact]
        public void WalkabilityViewUpdatesOnRemove()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);

            var obj = new GameObject((1, 1), 0, false, isTransparent: true);
            map.SetTerrain(obj);

            var obj2 = new GameObject((1, 2), 1, true, false);
            map.AddEntity(obj2);

            Assert.False(map.WalkabilityView[1, 1]);
            Assert.True(map.WalkabilityView[1, 2]);

            map.RemoveEntity(obj2);
            Assert.False(map.WalkabilityView[1, 1]);
            Assert.True(map.WalkabilityView[1, 2]);

            map.RemoveTerrain(obj);
            Assert.True(map.WalkabilityView[1, 1]);
            Assert.True(map.WalkabilityView[1, 2]);
        }

        [Fact]
        public void TransparencyViewUpdatesOnAdd()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            Assert.True(map.TransparencyView[1, 1]);

            map.SetTerrain(new GameObject((1, 1), 0, isWalkable: false, isTransparent: true));
            Assert.True(map.TransparencyView[1, 1]);

            map.SetTerrain(new GameObject((1, 1), 0, isWalkable: true, isTransparent: false));
            Assert.False(map.TransparencyView[1, 1]);

            map.AddEntity(new GameObject((1, 2), 1, isWalkable: false, isTransparent: true));
            Assert.True(map.TransparencyView[1, 2]);

            map.AddEntity(new GameObject((1, 2), 1, isWalkable: true, isTransparent: false));
            Assert.False(map.TransparencyView[1, 2]);
        }

        [Fact]
        public void TransparencyViewUpdatesOnMove()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);
            var obj = new GameObject((1, 1), 1, true, false);
            map.AddEntity(obj);

            var obj2 = new GameObject((1, 2), 1, true, false);
            map.AddEntity(obj2);

            var obj3 = new GameObject((1, 3), 1, isWalkable: true, isTransparent: true);
            map.AddEntity(obj3);

            Assert.False(map.TransparencyView[obj.Position]);
            Assert.False(map.TransparencyView[obj2.Position]);
            Assert.True(map.TransparencyView[obj3.Position]);

            obj.Position = obj3.Position;
            Assert.Equal(obj3.Position, obj.Position);
            Assert.True(map.TransparencyView[1, 1]);
            Assert.False(map.TransparencyView[obj2.Position]);
            Assert.False(map.TransparencyView[obj3.Position]); // Changed to false

            obj.Position = obj2.Position;
            Assert.Equal(obj2.Position, obj.Position);
            Assert.True(map.TransparencyView[1, 1]);
            Assert.False(map.TransparencyView[obj2.Position]);
            Assert.True(map.TransparencyView[obj3.Position]); // Back to true

            obj.Position = (1, 1);
            Assert.Equal(new Point(1, 1), obj.Position);
            Assert.False(map.TransparencyView[obj.Position]);
            Assert.False(map.TransparencyView[obj2.Position]);
            Assert.True(map.TransparencyView[obj3.Position]);
        }

        [Fact]
        public void TransparencyViewUpdatesOnRemove()
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev);

            var obj = new GameObject((1, 1), 0, false, isTransparent: true);
            map.SetTerrain(obj);

            var obj2 = new GameObject((1, 1), 1, true, false);
            map.AddEntity(obj2);

            Assert.False(map.TransparencyView[1, 1]);

            map.RemoveEntity(obj2);
            Assert.True(map.TransparencyView[1, 1]);

            map.AddEntity(obj2);
            Assert.False(map.TransparencyView[1, 1]);

            map.RemoveTerrain(obj);
            Assert.False(map.TransparencyView[1, 1]);

            map.RemoveEntity(obj2);
            Assert.True(map.TransparencyView[1, 1]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TransparencyViewUpdatesOnChange(bool useCachedGridViews)
        {
            var map = new Map(10, 10, 1, Distance.Chebyshev, useCachedGridViews: useCachedGridViews);

            var obj = new GameObject((1, 1), 0, false, isTransparent: true);
            map.SetTerrain(obj);

            var obj2 = new GameObject((1, 1), 1, true, false);
            map.AddEntity(obj2);

            Assert.False(map.TransparencyView[obj2.Position]);

            obj2.IsTransparent = true;
            Assert.True(map.TransparencyView[obj2.Position]);

            obj2.IsTransparent = false;
            Assert.False(map.TransparencyView[obj2.Position]);
        }

        [Fact]
        public void GetObjectsAt()
        {
            var map = new Map(10, 10, 4, Distance.Chebyshev);
            var objectsAtValid = new IGameObject[]
            {
                new GameObject((1, 1), 4),
                new GameObject((1, 1), 2),
                new GameObject((1, 1), 1),
                new GameObject((1, 1), 0)
            };
            var objectsAtInvalid = new IGameObject[] { new GameObject((1, 2), 1), new GameObject((1, 2), 0) };

            foreach (var obj in objectsAtValid[..^1])
                map.AddEntity(obj);

            map.SetTerrain(objectsAtValid[^1]);

            foreach (var obj in objectsAtInvalid[..^1])
                map.AddEntity(obj);

            map.SetTerrain(objectsAtInvalid[^1]);

            // Test GetObjectsAt
            var objects = map.GetObjectsAt((1, 1)).ToArray();
            Assert.Equal(objectsAtValid, objects);

            // Mask with layer which has no items, and missing one which does have items
            objects = map.GetObjectsAt((1, 1), map.LayerMasker.Mask(3, 2, 1, 0)).ToArray();
            Assert.Equal(objectsAtValid[1..], objects);

            // No terrain
            objects = map.GetObjectsAt((1, 1), map.LayerMasker.MaskAllAbove(1)).ToArray();
            Assert.Equal(objectsAtValid[..^1], objects);

            // Only terrain
            objects = map.GetObjectsAt((1, 1), map.LayerMasker.Mask(0)).ToArray();
            Assert.Equal(objectsAtValid[^1..], objects);

            // No items
            objects = map.GetObjectsAt((1, 1), 0u).ToArray();
            Assert.Equal(Array.Empty<IGameObject>(), objects);
        }

        [Fact]
        public void GetObjectsAtCast()
        {
            var map = new Map(10, 10, 4, Distance.Chebyshev);
            var objectsAtValidSubclass = new[]
            {
                new MockGameObject((1, 1), 3),
                new MockGameObject((1, 1), 1),
                new MockGameObject((1, 1), 0)
            };
            var objectsAtValidRegular = new IGameObject[]
            {
                new GameObject((1, 1), 4),
                new GameObject((1, 1), 2),
            };
            var objectsAtInvalid = new IGameObject[] { new GameObject((1, 2), 1), new GameObject((1, 2), 0) };

            foreach (var obj in objectsAtValidSubclass[..^1])
                map.AddEntity(obj);

            map.SetTerrain(objectsAtValidSubclass[^1]);

            foreach (var obj in objectsAtValidRegular)
                map.AddEntity(obj);

            foreach (var obj in objectsAtInvalid[..^1])
                map.AddEntity(obj);

            map.SetTerrain(objectsAtInvalid[^1]);

            // Test GetObjectsAt
            var objects = map.GetObjectsAt<MockGameObject>((1, 1)).ToArray();
            Assert.Equal(objectsAtValidSubclass, objects);

            // Mask with layer which has no items, and missing one which does have items
            objects = map.GetObjectsAt<MockGameObject>((1, 1), map.LayerMasker.Mask(4, 2, 1, 0)).ToArray();
            Assert.Equal(objectsAtValidSubclass[1..], objects);

            // No terrain
            objects = map.GetObjectsAt<MockGameObject>((1, 1), map.LayerMasker.MaskAllAbove(1)).ToArray();
            Assert.Equal(objectsAtValidSubclass[..^1], objects);

            // Only terrain
            objects = map.GetObjectsAt<MockGameObject>((1, 1), map.LayerMasker.Mask(0)).ToArray();
            Assert.Equal(objectsAtValidSubclass[^1..], objects);

            // No items
            objects = map.GetObjectsAt<MockGameObject>((1, 1), map.LayerMasker.Mask(2)).ToArray();
            Assert.Equal(Array.Empty<MockGameObject>(), objects);
        }

        [Fact]
        public void GetEntitiesAtCast()
        {
            var map = new Map(10, 10, 4, Distance.Chebyshev);
            var objectsAtValidSubclass = new[]
            {
                new MockGameObject((1, 1), 3),
                new MockGameObject((1, 1), 1),
            };
            var objectsAtValidRegular = new IGameObject[]
            {
                new GameObject((1, 1), 4),
                new GameObject((1, 1), 2),
                new GameObject((1, 1), 0)
            };
            var objectsAtInvalid = new IGameObject[] { new GameObject((1, 2), 1), new GameObject((1, 2), 0) };

            foreach (var obj in objectsAtValidSubclass)
                map.AddEntity(obj);

            foreach (var obj in objectsAtValidRegular[..^1])
                map.AddEntity(obj);

            map.SetTerrain(objectsAtValidRegular[^1]);

            foreach (var obj in objectsAtInvalid[..^1])
                map.AddEntity(obj);

            map.SetTerrain(objectsAtInvalid[^1]);

            // Test GetEntitiesAt
            var objects = map.GetEntitiesAt<MockGameObject>((1, 1)).ToArray();
            Assert.Equal(objectsAtValidSubclass, objects);

            // Mask with layer which has no items, and missing one which does have items
            objects = map.GetEntitiesAt<MockGameObject>((1, 1), map.LayerMasker.Mask(4, 2, 1, 0)).ToArray();
            Assert.Equal(objectsAtValidSubclass[1..], objects);

            // No items
            objects = map.GetEntitiesAt<MockGameObject>((1, 1), map.LayerMasker.Mask(2)).ToArray();
            Assert.Equal(Array.Empty<MockGameObject>(), objects);
        }
    }
}
