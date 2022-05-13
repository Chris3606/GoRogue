using JetBrains.Annotations;

namespace GoRogue.Components.ParentAware
{
    /// <summary>
    /// Interface providing a convention for objects that use component collections to store components associated
    /// with themselves.
    /// </summary>
    [PublicAPI]
    public interface IObjectWithComponents
    {
        /// <summary>
        /// Collection holding components that GoRogue has recorded as being attached to the implementing object.
        /// </summary>
        public IComponentCollection GoRogueComponents { get; }
    }
}
