using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.SpatialMaps;

namespace GoRogue.GameFramework
{
    /// <summary>
    /// A custom enumerator used to iterate over all objects on a <see cref="GameFramework.Map"/> which are at a given location on given layers
    /// efficiently.  Items are returned in order from highest layer to lowest, as applicable.
    ///
    /// Generally, you should use <see cref="Map.GetObjectsAt(Point,uint)"/> or one of that function's overloads to get an
    /// instance of this enumerator, rather than creating one yourself.
    /// </summary>
    /// <remarks>
    /// This type is a struct, and as such is much more efficient when used in a foreach loop than a function returning
    /// IEnumerable&lt;IGameObject&gt; by using "yield return".  This type does implement <see cref="IEnumerable{IGameObject}"/>,
    /// so you can pass it to functions which require one (for example, System.LINQ).  However, this will have reduced
    /// performance due to boxing of the iterator.
    /// </remarks>
    [PublicAPI]
    public struct MapObjectsAtEnumerator : IEnumerable<IGameObject>, IEnumerator<IGameObject>
    {
        // Suppress warning stating to use auto-property because we want to guarantee micro-performance
        // characteristics.
#pragma warning disable IDE0032 // Use auto property
        private IGameObject _current;
#pragma warning restore IDE0032 // Use auto property

        /// <summary>
        /// The current value for enumeration.
        /// </summary>
        public IGameObject Current => _current;

        private readonly Map _map;
        private readonly Point _position;
        private readonly uint _layerMask;
        private bool _processedEntities;
        private ReadOnlyLayeredSpatialMapItemsAtEnumerator<IGameObject> _entitiesEnumerator;

