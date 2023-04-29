using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.SpatialMaps;

namespace GoRogue.GameFramework
{
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

        public MapObjectsAtEnumerator(Map map, Point position, uint layerMask)
        {
            _map = map;
            _position = position;
            _layerMask = layerMask;

            _entitiesEnumerator = map.Entities.GetItemsAt(position, layerMask);
            _current = null!;
            _processedEntities = false;
        }

        public bool MoveNext()
        {
            if (_processedEntities) return false;

            if (_entitiesEnumerator.MoveNext())
            {
                _current = _entitiesEnumerator.Current;
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

        public MapObjectsAtCastEnumerator(Map map, Point position, uint layerMask)
        {
            _allObjectsEnumerator = new MapObjectsAtEnumerator(map, position, layerMask);
            _current = default!;
        }

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

        public MapEntitiesAtCastEnumerator(Map map, Point position, uint layerMask)
        {
            _allObjectsEnumerator = map.Entities.GetItemsAt(position, layerMask);
            _current = default!;
        }

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
