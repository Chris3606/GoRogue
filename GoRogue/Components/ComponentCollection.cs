using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;

namespace GoRogue.Components
{
    /// <summary>
    /// A class implementing a flexible, type-based system for keeping track of components that are added to objects.
    /// A ComponentCollection can simply be added as a member of an object that needs components attached to it, then
    /// used by a list.
    /// </summary>
    /// <remarks>
    /// This collection allows you to add arbitrary objects to it, optionally associated with a string tag.  It then
    /// allows you to very efficiently query whether or not an object has a component of a given type and tag, and
    /// retrieve that object, in a type-safe way.  It handles cases where multiple objects of a single type are added,
    /// as well as cases where inheritance and/or interfaces are involved.
    ///
    /// While components may be of any type, thanks to value-type defensive copying it is highly recommended that you
    /// refrain from using value types as components.
    ///
    /// You may also control the order/priority with which components are retrieved.  Components are retrieved in the
    /// order they were added by default.  If you would like more control than this, you may have your components
    /// implement <see cref="ISortedComponent"/>.  Although implementing this interface is not required, any components
    /// that do implement it are returned before any components that do not.  Similarly, components with a lower sort
    /// order are returned before those with higher sort orders.
    ///
    /// It is worthy of note that, when given as a tag parameter, "null" is used to mean "no particular tag", rather
    /// than "no tag whatsoever"; a call to a function that retrieves components that is given a tag of "null" can
    /// retrieve any component meeting the type restrictions, regardless of whether it is associated with a tag.
    /// </remarks>
    [PublicAPI]
    public class ComponentCollection : ITaggableComponentCollection
    {
        private readonly Dictionary<Type, List<object>> _components;

        // Needed for finding tag by item to remove from _tagsToComponents without iteration when components are
        // removed.  Also used a distinct list of all components
        private readonly Dictionary<object, string?> _componentsToTags;

        // Used for tag-based lookups
        private readonly Dictionary<string, object> _tagsToComponents;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ComponentCollection()
        {
            _components = new Dictionary<Type, List<object>>();
            _componentsToTags = new Dictionary<object, string?>();
            _tagsToComponents = new Dictionary<string, object>();
        }

        /// <inheritdoc />
        public event EventHandler<ComponentChangedEventArgs>? ComponentAdded;

        /// <inheritdoc />
        public event EventHandler<ComponentChangedEventArgs>? ComponentRemoved;

        /// <inheritdoc />
        public void Add(object component, string? tag = null)
        {
            var realType = component.GetType();

            if (_components.ContainsKey(realType) && _components[realType].Contains(component))
                throw new ArgumentException("Tried to add the same component instance to an object twice.",
                    nameof(component));

            if (tag != null && _tagsToComponents.ContainsKey(tag))
                throw new ArgumentException(
                    $"Tried to add two components with the same tag \"{tag}\" to an instance of an object.");

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

            _componentsToTags.Add(component, tag);

            if (tag != null)
                _tagsToComponents.Add(tag, component);

            ComponentAdded?.Invoke(this, new ComponentChangedEventArgs(component));
        }

        /// <inheritdoc />
        public void Remove(object component) => Remove(new[] { component });

        /// <inheritdoc />
        public void Remove(params object[] components)
        {
            foreach (var component in components)
            {
                var realComponentType = component.GetType();
                if (!_components.ContainsKey(realComponentType))
                    throw new ArgumentException(
                        $"Tried to remove a component of type {realComponentType}, that did not exist on the object.",
                        nameof(components));

                // Can't be invalid key because above check passed.
                if (!_components[realComponentType].Contains(component))
                    throw new ArgumentException(
                        $"Tried to remove a component of type {realComponentType}, that did not exist on the object.",
                        nameof(components));

                foreach (var type in ReflectionAddons.GetTypeTree(realComponentType))
                    // Guaranteed to be a valid key because the above checks passed, so the component does exist.
                    if (_components[type].Count == 1)
                        _components.Remove(type);
                    else
                        _components[type].Remove(component);

                // Remove from tags lists as needed
                string? tag = _componentsToTags[component];
                _componentsToTags.Remove(component);
                if (tag != null)
                    _tagsToComponents.Remove(tag);

                ComponentRemoved?.Invoke(this, new ComponentChangedEventArgs(component));
            }
        }

