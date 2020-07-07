using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace GoRogue
{
    /// <summary>
    /// Arguments for events fired when components are added/removed from an object.
    /// </summary>
    [PublicAPI]
    public class ComponentChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
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
    /// Interface for an object that has components that can be added, removed, checked for, and retrieved by type.  Typically,
    /// you would implement this via a backing field of type <see cref="ComponentContainer" />, which implements the logic for
    /// these functions.
    /// </summary>
    [PublicAPI]
    public interface IHasComponents
    {
        /// <summary>
        /// Fired when a component is added to the component container.
        /// </summary>
        public event EventHandler<ComponentChangedEventArgs>? ComponentAdded;

        /// <summary>
        /// Fired when a component is removed from the component container.
        /// </summary>
        public event EventHandler<ComponentChangedEventArgs>? ComponentRemoved;

        /// <summary>
        /// Adds the given object as a component.  Throws ArgumentException if the specific instance has already been added.
        /// </summary>
        /// <param name="component">Component to add.</param>
        void Add(object component);

        /// <summary>
        /// Gets the first component of type T that was added, or default(T) if no component of that type has
        /// been added.
        /// </summary>
        /// <typeparam name="T">Type of component to retrieve.</typeparam>
        /// <returns>
        /// The first component of Type T that was attached, or default(T) if no components of the given type
        /// have been attached.
        /// </returns>
        [return: MaybeNull]
        T GetFirstOrDefault<T>() where T : notnull;

        /// <summary>
        /// Gets the first component of type T that was added, or throws InvalidOperationException if
        /// no component of that type has been added.
        /// </summary>
        /// <typeparam name="T">Type of component to retrieve.</typeparam>
        /// <returns>The first component of Type T that was attached.</returns>
        T GetFirst<T>() where T : notnull;

        /// <summary>
        /// Gets all components of type T that are added.
        /// </summary>
        /// <typeparam name="T">Type of components to retrieve.</typeparam>
        /// <returns>All components of Type T that are attached.</returns>
        IEnumerable<T> GetAll<T>() where T : notnull;

        /// <summary>
        /// Returns whether or not there is at least one component of the specified type attached.  Type may be specified
        /// by using typeof(MyComponentType).
        /// </summary>
        /// <param name="componentType">The type of component to check for.</param>
        /// <returns>True if the implementer has at least one component of the specified type, false otherwise.</returns>
        bool Contains(Type componentType);

        /// <summary>
        /// Returns whether or not there is at least one component of type T attached.
        /// </summary>
        /// <typeparam name="T">Type of component to check for.</typeparam>
        /// <returns>True if the implemented has at least one component of the specified type attached, false otherwise.</returns>
        bool Contains<T>() where T : notnull;

        /// <summary>
        /// Returns whether or not the implementer has at least one of all of the given types of components attached.  Types may be
        /// specified by
        /// using typeof(MyComponentType)
        /// </summary>
        /// <param name="componentTypes">One or more component types to check for.</param>
        /// <returns>True if the implementer has at least one component of each specified type, false otherwise.</returns>
        bool Contains(params Type[] componentTypes);

        /// <summary>
        /// Removes the given component.  Throws an exception if the component isn't attached.
        /// </summary>
        /// <param name="component">Component to remove.</param>
        void Remove(object component);

        /// <summary>
        /// Removes the given component(s).  Throws an exception if a component given isn't attached.
        /// </summary>
        /// <param name="components">One or more component instances to remove.</param>
        void Remove(params object[] components);
    }
}
