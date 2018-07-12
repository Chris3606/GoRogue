namespace GoRogue
{
    /// <summary>
    /// A class that contains a rectangle Area, that when it is set, is automatically "locked" to being inside the BoundingBox. A typical use might be keeping track of 
    /// a camera view area.
    /// </summary>
    public class BoundedRectangle
    {
        private Rectangle _area;
        /// <summary>
        /// The rectangle, automatically guaranteed to be contained within the BoundingBox.  Does NOT provide a get accessor -- for read access, see GetArea(), which returns
        /// the rectangle as a reference.  When set, the rectangle is automatically modified as necessary to ensure it stays contained within the bounding box.
        /// </summary>
        public Rectangle Area
        {
            set
            {
                _area = value;
                boundLock();
            }
        }

        private Rectangle _boundingBox;
        /// <summary>
        /// The rectangle which Area is automatically bounded to.  Similar to area, this property does NOT provided a get accessor -- instead, use GetBoundingBox().
        /// When set, the Area rectangle is automatically modified as necessary to make sure it is contained within the new bounding box.
        /// </summary>
        public Rectangle BoundingBox
        {
            set
            {
                _boundingBox = value;
                boundLock();
            }
        }

        /// <summary>
        /// Constructor.  Takes the initial area and bounding box.
        /// </summary>
        /// <param name="area">Initial value for Area.</param>
        /// <param name="boundingBox">Initial bounding box for Area.</param>
        public BoundedRectangle(Rectangle area, Rectangle boundingBox)
        {
            _boundingBox = boundingBox;
            _area = area;

            boundLock();
        }

        /// <summary>
        /// Returns a reference to the Area rectangle.  Used for read-access to the Area property.
        /// </summary>
        /// <returns>A reference to the Area rectangle.</returns>
        public ref Rectangle GetArea() => ref _area;

        /// <summary>
        /// Returns a reference to the BoundingBox rectangle.  Used for read-access to the BoundingBox property.
        /// </summary>
        /// <returns>A reference to the BoundingBox rectangle.</returns>
        public ref Rectangle GetBoundingBox() => ref _boundingBox;

        private void boundLock()
        {
            int x = _area.X, y = _area.Y, width = _area.Width, height = _area.Height;

            bool changed = false;
            if (width > _boundingBox.Width)
            {
                width = _boundingBox.Width;
                changed = true;
            }
            if (height > _boundingBox.Height)
            {
                height = _boundingBox.Height;
                changed = true;
            }

            if (x < 0)
            {
                x = 0;
                changed = true;
            }
            if (y < 0)
            {
                y = 0;
                changed = true;
            }

            if (_area.MaxX > _boundingBox.Width)
            {
                x = _boundingBox.Width - _area.Width;
                changed = true;
            }
            if (_area.MaxY > _boundingBox.Height)
            {
                y = _boundingBox.Height - _area.Height;
                changed = true;
            }

            if (changed)
                _area = new Rectangle(x, y, width, height);
        }
    }
}
