using System.Linq;
using GoRogue.SpatialMaps;
using Xunit;
using Xunit.Abstractions;

namespace GoRogue.UnitTests.SpatialMaps
{
    public class LayerMaskTests
    {
        private readonly ITestOutputHelper _output;

        public LayerMaskTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AddLayer()
        {
            var masker = new LayerMasker();

            var mask = masker.Mask(0); // First layer;

            mask = masker.AddLayers(mask, 3);
            Assert.Equal(masker.Mask(0, 3), mask);

            mask = masker.AddLayers(mask, 31);
            Assert.Equal(masker.Mask(0, 3, 31), mask);

            masker = new LayerMasker(2);
            mask = masker.Mask(1);

            mask = masker.AddLayers(mask, 0);
            Assert.Equal(masker.Mask(1, 0), mask);

            mask = masker.AddLayers(mask, 3); // Out of range
            Assert.Equal(masker.Mask(0, 1), mask); // Should be same since we added a layer out of range

            mask = masker.AddLayers(mask, 31); // Out of range
            Assert.Equal(masker.Mask(0, 1), mask); // Should be same since we added a layer out of range
        }

        [Fact]
        public void GetLayers()
        {
            var masker = new LayerMasker();
            int[] layers = { 0, 2, 5 };

            var mask = masker.Mask(layers);
            var layerReturn = masker.Layers(mask).ToEnumerable().ToArray();

            layers = layers.OrderByDescending(i => i).ToArray();

            _output.WriteLine("Actual layers");
            _output.WriteLine(layers.ExtendToString());
            _output.WriteLine("Returned layers:");
            _output.WriteLine(layerReturn.ExtendToString());

            Assert.Equal(layers.Length, layerReturn.Length);
            for (var i = 0; i < layers.Length; i++)
                Assert.Equal(layers[i], layerReturn[i]);

            masker = new LayerMasker(3);
            layerReturn = masker.Layers(mask).ToEnumerable().ToArray();
            layers = layers.OrderByDescending(i => i).Where(i => i < 3).ToArray();
            Assert.Equal(layers.Length, layerReturn.Length);
            for (var i = 0; i < layers.Length; i++)
                Assert.Equal(layers[i], layerReturn[i]);
        }

        [Fact]
        public void Mask()
        {
            var masker = new LayerMasker();

            var mask = masker.Mask(0, 2, 5);
            Assert.Equal((uint)37, mask);

            mask = masker.Mask(31);
            Assert.Equal(2147483648, mask);

            mask = masker.Mask(Enumerable.Range(0, 32));
            Assert.Equal(uint.MaxValue, masker.ALL_LAYERS);
            Assert.Equal(masker.ALL_LAYERS, mask);

            mask = masker.NO_LAYERS;
            Assert.Equal((uint)0, mask);

            masker = new LayerMasker(3);
            mask = masker.Mask(0, 2, 5);
            Assert.Equal((uint)5, mask); // 5 should be excluded since only 3 layers

            mask = masker.Mask(Enumerable.Range(0, 32));
            Assert.Equal((uint)7, masker.ALL_LAYERS);
            Assert.Equal(masker.ALL_LAYERS, mask); // All layers that don't exist are ignored
        }

        [Fact]
        public void MaskAllAbove()
        {
            var masker = new LayerMasker();
            var mask = masker.MaskAllAbove(3);
            Assert.Equal(uint.MaxValue - 7, mask);

            mask = masker.MaskAllAbove(8);
            Assert.Equal(uint.MaxValue - 255, mask);

            mask = masker.MaskAllAbove(0);
            Assert.Equal(uint.MaxValue, mask);

            mask = masker.MaskAllAbove(31);
            Assert.Equal(2147483648, mask);

            masker = new LayerMasker(3);
            mask = masker.MaskAllAbove(2);
            Assert.Equal((uint)4, mask);

            mask = masker.MaskAllAbove(1);
            Assert.Equal((uint)6, mask);

            mask = masker.MaskAllAbove(3);
            Assert.Equal((uint)0, mask); // Layers should be ignored that don't exist
        }

        [Fact]
        public void MaskAllBelow()
        {
            var masker = new LayerMasker();
            var mask = masker.MaskAllBelow(3);
            Assert.Equal((uint)15, mask);

            mask = masker.MaskAllBelow(7);
            Assert.Equal((uint)255, mask);

            mask = masker.MaskAllBelow(0);
            Assert.Equal((uint)1, mask);

            mask = masker.MaskAllBelow(31);
            Assert.Equal(uint.MaxValue, mask);

            masker = new LayerMasker(3);
            mask = masker.MaskAllBelow(2);
            Assert.Equal(masker.ALL_LAYERS, mask);
            Assert.Equal((uint)7, masker.ALL_LAYERS);

            mask = masker.MaskAllBelow(1);
            Assert.Equal((uint)3, mask);

            mask = masker.MaskAllBelow(31);
            Assert.Equal((uint)7, masker.ALL_LAYERS);
            Assert.Equal(masker.ALL_LAYERS, mask); // Layers should be ignored that don't exist
        }
    }
}
