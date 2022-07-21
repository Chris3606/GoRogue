using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace GoRogue.SpatialMaps
{
    /// <summary>
    /// A type designed to be returned from <see cref="LayerMasker.Layers"/> in order to enumerate over a set of layers
    /// in a layer mask using foreach.
    /// </summary>
    /// <remarks>
    /// This type is a struct, and as such is much more efficient than using the otherwise equivalent type of
    /// IEnumerable&lt;int&gt; with "yield return".  The type does contain a function <see cref="ToEnumerable"/> which
    /// creates an IEnumerable&lt;int&gt;, which can be convenient for allowing the returned layer sets to be used with
    /// LINQ; however using this function is not recommended in situations where runtime performance is a primary
    /// concern.
    ///
    /// This iterator will enumerate the layers in reverse order; ie. highest active number to lowest active number.
    /// </remarks>
    [PublicAPI]
    public struct LayerCollectionEnumerable
    {
        private uint _mask;
#pragma warning disable IDE0032 // Supress use auto property; we need to guarantee performance characteristics
        private int _current;
#pragma warning restore IDE0032
        /// <summary>
        /// The current value for enumeration.
        /// </summary>
        public int Current => _current;

        /// <summary>
        /// Constructs a new enumerator.
        /// </summary>
        /// <param name="mask">Layer mask to enumerate.</param>
        /// <param name="numLayers">Number of layers to be considered within the mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayerCollectionEnumerable(uint mask, int numLayers)
        {
            _mask = mask << (32 - numLayers);
            _current = numLayers - 1;
        }

        /// <summary>
        /// Advances the iterator to the next active layer.
        /// </summary>
        /// <returns>True if an active layer was found; false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            // Advance iterator to next layer present
            while ((_mask & 0x80000000) == 0)
            {
                _mask <<= 1;
                _current--;
                // No more layers in mask
                if (_mask == 0) return false;
            }

            // Mask out layer we're currently on so we don't count it next time, and return
            _mask &= 0x7FFFFFFF;
            return true;
        }

        /// <summary>
        /// Converts the result of the enumerable to a <see cref="IEnumerable{T}"/>, which can be useful if you need
        /// to use the result with LINQ.
        /// </summary>
        /// <remarks>
        /// Note that this function advances the state of the enumerator, evaluating it to its fullest extent.  Also
        /// note that it is NOT recommended to use this function in cases where performance is critical.
        /// </remarks>
        /// <returns>An IEnumerable&lt;int&gt; which contains all values active in the layer mask being enumerated by this enumerator.</returns>
        public IEnumerable<int> ToEnumerable()
        {
            foreach (int layer in this)
                yield return layer;
        }

        /// <summary>
        /// Returns this enumerator.
        /// </summary>
        /// <returns>This enumerator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayerCollectionEnumerable GetEnumerator() => this;
    }

    /// <summary>
    /// Allows convenient interpretation and creation of layer masks (bit-masks) that can be used
    /// to interact with <see cref="LayeredSpatialMap{T}" /> and <see cref="GameFramework.Map" />.
    /// </summary>
    /// <remarks>
    /// A layer mask is simply a list of layers.  It is frequently used in <see cref="LayeredSpatialMap{T}" />
    /// and <see cref="GameFramework.Map" /> as an optional parameter that indicates what layers should apply
    /// to an operation or given set of functionality.
    /// LayeredSpatialMap and Map both define their own LayerMask variable that should be used to retrieve
    /// layer masks whenever possible.  For layer masks needed outside of that, or when constructing
    /// those classes, use <see cref="Default" />.  There are also constants defined in
    /// LayerMasker to represent "all layers" and "no layers".
    /// </remarks>
    [PublicAPI]
    public sealed class LayerMasker
    {
        /// <summary>
        /// Default layer masker, that excludes no possible layers from its results.
        /// </summary>
        public static readonly LayerMasker Default = new LayerMasker();

        /// <summary>
        /// Layer-mask representing all layers.
        /// </summary>
        public readonly uint AllLayers;

        /// <summary>
        /// Layer mask representing no layers.
        /// </summary>
        public readonly uint NoLayers;

        /// <summary>
        /// Constructor. Takes the number of layers allowed, which must be more than 0 and less than
        /// or equal to 32.
        /// </summary>
        /// <param name="numberOfLayers">The number of layers supported by this layer mask generator.</param>
        public LayerMasker(int numberOfLayers = 32)
        {
            if (numberOfLayers > 32 || numberOfLayers <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfLayers),
                    $"{nameof(LayerMasker)} must support 1 and can support a maximum of 32 layers.");

            NumberOfLayers = numberOfLayers; // Cast is fine since it's under 32 and obviously above 0
            AllLayers = (uint)1 << (NumberOfLayers - 1); // Last layer index
            AllLayers |= AllLayers - 1; // Propagate the right-most 1-bit all the way down
            NoLayers = 0;
        }

        /// <summary>
        /// Maximum number of layers supported by this layer masker. Functions using layer masks enforce this limit,
        /// and will not consider layers outside the range [0, NumberOfLayers - 1]
        /// </summary>
        public int NumberOfLayers { get; }

        /// <summary>
        /// Adds the given layers to the given layer mask, provided those layers are within the
        /// supported number of layers. Any layer outside of this range will not be added.
        /// </summary>
        /// <param name="mask">The mask to add the layers to.</param>
        /// <param name="layers">Layers to include in the resulting layer mask.</param>
        /// <returns>
        /// A layer mask including any original layers that were in the given mask (regardless of
        /// whether they were within the supported number of layers), as well as the new layers
        /// provided (provided they are within the supported number of layers).
        /// </returns>
        public uint AddLayers(uint mask, params int[] layers)
        {
            uint newMask = 0;
            foreach (int layer in layers)
                newMask |= (uint)1 << layer;

            newMask &= AllLayers;
            return mask | newMask;
        }

        /// <summary>
        /// Adds the given layers to the given layer mask, provided those layers are within the
        /// supported number of layers. Any layer outside of this range will not be added.
        /// </summary>
        /// <param name="mask">The mask to add the layers to.</param>
        /// <param name="layers">Layers to include in the resulting layer mask.</param>
        /// <returns>
        /// A layer mask including any original layers that were in the given mask (regardless of
        /// whether they were within the supported number of layers), as well as the new layers
        /// provided (provided they are within the supported number of layers).
        /// </returns>
        public uint AddLayers(uint mask, IEnumerable<int> layers)
        {
            uint newMask = 0;
            foreach (int layer in layers)
                newMask |= (uint)1 << layer;

            newMask &= AllLayers;
            return mask | newMask;
        }

        /// <summary>
        /// Adds layers active in the mask <paramref name="addMask"/> to the <paramref name="mask"/> value, and returns
        /// the result.  The result will include all layers on in the original mask, as well as all layers on in
        /// <paramref name="addMask"/>, provided those layers are within the supported number of layers. Any layer
        /// outside of this range will not be added.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="addMask"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint AddLayers(uint mask, uint addMask) => (mask | addMask) & AllLayers;

        /// <summary>
        /// Returns whether or not a layer is contained within the given layer mask. Returns false if
        /// the layer given is outside the supported number of layers.
        /// </summary>
        /// <param name="mask">Layer mask to check.</param>
        /// <param name="layer">Layer to check for.</param>
        /// <returns>
        /// True if the given layer is present in the given layer mask, false if it is not or the
        /// layer is outside the supported number of layers for this LayerMasker.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasLayer(uint mask, int layer) => (mask & ((uint)1 << layer) & AllLayers) != 0;

        /// <summary>
        /// Returns a custom enumerator that will enumerate all layers contained within the given layer mask (that fall
        /// within the supported number of layers).  This function is intended to be used primarily with a foreach loop
        /// (see remarks).
        /// </summary>
        /// <remarks>
        /// This function returns a custom value type which can be treated like an enumerator and used with C#'s foreach
        /// loop.  It is intended to be used like so:
        /// <code>
        /// foreach (int layerID in myLayerMasker.Layers(myLayerMask))
        ///     Console.WriteLine($"Do something with layer {layerID}.")
        /// </code>
        ///
        /// This custom iterator will perform many times better (up to 25x better, in fact) than an IEnumerable, when
        /// used in a foreach loop, since it is a value type and thus incurs no GC allocation and no boxing/unboxing.
        ///
        /// The returned enumerator also contains a function you can use to retrieve an IEnumerable if you wish to use
        /// the returned values with System.Linq or similar:
        /// <code>
        /// var array = myLayerMasker.Layers(myLayerMask).ToEnumerable().ToArray()
        /// </code>
        ///
        /// Note, however, that using this function will entirely erase the performance benefits of the custom type.
        /// </remarks>
        /// <param name="mask">Layer mask to return layers for.</param>
        /// <returns>
        /// All layers contained within the given layer mask that fall within the supported number of
        /// layers for this LayerMasker.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LayerCollectionEnumerable Layers(uint mask) => new LayerCollectionEnumerable(mask, NumberOfLayers);

        /// <summary>
        /// Gets a layer mask including exactly the given layer indices. Any layer given outside the
        /// maximum number of layers is ignored, and will not be added to the resulting mask.
        /// </summary>
        /// <param name="layers">One or more layers to include in generated layer mask.</param>
        /// <returns>
        /// A layer mask including exactly those layers in the input that were within the supported
        /// number of layers.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Mask(params int[] layers)
        {
            uint mask = 0;

            foreach (var layer in layers)
                mask |= (uint)1 << layer;

            return mask & AllLayers; // And to ensure we do not set any layers that are out of the ones we care about
        }

        /// <summary>
        /// Gets a layer mask including exactly the given layer indices. Any layer given outside the
        /// maximum number of layers is ignored, and will not be added to the resulting mask.
        /// </summary>
        /// <param name="layers">Layers to include in generated layer mask.</param>
        /// <returns>
        /// A layer mask including exactly those layers in the input that were within the supported
        /// number of layers.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Mask(IEnumerable<int> layers)
        {
            uint mask = 0;

            foreach (var layer in layers)
                mask |= (uint)1 << layer;

            return mask & AllLayers; // And to ensure we do not set any layers that are out of the ones we care about
        }

        /// <summary>
        /// Generates and returns a layer mask including the given layer and any layer above (greater
        /// than) it. Layers not in the supported number of layers will be ignored, and will not be
        /// added to the resulting layer mask.
        /// </summary>
        /// <param name="layer">Layer to start the mask at.</param>
        /// <returns>
        /// A layer mask including the specified layer and all layers above it, provided those layers
        /// fall within the supported number of layers.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint MaskAllAbove(int layer)
        {
            if (layer == 0) // Special case to avoid signing issues (shifting by negative number very bad)
                return AllLayers;

            var mask = (uint)1 << (layer - 1);
            mask |= mask - 1; // Propagate the right-most 1-bit all the way down
            return ~mask & AllLayers; // Invert so 1's are in upper half
        }

        /// <summary>
        /// Generates and returns a layer mask including the given layer and any layer below (less
        /// than) it. Layers not in the supported number of layers will be ignored, and will not be
        /// added to the resulting layer mask.
        /// </summary>
        /// <param name="layer">Layer to start the mask at.</param>
        /// <returns>
        /// A layer mask including the specified layer and all layers below it, provided those layers
        /// fall within the supported number of layers.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint MaskAllBelow(int layer)
        {
            var mask = (uint)1 << layer;
            mask |= mask - 1; // Propagate the right-most 1-bit all the way down
            return mask & AllLayers;
        }
    }
}
