namespace GoRogue
{
	/// <summary>
	/// Interface for a class that has an ID, typically used for hashing purposes. The ID should be
	/// unique or close to unique over all instances of the class (for efficiency). Typical
	/// implementation could be simply random-genning the ID, or for completely unique IDs can
	/// involve using an IDGenerator, potentially as follows: <example>
	/// <code>
	/// class SomeClass : IHasID
	/// {
	/// private static IDGenerator generator = new IDGenerator();
	/// public int ID { get; private set; }
	/// /// public SomeClass(...)
	/// {
	/// ID = generator.UseID();
	/// }
	/// }
	/// </code>
	/// </example> A class that wishes to be able to have these IDs serialized and the state resumed
	/// later might have to do something more advanced than a static variable (say, a "global" array
	/// of generators whose states are read in from a file at the start of the game), but the
	/// principle would remain the same. Interface is used for SpatialMap to work correctly, and as
	/// well in general provides a convenient way to hash entities that implement this interface.
	/// </summary>
	public interface IHasID
	{
		/// <summary>
		/// ID assigned to this entity.
		/// </summary>
		uint ID { get; }
	}
}