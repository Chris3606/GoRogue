namespace GoRogue
{
	/// <summary>
	/// Class representing a node in a <see cref="LinkedListPriorityQueue{T}"/>.
	/// </summary>
	public sealed class LinkedListPriorityQueueNode<T>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="value">The value the node represents.</param>
		/// <param name="expectedCost">Cost of the node.</param>
		public LinkedListPriorityQueueNode(T value, double expectedCost)
		{
			this.Value = value;
			this.ExpectedCost = expectedCost;
		}

		/// <summary>
		/// The value the node represents.
		/// </summary>
		public readonly T Value;
		/// <summary>
		/// 
		/// </summary>
		public readonly double ExpectedCost;

		/// <summary>
		/// Next node in the priority queue, or null if there is no next node.
		/// </summary>
		public LinkedListPriorityQueueNode<T> Next { get; internal set; }
	}
}
