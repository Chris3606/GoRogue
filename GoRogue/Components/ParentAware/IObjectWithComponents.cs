using JetBrains.Annotations;

namespace GoRogue.Components.ParentAware
{
    /// <summary>
    /// Interface providing a convention for objects that use component collections to store components associated
    /// with themselves.  If possible, you should inherit from <see cref="ObjectWithComponentsBase"/> instead of implementing
    /// this interface directly, as that implements most of the useful functionality.  If not, it is recommended that you
    /// use that class as an example/starting point for your own.
    /// </summary>
    [PublicAPI]
    public interface IObjectWithComponents
    {
        /// <summary>
        /// Collection holding components that GoRogue has recorded as being attached to this object.
        /// </summary>
        public ITaggableComponentCollection GoRogueComponents { get; }
    }
}
