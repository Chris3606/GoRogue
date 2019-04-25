using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class LayerMaskTests
	{
		[TestMethod]
		public void AddLayer()
		{
			var masker = new LayerMasker();

			uint mask = masker.Mask(0); // First layer;

			mask = masker.AddLayers(mask, 3);
			Assert.AreEqual(masker.Mask(0, 3), mask);

			mask = masker.AddLayers(mask, 31);
			Assert.AreEqual(masker.Mask(0, 3, 31), mask);

			masker = new LayerMasker(2);
			mask = masker.Mask(1);

			mask = masker.AddLayers(mask, 0);
			Assert.AreEqual(masker.Mask(1, 0), mask);

			mask = masker.AddLayers(mask, 3); // Out of range
			Assert.AreEqual(masker.Mask(0, 1), mask); // Should be same since we added a layer out of range

			mask = masker.AddLayers(mask, 31); // Out of range
			Assert.AreEqual(masker.Mask(0, 1), mask); // Should be same since we added a layer out of range
		}

		[TestMethod]
		public void Layers()
		{
			var masker = new LayerMasker();
			int[] layers = new int[] { 0, 2, 5 };

			var mask = masker.Mask(layers);
			var layerReturn = masker.Layers(mask).ToArray();

			layers = layers.OrderByDescending(i => i).ToArray();

			Console.WriteLine("Actual layers");
			Console.WriteLine(layers.ExtendToString());
			Console.WriteLine("Returned layers:");
			Console.WriteLine(layerReturn.ExtendToString());

			Assert.AreEqual(layers.Length, layerReturn.Length);
			for (int i = 0; i < layers.Length; i++)
				Assert.AreEqual(layers[i], layerReturn[i]);

			masker = new LayerMasker(3);
			layerReturn = masker.Layers(mask).ToArray();
			layers = layers.OrderByDescending(i => i).Where(i => i < 3).ToArray();
			Assert.AreEqual(layers.Length, layerReturn.Length);
			for (int i = 0; i < layers.Length; i++)
				Assert.AreEqual(layers[i], layerReturn[i]);
		}

		[TestMethod]
		public void Mask()
		{
			var masker = new LayerMasker();

			uint mask = masker.Mask(0, 2, 5);
			Assert.AreEqual((uint)37, mask);

			mask = masker.Mask(31);
			Assert.AreEqual((uint)2147483648, mask);

			mask = masker.Mask(Enumerable.Range(0, 32));
			Assert.AreEqual(uint.MaxValue, masker.ALL_LAYERS);
			Assert.AreEqual(masker.ALL_LAYERS, mask);

			mask = masker.NO_LAYERS;
			Assert.AreEqual((uint)0, mask);

			masker = new LayerMasker(3);
			mask = masker.Mask(0, 2, 5);
			Assert.AreEqual((uint)5, mask); // 5 should be excluded since only 3 layers

			mask = masker.Mask(Enumerable.Range(0, 32));
			Assert.AreEqual((uint)7, masker.ALL_LAYERS);
			Assert.AreEqual(masker.ALL_LAYERS, mask); // All layers that don't exist are ignored
		}

		[TestMethod]
		public void MaskAllAbove()
		{
			var masker = new LayerMasker();
			uint mask = masker.MaskAllAbove(3);
			Assert.AreEqual(uint.MaxValue - 7, mask);

			mask = masker.MaskAllAbove(8);
			Assert.AreEqual(uint.MaxValue - 255, mask);

			mask = masker.MaskAllAbove(0);
			Assert.AreEqual(uint.MaxValue, mask);

			mask = masker.MaskAllAbove(31);
			Assert.AreEqual((uint)2147483648, mask);

			masker = new LayerMasker(3);
			mask = masker.MaskAllAbove(2);
			Assert.AreEqual((uint)4, mask);

			mask = masker.MaskAllAbove(1);
			Assert.AreEqual((uint)6, mask);

			mask = masker.MaskAllAbove(3);
			Assert.AreEqual((uint)0, mask);  // Layers should be ignored that don't exist
		}

		[TestMethod]
		public void MaskAllBelow()
		{
			var masker = new LayerMasker();
			uint mask = masker.MaskAllBelow(3);
			Assert.AreEqual((uint)15, mask);

			mask = masker.MaskAllBelow(7);
			Assert.AreEqual((uint)255, mask);

			mask = masker.MaskAllBelow(0);
			Assert.AreEqual((uint)1, mask);

			mask = masker.MaskAllBelow(31);
			Assert.AreEqual(uint.MaxValue, mask);

			masker = new LayerMasker(3);
			mask = masker.MaskAllBelow(2);
			Assert.AreEqual(masker.ALL_LAYERS, mask);
			Assert.AreEqual((uint)7, masker.ALL_LAYERS);

			mask = masker.MaskAllBelow(1);
			Assert.AreEqual((uint)3, mask);

			mask = masker.MaskAllBelow(31);
			Assert.AreEqual((uint)7, masker.ALL_LAYERS);
			Assert.AreEqual(masker.ALL_LAYERS, mask); // Layers should be ignored that don't exist
		}
	}
}