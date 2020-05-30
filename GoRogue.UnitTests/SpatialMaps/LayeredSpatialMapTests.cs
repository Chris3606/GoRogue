using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoRogue.SpatialMaps;
using GoRogue.UnitTests.Mocks;
using Xunit;

namespace GoRogue.UnitTests.SpatialMaps
{
    public class LayeredSpatialMapTests
    {
        [Fact]
        public void TestConstruction()
        {
            // No multiple item layers, 32 layers, starting at 0
            var sm = new LayeredSpatialMap<MockSpatialMapItem>(32);
            Assert.Equal(32, sm.NumberOfLayers);
            Assert.Equal(0, sm.StartingLayer);
            foreach (var layer in sm.Layers)
                Assert.True(layer is AdvancedSpatialMap<MockSpatialMapItem>);

            // Test multiple item layers
            uint multipleItemLayerMask = LayerMasker.DEFAULT.Mask(0, 2, 5);
            sm = new LayeredSpatialMap<MockSpatialMapItem>(10, 0, multipleItemLayerMask);
            Assert.Equal(10, sm.NumberOfLayers);
            Assert.Equal(0, sm.StartingLayer);

            int layerNum = 0;
            foreach (var layer in sm.Layers)
            {
                Assert.Equal(LayerMasker.DEFAULT.HasLayer(multipleItemLayerMask, layerNum), layer is AdvancedMultiSpatialMap<MockSpatialMapItem>);
                layerNum++;
            }

            // Test arbitrary starting layer (initial values)
            int startingLayer = 1;
            int numberOfLayers = 5;
            sm = new LayeredSpatialMap<MockSpatialMapItem>(numberOfLayers, startingLayer);

            Assert.Equal(numberOfLayers, sm.NumberOfLayers);
            Assert.Equal(startingLayer, sm.StartingLayer);
        }

        [Fact]
        public void TestAdd()
        {
            IEnumerable<MockSpatialMapItem> answer;
            var sm = new LayeredSpatialMap<MockSpatialMapItem>(5);
            int previousCount = 0;
            for (int i = 0; i < 5; i++)
            {
                previousCount = sm.GetItemsAt(1, 2).Count();
                var item = new MockSpatialMapItem(i);
                sm.Add(item, (1, 2));
                answer = sm.GetItemsAt((1, 2));
                Assert.Equal(previousCount + 1, answer.Count());
            }

            Assert.Throws<InvalidOperationException>(() => sm.Add(new MockSpatialMapItem(5), (2, 3)));

            sm = new LayeredSpatialMap<MockSpatialMapItem>(5, 1);
            Assert.Throws<InvalidOperationException>(() => sm.Add(new MockSpatialMapItem(0), (5, 6)));
            answer = sm.GetItemsAt((5, 6));
            Assert.Empty(answer);
        }

        [Fact]
        public void TestRemove()
        {
            IEnumerable<MockSpatialMapItem> answer;
            var itemsAdded = new List<MockSpatialMapItem>();
            var sm = new LayeredSpatialMap<MockSpatialMapItem>(5);
            int previousCount = 0;
            for (int i = 0; i < 5; i++)
            {
                var item = new MockSpatialMapItem(i);
                itemsAdded.Add(item);
                sm.Add(item, (1, 2));
                answer = sm.GetItemsAt((1, 2));
            }


            var nonAddedItem = new MockSpatialMapItem(2);

            Assert.Throws<InvalidOperationException>(()=>sm.Remove(nonAddedItem));
            previousCount = sm.GetItemsAt((1, 2)).Count();
            answer = sm.GetItemsAt((1, 2));
            Assert.NotEmpty(answer);
            Assert.Equal(previousCount, answer.Count());

            foreach (var i in itemsAdded)
            {
                previousCount = sm.Items.Count();
                sm.Remove(i);
                Assert.Equal(previousCount - 1, sm.Items.Count());
            }


            foreach (var i in itemsAdded)
            {
                previousCount = sm.Items.Count();
                sm.Add(i, (1, 2));
                Assert.Equal(previousCount + 1, sm.Items.Count());
            }
            
            List<MockSpatialMapItem> itemsRemoved = sm.Remove((5, 6)).ToList();
            Assert.Empty(itemsRemoved);

            itemsRemoved = sm.Remove((1, 2)).ToList();
            Assert.Equal(itemsAdded.Count, itemsRemoved.Count);

        }

