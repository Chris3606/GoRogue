using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.SpatialMaps
{
    /// <summary>
    /// Interface implementing only the read-only functions for <see cref="LayeredSpatialMap{T}" />/
    /// <see cref="AdvancedLayeredSpatialMap{T}" />.
    /// </summary>
    /// <typeparam name="T">
    /// Type of element stored in the layered spatial map -- must implement <see cref="IHasLayer" />.
    /// </typeparam>
    [PublicAPI]
    public interface IReadOnlyLayeredSpatialMap<T> : IReadOnlySpatialMap<T> where T : IHasLayer
    {
        /// <summary>
        /// Object used to get layer masks as they pertain to this spatial map.
        /// </summary>
        LayerMasker LayerMasker { get; }

        /// <summary>
        /// Gets read-only spatial maps representing each layer. To access a specific layer, instead
        /// use <see cref="GetLayer(int)" />.
        /// </summary>
        IEnumerable<IReadOnlySpatialMap<T>> Layers { get; }

        /// <summary>
        /// Gets the number of layers contained in the spatial map.
        /// </summary>
        int NumberOfLayers { get; }

        /// <summary>
        /// Starting index for layers contained in this spatial map.
        /// </summary>
        int StartingLayer { get; }

        /// <summary>
        /// Returns a read-only reference to the spatial map. Convenient for "safely" exposing the
        /// spatial map as a property.
        /// </summary>
        /// <returns>The current spatial map, as a "read-only" reference.</returns>
        new IReadOnlyLayeredSpatialMap<T> AsReadOnly();


        /// <summary>
        /// Returns whether or not there is an item in the spatial map at the given position that
        /// is on a layer included in the given layer mask. Defaults to searching on all layers.
        /// </summary>
        /// <param name="position">The position to check for.</param>
        /// <param name="layerMask">
        /// Layer mask that indicates which layers to check. Defaults to all layers.
        /// </param>
        /// <returns>
        /// True if there is some item at the given position on a layer included in the given layer
        /// mask, false if not.
        /// </returns>
        bool Contains(Point position, uint layerMask = uint.MaxValue);

        /// <summary>
        /// Returns whether or not there is an item in the data structure at the given position, that
        /// is on a layer included in the given layer mask.
        /// </summary>
        /// <param name="x">X-value of the position to check for.</param>
        /// <param name="y">Y-value of the position to check for.</param>
        /// <param name="layerMask">
        /// Layer mask that indicates which layers to check. Defaults to all layers.
        /// </param>
        /// <returns>
        /// True if there is some item at the given position on a layer included in the given layer
        /// mask, false if not.
        /// </returns>
        bool Contains(int x, int y, uint layerMask = uint.MaxValue);

        /// <summary>
        /// Gets the item(s) associated with the given position that reside on any layer included in
        /// the given layer mask. Returns nothing if there is nothing at that position on a layer
        /// included in the given layer mask.
        /// </summary>
        /// <param name="position">The position to return the item(s) for.</param>
        /// <param name="layerMask">
        /// Layer mask that indicates which layers to check. Defaults to all layers.
        /// </param>
        /// <returns>
        /// The item(s) at the given position that reside on a layer included in the layer mask if
        /// there are any items, or nothing if there is nothing at that position.
        /// </returns>
        IEnumerable<T> GetItemsAt(Point position, uint layerMask = uint.MaxValue);

        /// <summary>
        /// Gets the item(s) associated with the given position that reside on any layer included in
        /// the given layer mask. Returns nothing if there is nothing at that position on a layer
        /// included in the given layer mask.
        /// </summary>
        /// <param name="x">X-value of the position to return the item(s) for.</param>
        /// <param name="y">Y-value of the position to return the item(s) for.</param>
        /// <param name="layerMask">
        /// Layer mask that indicates which layers to check. Defaults to all layers.
        /// </param>
        /// <returns>
        /// The item(s) at the given position that reside on a layer included in the layer mask if
        /// there are any items, or nothing if there is nothing at that position.
        /// </returns>
        IEnumerable<T> GetItemsAt(int x, int y, uint layerMask = uint.MaxValue);

        /// <summary>
        /// Gets a read-only spatial map representing the layer specified.
        /// </summary>
        /// <param name="layer">The layer to retrieve.</param>
        /// <returns>The IReadOnlySpatialMap that represents the given layer.</returns>
        IReadOnlySpatialMap<T> GetLayer(int layer);

        /// <summary>
        /// Returns read-only spatial maps that represent each layer included in the given layer
        /// mask. Defaults to all layers.
        /// </summary>
        /// <param name="layerMask">
        /// Layer mask indicating which layers to return. Defaults to all layers.
        /// </param>
        /// <returns>Read-only spatial maps representing each layer in the given layer mask.</returns>
        IEnumerable<IReadOnlySpatialMap<T>> GetLayersInMask(uint layerMask = uint.MaxValue);

        /// <summary>
        /// Returns true if there are items at <paramref name="current" /> on one or more of the layers specified by the layer
        /// mask,
        /// and all items on those layers at that position can be moved to <paramref name="target" />; false otherwise.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        /// <param name="layerMask">Layer mask indicating which layers to check items on.</param>
        /// <returns>
        /// true if all items at the position current can be moved to the position target; false if one or more items
        /// cannot be moved or there are no items to move.
        /// </returns>
        public bool CanMoveAll(Point current, Point target, uint layerMask = uint.MaxValue);

        /// <summary>
        /// Returns true if there are items at the current position on one or more of the layers specified by the layer mask,
        /// and all items on those layers at that position can be moved to the target position; false otherwise.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        /// <param name="layerMask">Layer mask indicating which layers to check items on.</param>
        /// <returns>
        /// true if all items at the position current can be moved to the position target; false if one or more items
        /// cannot be moved or there are no items to move.
        /// </returns>
        public bool CanMoveAll(int currentX, int currentY, int targetX, int targetY, uint layerMask = uint.MaxValue);
    }
}
