namespace GoRogue
{
    /// <summary>
    /// A class that contains a rectangle Area that is automatically "locked" to being inside the
    /// BoundingBox. A typical use might be keeping track of a camera view area.
    /// </summary>
    public class BoundedRectangle
    {
        private Rectangle _area;
        private Rectangle _boundingBox;

        /// <summary>
        /// Constructor. Takes the initial area and bounding box.
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
        /// The rectangle that is guaranteed to be contained within the BoundingBox. Although it does
        /// not specifically provide a set accessor, this property is returning a reference and as
        /// such may be assigned to. Whenever the rectangle is retrieved, if it is not contained
        /// within BoundingBox it will be modified as necessary to keep it within those bounds.
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
        /// The rectangle which Area is automatically bounded to. Similar to area, although this
        /// property does not explicitly provide a set accessor, it is returning a reference so the
        /// property may be assigned to.
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

            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;

            if (_area.MaxExtentX > _boundingBox.Width)
                x = _boundingBox.Width - _area.Width;
            if (_area.MaxExtentY > _boundingBox.Height)
                y = _boundingBox.Height - _area.Height;

            _area = new Rectangle(x, y, width, height);
        }
    }
}