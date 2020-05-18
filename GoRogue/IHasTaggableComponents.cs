using System;
using System.Diagnostics.CodeAnalysis;

namespace GoRogue
{
    /// <summary>
    /// Interface for an object that has components that can be added, removed, checked for, and retrieved by type and a unique "tag"
    /// string.  Typically, you would implement this via a backing field of type <see cref="ComponentContainer"/>, which implements the
    /// logic for these functions.
    /// </summary>
    public interface IHasTaggableComponents : IHasComponents
    {
        /// <summary>
        /// Adds the given object as a component, optionally giving it a tag.  Throws an exception if that specific
        /// instance is already in this ComponentContainer.
        /// </summary>
        /// <param name="component">Component to add.</param>
        /// <param name="tag">An optional tag to give the component.  Defaults to no tag.</param>
        void AddComponent(object component, string? tag = null);

        void RemoveComponent(string tag);

        void RemoveComponents(params string[] tags);

        bool HasComponents(params (Type type, string? tag)[] componentTypesAndTags);

        bool HasComponent(Type componentType, string? tag = null);

        bool HasComponent<T>(string? tag = null) where T : notnull;

        T GetComponent<T>(string? tag = null) where T : notnull;

        /// <inheritdoc/>
        void IHasComponents.AddComponent(object component) => AddComponent(this, null);

        /// <inheritdoc/>
        [return: MaybeNull]
        T IHasComponents.GetComponent<T>() => GetComponent<T>(null);

        /// <inheritdoc/>
        bool IHasComponents.HasComponent(Type componentType) => HasComponent(componentType, null);

        /// <inheritdoc/>
        bool IHasComponents.HasComponent<T>() => HasComponent<T>(null);
    }
}
