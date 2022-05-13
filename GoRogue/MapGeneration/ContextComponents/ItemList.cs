using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace GoRogue.MapGeneration.ContextComponents
{
    /// <summary>
    /// A generic list of items added by map generation steps, that tracks what generation step added what item.
    /// </summary>
    /// <typeparam name="TItem">Type of item being stored.</typeparam>
    [PublicAPI]
    [DataContract]
    public class ItemList<TItem> : IEnumerable<ItemStepPair<TItem>>
        where TItem : notnull
    {
        private readonly List<TItem> _items;

        private readonly Dictionary<TItem, string> _itemToStepMapping;

        /// <summary>
        /// Creates an empty item list.
        /// </summary>
        public ItemList()
        {
            _items = new List<TItem>();
            _itemToStepMapping = new Dictionary<TItem, string>();
        }

        /// <summary>
        /// Creates a new item list and adds the given items to it.
        /// </summary>
        /// <param name="initialItems">Initial item/step pairs to add to the list.</param>
        public ItemList(IEnumerable<ItemStepPair<TItem>> initialItems)
            : this()
        {
            foreach (var (item, step) in initialItems)
                Add(item, step);
        }

        /// <summary>
        /// Creates an empty item list with the specified initial capacity for items.
        /// </summary>
        /// <param name="initialItemCapacity"></param>
        public ItemList(int initialItemCapacity)
        {
            _items = new List<TItem>(initialItemCapacity);
            _itemToStepMapping = new Dictionary<TItem, string>(initialItemCapacity);
        }

        /// <summary>
        /// The list of items added.
        /// </summary>
        public IReadOnlyList<TItem> Items => _items;

        /// <summary>
        /// A mapping of each item to the <see cref="GenerationStep.Name" /> of the generation step that created the item.
        /// </summary>
        public IReadOnlyDictionary<TItem, string> ItemToStepMapping => _itemToStepMapping.AsReadOnly();

        /// <summary>
        /// Adds an item to to the list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="generationStepName">The <see cref="GenerationStep.Name" /> of the generation step that created the item.</param>
        public void Add(TItem item, string generationStepName)
        {
            _items.Add(item);
            _itemToStepMapping.Add(item, generationStepName);
        }

        /// <summary>
        /// Adds the given items to the list.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <param name="generationStepName">The <see cref="GenerationStep.Name" /> of the generation step that created the items.</param>
        public void AddRange(IEnumerable<TItem> items, string generationStepName)
        {
            foreach (var item in items)
            {
                _items.Add(item);
                _itemToStepMapping.Add(item, generationStepName);
            }
        }

        /// <summary>
        /// Removes the given item from the list.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        public void Remove(TItem item) => Remove(item.Yield());

        /// <summary>
        /// Removes the given items from the list.
        /// </summary>
        /// <param name="items">Items to remove.</param>
        public void Remove(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                if (!_itemToStepMapping.ContainsKey(item))
                    throw new ArgumentException(
                        $"Tried to remove a value from an {nameof(ItemList<TItem>)} that was not present.");

                _items.Remove(item);
                _itemToStepMapping.Remove(item);
            }
        }

        /// <summary>
        /// Removes all items from the list for which the given function returns true.
        /// </summary>
        /// <param name="predicate">Predicate to determine which elements to remove.</param>
        public void Remove(Func<TItem, bool> predicate)
        {
            var toRemove = _items.Where(predicate).ToList();

            _items.RemoveAll(i => predicate(i));
            foreach (var item in toRemove)
                _itemToStepMapping.Remove(item);
        }

        /// <summary>
        /// Gets an enumerator of all items and the step that added them.
        /// </summary>
        /// <returns/>
        public IEnumerator<ItemStepPair<TItem>> GetEnumerator()
        {
            foreach (var obj in _items)
                yield return new ItemStepPair<TItem>(obj, _itemToStepMapping[obj]);
        }

        /// <summary>
        /// Gets a generic enumerator of all items and the step that added them.
        /// </summary>
        /// <returns/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
