namespace GoRogue
{
	/// <summary>
	/// Class designed as a helper for situations where you need to generate and assign a unique
	/// integer to each instance of a class, eg. for a class implementing <see cref="IHasID"/>.
	/// </summary>
	/// <remarks>
	/// The class may be initialized with a starting unsigned integer -- if none is given, 0 is the default
	/// starting point. To assign an ID, call <see cref="UseID"/>, and assign the value that it returns.
	/// This class is NOT thread-safe on its own -- if it needs to be, you can simply use a lock to wrap
	/// any calls to UseID.
	/// </remarks>
	public class IDGenerator
	{
		private uint currentInteger;

		private bool lastAssigned;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="startingInt">
		/// Unsigned integer to start at (one that will be returned first time <see cref="UseID"/> is called).
		/// Defaults to 0.
		/// </param>
		public IDGenerator(uint startingInt = 0)
		{
			currentInteger = startingInt;
			lastAssigned = false;
		}

		/// <summary>
		/// Call every time you wish to "assign" an ID. The integer returned will never be returned
		/// again (each integer returned by this function will be unique).
		/// </summary>
		/// <returns>The ID that has been assigned.</returns>
		public uint UseID()
		{
			if (lastAssigned)
				throw new System.Exception($"An {nameof(IDGenerator)} ran out of IDs to assign, as uint.MaxValue was hit.");
			else if (currentInteger == uint.MaxValue) // We're about to assign the last box
				lastAssigned = true;
			return currentInteger++;
		}
	}
}
