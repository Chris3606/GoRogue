namespace GoRogue
{
	/// <summary>
	/// This class defines a rectanglar area, whose position is automatically "locked" to
	/// being inside a rectangular bounding box as it is changed. A typical use might be
	/// keeping track of a camera's view area.
	/// </summary>
	public class BoundedRectangle
	{
		private Rectangle _area;
		private Rectangle _boundingBox;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="area">Initial area for the rectangle.</param>
		/// <param name="boundingBox">Initial bounding box by which to bound the rectangle.</param>
		public BoundedRectangle(Rectangle area, Rectangle boundingBox)
		{
			_boundingBox = boundingBox;
			_area = area;

			boundLock();
		}

		/// <summary>
		/// The rectangle that is guaranteed to be contained completely within <see cref="BoundingBox"/>.
		/// Although it does not specifically provide a set accessor, this property is returning a
		/// reference and as such may be assigned to.
		/// </summary>
		public ref Rectangle Area
		{
			get
			{
				if (_area.X < _boundingBox.X || _area.Y < _boundingBox.Y ||
					_area.MaxExtentX > _boundingBox.MaxExtentX || _area.MaxExtentY > _boundingBox.MaxExtentY)
					boundLock();

				return ref _area;
			}
		}

		/// <summary>
		/// The rectangle which <see cref="Area"/> is automatically bounded to be within.  Although this
		/// property does not explicitly provide a set accessor, it is returning a reference so therefore
		/// the property may be assigned to.
		/// </summary>
		public ref Rectangle BoundingBox
		{
			get => ref _boundingBox;
		}

		private void boundLock()
		{
			int x = _area.X, y = _area.Y, width = _area.Width, height = _area.Height;

			if (width > _boundingBox.Width)
				width = _boundingBox.Width;
			if (height > _boundingBox.Height)
				height = _boundingBox.Height;

			if (x < _boundingBox.X)
				x = _boundingBox.X;
			if (y < _boundingBox.Y)
				y = _boundingBox.Y;

			if (x > _boundingBox.MaxExtentX - width + 1)
				x = _boundingBox.MaxExtentX - width + 1;
			if (y > _boundingBox.MaxExtentY - height + 1)
				y = _boundingBox.MaxExtentY - height + 1;

			_area = new Rectangle(x, y, width, height);
		}
	}
}
