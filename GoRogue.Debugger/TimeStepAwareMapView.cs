using System;
using System.Collections.Generic;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.Debugger
{
    /// <summary>
    /// Records a value change between two steps.
    /// </summary>
    /// <typeparam name="T">Type of value being changed.</typeparam>
    public readonly struct ValueChange<T>
    {
        /// <summary>
        /// Position whose value was changed.
        /// </summary>
        public readonly Point Position;

        /// <summary>
        /// Original value that was changed.
        /// </summary>
        public readonly T OldValue;

        /// <summary>
        /// New value that was set.
        /// </summary>
        public readonly T NewValue;

        /// <summary>
        /// Creates a new ValueChange object.
        /// </summary>
        /// <param name="position">Position whose value was changed.</param>
        /// <param name="oldValue">Original value that was changed.</param>
        /// <param name="newValue">New value that was set.</param>
        public ValueChange(Point position, T oldValue, T newValue)
        {
            Position = position;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Represents the changes for one unit of time in a routine.
    /// </summary>
    /// <typeparam name="T">Type of value being changed.</typeparam>
    public class TimeStep<T>
    {
        /// <summary>
        /// Index of the time step (chronological order).
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// List of changes made in this time step.
        /// </summary>
        public readonly List<ValueChange<T>> Changes;

        /// <summary>
        /// Constructs a new time step with no changes.
        /// </summary>
        /// <param name="index">Index of the time step (chronological order)</param>
        public TimeStep(int index)
        {
            Index = index;
            Changes = new List<ValueChange<T>>();
        }
    }

    /// <summary>
    /// A map view that records diffs between time-steps.
    /// </summary>
    /// <typeparam name="T">Type of item stored in the map view.</typeparam>
    public class TimeStepAwareMapView<T> : SettableMapViewBase<T>
    {
        private readonly List<TimeStep<T>> _timeSteps;
        /// <summary>
        /// Time steps and their changes.
        /// </summary>
        public IReadOnlyList<TimeStep<T>> TimeSteps => _timeSteps;

        /// <summary>
        /// Index of current step.
        /// </summary>
        public int CurrentStep { get; private set; }

        /// <summary>
        /// Map being wrapped.
        /// </summary>
        public readonly ISettableMapView<T> BaseMap;

        /// <inheritdoc />
        public override int Height => BaseMap.Height;

        /// <inheritdoc />
        public override int Width => BaseMap.Width;

        /// <inheritdoc />
        public override T this[Point pos]
        {
            get => BaseMap[pos];
            set
            {
                if (CurrentStep != _timeSteps.Count - 1)
                    throw new Exception($"Cannot set values in {nameof(TimeStepAwareMapView<T>)} when at a " +
                                        "non-current time step.");

                T oldValue = BaseMap[pos];
                BaseMap[pos] = value;
                _timeSteps[^1].Changes.Add(new ValueChange<T>(pos, oldValue, value));
            }
        }

        /// <summary>
        /// Construct a new map view that wraps an existing one.
        /// </summary>
        /// <param name="baseMap">Map to wrap around.</param>
        public TimeStepAwareMapView(ISettableMapView<T> baseMap)
        {
            BaseMap = baseMap;
            CurrentStep = 0;
            _timeSteps = new List<TimeStep<T>> { new TimeStep<T>(0) };
        }

        /// <summary>
        /// Construct a new map view with a new ArrayMap as the backing map view.
        /// </summary>
        /// <param name="width">Width of the map view.</param>
        /// <param name="height">Height of the map view.</param>
        public TimeStepAwareMapView(int width, int height)
            : this(new ArrayMap<T>(width, height))
        { }

        /// <summary>
        /// Modify the map to be in line with the next time step.  If there is no next time-step,
        /// simply create a new one.
        /// </summary>
        /// <returns>True if a new time step was created, false otherwise.</returns>
        public bool NextTimeStep()
        {
            // Increment time step state
            CurrentStep += 1;

            // If we're not at the final time step that we've calculated, simply re-apply existing changes
            if (CurrentStep != _timeSteps.Count)
            {
                var timeStep = _timeSteps[CurrentStep];
                foreach (var change in timeStep.Changes)
                    BaseMap[change.Position] = change.NewValue;

                return false;
            }

            // Otherwise, we should have already recorded the changes so just create a new step.
            _timeSteps.Add(new TimeStep<T>(_timeSteps.Count));
            return true;
        }

        /// <summary>
        /// Reverts <see cref="BaseMap"/> to the previous time step's state, if there is a previous time step.
        /// Does nothing if there is no previous time step.
        /// </summary>
        public void LastTimeStep()
        {
            // Do nothing if no previous time steps
            if (CurrentStep == -1)
                return;

            // Revert all changes made by the current step
            foreach (var change in _timeSteps[CurrentStep].Changes)
                BaseMap[change.Position] = change.OldValue;

            // Decrement step tracker to reflect new state
            CurrentStep -= 1;
        }

        /// <summary>
        /// Fills the map view with the given value.
        /// </summary>
        /// <param name="value">The value to fill with.</param>
        public void Fill(T value)
        {
            foreach (var pos in this.Positions())
                this[pos] = value;
        }
    }
}
