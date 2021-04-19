using System;
using JetBrains.Annotations;

namespace GoRogue.Components.ParentAware
{
    /// <summary>
    /// Abstract base class providing basic functionality and a convention for objects that use component collections to
    /// store components associated with themselves.  If possible, you should inherit from this class when doing this;
    /// If not, it is recommended that you use this class as an example/starting point for your own implementation of
    /// <see cref="IObjectWithComponents"/>.
    /// </summary>
    [PublicAPI]
    public class ObjectWithComponentsBase : IObjectWithComponents
    {
        /// <inheritdoc/>
        public ITaggableComponentCollection GoRogueComponents { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="customComponentCollection">
        /// A custom component collection to use for objects.  If not specified, a <see cref="ComponentCollection"/> is
        /// used.  Typically you will not need to specify this, as a ComponentCollection is sufficient for nearly all
        /// use cases.
        /// </param>
        public ObjectWithComponentsBase(ITaggableComponentCollection? customComponentCollection = null)
        {
            GoRogueComponents = customComponentCollection ?? new ComponentCollection();
            GoRogueComponents.ComponentAdded += On_ComponentAdded;
            GoRogueComponents.ComponentRemoved += On_ComponentRemoved;
        }

        private void On_ComponentAdded(object? s, ComponentChangedEventArgs e)
        {
            if (!(e.Component is IParentAwareComponent c))
                return;

            if (c.Parent != null)
                throw new ArgumentException(
                    $"Components implementing {nameof(IParentAwareComponent)} cannot be added to multiple objects at once.");

            c.Parent = this;
        }

        private void On_ComponentRemoved(object? s, ComponentChangedEventArgs e)
        {
            if (e.Component is IParentAwareComponent c)
                c.Parent = null;
        }
    }
}
