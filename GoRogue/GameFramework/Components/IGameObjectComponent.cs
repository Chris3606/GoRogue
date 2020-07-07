using JetBrains.Annotations;

namespace GoRogue.GameFramework.Components
{
    /// <summary>
    /// Optional interface for components that are attached to an <see cref="IGameObject" />.  While the implementation
    /// of this interface is not required for GameObject components, if it is used the <see cref="Parent" /> field
    /// is automatically updated when you add or remove this component from a GameObject's
    /// <see cref="GameObject.GoRogueComponents" /> list.  A component implementing this interface cannot be added to
    /// multiple GameObjects at once.
    /// </summary>
    [PublicAPI]
    public interface IGameObjectComponent
    {
        /// <summary>
        /// The object to which this component is attached, or null if it is not attached.  Should not be set manually;
        /// it's set automatically when added/removed from a GameObject's <see cref="GameObject.GoRogueComponents"/>
        /// list.
        /// </summary>
        IGameObject? Parent { get; set; }
    }
}
