namespace GoRogue
{
	/// <summary>
	/// Class designed as a helper for situations where you need to generate and assign a unique
	/// integer to each instance of a class, eg. for a class implementing IHasID (see that interface
	/// documentation for usage example). One may initialize it with a starting unsigned integer, or
	/// 0 if none is specified. Then, every time one wants to use an integer, one should call
	/// UseID(), and use the one it returns. It is not thread-safe on its own -- if it needs to be,
	/// one might consider using a lock around any UseID calls.
	/// </summary>
	public class IDGenerator
	{
		private uint currentInteger;

		private bool lastAssigned;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="startingInt">
		/// Unsigned integer to start at (one that will be returned first time UseID is called).
		/// Default 0.
		/// </param>
		public IDGenerator(uint startingInt = 0)
		{
			currentInteger = startingInt;
			lastAssigned = false;
		}

		/// <summary>
		/// Call every time you wish to "assign" an ID. The integer returned will never be returned
		/// again (each integer will be unique, per instance of this class).
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