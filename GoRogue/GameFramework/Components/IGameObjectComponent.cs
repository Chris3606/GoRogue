using JetBrains.Annotations;

namespace GoRogue.GameFramework.Components
{
    /// <summary>
    /// Optional interface for components that are attached to a <see cref="GameObject" />, or any
    /// <see cref="IGameObject"/> implementation that properly sets the <see cref="Parent"/> field.
    /// </summary>
    /// <remarks>
    /// While the implementation of this interface is not required for GameObject components, if it is used the
    /// <see cref="Parent" /> field is automatically updated when you add or remove this component from a GameObject's
    /// <see cref="GameObject.GoRogueComponents" /> list.  A component implementing this interface cannot be added to
    /// multiple GameObjects at once.
    /// </remarks>
    [PublicAPI]
    public interface IGameObjectComponent
    {
        /// <summary>
        /// The object to which this component is attached, or null if it is not attached.
        /// </summary>
        /// <remarks>
        /// Should not be set manually outside of a custom implementation of <see cref="IGameObject"/>; it's set
        /// automatically when added/removed from a GameObject's <see cref="GameObject.GoRogueComponents"/> list.
        /// </remarks>
        IGameObject? Parent { get; set; }
    }
}
