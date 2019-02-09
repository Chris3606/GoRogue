namespace GoRogue
{
	/// <summary>
	/// Interface to be implemented by objects that will be used in LayeredSpatialMap/Map classes.
	/// </summary>
	public interface IHasLayer
	{
		/// <summary>
		/// The layer on which the object should reside. Higher numbers indicate layers closer to the
		/// "top". This is assumed to remain constant while the object is within a data structure
		/// that uses this interface, if it is modified, that data structure will become out of sync.
		/// </summary>
		int Layer { get; }
	}
}