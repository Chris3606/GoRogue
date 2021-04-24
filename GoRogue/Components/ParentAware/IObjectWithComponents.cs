using JetBrains.Annotations;

namespace GoRogue.Components.ParentAware
{
    /// <summary>
    /// Interface providing a convention for objects that use component collections to store components associated
    /// with themselves.  If the object should use a <see cref="ITaggableComponentCollection"/> instead, use
    /// <see cref="IObjectWithTaggableComponents"/> in place of this interface.
    /// </summary>
    [PublicAPI]
    public interface IObjectWithComponents
    {
        /// <summary>
        /// Collection holding components that GoRogue has recorded as being attached to this object.
        /// </summary>
        public IBasicComponentCollection GoRogueComponents { get; }
    }
}
