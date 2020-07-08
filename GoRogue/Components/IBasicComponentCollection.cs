using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// Interface for a collection of components, of arbitrary types.  A concrete implementation is provided;
    /// see <see cref="ComponentCollection"/>.
    /// </summary>
    /// <remarks>
    /// Typically, you will not need to implement this yourself, as <see cref="ComponentCollection"/> should be suffice
    /// for most use cases.  Nonetheless, the interface is provided for completeness, or simplicity if you want to
    /// expose a simplified API for a <see cref="ComponentCollection"/>, without the tag parameters.
    /// </remarks>
    [PublicAPI]
    public interface IBasicComponentCollection
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
        /// Adds the given object to the collection.  Throws ArgumentException if the specific instance has already
        /// been added.
        /// </summary>
        /// <param name="component">Component to add.</param>
        void Add(object component);

        /// <summary>
        /// Gets the component of type T in the collection that has the lowest <see cref="ISortedComponent.SortOrder"/>.
        /// Among components with equal sort orders or components that do not implement <see cref="ISortedComponent"/>,
        /// the first component of the given type that was added is returned.
        ///
        /// Returns default(T) if no component of the given type has been added.
        /// </summary>
        /// <remarks>
        /// If you would instead like to throw an exception if a component of the given type is not found, see
        /// <see cref="GetFirst{T}"/>.
        /// </remarks>
        /// <typeparam name="T">Type of component to retrieve.</typeparam>
        /// <returns/>
        [return: MaybeNull]
        T GetFirstOrDefault<T>() where T : notnull;

        /// <summary>
        /// Gets the component of type T in the collection that has the lowest <see cref="ISortedComponent.SortOrder"/>.
        /// Among components with equal sort orders or components that do not implement <see cref="ISortedComponent"/>,
        /// the first component of the given type that was added is returned.
        ///
        /// Throws ArgumentException if no component of the given type has been added.
        /// </summary>
        /// <typeparam name="T">Type of component to retrieve.</typeparam>
        /// <returns/>
        T GetFirst<T>() where T : notnull;

        /// <summary>
        /// Gets all components of type T that have been added, with components having a lower
        /// <see cref="ISortedComponent.SortOrder"/> being returned first.  Components that do not implement
        /// <see cref="ISortedComponent"/> are returned after any that do.  Among components with equal sort orders
        /// or components that do not implement <see cref="ISortedComponent"/>, components are returned in the order
        /// they were added.
        /// </summary>
        /// <typeparam name="T">Type of components to retrieve.</typeparam>
        /// <returns/>
        IEnumerable<T> GetAll<T>() where T : notnull;

        /// <summary>
        /// True if any component of the specified type has been added; false otherwise.
        /// </summary>
        /// <param name="componentType">The type of component to check for.</param>
        /// <returns/>
        bool Contains(Type componentType);

        /// <summary>
        /// True if any component of the specified type has been added; false otherwise.
        /// </summary>
        /// <typeparam name="T">Type of component to check for.</typeparam>
        /// <returns/>
        bool Contains<T>() where T : notnull;

        /// <summary>
        /// True if at least one component of each type specified has been added; false otherwise.
        /// </summary>
        /// <param name="componentTypes">One or more component types to check for.</param>
        /// <returns/>
        bool Contains(params Type[] componentTypes);

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
    }
}
