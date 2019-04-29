namespace GoRogue.GameFramework.Components
{
	/// <summary>
	/// Optional interface for components that are attached to an <see cref="IGameObject"/>.  While the implementation of this interface is not
	/// required for IGameObject components, if <see cref="GameObject"/>'s implementation of IHasComponents is used, the <see cref="Parent"/> field
	/// is automatically kept up to date as you call <see cref="GameObject.AddComponent(object)"/>/<see cref="IHasComponents.RemoveComponent(object)"/>
	/// on objects that implement this interface.  A component implmenting this interface cannot be added to multiple GameObjects
	/// at once.
	/// </summary>
	public interface IGameObjectComponent
	{
		/// <summary>
		/// The object to which this component is attached, or null if it is not attached.  Should not be set manually, as this is taken
		/// care of by <see cref="GameObject.AddComponent(object)"/>/<see cref="IHasComponents.RemoveComponent(object)"/>
		/// </summary>
		IGameObject Parent { get; set; }
	}
}
