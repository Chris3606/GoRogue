using System.Collections.Generic;

namespace GoRogue.MapGeneration.ContextComponents
{
    /// <summary>
    /// A generic list of items added by map generation steps, that tracks what generation step added what item.
    /// </summary>
    /// <typeparam name="TItem">Type of item being stored.</typeparam>
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
    }
}