        object IEnumerator.Current => _current;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map">Map to iterate over objects on.</param>
        /// <param name="position">The position to get objects at.</param>
        /// <param name="layerMask">A layer mask indicating what layers to include in the search for items.</param>
        public MapObjectsAtEnumerator(Map map, Point position, uint layerMask)
        {
            _map = map;
            _position = position;
            _layerMask = layerMask;

            _entitiesEnumerator = map.Entities.GetItemsAt(position, layerMask);
            _current = null!;
            _processedEntities = false;
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (_processedEntities) return false;

            if (_entitiesEnumerator.MoveNext())
            {
                // Suppression is required by .NET 8
                // ReSharper disable once RedundantSuppressNullableWarningExpression
                _current = _entitiesEnumerator.Current!;
                return true;
            }

            // Done entities; check terrain
            _processedEntities = true;
            if (_map.LayerMasker.HasLayer(_layerMask, 0))
            {

                var terrain = _map.Terrain[_position];
                if (terrain != null)
                {
                    _current = terrain;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns this enumerator.
        /// </summary>
        /// <returns>This enumerator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MapObjectsAtEnumerator GetEnumerator() => this;

        // Explicitly implemented to ensure we prefer the non-boxing versions where possible

        #region Explicit Interface Implementations

        /// <inheritdoc />
        void IEnumerator.Reset()
        {
            ((IEnumerator)_entitiesEnumerator).Reset();
            _processedEntities = false;
        }

        IEnumerator<IGameObject> IEnumerable<IGameObject>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        void IDisposable.Dispose()
        {
            ((IDisposable)_entitiesEnumerator).Dispose();
        }

        #endregion
    }

    /// <summary>
    /// A custom enumerator used to iterate over all objects on a <see cref="GameFramework.Map"/> which are at a given location on given layers
    /// and are of a given type efficiently.  Items are returned in order from highest layer to lowest, as applicable.  Any items not of the given
    /// type are ignored.
    ///
    /// Generally, you should use <see cref="Map.GetObjectsAt{T}(Point,uint)"/> or one of that function's overloads to get an
    /// instance of this enumerator, rather than creating one yourself.
    /// </summary>
    /// <remarks>
    /// This type is a struct, and as such is much more efficient when used in a foreach loop than a function returning
    /// IEnumerable&lt;T&gt; by using "yield return".  This type does implement <see cref="IEnumerable{T}"/>,
    /// so you can pass it to functions which require one (for example, System.LINQ).  However, this will have reduced
    /// performance due to boxing of the iterator.
    /// </remarks>
    /// <typeparam name="T">The type of items to look for.</typeparam>
    [PublicAPI]
    public struct MapObjectsAtCastEnumerator<T> : IEnumerable<T>, IEnumerator<T>
        where T : IGameObject
    {
        // Suppress warning stating to use auto-property because we want to guarantee micro-performance
        // characteristics.
#pragma warning disable IDE0032 // Use auto property
        private T _current;
#pragma warning restore IDE0032 // Use auto property

        /// <summary>
        /// The current value for enumeration.
        /// </summary>
        public T Current => _current;

        private MapObjectsAtEnumerator _allObjectsEnumerator;

        object IEnumerator.Current => _current;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map">Map to iterate over objects on.</param>
        /// <param name="position">The position to get objects at.</param>
        /// <param name="layerMask">A layer mask indicating what layers to include in the search for items.</param>
        public MapObjectsAtCastEnumerator(Map map, Point position, uint layerMask)
        {
            _allObjectsEnumerator = new MapObjectsAtEnumerator(map, position, layerMask);
            _current = default!;
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            while (_allObjectsEnumerator.MoveNext())
            {
                if (_allObjectsEnumerator.Current is T t)
                {
                    _current = t;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns this enumerator.
        /// </summary>
        /// <returns>This enumerator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MapObjectsAtCastEnumerator<T> GetEnumerator() => this;

        // Explicitly implemented to ensure we prefer the non-boxing versions where possible

        #region Explicit Interface Implementations

        /// <inheritdoc />
        void IEnumerator.Reset()
        {
            ((IEnumerator)_allObjectsEnumerator).Reset();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        void IDisposable.Dispose()
        {
            ((IDisposable)_allObjectsEnumerator).Dispose();
        }

        #endregion
    }

    /// <summary>
    /// A custom enumerator used to iterate over all entities on a <see cref="GameFramework.Map"/> which are at a given location on given layers
    /// and are of a given type efficiently.  Entities are returned in order from highest layer to lowest, as applicable.  Any entities not of the given
    /// type are ignored.
    ///
    /// Generally, you should use <see cref="Map.GetEntitiesAt{T}(Point,uint)"/> or one of that function's overloads to get an
    /// instance of this enumerator, rather than creating one yourself.
    /// </summary>
    /// <remarks>
    /// This type is a struct, and as such is much more efficient when used in a foreach loop than a function returning
    /// IEnumerable&lt;T&gt; by using "yield return".  This type does implement <see cref="IEnumerable{T}"/>,
    /// so you can pass it to functions which require one (for example, System.LINQ).  However, this will have reduced
    /// performance due to boxing of the iterator.
    /// </remarks>
    /// <typeparam name="T">The type of entities to look for.</typeparam>
    [PublicAPI]
    public struct MapEntitiesAtCastEnumerator<T> : IEnumerable<T>, IEnumerator<T>
        where T : IGameObject
    {
        // Suppress warning stating to use auto-property because we want to guarantee micro-performance
        // characteristics.
#pragma warning disable IDE0032 // Use auto property
        private T _current;
#pragma warning restore IDE0032 // Use auto property

        /// <summary>
        /// The current value for enumeration.
        /// </summary>
        public T Current => _current;

        private ReadOnlyLayeredSpatialMapItemsAtEnumerator<IGameObject> _allObjectsEnumerator;

        object IEnumerator.Current => _current;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map">Map to iterate over entities on.</param>
        /// <param name="position">The position to get entities at.</param>
        /// <param name="layerMask">A layer mask indicating what layers to include in the search for entities.</param>
        public MapEntitiesAtCastEnumerator(Map map, Point position, uint layerMask)
        {
            _allObjectsEnumerator = map.Entities.GetItemsAt(position, layerMask);
            _current = default!;
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            while (_allObjectsEnumerator.MoveNext())
            {
                if (_allObjectsEnumerator.Current is T t)
                {
                    _current = t;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns this enumerator.
        /// </summary>
        /// <returns>This enumerator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MapEntitiesAtCastEnumerator<T> GetEnumerator() => this;

        // Explicitly implemented to ensure we prefer the non-boxing versions where possible

        #region Explicit Interface Implementations

        /// <inheritdoc />
        void IEnumerator.Reset()
        {
            ((IEnumerator)_allObjectsEnumerator).Reset();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        void IDisposable.Dispose()
        {
            ((IDisposable)_allObjectsEnumerator).Dispose();
        }

        #endregion
    }
}
