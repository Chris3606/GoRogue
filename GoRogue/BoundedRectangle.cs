using System;

namespace GoRogue
{
	/// <summary>
	/// This class defines a rectanglar area, whose position is automatically "locked" to
	/// being inside a rectangular bounding box as it is changed. A typical use might be
	/// keeping track of a camera's view area.
	/// </summary>
	[Serializable]
	public class BoundedRectangle : IEquatable<BoundedRectangle>
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

		/// <summary>
		/// Compares the current BoundedRectangle to the object given.
		/// </summary>
		/// <param name="obj"/>
		/// <returns>True if the given object is a BoundedRectangle that represents the same area, false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is BoundedRectangle e)
				return Equals(e);

			return false;
		}

		/// <summary>
		/// Compares the current BoundedRectangle to the one given.
		/// </summary>
		/// <param name="other"/>
		/// <returns>True if the given BoundedRectangle represents the same area, false otherwise.</returns>
		public bool Equals(BoundedRectangle other) => !ReferenceEquals(other, null) && BoundingBox.Equals(other.BoundingBox) && Area.Equals(other.Area);

		/// <summary>
		/// Returns a hash-value for this object.
		/// </summary>
		/// <returns/>
		public override int GetHashCode() => BoundingBox.GetHashCode() ^ Area.GetHashCode();

		/// <summary>
		/// Compares the two BoundedRectangle instances.
		/// </summary>
		/// <param name="lhs"/>
		/// <param name="rhs"/>
		/// <returns>True if the two given BoundedRectangle instances represent the same area, false otherwise.</returns>
		public static bool operator ==(BoundedRectangle lhs, BoundedRectangle rhs) => lhs?.Equals(rhs) ?? rhs is null;

		/// <summary>
		/// Compares the two BoundedRectangle instances.
		/// </summary>
		/// <param name="lhs"/>
		/// <param name="rhs"/>
		/// <returns>True if the two given BoundedRectangle instances do NOT represent the same area, false otherwise.</returns>
		public static bool operator !=(BoundedRectangle lhs, BoundedRectangle rhs) => !(lhs == rhs);

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
