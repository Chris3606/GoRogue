using System.Collections.Generic;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Since some algorithms that use MapViews can be expensive to run entirely on large maps (such as GoalMaps), Viewport is a class that
    /// effectively creates and maintains a "viewport" of the map.  Its indexers perform relative to absolute coordinate translations.
    /// </summary>
    public class Viewport<T> : IMapView<T>
    {
        public IMapView<T> MapView { get; private set; }

        private Rectangle _viewArea;
        public Rectangle ViewArea
        {
            get => _viewArea;
            set
            {
                _viewArea = value;
                boundLock();
            }
        }

        public int X
        {
            get => _viewArea.X;
            set
            {
                _viewArea.X = value;
                boundLock();
            }
        }

        public int Y
        {
            get => _viewArea.Y;
            set
            {
                _viewArea.Y = value;
                boundLock();
            }
        }

        public Coord MinCorner
        {
            get => _viewArea.MinCorner;
            set
            {
                _viewArea.MinCorner = value;
                boundLock();
            }
        }

        public int MaxX
        {
            get => _viewArea.MaxX;
            set
            {
                _viewArea.MaxX = value;
                boundLock();
            }
        }

        public int MaxY
        {
            get => _viewArea.MaxY;
            set
            {
                _viewArea.MaxY = value;
                boundLock();
            }
        }

        public Coord MaxCorner
        {
            get => _viewArea.MaxCorner;
            set
            {
                _viewArea.MaxCorner = value;
                boundLock();
            }
        }

        public Coord Center
        {
            get => _viewArea.Center;
            set
            {
                _viewArea.Center = value;
                boundLock();
            }
        }

        public int Width
        {
            get => _viewArea.Width;
            set
            {
                _viewArea.Width = value;
                boundLock();
            }
        }

        public int Height
        {
            get => _viewArea.Height;
            set
            {
                _viewArea.Height = value;
                boundLock();
            }
        }

        public T this[Coord relativePosition] => MapView[_viewArea.MinCorner + relativePosition];

        public T this[int relativeX, int relativeY] => MapView[_viewArea.X + relativeX, _viewArea.Y + relativeY];

        public Viewport(IMapView<T> mapView, Rectangle viewArea)
        {
            MapView = mapView;
            ViewArea = viewArea;
        }

        private void boundLock()
        {
            if (_viewArea.Width > MapView.Width)
                _viewArea.Width = MapView.Width;
            if (_viewArea.Height > MapView.Height)
                _viewArea.Height = MapView.Height;

            if (_viewArea.X < 0)
                _viewArea.X = 0;
            if (_viewArea.Y < 0)
                _viewArea.Y = 0;

            if (_viewArea.MaxX > MapView.Width)
                _viewArea.X = MapView.Width - _viewArea.Width;
            if (_viewArea.MaxY > MapView.Height)
                _viewArea.Y = MapView.Height - _viewArea.Height;
        }
    }
}
