using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace GoRogue.MapGeneration.ContextComponents
{
    /// <summary>
    /// A generic list of items added by map generation steps, that tracks what generation step added what item.
    /// </summary>
    /// <typeparam name="TItem">Type of item being stored.</typeparam>
    [PublicAPI]
    public class ItemList<TItem>
    {
        private readonly List<TItem> _items;
        /// <summary>
        /// The list of items added.
        /// </summary>
        public IReadOnlyList<TItem> Items => _items;

        private readonly Dictionary<TItem, string> _itemToStepMapping;

        /// <summary>
        /// A mapping of each item to the <see cref="GenerationStep.Name"/> of the generation step that created the item.
        /// </summary>
        public IReadOnlyDictionary<TItem, string> ItemToStepMapping => _itemToStepMapping.AsReadOnly();

        /// <summary>
        /// Creates an empty item list.
        /// </summary>
        public ItemList()
        {
            _items = new List<TItem>();
            _itemToStepMapping = new Dictionary<TItem, string>();
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
        /// Adds an item to to the list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="generationStepName">The <see cref="GenerationStep.Name"/> of the generation step that created the item.</param>
        public void AddItem(TItem item, string generationStepName)
        {
            _items.Add(item);
            _itemToStepMapping.Add(item, generationStepName);
        }

        /// <summary>
        /// Adds the given items to the list.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <param name="generationStepName">The <see cref="GenerationStep.Name"/> of the generation step that created the items.</param>
        public void AddItems(IEnumerable<TItem> items, string generationStepName)
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
        public void RemoveItem(TItem item) => RemoveItems(item.Yield());

        /// <summary>
        /// Removes the given items from the list.
        /// </summary>
        /// <param name="items">Items to remove.</param>
        public void RemoveItems(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                if (!_itemToStepMapping.ContainsKey(item))
                    throw new ArgumentException($"Tried to remove a value from an {nameof(ItemList<TItem>)} that was not present.");

                _items.Remove(item);
                _itemToStepMapping.Remove(item);
            }
        }

        /// <summary>
        /// Removes all items from the list for which the given function returns true.
        /// </summary>
        /// <param name="predicate">Predicate to determine which elements to remove.</param>
        public void RemoveItems(Func<TItem, bool> predicate)
        {
            var toRemove = _items.Where(predicate).ToList();

            _items.RemoveAll(i => predicate(i));
            foreach (var item in toRemove)
                _itemToStepMapping.Remove(item);
        }
    }
}
