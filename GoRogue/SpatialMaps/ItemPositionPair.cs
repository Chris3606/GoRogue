using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.SpatialMaps
{
    /// <summary>
    /// Item-location pair denoting an entry in a spatial map.
    /// </summary>
    /// <typeparam name="TItem">The type of item stored in the pairing.</typeparam>
    [DataContract]
    [PublicAPI]
    // Tuples do not resolve names properly; function is provided
    [SuppressMessage("ReSharper", "CA2225")]
    public readonly struct ItemPositionPair<TItem> : IEquatable<ItemPositionPair<TItem>>
        where TItem : notnull
    {
        /// <summary>
        /// The item.
        /// </summary>
        [DataMember] public readonly TItem Item;

        /// <summary>
        /// The position the item is located at.
        /// </summary>
        [DataMember] public readonly Point Position;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item"/>
        /// <param name="position"/>
        public ItemPositionPair(TItem item, Point position)
        {
            Item = item;
            Position = position;
        }

        /// <summary>
        /// Returns a string representing the item and the position it's located at.
        /// </summary>
        /// <returns/>
        public override string ToString() => $"{Item}: {Position}";

        #region Tuple Compatibility

        /// <summary>
        /// Supports C# Deconstruction syntax.
        /// </summary>
        /// <param name="item"/>
        /// <param name="position"/>
        public void Deconstruct(out TItem item, out Point position)
        {
            item = Item;
            position = Position;
        }

        /// <summary>
        /// Implicitly converts an ItemPositionPair to an equivalent tuple.
        /// </summary>
        /// <param name="pair"/>
        /// <returns/>
        public static implicit operator (TItem item, Point position)(ItemPositionPair<TItem> pair) => pair.ToTuple();

        /// <summary>
        /// Implicitly converts a tuple to its equivalent ItemPositionPair.
        /// </summary>
        /// <param name="tuple"/>
        /// <returns/>
        public static implicit operator ItemPositionPair<TItem>((TItem item, Point position) tuple) => FromTuple(tuple);

        /// <summary>
        /// Converts the pair to an equivalent tuple.
        /// </summary>
        /// <returns/>
        public (TItem item, Point position) ToTuple() => (Item, Position);

        /// <summary>
        /// Converts the tuple to an equivalent ItemPositionPair.
        /// </summary>
        /// <param name="tuple"/>
        /// <returns/>
        [SuppressMessage("ReSharper", "CA1000")] // Must be static to comply with implicit operator rules
        public static ItemPositionPair<TItem> FromTuple((TItem item, Point position) tuple)
            => new ItemPositionPair<TItem>(tuple.item, tuple.position);

        #endregion

        #region EqualityComparison

        /// <summary>
        /// True if the given pair has equivalent items and positions; false otherwise.
        /// </summary>
        /// <param name="other"/>
        /// <returns/>
        public bool Equals(ItemPositionPair<TItem> other)
            => Item.Equals(other.Item) && Position.Equals(other.Position);

        /// <summary>
        /// True if the given object is an ItemPositionPair that has an equivalent item and position; false otherwise.
        /// </summary>
        /// <param name="obj"/>
        /// <returns/>
        public override bool Equals(object? obj)
            => obj is ItemPositionPair<TItem> pair && Equals(pair);

        /// <summary>
        /// Returns a hash code based on all of the pair's fields.
        /// </summary>
        /// <returns/>
        public override int GetHashCode() => Item.GetHashCode() ^ Position.GetHashCode();

        /// <summary>
        /// True if the given pairs have the same item and position; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator ==(ItemPositionPair<TItem> left, ItemPositionPair<TItem> right)
            => left.Equals(right);

        /// <summary>
        /// True if the given pairs have different items and/or positions; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator !=(ItemPositionPair<TItem> left, ItemPositionPair<TItem> right)
            => !(left == right);

        #endregion
    }
}
