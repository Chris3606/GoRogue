using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.Debugger
{
    /// <summary>
    /// Represents a current view of a routine, along with a corresponding viewport.
    /// </summary>
    public class RoutineViewport
    {
        private readonly IRoutine _routine;
        private int _viewIndex;

        /// <summary>
        /// Name of the view currently being displayed
        /// </summary>
        public string CurrentViewName { get; private set; }

        /// <summary>
        /// Current view being displayed.
        /// </summary>
        public IMapView<char> CurrentView { get; private set; }
        /// <summary>
        /// The actual viewport of the current view being displayed.
        /// </summary>
        public Viewport<char> CurrentViewport { get; private set; }

        /// <summary>
        /// Constructs a new viewport with the given width/height.
        /// </summary>
        /// <param name="routine">Routine to view.</param>
        /// <param name="width">Viewport width.</param>
        /// <param name="height">Viewport height.</param>
        public RoutineViewport(IRoutine routine, int width, int height)
        {
            _routine = routine;

            // Initialize initial views
            _viewIndex = 0;
            (CurrentViewName, CurrentView) = _routine.Views[_viewIndex];

            // Initialize initial viewport
            CurrentViewport = new Viewport<char>(CurrentView, (0, 0, width, height));
            CurrentViewport.SetViewArea(
                CurrentViewport.ViewArea.WithCenter((_routine.Map.Width / 2, _routine.Map.Height / 2)));
        }

        /// <summary>
        /// Advance to next view for routine.
        /// </summary>
        public void NextView()
        {
            // Update index and views
            _viewIndex = MathHelpers.WrapAround(_viewIndex + 1, _routine.Views.Count);
            (CurrentViewName, CurrentView) = _routine.Views[_viewIndex];

            // Create new viewport based on new view.
            CurrentViewport = new Viewport<char>(CurrentView, CurrentViewport.ViewArea);
        }

        /// <summary>
        /// Revert to previous view for routine.
        /// </summary>
        public void PreviousView()
        {
            // Update index and views
            _viewIndex = MathHelpers.WrapAround(_viewIndex - 1, _routine.Views.Count);
            (CurrentViewName, CurrentView) = _routine.Views[_viewIndex];

            // Create new viewport based on new view.
            CurrentViewport = new Viewport<char>(CurrentView, CurrentViewport.ViewArea);
        }

        /// <summary>
        /// Centers the viewport on the given position.  Returns whether or not view actually moved.
        /// </summary>
        /// <param name="center">New center for the viewport.</param>
        /// <returns></returns>
        public bool CenterViewOn(Point center)
        {
            Point oldCenter = CurrentViewport.ViewArea.Center;
            CurrentViewport.SetViewArea(CurrentViewport.ViewArea.WithCenter(center));

            return oldCenter != CurrentViewport.ViewArea.Center;
        }

        /// <summary>
        /// Resizes the viewport to the given values (if they are different than the current ones),
        /// making sure to preserve the center point where possible.
        /// </summary>
        /// <param name="width">New viewport width</param>
        /// <param name="height">New viewport height</param>
        public void ResizeViewport(int width, int height)
        {
            if (CurrentViewport.ViewArea.Width == width && CurrentViewport.ViewArea.Height == height)
                return;

            Point center = CurrentViewport.ViewArea.Center;
            CurrentViewport.SetViewArea(
                CurrentViewport.ViewArea.WithSize(width, height).WithCenter(center));
        }
    }
}
