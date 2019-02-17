using System;
using System.Collections.Generic;

namespace GoRogue.GameFramework
{
	/// <summary>
	/// A class implementing a flexible, type-based system for adding components to objects.  To utilize it, you can either give your
	/// object that you want to have components a ComponentContainer field, or if you wish to avoid the extra field to access components,
	/// you may have your object implement IHasComponents via a backing field of type ComponentContainer (see GameFramework.GameObject) for 
	/// an example).
	/// </summary>
	/// <remarks>
	/// The component system is designed to be as efficient as possible at run-time for accessing components and determining if a
	/// ComponentContainer possesses one or more given types of components, and as well remains flexible and type-safe.
	/// Components may be of any type, although it is recommended that you _not_ use value-types.  The system also remains type-safe, even when
	/// the components added implement interfaces or have an inheritance heirarchy.
	/// 
	/// For example, suppose we have the following structure:
	/// <code>
	/// interface ITickable
	/// {
	///     void Tick();
	/// }
	/// 
	/// public class PlayerTickable
	/// {
	///     public void Tick() => Console.WriteLine("Player ticked!");
	/// }
	/// </code>
	/// 
	/// Let's say we add a component of type PlayerTickable to an object called obj.  obj.GetComponent&lt;PlayerTickable&gt;() then returns
	/// the instance we added, as does obj.GetComponent&lt;ITickable&gt;().  Similarly, obj.HasComponent(typeof(ITickable)) and
	/// obj.HasComponent(typeof(PlayerTickable)) both return true.
	/// </remarks>
	public class ComponentContainer : IHasComponents
	{
		// TODO: Test with Type instead of string for performance, if it becomes an issue.  My suspicion is that string GetHashCode
		// will prove to yield slightly better performance.
		private Dictionary<string, List<object>> _components;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ComponentContainer()
		{
			_components = new Dictionary<string, List<object>>();
		}

		/// <summary>
		/// Adds the given object as a component.  Throws an exception if that specific instance is already in this ComponentContainer.
		/// </summary>
		/// <param name="component">Component to add.</param>
		public void AddComponent(object component)
		{
			var realType = component.GetType();
			if (_components.ContainsKey(realType.FullName) && _components[realType.FullName].Contains(component))
				throw new ArgumentException($"Tried to add the same component instance to a {nameof(ComponentContainer)} twice.");

			foreach (var type in GetTree(component))
			{
				// Because we remove empty lists in Remove, we can assume that there are no empty lists in the Dictionary
				if (!_components.ContainsKey(type.FullName))
					_components[type.FullName] = new List<object>();

				_components[type.FullName].Add(component);
			}
		}

		/// <summary>
		/// Removes the given component.  Throws an exception if the component does not exist in the ComponentContainer.
		/// </summary>
		/// <param name="component">Component to remove.</param>
		public void RemoveComponent(object component) => RemoveComponents(component);

		/// <summary>
		/// Removes the given component(s).  Throws an exception if a component given does not exist in the ComponentContainer.
		/// </summary>
		/// <param name="components">One or more component instances to remove.</param>
		public void RemoveComponents(params object[] components)
		{
			foreach (var component in components)
			{
				var componentFullName = component.GetType().FullName;
				if (!_components.ContainsKey(componentFullName))
					throw new ArgumentException($"Tried to remove a component of type {componentFullName}, that did not exist on the object.", nameof(component));

				// Can't be invalid key because above check passed.
				if (!_components[componentFullName].Contains(component))
					throw new ArgumentException($"Tried to remove a component of type {componentFullName}, that did not exist on the object.", nameof(component));

				foreach (var type in GetTree(component))
				{
					// Guaranteed to be a valid key because the above checks passed, so the component does exist.
					if (_components[type.FullName].Count == 1)
						_components.Remove(type.FullName);
					else
						_components[type.FullName].Remove(component);
				}
			}
		}

		/// <summary>
		/// Returns whether or not the current ComponentContainer has all the given types of components.  Types may be specified by
		/// using typeof(MyComponentType)
		/// </summary>
		/// <param name="componentTypes">One or more component types to check for.</param>
		/// <returns>True if the ComponentContainer has at least one component of each specified type, false otherwise.</returns>
		public bool HasComponents(params Type[] componentTypes)
		{
			foreach (var component in componentTypes)
				if (!_components.ContainsKey(component.FullName))
					return false;

			return true;
		}

		/// <summary>
		/// Returns whether or not the current ComponentContainer has at least one component of the specified type.  Type may be specified
		/// by using typeof(MyComponentType).
		/// </summary>
		/// <param name="componentType">The type of component to check for.</param>
		/// <returns>True if the ComponentContainer has at least one component of the specified type, false otherwise.</returns>
		public bool HasComponent(Type componentType) => HasComponents(componentType);

		/// <summary>
		/// Returns whether or not the current ComponentContainer has at least one component of type T.
		/// </summary>
		/// <typeparam name="T">Type of component to check for.</typeparam>
		/// <returns>True if the ComponentContainer has at least one component of the specified type, false otherwise.</returns>
		public bool HasComponent<T>() => HasComponents(typeof(T));

		/// <summary>
		/// Gets the first component of type T that was attached to the given object, or default(T) if no component of that type has
		/// been added.
		/// </summary>
		/// <typeparam name="T">Type of component to retrieve.</typeparam>
		/// <returns>The first component of Type T that was added to the ComponentContainer, or default(T) if no
		/// components of the given type have been added.</returns>
		public T GetComponent<T>()
		{
			Type typeOfT = typeof(T);

			if (!_components.ContainsKey(typeOfT.FullName))
				return default(T);

			// We can know there is at least 1 element, because remove functions don't leave empty lists in the Dictionary.
			// Cast will succeed because the dicationary is literally keyed by types and type can't change after compile-time
			return (T)(_components[typeOfT.FullName][0]);
		}

		/// <summary>
		/// Gets all components of type T that was attached to the given object.
		/// </summary>
		/// <typeparam name="T">Type of components to retrieve.</typeparam>
		/// <returns>All components of Type T that are in the ComponentContainer.</returns>
		public IEnumerable<T> GetComponents<T>()
		{
			Type typeOfT = typeof(T);

			if (_components.ContainsKey(typeOfT.FullName))
			{
				// Cast will succeed because the dicationary is literally keyed by types and type can't change after compile-time
				foreach (var component in _components[typeOfT.FullName])
					yield return (T)component;
			}
		}

		// Gets the entire inheritance tree for the run-time actual type of the given component,
		// including the fully derived class, all classes it implements (down to System.Object), and interfaces
		// it implements.
		private static IEnumerable<Type> GetTree(object component)
		{
			Type classType = component.GetType();

			while (classType != null)
			{
				yield return classType;
				classType = classType.BaseType;
			}

			foreach (Type implementedInterface in component.GetType().GetInterfaces())
				yield return implementedInterface;
		}
	}
}
