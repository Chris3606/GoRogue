using GoRogue.MapGeneration.ContextComponents;
using JetBrains.Annotations;

namespace GoRogue.MapGeneration.Steps.Translation
{
    /// <summary>
    /// Appends an item list onto another one, optionally removing the one that was appended from the context.
    /// Context Components Required:
    /// - <see cref="ItemList{TItem}" /> (tag <see cref="BaseListTag" />): The base list onto which the other list is appended
    /// - <see cref="ItemList{TItem}" /> (tag <see cref="ListToAppendTag" />: The list whose items are appended onto the base
    /// list.  This component will be removed from the context if <see cref="RemoveAppendedComponent" /> is true.
    /// </summary>
    /// <typeparam name="TItem">Type of item in the lists being appended.</typeparam>
    [PublicAPI]
    public class AppendItemLists<TItem> : GenerationStep
    {
        /// <summary>
        /// A tag that must be attached to the component that will have items from the other list appended onto it.
        /// </summary>
        public readonly string BaseListTag;

        /// <summary>
        /// A tag that must be attached to the component that will have its items appended onto the base list.
        /// </summary>
        public readonly string ListToAppendTag;

        /// <summary>
        /// Whether or not to remove the component with the tag <see cref="ListToAppendTag" /> after its items have been added to
        /// the base list.  Defaults to false.
        /// </summary>
        public bool RemoveAppendedComponent;

        /// <summary>
        /// Creates a new generation component that appends lists.
        /// </summary>
        /// <param name="name">Name of this component.</param>
        /// <param name="baseListTag">
        /// A tag that must be attached to the component that will have items from the other list
        /// appended onto it.
        /// </param>
        /// <param name="listToAppendTag">
        /// A tag that must be attached to the component that will have its items appended onto the
        /// base list.
        /// </param>
        public AppendItemLists(string? name, string baseListTag, string listToAppendTag)
            : base(name, (typeof(ItemList<TItem>), baseListTag), (typeof(ItemList<TItem>), listToAppendTag))
        {
            BaseListTag = baseListTag;
            ListToAppendTag = listToAppendTag;

            // Check here since the tags are given in the constructor
            if (BaseListTag == ListToAppendTag)
                throw new InvalidConfigurationException(this, nameof(BaseListTag),
                    $"An ItemList cannot be appended to itself, so the base tag must be different than the {nameof(ListToAppendTag)}.");
        }

        /// <summary>
        /// Creates a new generation component that appends lists.
        /// </summary>
        /// <param name="baseListTag">
        /// A tag that must be attached to the component that will have items from the other list
        /// appended onto it.
        /// </param>
        /// <param name="listToAppendTag">
        /// A tag that must be attached to the component that will have its items appended onto the
        /// base list.
        /// </param>
        public AppendItemLists(string baseListTag, string listToAppendTag)
            : this(null, baseListTag, listToAppendTag)
        { }

        /// <inheritdoc />
        protected override void OnPerform(GenerationContext context)
        {
            // Get required components
            var baseList =
                context.GetComponent<ItemList<TItem>>(BaseListTag)!; // Not null because is in required components list
            var listToAppend =
                context.GetComponent<ItemList<TItem>>(
                    ListToAppendTag)!; // Not null because is in required components list

            // Iterate over each individual position and add to original list, so we keep the original generation step that created it with it.
            foreach (var item in listToAppend.Items)
                baseList.AddItem(item, listToAppend.ItemToStepMapping[item]);
        }
    }
}
