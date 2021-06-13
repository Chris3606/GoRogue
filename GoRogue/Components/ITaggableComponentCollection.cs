using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace GoRogue.Components
{
    /// <summary>
    /// Interface for a collection of components, of arbitrary types, that can optionally be associated with a unique
    /// tag string.  A concrete implementation is provided; see <see cref="ComponentCollection"/>.
    /// </summary>
    /// <remarks>
    /// Typically, you will not need to implement this yourself, as <see cref="ComponentCollection"/> should be suffice
    /// for most use cases.  Nonetheless, the interface is provided for completeness, or if you have a need to
    /// re-implement it for a corner case of performance.
    ///
    /// It is worthy of note that "null" is used to mean "no particular tag", rather than "no tag whatsoever";
    /// a call to a function that retrieves components that is given a tag of "null" can retrieve any component meeting
    /// the type restrictions, regardless of whether it is associated with a tag.
    /// </remarks>
    [PublicAPI]
    public interface ITaggableComponentCollection : IBasicComponentCollection, IEnumerable<ComponentTagPair>
    {
        /// <inheritdoc />
        void IBasicComponentCollection.Add<T>(T component) where T : class => Add(component);

        /// <inheritdoc />
        [return: MaybeNull]
        T IBasicComponentCollection.GetFirstOrDefault<T>() where T : class => GetFirstOrDefault<T>();

        /// <inheritdoc />
        T IBasicComponentCollection.GetFirst<T>() where T : class => GetFirst<T>();

        /// <inheritdoc />
        bool IBasicComponentCollection.Contains(Type componentType) => Contains(componentType);

        /// <inheritdoc />
        bool IBasicComponentCollection.Contains<T>() where T : class => Contains<T>();

        /// <summary>
        /// Adds the given object as a component, optionally giving it a tag.  Throws ArgumentException if the given
        /// object is already in this collection.
        /// </summary>
        /// <param name="component">Component to add.</param>
        /// <param name="tag">An optional tag to give the component.  Defaults to no tag.</param>
        void Add<T>(T component, string? tag = null) where T : class;

        /// <summary>
        /// Removes the component with the given tag.  Throws ArgumentException if a component with the specified tag
        /// does not exist.
        /// </summary>
        /// <param name="tag">Tag for component to remove.</param>
        void Remove(string tag);

        /// <summary>
        /// Removes the component(s) with the given tags.  Throws ArgumentException if a tag is encountered that does
        /// not have an object associated with it.
        /// </summary>
        /// <param name="tags">Tag(s) of components to remove.</param>
        void Remove(params string[] tags);

        /// <summary>
        /// True if, for each pair specified, there exists a component of the given type with the given tag in the
        /// collection.
        /// </summary>
        /// <remarks>
        /// If "null" is specified as a tag, it indicates no particular tag; eg. any object of the given type will meet
        /// the requirement, regardless of whether or not it has a tag.
        /// </remarks>
        /// <param name="componentTypesAndTags">One or more component types and corresponding tags to check for.</param>
        /// <returns/>
        bool Contains(params ComponentTypeTagPair[] componentTypesAndTags);

        /// <summary>
        /// True if a component of the specified type associated with the specified tag has been added; false otherwise.
        /// </summary>
        /// <remarks>
        /// If "null" is specified for <paramref name="tag"/>, it indicates no particular tag; eg. any object of the
        /// given type will meet the requirement, regardless of whether or not it has a tag.
        /// </remarks>
        /// <param name="componentType">The type of component to check for.</param>
        /// <param name="tag">The tag to check for.  If null is specified, no particular tag is checked for.</param>
        /// <returns/>
        bool Contains(Type componentType, string? tag = null);

        /// <summary>
        /// True if a component of the specified type associated with the specified tag has been added; false otherwise.
        /// </summary>
        /// <remarks>
        /// If "null" is specified for <paramref name="tag"/>, it indicates no particular tag; eg. any object of the
        /// given type will meet the requirement, regardless of whether or not it has a tag.
        /// </remarks>
        /// <typeparam name="T">Type of component to check for.</typeparam>
        /// <param name="tag">The tag to check for.  If null is specified, no particular tag is checked for.</param>
        /// <returns/>
        bool Contains<T>(string? tag = null) where T : class;

        /// <summary>
        /// Gets the component of type T in the collection that has been associated with the given tag.
        /// Throws ArgumentException if no component of the given type associated with the given tag has been added.
        /// </summary>
        /// <remarks>
        /// If "null" is specified for <param name="tag"></param>, it indicates no particular tag; eg. any object of the
        /// given type will meet the requirement.  In this case,the normal sorting priority rules apply; the component
        /// with the lowest <see cref="ISortedComponent.SortOrder"/> is returned.  Among components with equal sort
        /// orders or components that do not implement <see cref="ISortedComponent"/>, the first component of the given
        /// type that was added is returned.
        /// </remarks>
        /// <typeparam name="T">Type of component to retrieve.</typeparam>
        /// <returns/>
        T GetFirst<T>(string? tag = null) where T : class;

        /// <summary>
        /// Gets the component of type T in the collection that has been associated with the given tag.
        /// Returns default(T) if no component of the given type associated with the given tag has been added.
        /// </summary>
        /// <remarks>
        /// If you would instead like to throw an exception if a component of the given type is not found, see
        /// <see cref="GetFirst{T}"/>.
        ///
        /// If "null" is specified for <paramref name="tag"/>, it indicates no particular tag; eg. any object of the
        /// given type will meet the requirement.  In this case,the normal sorting priority rules apply; the component
        /// with the lowest <see cref="ISortedComponent.SortOrder"/> is returned.  Among components with equal sort
        /// orders or components that do not implement <see cref="ISortedComponent"/>, the first component of the given
        /// type that was added is returned.
        /// </remarks>
        /// <param name="tag">Type of component to retrieve.</param>
        /// <returns/>
        [return: MaybeNull]
        T GetFirstOrDefault<T>(string? tag = null) where T : class;
    }
}