        //[Fact]
        //public void TestMove()
        //{
        //    IEnumerable<MockSpatialMapItem> answer;
        //    var itemsAdded = new List<MockSpatialMapItem>();
        //    var sm = new LayeredSpatialMap<MockSpatialMapItem>(5);
        //    int previousCount = 0;
        //    for (int i = 0; i < 5; i++)
        //    {
        //        previousCount = itemsAdded.Count;
        //        var item = new MockSpatialMapItem(i);
        //        itemsAdded.Add(item);
        //        sm.Add(item, (1, 2));
        //        Assert.Equal(previousCount + 1, itemsAdded.Count);
        //    }

        //    var lastItem = new MockSpatialMapItem(3);
        //    itemsAdded.Add(lastItem);
        //    sm.Add(lastItem, (5, 6));

        //    bool result = sm.Move(lastItem, 1, 2);
        //    Assert.Equal(false, result); // Blocked by first 5 adds and they no layers support multiple items

        //    result = sm.Move(lastItem, (2, 3)); // At 2, 3 we now have item on layer 3
        //    Assert.Equal(true, result);

        //    List<MockSpatialMapItem> itemsMoved = sm.Move((1, 2), (2, 3)).ToList();
        //    Assert.Equal(4, itemsMoved.Count); // All original items minus 1 because that one was blocked

        //    itemsMoved = sm.Move((2, 3), (1, 2), ~sm.LayerMasker.Mask(2)).ToList();
        //    Assert.Equal(3, itemsMoved.Count); // lastItem from above masked out by layer

        //    sm = new LayeredSpatialMap<MockSpatialMapItem>(5, 1);
        //    for (int i = 0; i < itemsAdded.Count - 1; i++) // All but that last item so we add at diff pos
        //        sm.Add(itemsAdded[i], (1, 2));

        //    sm.Add(lastItem, (2, 3));
        //    // We should have added all but the one that was layer 0
        //    itemsMoved = sm.Move(1, 2, 2, 3).ToList();
        //    Assert.Equal(3, itemsMoved.Count);

        //    itemsMoved = sm.Move(2, 3, 1, 2, ~sm.LayerMasker.Mask(2)).ToList();
        //    Assert.Equal(2, itemsMoved.Count);
        //    List<MockSpatialMapItem> itemsLeft = sm.GetItems((2, 3)).ToList();
        //    Assert.Equal(2, itemsLeft.Count);
        //    bool found = false;
        //    foreach (var item in itemsLeft)
        //        found = found || item == itemsAdded[2];
        //    Assert.Equal(true, found);
        //}

        [Fact]
        public void ManualTestPrint()
        {
            var map = new LayeredSpatialMap<MockSpatialMapItem>(3, 1, LayerMasker.DEFAULT.Mask(2));

            map.Add(new MockSpatialMapItem(1), (1, 2));
            map.Add(new MockSpatialMapItem(1), (3, 4));
            map.Add(new MockSpatialMapItem(2), (1, 1));
            map.Add(new MockSpatialMapItem(3), (0, 0));

            Console.WriteLine("SpatialMap: ");
            Console.WriteLine(map);

            Console.WriteLine("No need to test stringifier, as used by impl of regular ToString()");
        }

        // TODO: Implement
        /*
		[TestMethod]
		public void TestContains()
		{
			throw new NotImplementedException();
		}
		[TestMethod]
		public void TestGetItems()
		{
			throw new NotImplementedException();
		}
		*/
    }
}
