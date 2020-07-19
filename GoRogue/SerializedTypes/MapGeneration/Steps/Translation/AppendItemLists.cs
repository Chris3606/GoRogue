using System;
using System.Diagnostics.CodeAnalysis;
using GoRogue.MapGeneration.Steps.Translation;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.MapGeneration.Steps.Translation
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="AppendItemLists{TItem}"/>
    /// </summary>
    [PublicAPI]
    [Serializable]
    [SuppressMessage("ReSharper", "CA1815")] // Type should only be used for serialization
    public struct AppendItemListsSerialized<TItem> where TItem : notnull
    {
        /// <summary>
        /// Name of the generation step.
        /// </summary>
        public string Name;

        /// <summary>
        /// A tag that must be attached to the component that will have items from the other list appended onto it.
        /// </summary>
        public string BaseListTag;

        /// <summary>
        /// A tag that must be attached to the component that will have its items appended onto the base list.
        /// </summary>
        public string ListToAppendTag;

        /// <summary>
        /// Whether or not to remove the component with the tag <see cref="ListToAppendTag" /> after its items have been
        /// added to the base list.
        /// </summary>
        public bool RemoveAppendedComponent;

        /// <summary>
        /// Converts <see cref="AppendItemLists{TItem}"/> to <see cref="AppendItemListsSerialized{TItem}"/>.
        /// </summary>
        /// <param name="step"/>
        /// <returns/>
        public static implicit operator AppendItemListsSerialized<TItem>(AppendItemLists<TItem> step)
            => FromAppendItemLists(step);

        /// <summary>
        /// Converts <see cref="AppendItemListsSerialized{TItem}"/> to <see cref="AppendItemLists{TItem}"/>.
        /// </summary>
        /// <param name="step"/>
        /// <returns/>
        public static implicit operator AppendItemLists<TItem>(AppendItemListsSerialized<TItem> step)
            => step.ToAppendItemLists();

        /// <summary>
        /// Converts <see cref="AppendItemLists{TItem}"/> to <see cref="AppendItemListsSerialized{TItem}"/>.
        /// </summary>
        /// <param name="step"/>
        /// <returns/>
        [SuppressMessage("ReSharper", "CA1000")] // Static method is required to implement implicit ops
        public static AppendItemListsSerialized<TItem> FromAppendItemLists(AppendItemLists<TItem> step)
            => new AppendItemListsSerialized<TItem>
            {
                Name = step.Name,
                BaseListTag = step.BaseListTag,
                ListToAppendTag = step.ListToAppendTag,
                RemoveAppendedComponent = step.RemoveAppendedComponent
            };

        /// <summary>
        /// Converts <see cref="AppendItemListsSerialized{TItem}"/> to <see cref="AppendItemLists{TItem}"/>.
        /// </summary>
        /// <returns/>
        public AppendItemLists<TItem> ToAppendItemLists()
            => new AppendItemLists<TItem>(Name, BaseListTag, ListToAppendTag)
            {
                RemoveAppendedComponent = RemoveAppendedComponent
            };
    }
}
