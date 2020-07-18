using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GoRogue.MapGeneration.ContextComponents;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.MapGeneration.ContextComponents
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="ItemList{TItem}"/>
    /// </summary>
    [PublicAPI]
    [Serializable]
    [SuppressMessage("ReSharper", "CA1815")] // Type should only be used for serialization
    public struct ItemListSerialized<TItem> where TItem : notnull
    {
        /// <summary>
        /// Items added paired with the name of the step that added them.
        /// </summary>
        public List<ItemStepPair<TItem>> Items;

        /// <summary>
        /// Converts <see cref="ItemList{TItem}"/> to <see cref="ItemListSerialized{TItem}"/>.
        /// </summary>
        /// <param name="list"/>
        /// <returns/>
        public static implicit operator ItemListSerialized<TItem>(ItemList<TItem> list)
            => FromItemList(list);

        /// <summary>
        /// Converts <see cref="ItemListSerialized{TItem}"/> to <see cref="ItemList{TItem}"/>.
        /// </summary>
        /// <param name="list"/>
        /// <returns/>
        public static implicit operator ItemList<TItem>(ItemListSerialized<TItem> list)
            => list.ToItemList();

        /// <summary>
        /// Converts <see cref="ItemList{TItem}"/> to <see cref="ItemListSerialized{TItem}"/>.
        /// </summary>
        /// <param name="list"/>
        /// <returns/>
        [SuppressMessage("ReSharper", "CA1000")] // Static method is required to implement implicit ops
        public static ItemListSerialized<TItem> FromItemList(ItemList<TItem> list)
            => new ItemListSerialized<TItem> { Items = list.ToList() };

        /// <summary>
        /// Converts <see cref="ItemListSerialized{TItem}"/> to <see cref="ItemList{TItem}"/>.
        /// </summary>
        /// <returns/>
        public ItemList<TItem> ToItemList()
        {
            var itemList = new ItemList<TItem>();

            foreach (var (item, step) in Items)
                itemList.Add(item, step);

            return itemList;
        }
    }
}
