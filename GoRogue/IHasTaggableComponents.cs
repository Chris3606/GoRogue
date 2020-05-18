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

        /// <summary>
        /// Removes the component with the given tag.  Throws an exception if a component with the specified tag does not exist. 
        /// </summary>
        /// <param name="tag">Tag for component to remove.</param>
        void RemoveComponent(string tag);

        /// <summary>
        /// Removes the component(s) with the given tags.  Throws an exception if a tag is encountered that does not have an object associated with it.
        /// </summary>
        /// <param name="tags">Tag(s) of components to remove.</param>
        void RemoveComponents(params string[] tags);

        /// <summary>
        /// Returns whether or not the implementer has at least one of all of the given types of components attached, optionally with the specified tags.
        /// Types may be specified by using typeof(MyComponentType).  If null is specified for any tag, any component meeting the type requirement will qualify.
        /// </summary>
        /// <param name="componentTypesAndTags">One or more component types to check for, and corresponding tags.</param>
        /// <returns>True if the implementer has at least one component of each specified type that has the tag given, false otherwise.</returns>    
        bool HasComponents(params (Type type, string? tag)[] componentTypesAndTags);

        /// <summary>
        /// Returns whether or not there is at least one component of the specified type attached, that has the given tag associated with it.  Type may be
        /// specified by using typeof(MyComponentType).  If null is specified for the tag, any component meeting the type requirement will qualify.
        /// </summary>
        /// <param name="componentType">The type of component to check for.</param>
        /// <param name="tag">A tag to check for.  If null is specified, any component meeting the type requirement will qualify.</param>
        /// <returns>True if the implementer has at least one component of the specified type, and has the specified tag if one is specified; false otherwise.</returns>
        bool HasComponent(Type componentType, string? tag = null);

        /// <summary>
        /// Returns whether or not there is at least one component of type T attached, that has the given tag associated with it if one is specified.  If no tag
        /// is specified, any component of type T will qualify.
        /// </summary>
        /// <typeparam name="T">Type of component to check for.</typeparam>
        /// <param name="tag">The tag to check for.  If null is specified, no particular tag is checked for.</param>
        /// <returns>True if the implemented has at least one component of the specified type attached, false otherwise.</returns>
        bool HasComponent<T>(string? tag = null) where T : notnull;

        /// <summary>
        /// Gets the first component of type T that was added andhas the given tag associated with it, if one is specified, or default(T) if no component of that type
        /// that meets the tag and and type requirements has been added. If null is specified for the tag, no particular tag is checked for; the function will get the first
        /// component it encounters that meets the type requirement.
        /// </summary>
        /// <typeparam name="T">Type of component to retrieve.</typeparam>
        /// <returns>The first component of Type T that was attached, or default(T) if no components of the given type
        /// have been attached.</returns>
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
