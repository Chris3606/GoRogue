using System;
using GoRogue.MapViews;
using JetBrains.Annotations;
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
        [PublicAPI]
        public string CurrentViewName { get; private set; }

        /// <summary>
        /// Current view being displayed.
        /// </summary>
        [PublicAPI]
        public IGridView<char> CurrentView { get; private set; }
        /// <summary>
        /// The actual viewport of the current view being displayed.
        /// </summary>
        [PublicAPI]
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

            // Validate that routines defined views the way we expect
            if (_routine.Views.Count == 0)
                throw new Exception($"{_routine.GetType().Name} defines 0 views.");

            for (int i = 1; i < _routine.Views.Count; i++)
            {
                var curView = _routine.Views[i].view;
                var prevView = _routine.Views[i - 1].view;

                if (curView.Width != prevView.Width || curView.Height != prevView.Height)
                    throw new Exception($"{_routine.GetType().Name} defines views that are not the same size.");
            }

            // Initialize initial views
            _viewIndex = 0;
            (CurrentViewName, CurrentView) = _routine.Views[_viewIndex];

            // Initialize initial viewport
            CurrentViewport = new Viewport<char>(CurrentView, (0, 0, width, height));
            CurrentViewport.SetViewArea(
                CurrentViewport.ViewArea.WithCenter((CurrentView.Width / 2, CurrentView.Height / 2)));
        }

        /// <summary>
        /// Advance to next view for routine.
        /// </summary>
        public void NextView()
        {
            var oldView = CurrentView;

            // Update index and views
            _viewIndex = MathHelpers.WrapAround(_viewIndex + 1, _routine.Views.Count);
            (CurrentViewName, CurrentView) = _routine.Views[_viewIndex];

            // Validate the size hasn't changed
            if (oldView.Width != CurrentView.Width || oldView.Height != CurrentView.Height)
                throw new Exception($"{_routine.GetType().Name} defines views that are not the same size.");

            // Create new viewport based on new view.
            CurrentViewport = new Viewport<char>(CurrentView, CurrentViewport.ViewArea);
        }

        /// <summary>
        /// Revert to previous view for routine.
        /// </summary>
        public void PreviousView()
        {
            var oldView = CurrentView;

            // Update index and views
            _viewIndex = MathHelpers.WrapAround(_viewIndex - 1, _routine.Views.Count);
            (CurrentViewName, CurrentView) = _routine.Views[_viewIndex];

            // Validate the size hasn't changed
            if (oldView.Width != CurrentView.Width || oldView.Height != CurrentView.Height)
                throw new Exception($"{_routine.GetType().Name} defines views that are not the same size.");

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