        /// <inheritdoc />
        public void Remove(string tag) => Remove(new[] { tag });

        /// <inheritdoc />
        public void Remove(params string[] tags)
        {
            foreach (var tag in tags)
                if (_tagsToComponents.TryGetValue(tag, out var component))
                    Remove(component);
                else
                    throw new ArgumentException(
                        $"Tried to remove a component with tag {tag}, but no such component exists on the object.");
        }

        /// <inheritdoc />
        public bool Contains(params Type[] componentTypes)
            => componentTypes.All(component => _components.ContainsKey(component));

        /// <inheritdoc />
        public bool Contains(params (Type type, string? tag)[] componentTypesAndTags)
        {
            foreach (var (type, tag) in componentTypesAndTags)
            {
                if (!_components.ContainsKey(type)) // Make sure we have one or more components of the proper types
                    return false;

                if (tag == null) continue;

                if (!_tagsToComponents.ContainsKey(tag)) // Make sure some item of the proper tag exists
                    return false;

                if (!_components[type].Contains(_tagsToComponents[tag])
                ) // Make sure the item with the tag is the correct type
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool Contains(Type componentType, string? tag = null) => Contains((componentType, tag));

        /// <inheritdoc />
        public bool Contains<T>(string? tag = null) where T : notnull => Contains((typeof(T), tag));

        /// <inheritdoc />
        [return: MaybeNull]
        public T GetFirstOrDefault<T>(string? tag = null) where T : notnull
        {
            Type typeOfT = typeof(T);

            if (tag == null)
            {
                if (!_components.ContainsKey(typeOfT))
                    return default;

                // We can know there is at least 1 element, because remove functions don't leave empty lists in the Dictionary.
                // Cast will succeed because the dictionary is literally keyed by types and type can't change after compile-time
                return (T)_components[typeOfT][0];
            }

            if (!_tagsToComponents.ContainsKey(tag))
                return default;

            if (_tagsToComponents[tag] is T item)
                return item;

            return default;
        }

        /// <inheritdoc />
        public T GetFirst<T>(string? tag = null) where T : notnull
        {
            Type typeOfT = typeof(T);

            if (tag == null)
            {
                if (!_components.ContainsKey(typeOfT))
                    throw new ArgumentException($"No component of type {nameof(T)} has been added to the {nameof(ComponentCollection)}.");

                // We can know there is at least 1 element, because remove functions don't leave empty lists in the Dictionary.
                // Cast will succeed because the dictionary is literally keyed by types and type can't change after compile-time
                return (T)_components[typeOfT][0];
            }

            if (!_tagsToComponents.ContainsKey(tag))
                throw new ArgumentException($"No component with the tag {tag} has been added to the {nameof(ComponentCollection)}.", nameof(tag));

            if (_tagsToComponents[tag] is T item)
                return item;

            throw new ArgumentException($"Component of type {nameof(T)} with tag {tag} was requested from the {nameof(ComponentCollection)}, but the component with that tag is not of that type.", nameof(tag));
        }

        /// <inheritdoc />
        public IEnumerable<T> GetAll<T>() where T : notnull
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
            var insertionPoint = componentList.Count;
            for (var i = 0; i < componentList.Count; i++)
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

            componentList.Insert(insertionPoint, component);
        }

        /// <summary>
        /// Returns all components paired with their tags.  Ordered with respect to sorted components.
        /// </summary>
        /// <returns/>
        public IEnumerator<(object component, string? tag)> GetEnumerator()
        {
            var ordered = _componentsToTags.OrderBy(
                val => val.Key is ISortedComponent sorted ? sorted.SortOrder : uint.MaxValue);

            foreach (var (component, tag) in ordered)
                yield return (component, tag);
        }

        /// <summary>
        /// Returns components paired with their tags.
        /// </summary>
        /// <returns/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
