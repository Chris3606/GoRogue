namespace GoRogue
{
	/// <summary>
	/// Optional interface that may be implemented by components to ensure they are returned in a specific order
	/// when <see cref="ComponentContainer.GetComponent{T}"/> is called.  Components are not required to implement
	/// this interface, however components that do not will be returned after any components that do.
	/// </summary>
	/// <remarks>
	/// When the GetComponents function of either ComponentContainer or any object that uses one to implement the component
	/// system (such as <see cref="GameFramework.IGameObject"/> is called, any components with a lower <see cref="SortOrder"/>
	/// are guaranteed to be returned BEFORE any components with a higher SortOrder.  This can be useful to enforce that certain
	/// types of or instances of components are processed before some other type of or instance of components.  Components that
	/// do not implement ISortedComponent will always be retorned AFTER any components that do implement that interface. 
	/// </remarks>
	public interface ISortedComponent
	{
		/// <summary>
		/// Value indicating the relative ordering of this component.  A lower value cause a component to be retrieved before
		/// any components with a higher value.
		/// </summary>
		uint SortOrder { get; }
	}
}
