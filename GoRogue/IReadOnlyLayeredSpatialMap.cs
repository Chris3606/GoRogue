using System;
using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Read-only interface for LayeredSpatialMap -- useful for exposing LayeredSpatialMap instances as read-only properties.
	/// </summary>
	/// <typeparam name="T">Type of element stored in the layered spatial map -- must implement IHasLayer.</typeparam>
	public interface IReadOnlyLayeredSpatialMap<T> : IReadOnlySpatialMap<T> where T : IHasLayer
	{ 
		/// <summary>
		/// Starting index for layers included in this structure.
		/// </summary>
		int StartingLayer { get; }

		/// <summary>
		/// Gets read-only spatial maps representing each layer.  To access a specific layer, instead use GetLayer.
		/// </summary>
		IEnumerable<IReadOnlySpatialMap<T>> Layers { get; }

		/// <summary>
		/// Object that helps get layer masks as they pertain to this LayeredSpatialMap
		/// </summary>
		LayerMasker LayerMasker { get; }

		/// <summary>
		/// Gets the number of layers represented.
		/// </summary>
		int NumberOfLayers { get; }

		/// <summary>
		/// Returns a read-only reference to the data structure. Convenient for "safely" exposing the
		/// structure as a property.
		/// </summary>
		/// <returns>The current data structure, as a "read-only" reference.</returns>
		new IReadOnlyLayeredSpatialMap<T> AsReadOnly();

		/// <summary>
		/// Gets the item(s) associated with the given position that reside on any layer included in the given layer mask.  Returns
		/// nothing if there is nothing at that position on a layer included in the given layer mask.
		/// </summary>
		/// <param name="position">The position to return the item(s) for.</param>
		/// <param name="layerMask">Layer mask that indicates which layers to check. Defaults to all layers.</param>
		/// <returns>
		/// The item(s) at the given position that reside on a layer included in the layer mask if there are any items, or nothing if there is nothing
		/// at that position.
		/// </returns>
		IEnumerable<T> GetItems(Coord position, uint layerMask = uint.MaxValue);

		/// <summary>
		/// Gets the item(s) associated with the given position that reside on any layer included in the given layer mask.  Returns
		/// nothing if there is nothing at that position on a layer included in the given layer mask.
		/// </summary>
		/// <param name="x">X-value of the position to return the item(s) for.</param>
		/// <param name="y">Y-value of the position to return the item(s) for.</param>
		/// <param name="layerMask">Layer mask that indicates which layers to check. Defaults to all layers.</param>
		/// <returns>
		/// The item(s) at the given position that reside on a layer included in the layer mask if there are any items, or nothing if there is nothing
		/// at that position.
		/// </returns>
		IEnumerable<T> GetItems(int x, int y, uint layerMask = uint.MaxValue);

		/// <summary>
		/// Returns whether or not there is an item in the data structure at the given position that is on a layer included in the given layer mask.  Defaults
		/// to searching on all layers.
		/// </summary>
		/// <param name="position">The position to check for.</param>
		/// <param name="layerMask">Layer mask that indicates which layers to check.  Defaults to all layers.</param>
		/// <returns>True if there is some item at the given position on a layer included in the given layer mask, false if not.</returns>
		bool Contains(Coord position, uint layerMask = uint.MaxValue);

		/// <summary>
		/// Returns whether or not there is an item in the data structure at the given position, that is on a layer included in the given layer mask.
		/// </summary>
		/// <param name="x">X-value of the position to check for.</param>
		/// <param name="y">Y-value of the position to check for.</param>
		/// <param name="layerMask">Layer mask that indicates which layers to check. Defaults to all layers.</param>
		/// <returns>True if there is some item at the given position on a layer included in the given layer mask, false if not.</returns>
		bool Contains(int x, int y, uint layerMask = uint.MaxValue);

		/// <summary>
		/// Gets a read-only spatial map representing the layer given.
		/// </summary>
		/// <param name="layer">The layer to retrieve.</param>
		/// <returns>The IReadOnlySpatialMap that represents the given layer.</returns>
		IReadOnlySpatialMap<T> GetLayer(int layer);

		/// <summary>
		/// Returns read-only spatial maps that represent each layer included in the given layer mask.  Defaults to all layers.
		/// </summary>
		/// <param name="layerMask">Layer mask indicating which layers to return.  Defaults to all layers.</param>
		/// <returns></returns>
		IEnumerable<IReadOnlySpatialMap<T>> GetLayers(uint layerMask = uint.MaxValue);
	}

	/// <summary>
	/// Allows convenient interpretation and creation of layer masks (bit-masks) mask layers represented by
	/// integers [0-NumberOfLayers - 1].
	/// </summary>
	public sealed class LayerMasker
	{
		/// <summary>
		/// Default layer masker, that excludes no possible layers from its results.
		/// </summary>
		public static readonly LayerMasker DEFAULT = new LayerMasker(32);

		/// <summary>
		/// Maximum number of layers supported by this layer masker.  Functions enforce this limit, and will not consider
		/// layers outside the range [0, NumberOfLayers - 1]
		/// </summary>
		public int NumberOfLayers { get; }

		/// <summary>
		/// Layer-mask representing all layers.
		/// </summary>
		public readonly uint ALL_LAYERS;
		/// <summary>
		/// Layer mask representing no layers.
		/// </summary>
		public readonly uint NO_LAYERS;

		/// <summary>
		/// Constructor.  Takes the number of layers allowed, which must be more than 0 and less than or equal to 32.
		/// </summary>
		/// <param name="numberOfLayers">The number of layers supported by this layer mask generator.</param>
		public LayerMasker(int numberOfLayers = 32)
		{
			if (numberOfLayers > 32 || numberOfLayers <= 0)
				throw new ArgumentOutOfRangeException(nameof(numberOfLayers), $"{nameof(LayerMasker)} must support 1 and can support a maximum of 32 layers.");

			NumberOfLayers = numberOfLayers; // Cast is fine since it's under 32 and obviously above 0
			ALL_LAYERS = (uint)1 << (NumberOfLayers - 1); // Last layer index
			ALL_LAYERS |= ALL_LAYERS - 1; // Propagate the right-most 1-bit all the way down
			NO_LAYERS = 0;
		}

		/// <summary>
		/// Gets a layer mask including exactly the given layer indices.  Any layer given outside the maximum number of layers is ignored,
		/// and will not be added to the resulting mask.
		/// </summary>
		/// <param name="layers">One or more layers to include in generated layer mask.</param>
		/// <returns>A layer mask including exactly those layers in the input that were within the supported number of layers.</returns>
		public uint Mask(params int[] layers) => Mask((IEnumerable<int>)layers);

		/// <summary>
		/// Gets a layer mask including exactly the given layer indices.  Any layer given outside the maximum number of layers is ignored,
		/// and will not be added to the resulting mask.
		/// </summary>
		/// <param name="layers">Layers to include in generated layer mask.</param>
		/// <returns>A layer mask including exactly those layers in the input that were within the supported number of layers.</returns>
		public uint Mask(IEnumerable<int> layers)
		{
			uint mask = 0;

			foreach (var layer in layers)
				mask |= ((uint)1 << layer);

			return mask & ALL_LAYERS; // And to ensure we do not set any layers that are out of the ones we care about
		}

		/// <summary>
		/// Adds the given layers to the given layer mask, provided those layers are within the supported number of layers.  Any layer
		/// outside of this range will not be added.
		/// </summary>
		/// <param name="mask">The mask to add the layers to.</param>
		/// <param name="layers">Layers to include in the resulting layer mask.</param>
		/// <returns>A layer mask including any original layers that were in the given mask (regardless of whether they were within the supported number of
		/// layers), as well as the new layers provided (provided they are within the supported number of layers).</returns>
		public uint AddLayers(uint mask, params int[] layers) => AddLayers(mask, (IEnumerable<int>)layers);

		/// <summary>
		/// Adds the given layers to the given layer mask, provided those layers are within the supported number of layers.  Any layer
		/// outside of this range will not be added.
		/// </summary>
		/// <param name="mask">The mask to add the layers to.</param>
		/// <param name="layers">Layers to include in the resulting layer mask.</param>
		/// <returns>A layer mask including any original layers that were in the given mask (regardless of whether they were within the supported number of
		/// layers), as well as the new layers provided (provided they are within the supported number of layers).</returns>
		public uint AddLayers(uint mask, IEnumerable<int> layers)
		{
			uint newMask = 0;
			foreach (var layer in layers)
				newMask |= ((uint)1 << layer);

			newMask &= ALL_LAYERS;
			return mask | newMask;
		}

		/// <summary>
		/// Generates and returns a layer mask including the given layer and any layer below (less than) it.  Layers not in the supported number
		/// of layers will be ignored, and will not be added to the resulting layer mask.
		/// </summary>
		/// <param name="layer">Layer to start the mask at.</param>
		/// <returns>A layer mask including the specified layer and all layers below it, provided those layers fall within the supported
		/// number of layers.</returns>
		public uint MaskAllBelow(int layer)
		{
			uint mask = (uint)1 << layer;
			mask |= (mask - 1); // Propagate the right-most 1-bit all the way down
			return mask & ALL_LAYERS;
		}

		/// <summary>
		/// Generates and returns a layer mask including the given layer and any layer above (greater than) it.  Layers not in the supported number
		/// of layers will be ignored, and will not be added to the resulting layer mask.
		/// </summary>
		/// <param name="layer">Layer to start the mask at.</param>
		/// <returns>A layer mask including the specified layer and all layers above it, provided those layers fall within the supported
		/// number of layers.</returns>
		public uint MaskAllAbove(int layer)
		{
			if (layer == 0) // Special case to avoid signing issues (shifting by negative number very bad)
				return ALL_LAYERS;

			uint mask = (uint)1 << (layer - 1);
			mask = mask | (mask - 1); // Propagate the right-most 1-bit all the way down
			return (~mask) & ALL_LAYERS; // Invert so 1's are in upper half
		}

		/// <summary>
		/// Returns whether or not a layer is contained within the given layer mask.  Returns false if the layer given is outside the supported number
		/// of layers.
		/// </summary>
		/// <param name="mask">Layer mask to check.</param>
		/// <param name="layer">Layer to check for.</param>
		/// <returns>True if the given layer is present in the given layer mask, false if it is not or the layer is outside the supported
		/// number of layers for this LayerMasker.</returns>
		public bool HasLayer(uint mask, int layer) => ((mask & ((uint)1 << layer)) & ALL_LAYERS) != 0;

		/// <summary>
		/// Returns an IEnumerable of all layers contained within the given layer mask (that fall within the supported number of layers).
		/// </summary>
		/// <param name="mask">Layer mask to return layers for.</param>
		/// <returns>All layers contained within the given layer mask that fall within the supported number of layers for this LayerMasker.</returns>
		public IEnumerable<int> Layers(uint mask)
		{
			for (int layer = NumberOfLayers - 1; layer >= 0; layer--)
			{
				if ((mask & ((uint)1 << layer)) != 0)
					yield return layer;
			}
		}
	}
}
