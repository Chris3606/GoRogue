namespace GoRogue.GameFramework.Components
{
	/// <summary>
	/// Optional interface for components that are attached to IGameObject.  While the implementation of this inteerface is not required for
	/// IGameObject components, if GameObject's implementation of IHasComponents is used, the Parent field is automatically kept up to date as
	/// you call AddComponent/RemoveComponent on objects that implement this interface.  This component cannot be added to multiple GameObjects
	/// at once.
	/// </summary>
	public interface IGameObjectComponent
	{
		/// <summary>
		/// The object to which this component is attached, or null if it is not attached.  Should not be set manually, as this is taken
		/// care of by GameObject.AddComponent/RemoveComponent(s)
		/// </summary>
		IGameObject Parent { get; set; }
	}
}
