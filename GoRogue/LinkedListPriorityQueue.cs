namespace GoRogue
{
	/// <summary>
	/// Minimum priority queue based on a sorted-linked-list data structure.
	/// </summary>
	public sealed class LinkedListPriorityQueue<T>
	{
		private LinkedListPriorityQueueNode<T> head;

		/// <summary>
		/// Returns whether or not there are any elements in the queue.
		/// </summary>        
		public bool IsEmpty() => this.head == null;

		/// <summary>
		/// Pushes a node onto the priority queue      
		/// </summary>
		public void Push(LinkedListPriorityQueueNode<T> node)
		{
			// If the heap is empty, just add the item to the top
			if (this.head == null)
			{
				this.head = node;
			}
			else if (node.ExpectedCost < this.head.ExpectedCost)
			{
				node.Next = this.head;
				this.head = node;
			}
			else
			{
				var current = this.head;
				while (current.Next != null && current.Next.ExpectedCost <= node.ExpectedCost)
				{
					current = current.Next;
				}

				node.Next = current.Next;
				current.Next = node;
			}
		}

		/// <summary>
		/// Pops the node with the smallest cost out of the queue, and returns it.
		/// </summary>
		public LinkedListPriorityQueueNode<T> Pop()
		{
			var top = this.head;
			this.head = this.head.Next;

			return top;
		}
	}
}
