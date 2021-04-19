using JetBrains.Annotations;

namespace GoRogue.Components.ParentAware
{
    /// <summary>
    /// Optional interface for components that are attached to something implementing <see cref="ObjectWithComponentsBase"/>.
    /// </summary>
    /// <remarks>
    /// While the implementation of this interface is not required for GoRogue components, when it is used with something
    /// inheriting from <see cref="ObjectWithComponentsBase"/> (or some custom implementation of its interface),
    /// the <see cref="Parent" /> field is automatically updated when you add or remove this component from the object's
    /// <see cref="IObjectWithComponents.GoRogueComponents" /> list.  A component implementing this interface cannot be
    /// added to multiple objects at the same time.
    /// </remarks>
    [PublicAPI]
    public interface IParentAwareComponent
    {
        /// <summary>
        /// The object to which this component is attached, or null if it is not attached.
        /// </summary>
        /// <remarks>
        /// Should not be assigned to manually, outside of a custom implementation of
        /// <see cref="IObjectWithComponents"/>.  It is set automatically when added/removed from an object's
        /// <see cref="IObjectWithComponents.GoRogueComponents"/> list.
        /// </remarks>
        public IObjectWithComponents? Parent { get; set; }
    }
}
