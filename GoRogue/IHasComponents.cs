using System;
using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Interface for an object that has components that can be added, removed, checked for, and retrieved by type.  Typically,
	/// you would implement this via a backing field of type <see cref="ComponentContainer"/>, which implements the logic for
	/// these functions.
	/// </summary>
	public interface IHasComponents
	{
		/// <summary>
		/// Adds the given object as a component.  Throws ArgumentException if the specific instance has already been added.
		/// </summary>
		/// <param name="component">Component to add.</param>
		void AddComponent(object component);

		/// <summary>
		/// Gets the first component of type T that was added, or default(T) if no component of that type has
		/// been added.
		/// </summary>
		/// <typeparam name="T">Type of component to retrieve.</typeparam>
		/// <returns>The first component of Type T that was attached, or default(T) if no components of the given type
		/// have been attached.</returns>
		T GetComponent<T>();

		/// <summary>
		/// Gets all components of type T that are added.
		/// </summary>
		/// <typeparam name="T">Type of components to retrieve.</typeparam>
		/// <returns>All components of Type T that are attached.</returns>
		IEnumerable<T> GetComponents<T>();

		/// <summary>
		/// Returns whether or not there is at least one component of the specified type attached.  Type may be specified
		/// by using typeof(MyComponentType).
		/// </summary>
		/// <param name="componentType">The type of component to check for.</param>
		/// <returns>True if the implementer has at least one component of the specified type, false otherwise.</returns>
		bool HasComponent(Type componentType);

		/// <summary>
		/// Returns whether or not there is at least one component of type T attached.
		/// </summary>
		/// <typeparam name="T">Type of component to check for.</typeparam>
		/// <returns>True if the implemented has at least one component of the specified type attached, false otherwise.</returns>
		bool HasComponent<T>();

		/// <summary>
		/// Returns whether or not the implementer has at least one of all of the given types of components attached.  Types may be specified by
		/// using typeof(MyComponentType)
		/// </summary>
		/// <param name="componentTypes">One or more component types to check for.</param>
		/// <returns>True if the implementer has at least one component of each specified type, false otherwise.</returns>
		bool HasComponents(params Type[] componentTypes);

		/// <summary>
		/// Removes the given component.  Throws an exception if the component isn't attached.
		/// </summary>
		/// <param name="component">Component to remove.</param>
		void RemoveComponent(object component);

		/// <summary>
		/// Removes the given component(s).  Throws an exception if a component given isn't attached.
		/// </summary>
		/// <param name="components">One or more component instances to remove.</param>
		void RemoveComponents(params object[] components);
	}
}
