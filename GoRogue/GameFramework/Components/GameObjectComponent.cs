namespace GoRogue.GameFramework.Components
{
	/// <summary>
	/// Optional base class for components that are attached to IGameObject.  While the use of this base class is not required as a base for
	/// IGameObject components, if GameObject's implementation of IHasComponents is used, the Parent field is automatically kept up to date as
	/// you call AddComponent/RemoveComponent.  This component cannot be added to multiple GameObjects at once.
	/// </summary>
	public class GameObjectComponent
	{
		/// <summary>
		/// The object to which this component is attached, or null if it is not attached.
		/// </summary>
		public IGameObject Parent { get; internal set; }
	}
}
