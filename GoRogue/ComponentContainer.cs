using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;

namespace GoRogue
{
    /// <summary>
    /// A class implementing a flexible, type-based system for adding components to objects.  To utilize it, you can either give your
    /// object that you want to have components a ComponentContainer field, or if you wish to avoid the extra field to access components,
    /// you may either inherit from ComponentContainer, or have your object implement <see cref="IHasComponents"/>via a backing field of
    /// type ComponentContainer.
    /// </summary>
    /// <remarks>
    /// The component system is designed to be as efficient as possible at run-time for accessing components and determining if a
    /// ComponentContainer possesses one or more given types of components (or if it has a component with a specific tag), and as well
    /// remains flexible and type-safe.  Components may be of any type, although it is recommended that you _do not_ use value types.
    /// The system also remains accurate with respect to types, even when the components added implement interfaces or have an inheritance
    /// hierarchy.
    /// 
    /// For example, suppose we have the following structure:
    /// <code>
    /// interface ITickable
    /// {
    ///     void Tick();
    /// }
    /// 
    /// public class PlayerTickable : ITickable
    /// {
    ///     public void Tick() => Console.WriteLine("Player ticked!");
    /// }
    /// </code>
    /// 
    /// If we then add a component of type PlayerTickable to an object called obj.  obj.GetComponent&lt;PlayerTickable&gt;() will return
    /// the instance we added, as will obj.GetComponent&lt;ITickable&gt;().  Similarly, obj.HasComponent(typeof(ITickable)) and
    /// obj.HasComponent(typeof(PlayerTickable)) both return true.
    /// </remarks>
    [PublicAPI]
    public class ComponentContainer : IHasTaggableComponents
    {
        private readonly Dictionary<Type, List<object>> _components;

        // Needed for finding tag by item to remove from _tagsToComponents without iteration when components are removed
        private readonly Dictionary<object, string> _componentsToTags;

        // Used for tag-based lookups
        private readonly Dictionary<string, object> _tagsToComponents;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ComponentContainer()
        {
            _components = new Dictionary<Type, List<object>>();
            _componentsToTags = new Dictionary<object, string>();
            _tagsToComponents = new Dictionary<string, object>();
        }

        /// <inheritdoc/>
        public event EventHandler<ComponentChangedEventArgs>? ComponentAdded;

        /// <inheritdoc/>
        public event EventHandler<ComponentChangedEventArgs>? ComponentRemoved;

        /// <inheritdoc/>
        public virtual void AddComponent(object component, string? tag = null)
        {
            var realType = component.GetType();

            if (_components.ContainsKey(realType) && _components[realType].Contains(component))
                throw new ArgumentException("Tried to add the same component instance to an object twice.", nameof(component));

            if (tag != null && _tagsToComponents.ContainsKey(tag))
                throw new ArgumentException($"Tried to add two components with the same tag \"{tag}\" to an instance of an object.");

            foreach (var type in ReflectionAddons.GetTypeTree(realType))
            {
                // Because we remove empty lists in Remove, we can assume that there are no empty lists in the Dictionary
                if (!_components.ContainsKey(type))
                    _components[type] = new List<object>();

                if (component is ISortedComponent sorted)
                    InsertComponent(sorted, _components[type]);
                else
                    _components[type].Add(component);
            }

            if (tag != null)
            {
                _componentsToTags.Add(component, tag);
                _tagsToComponents.Add(tag, component);
            }

            ComponentAdded?.Invoke(this, new ComponentChangedEventArgs(component));
        }

        /// <inheritdoc/>
        public void RemoveComponent(object component) => RemoveComponents(component);

        /// <inheritdoc/>
        public virtual void RemoveComponents(params object[] components)
        {
            foreach (var component in components)
            {
                var realComponentType = component.GetType();
                if (!_components.ContainsKey(realComponentType))
                    throw new ArgumentException($"Tried to remove a component of type {realComponentType}, that did not exist on the object.", nameof(components));

                // Can't be invalid key because above check passed.
                if (!_components[realComponentType].Contains(component))
                    throw new ArgumentException($"Tried to remove a component of type {realComponentType}, that did not exist on the object.", nameof(components));

                foreach (var type in ReflectionAddons.GetTypeTree(realComponentType))
                {
                    // Guaranteed to be a valid key because the above checks passed, so the component does exist.
                    if (_components[type].Count == 1)
                        _components.Remove(type);
                    else
                        _components[type].Remove(component);
                }

                // Add tag as needed
                if (_componentsToTags.ContainsKey(component))
                {
                    string tag = _componentsToTags[component];
                    _componentsToTags.Remove(component);
                    _tagsToComponents.Remove(tag);
                }

                ComponentRemoved?.Invoke(this, new ComponentChangedEventArgs(component));
            }
        }

        /// <inheritdoc/>
        public void RemoveComponent(string tag) => RemoveComponents(tag);

        /// <inheritdoc/>
        public void RemoveComponents(params string[] tags)
        {
            foreach (var tag in tags)
            {
                if (_tagsToComponents.TryGetValue(tag, out object? component))
                    RemoveComponent(component);
                else
                    throw new ArgumentException($"Tried to remove a component with tag {tag}, but no such component exists on the object.");
            }
        }

        /// <inheritdoc/>
        public bool HasComponents(params Type[] componentTypes)
            => componentTypes.All(component => _components.ContainsKey(component));

        /// <inheritdoc/>
        public bool HasComponents(params (Type type, string? tag)[] componentTypesAndTags)
        {
            foreach (var (type, tag) in componentTypesAndTags)
            {
                if (!_components.ContainsKey(type)) // Make sure we have one or more components of the proper types
                    return false;

                if (tag == null) continue;

                if (!_tagsToComponents.ContainsKey(tag)) // Make sure some item of the proper tag exists
                    return false;

                if (!_components[type].Contains(_tagsToComponents[tag])) // Make sure the item with the tag is the correct type
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool HasComponent(Type componentType, string? tag = null) => HasComponents((componentType, tag));

        /// <inheritdoc/>
        public bool HasComponent<T>(string? tag = null) where T : notnull => HasComponents((typeof(T), tag));

        /// <inheritdoc/>
        [return: MaybeNull]
        public T GetComponent<T>(string? tag = null) where T : notnull
        {
            Type typeOfT = typeof(T);

            if (tag == null)
            {
                if (!_components.ContainsKey(typeOfT))
                    return default;

                // We can know there is at least 1 element, because remove functions don't leave empty lists in the Dictionary.
                // Cast will succeed because the dictionary is literally keyed by types and type can't change after compile-time
                return (T)(_components[typeOfT][0]);
            }

            if (!_tagsToComponents.ContainsKey(tag))
                return default;

            if (_tagsToComponents[tag] is T item)
                return item;

            return default;
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetComponents<T>() where T : notnull
        {
            Type typeOfT = typeof(T);

            if (!_components.ContainsKey(typeOfT)) yield break;

            // Cast will succeed because the dictionary is literally keyed by types and type can't change after compile-time
            foreach (var component in _components[typeOfT])
                yield return (T)component;
        }

        // Insert in appropriate order to list based on what its SortOrder is
        private static void InsertComponent(ISortedComponent component, List<object> componentList)
        {
            int insertionPoint = componentList.Count;
            for (int i = 0; i < componentList.Count; i++)
            {
                if (componentList[i] is ISortedComponent sorted)
                {
                    if (sorted.SortOrder > component.SortOrder)
                    {
                        insertionPoint = i;
                        break;
                    }
                }
                else
                {
                    insertionPoint = i;
                    break;
                }
            }

            componentList.Insert(insertionPoint, component);
        }


    }
}
