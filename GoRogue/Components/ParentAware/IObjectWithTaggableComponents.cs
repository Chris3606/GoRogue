using JetBrains.Annotations;

namespace GoRogue.Components.ParentAware
{
    /// <summary>
    /// Interface providing a convention for objects that use component collections to store components associated
    /// with themselves.  If the object should use a <see cref="IBasicComponentCollection"/> instead, use
    /// <see cref="IObjectWithComponents"/> in place of this interface.
    /// </summary>
    [PublicAPI]
    public interface IObjectWithTaggableComponents : IObjectWithComponents
    {
        /// <summary>
        /// Collection holding components that GoRogue has recorded as being attached to this object.
        /// </summary>
        public new ITaggableComponentCollection GoRogueComponents { get; }

        /// <inheritdoc/>
        IBasicComponentCollection IObjectWithComponents.GoRogueComponents => GoRogueComponents;
    }
}
