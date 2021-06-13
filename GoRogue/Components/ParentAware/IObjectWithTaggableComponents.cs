using JetBrains.Annotations;

namespace GoRogue.Components.ParentAware
{
    /// <summary>
    /// Interface providing a convention for objects that use component collections to store components associated
    /// with themselves.
    /// </summary>
    [PublicAPI]
    public interface IObjectWithTaggableComponents
    {
        /// <summary>
        /// Collection holding components that GoRogue has recorded as being attached to this object.
        /// </summary>
        public ITaggableComponentCollection GoRogueComponents { get; }
    }
}
