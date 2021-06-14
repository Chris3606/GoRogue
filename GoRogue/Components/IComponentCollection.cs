using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GoRogue.Components.ParentAware;
using JetBrains.Annotations;

namespace GoRogue.Components
{
    /// <summary>
    /// Arguments for events fired when components are added/removed from a component collection.
    /// </summary>
    [PublicAPI]
    public class ComponentChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="component">The component that was added/removed.</param>
        public ComponentChangedEventArgs(object component)
        {
            Component = component;
        }

        /// <summary>
        /// The component that was added or removed.
        /// </summary>
        public readonly object Component;
    }

    /// <summary>
    /// Interface for a collection of components, of arbitrary types, that can optionally be associated with an arbitrary,
    /// unique tag string.  A concrete implementation is provided; see <see cref="ComponentCollection"/>.
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
    public interface IComponentCollection : IEnumerable<ComponentTagPair>
    {
        /// <summary>
        /// Fired when a component is added to the collection.
        /// </summary>
        public event EventHandler<ComponentChangedEventArgs>? ComponentAdded;

        /// <summary>
        /// Fired when a component is removed from the collection.
        /// </summary>
        public event EventHandler<ComponentChangedEventArgs>? ComponentRemoved;

        /// <summary>
        /// Number of components attached.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Object automatically set as the parent for any <see cref="IParentAwareComponent"/> added to the collection.
        /// Useful if you have components associated with an object.  This defaults to null, and if its value is null,
        /// no parent is set when components are added.
        /// </summary>
        public IObjectWithComponents? ParentForAddedComponents { get; set; }

        /// <summary>
        /// Removes all components from the collection.
        /// </summary>
        void Clear();

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
        /// Removes the given component from the collection.  Throws ArgumentException if the component isn't
        /// in the collection.
        /// </summary>
        /// <param name="component">Component to remove.</param>
        void Remove(object component);

        /// <summary>
        /// Removes the given component(s) from the collection.  Throws ArgumentException if a component given
        /// isn't in the collection.
        /// </summary>
        /// <param name="components">One or more components to remove.</param>
        void Remove(params object[] components);

        /// <summary>
        /// True if at least one component of each type specified has been added; false otherwise.
        /// </summary>
        /// <param name="componentTypes">One or more component types to check for.</param>
        /// <returns/>
        bool Contains(params Type[] componentTypes);

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
        /// True if a component of the specified type, and associated with the specified tag if one is specified,
        /// has been added; false otherwise.
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
        /// True if a component of the specified type, and associated with the specified tag if one is specified, has
        /// been added; false otherwise.
        /// </summary>
        /// <remarks>
        /// If "null" is specified for <paramref name="tag"/>, it indicates no particular tag; eg. ANY object of the
        /// given type will meet the requirement, whether that object has a tag or not.
        /// </remarks>
        /// <typeparam name="T">Type of component to check for.</typeparam>
        /// <param name="tag">The tag to check for.  If null is specified, no particular tag is checked for.</param>
        /// <returns/>
        bool Contains<T>(string? tag = null) where T : class;

        /// <summary>
        /// Gets the component of type T in the collection that has been associated with the given tag, or the
        /// component of type T with the lowest <see cref="ISortedComponent.SortOrder"/> if no tag is specified.
        /// </summary>
        /// <remarks>
        /// Among components with equal sort orders or components that do not implement <see cref="ISortedComponent"/>,
        /// the first component of the given type that was added is returned.
        ///
        /// Throws ArgumentException if no component of the given type associated with the given tag has been added.
        ///
        /// If "null" is specified for <paramref name="tag"/>, it indicates no particular tag; eg. ANY object of the
        /// given type will meet the requirement, whether that object has a tag or not.
        /// </remarks>
        /// <typeparam name="T">Type of component to retrieve.</typeparam>
        /// <param name="tag">Tag for component to retrieve.  If null is specified, no particular tag is checked for.</param>
        /// <returns/>
        T GetFirst<T>(string? tag = null) where T : class;

        /// <summary>
        /// Gets the component of type T in the collection that has been associated with the given tag, or the
        /// component of type T with the lowest <see cref="ISortedComponent.SortOrder"/> if no tag is specified.
        /// </summary>
        /// <remarks>
        /// Among components with equal sort orders or components that do not implement <see cref="ISortedComponent"/>,
        /// the first component of the given type that was added is returned.
        ///
        /// Returns default(T) if no component of the given type/associated with the given tag has been added.
        /// If you would instead like to throw an exception if a component of the given type is not found, see
        /// <see cref="GetFirst{T}"/>.
        ///
        /// If "null" is specified for <paramref name="tag"/>, it indicates no particular tag; eg. ANY object of the
        /// given type will meet the requirement, whether that object has a tag or not.
        /// </remarks>
        /// <typeparam name="T">Type of component to retrieve.</typeparam>
        /// <param name="tag">Tag for component to retrieve.  If null is specified, no particular tag is checked for.</param>
        /// <returns/>
        [return: MaybeNull]
        T GetFirstOrDefault<T>(string? tag = null) where T : class;

        /// <summary>
        /// Gets all components of type T that have been added, with components having a lower
        /// <see cref="ISortedComponent.SortOrder"/> being returned first.  Components that do not implement
        /// <see cref="ISortedComponent"/> are returned after any that do.  Among components with equal sort orders
        /// or components that do not implement <see cref="ISortedComponent"/>, components are returned in the order
        /// they were added.
        /// </summary>
        /// <typeparam name="T">Type of components to retrieve.</typeparam>
        /// <returns/>
        IEnumerable<T> GetAll<T>() where T : class;
    }
}
