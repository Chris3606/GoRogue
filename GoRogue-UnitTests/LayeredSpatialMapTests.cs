using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
	public class MySpatialMapItem : IHasID, IHasLayer
	{
		public int Layer { get; }
		public uint ID { get; }

		private static IDGenerator _idGen = new IDGenerator();

		public MySpatialMapItem(int layer)
		{
			ID = _idGen.UseID();
			Layer = layer;
		}
	}

	[TestClass]
	public class LayeredSpatialMapTests
	{
		[TestMethod]
		public void TestConstruction()
		{
			// No multiple item layers, 32 layers, starting at 0
			var sm = new LayeredSpatialMap<MySpatialMapItem>(32);
			Assert.AreEqual(32, sm.NumberOfLayers);
			Assert.AreEqual(0, sm.StartingLayer);
			foreach (var layer in sm.Layers)
				Assert.AreEqual(true, layer is SpatialMap<MySpatialMapItem>);

			// Test multiple item layers
			uint multipleItemLayerMask = LayerMasker.DEFAULT.Mask(0, 2, 5);
			sm = new LayeredSpatialMap<MySpatialMapItem>(10, 0, multipleItemLayerMask);
			Assert.AreEqual(10, sm.NumberOfLayers);
			Assert.AreEqual(0, sm.StartingLayer);

			int layerNum = 0;
			foreach (var layer in sm.Layers)
			{
				Assert.AreEqual(LayerMasker.DEFAULT.HasLayer(multipleItemLayerMask, layerNum), layer is MultiSpatialMap<MySpatialMapItem>);
				layerNum++;
			}

			// Test arbitrary starting layer (initial values)
			int startingLayer = 1;
			int numberOfLayers = 5;
			sm = new LayeredSpatialMap<MySpatialMapItem>(numberOfLayers, startingLayer);

			Assert.AreEqual(numberOfLayers, sm.NumberOfLayers);
			Assert.AreEqual(startingLayer, sm.StartingLayer);
		}

		[TestMethod]
		public void TestAdd()
		{
			var sm = new LayeredSpatialMap<MySpatialMapItem>(5);
			bool result;
			for (int i = 0; i < 5; i++)
			{
				var item = new MySpatialMapItem(i);
				result = sm.Add(item, Coord.Get(1, 2));
				Assert.AreEqual(true, result);
			}

			result = sm.Add(new MySpatialMapItem(5), Coord.Get(2, 3)); // Out of range layer
			Assert.AreEqual(false, result);

			sm = new LayeredSpatialMap<MySpatialMapItem>(5, 1);
			result = sm.Add(new MySpatialMapItem(0), Coord.Get(5, 6));
			Assert.AreEqual(false, result);
		}

		[TestMethod]
		public void TestRemove()
		{
			/*
			var itemsAdded = new List<MySpatialMapItem>();
			var sm = new LayeredSpatialMap<MySpatialMapItem>(5);

			for (int i = 0; i < 5; i++)
			{
				var item = new MySpatialMapItem(i);
				itemsAdded.Add(item);
				sm.Add(item, Coord.Get(1, 2));
			}

			bool result;
			
			var nonAddedItem = new MySpatialMapItem(2);
			result = sm.Remove(nonAddedItem);
			Assert.AreEqual(false, result);

			foreach (var i in itemsAdded)
			{
				result = sm.Remove(i);
				Assert.AreEqual(true, result);
			}

			
			foreach (var i in itemsAdded)
			{
				sm.Add(i, Coord.Get(1, 2));
			}

			List<MySpatialMapItem> itemsRemoved = sm.Remove(Coord.Get(5, 6)).ToList();
			Assert.AreEqual(0, itemsRemoved.Count);

			itemsRemoved = sm.Remove(Coord.Get(1, 2)).ToList();
			Assert.AreEqual(itemsAdded.Count, itemsRemoved.Count);
			*/
		}

		[TestMethod]
		public void TestMove()
		{
			
			var itemsAdded = new List<MySpatialMapItem>();
			
			var sm = new LayeredSpatialMap<MySpatialMapItem>(5);
			
			for (int i = 0; i < 5; i++)
			{
				var item = new MySpatialMapItem(i);
				itemsAdded.Add(item);
				sm.Add(item, Coord.Get(1, 2));
			}
			
			var lastItem = new MySpatialMapItem(3);
			itemsAdded.Add(lastItem);
			sm.Add(lastItem, Coord.Get(5, 6));
			
			bool result = sm.Move(lastItem, 1, 2);
			Assert.AreEqual(false, result); // Blocked by first 5 adds and they no layers support multiple items
			
			result = sm.Move(lastItem, Coord.Get(2, 3)); // At 2, 3 we now have item on layer 3
			Assert.AreEqual(true, result);

			List<MySpatialMapItem> itemsMoved = sm.Move(Coord.Get(1, 2), Coord.Get(2, 3)).ToList();
			Assert.AreEqual(4, itemsMoved.Count); // All original items minus 1 because that one was blocked

			itemsMoved = sm.Move(Coord.Get(2, 3), Coord.Get(1, 2), ~sm.LayerMasker.Mask(2)).ToList();
			Assert.AreEqual(3, itemsMoved.Count); // lastItem from above masked out by layer

			sm = new LayeredSpatialMap<MySpatialMapItem>(5, 1);
			for (int i = 0; i < itemsAdded.Count - 1; i++) // All but that last item so we add at diff pos
				sm.Add(itemsAdded[i], Coord.Get(1, 2));

			sm.Add(lastItem, Coord.Get(2, 3));
			// We should have added all but the one that was layer 0
			itemsMoved = sm.Move(1, 2, 2, 3).ToList();
			Assert.AreEqual(3, itemsMoved.Count);

			itemsMoved = sm.Move(2, 3, 1, 2, ~sm.LayerMasker.Mask(2)).ToList();
			Assert.AreEqual(2, itemsMoved.Count);
			List<MySpatialMapItem> itemsLeft = sm.GetItems(Coord.Get(2, 3)).ToList();
			Assert.AreEqual(2, itemsLeft.Count);
			bool found = false;
			foreach (var item in itemsLeft)
				found = found || item == itemsAdded[2];
			Assert.AreEqual(true, found);
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
