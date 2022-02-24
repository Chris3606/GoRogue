using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.SpatialMaps;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.SpatialMaps
{
    public class LayeredSpatialMapTests
    {
        //private readonly ITestOutputHelper _output;

        private readonly LayeredSpatialMap<MockSpatialMapItem> _spatialMap;
        private static readonly Point _initialItemsPos = (1, 2);
        private static readonly Point _newItemsPos = (2, 3);
        private const int StartingLayer = 1;
        private const int NumLayers = 6;
        private static readonly IEnumerable<int> _validLayers = Enumerable.Range(StartingLayer, NumLayers);
        private static List<MockSpatialMapItem> _initialItems = new List<MockSpatialMapItem>();


        public static (MockSpatialMapItem item, Point pos)[] InvalidAddCases =
        {
            // Above the maximum layer
            (new MockSpatialMapItem(StartingLayer + NumLayers), _newItemsPos),
            // Below the minimum layer
            (new MockSpatialMapItem(StartingLayer - 1), _newItemsPos),
            // Layer doesn't support multiple items and an item is already here
            (new MockSpatialMapItem(StartingLayer + 1), _initialItemsPos)
        };

        public LayeredSpatialMapTests()
        {
            //_output = output;

            _initialItems = new List<MockSpatialMapItem>();

            // Set up items on all but one layer of the spatial map.  No layers
            // support multiple items.
            _spatialMap = new LayeredSpatialMap<MockSpatialMapItem>(NumLayers, startingLayer: StartingLayer);

            foreach (int layer in _validLayers)
            {
                var item = new MockSpatialMapItem(layer);
                _spatialMap.Add(item, _initialItemsPos);
                _initialItems.Add(item);
            }
        }

        [Fact]
        public void AddItemValid()
        {
            // Just the starting items to begin with, and none at new location
            Assert.Empty(_spatialMap.GetItemsAt(_newItemsPos));
            Assert.Equal(_initialItems.Count, _spatialMap.Count);

            var itemsAdded = new List<MockSpatialMapItem>();
            foreach (int layer in _validLayers)
            {
                var item = new MockSpatialMapItem(layer);
                _spatialMap.Add(item, _newItemsPos);
                itemsAdded.Add(item);
                Assert.Equal(itemsAdded.Count, _spatialMap.GetItemsAt(_newItemsPos).Count());
                Assert.Equal(itemsAdded.Count + _initialItems.Count, _spatialMap.Count);
            }
        }

        [Theory]
        [MemberDataTuple(nameof(InvalidAddCases))]
        public void AddItemInvalid(MockSpatialMapItem item, Point position)
        {
            int prevAtPos = _spatialMap.GetItemsAt(position).Count();
            int prevCount = _spatialMap.Count;

            // Should throw exception and not add item
            Assert.Throws<ArgumentException>(() => _spatialMap.Add(item, position));
            Assert.Equal(prevAtPos, _spatialMap.GetItemsAt(position).Count());
            Assert.Equal(prevCount, _spatialMap.Count);
        }

        [Fact]
        public void RemoveItemValid()
        {
            int remainingItems = _initialItems.Count;
            foreach (var item in _initialItems)
            {
                _spatialMap.Remove(item);
                remainingItems--;

                Assert.Equal(remainingItems, _spatialMap.GetItemsAt((1, 2)).Count());
                Assert.Equal(remainingItems, _spatialMap.Count);
            }
        }

        [Fact]
        public void RemoveItemInvalid()
        {
            int prevAtPos = _spatialMap.GetItemsAt(_initialItemsPos).Count();
            int prevCount = _spatialMap.Count;

            var nonexistentItem = new MockSpatialMapItem(StartingLayer + 1);

            Assert.Throws<ArgumentException>(() => _spatialMap.Remove(nonexistentItem));
            Assert.Equal(prevAtPos, _spatialMap.GetItemsAt(_initialItemsPos).Count());
            Assert.Equal(prevCount, _spatialMap.Count);
        }

        [Fact]
        public void RemoveByPositionValid()
        {
            var itemsRemoved = _spatialMap.Remove(_initialItemsPos).ToList();
            Assert.Equal(_initialItems.Count, itemsRemoved.Count);
            Assert.Equal(0, _spatialMap.Count);
            Assert.Empty(_spatialMap.GetItemsAt(_initialItemsPos));
        }

        [Fact]
        public void RemoveByPositionInvalid()
        {
            var itemsRemoved = _spatialMap.Remove(_newItemsPos).ToList();
            Assert.Empty(itemsRemoved);
            Assert.Equal(_initialItems.Count, _spatialMap.Count);
            Assert.Equal(_initialItems.Count, _spatialMap.GetItemsAt(_initialItemsPos).Count());
        }

        [Fact]
        public void MoveItemValid()
        {
            Assert.Empty(_spatialMap.GetItemsAt(_newItemsPos));

            foreach (var (item, index) in _initialItems.Enumerate())
            {
                _spatialMap.Move(item, _newItemsPos);
                Assert.Equal(_initialItems.Count, _spatialMap.Count);
                Assert.Equal(_initialItems.Count - index - 1, _spatialMap.GetItemsAt(_initialItemsPos).Count());
                Assert.Equal(index + 1, _spatialMap.GetItemsAt(_newItemsPos).Count());
            }

            // TODO: Add item to layer supporting multiple items
        }

        [Fact]
        public void MoveItemInvalid()
        {
            // Create item and add to a different position
            var lastItem = new MockSpatialMapItem(3);
            _spatialMap.Add(lastItem, _newItemsPos);
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);

            // Throws because there is already an item at the initial position on layer 3, and layer 3 doesn't support
            // multiple items
            Assert.Throws<ArgumentException>(() => _spatialMap.Move(lastItem, _initialItemsPos));
        }

        [Fact]
        public void MoveValidItemsAllValid()
        {
            var movedItems = _spatialMap.MoveValid(_initialItemsPos, _newItemsPos);
            Assert.Equal(_initialItems.Count, movedItems.Count);
            Assert.Empty(_spatialMap.GetItemsAt(_initialItemsPos));
            Assert.Equal(_initialItems.Count, _spatialMap.GetItemsAt(_newItemsPos).Count());
        }

        [Fact]
        public void MoveValidItemsSomeValid()
        {
            // Create item and add it to a different position
            var lastItem = new MockSpatialMapItem(3);
            _spatialMap.Add(lastItem, _newItemsPos);
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);

            // Most of the objects can move; 1 layer's worth is blocked by lastItem
            Assert.Equal(_initialItems.Count - (1 * (_initialItems.Count / NumLayers)), _spatialMap.MoveValid(_initialItemsPos, _newItemsPos).Count);
            Assert.Single(_spatialMap.GetItemsAt(_initialItemsPos));
            Assert.Equal(_initialItems.Count, _spatialMap.GetItemsAt(_newItemsPos).Count());
        }

        [Fact]
        public void MoveValidItemsNoneValid()
        {
            // Create item and add it to a different position
            var lastItem = new MockSpatialMapItem(3);
            _spatialMap.Add(lastItem, _newItemsPos);
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);

            // The object here cannot move because it's blocked by an existing item
            Assert.Empty(_spatialMap.MoveValid(_newItemsPos, _initialItemsPos));
            Assert.Equal(_initialItems.Count, _spatialMap.GetItemsAt(_initialItemsPos).Count());
            Assert.Single(_spatialMap.GetItemsAt(_newItemsPos));
        }

        [Fact]
        public void MoveAllItemsValid()
        {
            // No items at new location to start
            Assert.Empty(_spatialMap.GetItemsAt(_newItemsPos));

            // No items blocked so should succeed
            _spatialMap.MoveAll(_initialItemsPos, _newItemsPos);
            Assert.Equal(_initialItems.Count, _spatialMap.GetItemsAt(_newItemsPos).Count());
            Assert.Empty(_spatialMap.GetItemsAt(_initialItemsPos));
            Assert.Equal(_initialItems.Count, _spatialMap.Count);
        }

        [Fact]
        public void MoveAllItemsInvalid()
        {
            // Create item and add it to a different position
            var lastItem = new MockSpatialMapItem(3);
            _spatialMap.Add(lastItem, _newItemsPos);
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);

            // There is one item that is blocked (by lastItem), so this should fail.  Some items will be moved.
            Assert.Throws<ArgumentException>(() => _spatialMap.MoveAll(_initialItemsPos, _newItemsPos));
        }

        [Fact]
        public void MoveAllItemsLayerMaskValid()
        {
            // Create item and add it to a different position
            var lastItem = new MockSpatialMapItem(3);
            _spatialMap.Add(lastItem, _newItemsPos);
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);

            // The only item blocked is not included in the layer mask, so should succeed
            _spatialMap.MoveAll(_initialItemsPos, _newItemsPos, ~_spatialMap.LayerMasker.Mask(3));
            Assert.Equal(_initialItems.Count, _spatialMap.GetItemsAt(_newItemsPos).Count());
            Assert.Single(_spatialMap.GetItemsAt(_initialItemsPos));
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);
        }

        [Fact]
        public void MoveAllItemsLayerMaskInvalid()
        {
            // Create item and add it to a different position
            var lastItem = new MockSpatialMapItem(3);
            _spatialMap.Add(lastItem, _newItemsPos);
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);

            // There is one item that is blocked (by lastItem), so this should fail.  Some items will be moved.
            Assert.Throws<ArgumentException>(() => _spatialMap.MoveAll(_initialItemsPos, _newItemsPos, _spatialMap.LayerMasker.Mask(1, 3, 5)));
        }

        [Fact]
        public void TryMoveAllItemsLayerMaskValid()
        {
            // Create item and add it to a different position
            var lastItem = new MockSpatialMapItem(3);
            _spatialMap.Add(lastItem, _newItemsPos);
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);

            // The only item blocked is not included in the layer mask, so should succeed
            var val = _spatialMap.TryMoveAll(_initialItemsPos, _newItemsPos, ~_spatialMap.LayerMasker.Mask(3));
            Assert.True(val);
            Assert.Equal(_initialItems.Count, _spatialMap.GetItemsAt(_newItemsPos).Count());
            Assert.Single(_spatialMap.GetItemsAt(_initialItemsPos));
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);
        }

        [Fact]
        public void TryMoveAllItemsLayerMaskSomeValid()
        {
            // Mask used for operations
            uint mask = _spatialMap.LayerMasker.Mask(1, 2, 3);
            const int layersInMask = 3;

            // Create item and add it to a different position
            var lastItem = new MockSpatialMapItem(3);
            _spatialMap.Add(lastItem, _newItemsPos);
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);

            // Most of the objects can move; 1 is blocked by lastItem
            var val = _spatialMap.TryMoveAll(_initialItemsPos, _newItemsPos, mask);
            Assert.False(val);
            Assert.Equal(2, layersInMask - 1);
            Assert.Equal(_initialItems.Count - 2, _spatialMap.GetItemsAt(_initialItemsPos).Count());
            Assert.Equal(2 + 1, _spatialMap.GetItemsAt(_newItemsPos).Count());
        }

        [Fact]
        public void MoveValidItemsLayerMaskAllValid()
        {
            var movedItems = _spatialMap.MoveValid(_initialItemsPos, _newItemsPos, ~_spatialMap.LayerMasker.Mask(2));
            Assert.Equal(_initialItems.Count - 1, movedItems.Count);
            Assert.Single(_spatialMap.GetItemsAt(_initialItemsPos));
            Assert.Equal(_initialItems.Count - 1, _spatialMap.GetItemsAt(_newItemsPos).Count());
        }

        [Fact]
        public void MoveValidItemsLayerMaskSomeValid()
        {
            // Mask used for operations
            uint mask = _spatialMap.LayerMasker.Mask(1, 2, 3);
            const int layersInMask = 3;

            // Create item and add it to a different position
            var lastItem = new MockSpatialMapItem(3);
            _spatialMap.Add(lastItem, _newItemsPos);
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);

            // Most of the objects can move; 1 is blocked by lastItem
            var count = _spatialMap.MoveValid(_initialItemsPos, _newItemsPos, mask).Count;
            Assert.Equal(layersInMask - 1, count);
            Assert.Equal(_initialItems.Count - count, _spatialMap.GetItemsAt(_initialItemsPos).Count());
            Assert.Equal(count + 1, _spatialMap.GetItemsAt(_newItemsPos).Count());
        }

        [Fact]
        public void MoveValidItemsLayerMaskNoneValid()
        {
            // Create item and add it to a different position
            var lastItem = new MockSpatialMapItem(3);
            _spatialMap.Add(lastItem, _newItemsPos);
            Assert.Equal(_initialItems.Count + 1, _spatialMap.Count);

            // The object here cannot move because it's blocked by an existing item
            Assert.Empty(_spatialMap.MoveValid(_newItemsPos, _initialItemsPos, _spatialMap.LayerMasker.Mask(3)));
            Assert.Equal(_initialItems.Count, _spatialMap.GetItemsAt(_initialItemsPos).Count());
            Assert.Single(_spatialMap.GetItemsAt(_newItemsPos));
        }

        [Fact]
        public void ContainsItem()
        {
            foreach (var item in _initialItems)
                Assert.True(_spatialMap.Contains(item));

            // Unadded item is not contained
            var unaddedItem = new MockSpatialMapItem(StartingLayer + 1);
            Assert.False(_spatialMap.Contains(unaddedItem));

            // Moved items are still in the spatial map
            _spatialMap.Move(_initialItems[0], _newItemsPos);
            foreach (var item in _initialItems)
                Assert.True(_spatialMap.Contains(item));
        }

        [Fact]
        public void ContainsPosition()
        {
            Assert.True(_spatialMap.Contains(_initialItemsPos));
            Assert.False(_spatialMap.Contains(_newItemsPos));

            // Moved items update results from contained
            _spatialMap.Move(_initialItems[0], _newItemsPos);
            Assert.True(_spatialMap.Contains(_initialItemsPos));
            Assert.True(_spatialMap.Contains(_newItemsPos));
        }

        // TODO: Remaining functions such as getting all items, etc.

        // TODO: Layer-based function tests for functions other than MoveValid/MoveAll

        [Fact]
        public void Construction()
        {
            // No multiple item layers, 32 layers, starting at 0
            var sm = new LayeredSpatialMap<MockSpatialMapItem>(32);
            Assert.Equal(32, sm.NumberOfLayers);
            Assert.Equal(0, sm.StartingLayer);
            foreach (var layer in sm.Layers)
                Assert.True(layer is AdvancedSpatialMap<MockSpatialMapItem>);

            // Test multiple item layers
            var multipleItemLayerMask = LayerMasker.Default.Mask(1, 2, 5);
            sm = new LayeredSpatialMap<MockSpatialMapItem>(10, startingLayer: 0,
                layersSupportingMultipleItems: multipleItemLayerMask);
            Assert.Equal(10, sm.NumberOfLayers);
            Assert.Equal(0, sm.StartingLayer);

            int layerNum = 0;
            foreach (var layer in sm.Layers)
            {
                Assert.Equal(LayerMasker.Default.HasLayer(multipleItemLayerMask, layerNum),
                    layer is AdvancedMultiSpatialMap<MockSpatialMapItem>);
                layerNum++;
            }

            // Test arbitrary starting layer (initial values)
            const int startingLayer = 1;
            const int numberOfLayers = 5;
            sm = new LayeredSpatialMap<MockSpatialMapItem>(numberOfLayers, startingLayer: startingLayer);

            Assert.Equal(numberOfLayers, sm.NumberOfLayers);
            Assert.Equal(startingLayer, sm.StartingLayer);
        }
    }
}
