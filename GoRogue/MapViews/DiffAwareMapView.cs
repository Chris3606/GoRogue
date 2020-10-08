using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Records a value change in a diff as recorded by a <see cref="DiffAwareMapView{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of value being changed.</typeparam>
    [PublicAPI]
    public readonly struct ValueChange<T> : IEquatable<ValueChange<T>>
        where T : struct
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
        /// Creates a new value change record.
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

        /// <inheritdoc />
        public bool Equals(ValueChange<T> other)
            => Position.Equals(other.Position) && OldValue.Equals(other.OldValue) && NewValue.Equals(other.NewValue);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ValueChange<T> other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Position, OldValue, NewValue);

        /// <summary>
        /// Tests the two changes by their fields for equality.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns>True if all the changes are equivalent; false otherwise.</returns>
        public static bool operator ==(ValueChange<T> left, ValueChange<T> right) => left.Equals(right);

        /// <summary>
        /// Tests the two changes by their fields for inequality.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns>True if all the changes are equivalent; false otherwise.</returns>
        public static bool operator !=(ValueChange<T> left, ValueChange<T> right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() => $"{Position}: {OldValue} -> {NewValue}";
    }

    /// <summary>
    /// Represents a unique patch/diff of the state of a <see cref="DiffAwareMapView{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of value stored in the map view.</typeparam>
    [PublicAPI]
    public class Diff<T> where T : struct
    {
        private List<ValueChange<T>> _changes = new List<ValueChange<T>>();
        /// <summary>
        /// Read-only list of changes made in this time step.
        /// </summary>
        public IReadOnlyList<ValueChange<T>> Changes => _changes.AsReadOnly();

        /// <summary>
        /// Whether or not the diff is currently known to be at the minimal possible size.
        /// </summary>
        public bool IsCompressed { get; private set; } = true;

        /// <summary>
        /// Adds a change to the diff.
        /// </summary>
        /// <param name="change">Change to add.</param>
        public void Add(ValueChange<T> change)
        {
            _changes.Add(change);

            // Change means its worth potentially minimizing, as this change might duplicate previous ones
            IsCompressed = false;
        }

        /// <summary>
        /// Reduces the diff to the minimum possible changes to achieve the resulting values by removing duplicate
        /// positions from the change list.
        /// </summary>
        public void Compress()
        {
            // No work to do
            if (IsCompressed)
                return;

            // Position of previous change we saw
            Point currentPositionGroup = Point.None;
            // NewValue of previous change we saw
            T prevNewValue = default;
            // Oldest OldValue we have encountered in the current position group.
            T firstOldValue = default;
            // New list of (minimal) changes
            var newChanges = new List<ValueChange<T>>();

            // Iterates in order sorted by position group, then by position in old list
            bool isFirstPositionGroup = true;
            foreach (var (_, change) in Changes
                                                .Select((change, index) => (index, change))
                                                .OrderBy(tuple => tuple.change.Position.X)
                                                .ThenBy(tuple => tuple.change.Position.Y)
                                                .ThenBy(tuple => tuple.index))
            {
                // Either we've changed position groups or this is the first group
                if (currentPositionGroup != change.Position)
                {
                    // In all cases but the initial group, we should add a change representing the starting value from
                    // the last time step and the newest value in this one (if they are different)
                    if (!isFirstPositionGroup)
                    {
                        // If the two are equal, the net result of the step is no change; otherwise add a change
                        if (!firstOldValue.Equals(prevNewValue))
                            newChanges.Add(new ValueChange<T>(currentPositionGroup, firstOldValue, prevNewValue));
                    }

                    // In any case, update the cached old value and position for start of a group
                    firstOldValue = change.OldValue;
                    currentPositionGroup = change.Position;
                    isFirstPositionGroup = false;
                }

                // Update cached new value for next iteration
                prevNewValue = change.NewValue;
            }

            // We still need to add a change for the last item in the set, since there was never a position group
            // switch for it
            if (!firstOldValue.Equals(prevNewValue))
                newChanges.Add(new ValueChange<T>(currentPositionGroup, firstOldValue, prevNewValue));

            // Switch out the change lists and mark the diff as fully compressed
            _changes = newChanges;
            IsCompressed = true;
        }
    }

    /// <summary>
    /// A map view wrapper useful for recording diffs (change-sets) of changes to a map view, and applying/removing
    /// those change-sets of values from the map view.  Only functions with value types.
    /// </summary>
    /// <remarks>
    /// Generally, this class is useful with values types/primitive types wherein values are completely replaced when
    /// they are modified.  It allows applying a series of change sets in forward or reverse order; and as such
    /// can be extremely useful for debugging and/or recording intermediate values of a map generation target view.
    /// </remarks>
    /// <typeparam name="T">Type of value in the map view.  Must be a value type.</typeparam>
    [PublicAPI]
    public class DiffAwareMapView<T> : SettableMapViewBase<T>
        where T : struct
    {
        private ISettableMapView<T> _baseMap;

        /// <summary>
        /// The map view whose changes are being recorded in diffs.
        /// </summary>
        public IMapView<T> BaseMap => _baseMap;

        /// <inheritdoc />
        public override int Height => BaseMap.Height;

        /// <inheritdoc />
        public override int Width => BaseMap.Width;

        /// <inheritdoc />
        public override T this[Point pos]
        {
            get => _baseMap[pos];
            set
            {
                T oldValue = _baseMap[pos];

                // No change necessary
                if (oldValue.Equals(value))
                    return;

                // First change for this diff so create the change object
                if (CurrentDiffIndex == _diffs.Count)
                    _diffs.Add(new Diff<T>());

                // We can't make changes when there's previously recorded states to apply
                if (CurrentDiffIndex != _diffs.Count - 1)
                    throw new InvalidOperationException(
                        $"Cannot set values to a {nameof(DiffAwareMapView<T>)} when there are existing diffs " +
                        "that are not applied.");

                // Apply change to base map and add to current diff
                _baseMap[pos] = value;
                _diffs[^1].Add(new ValueChange<T>(pos, oldValue, value));
            }
        }

        /// <summary>
        /// The index of the diff whose ending state is currently reflected in <see cref="BaseMap"/>. Returns -1
        /// if none of the diffs in the list have been applied (eg. the map is in the state it was in at the
        /// <see cref="DiffAwareMapView{T}"/>'s creation.
        /// </summary>
        /// <remarks>
        /// This index in <see cref="Diffs"/> MAY or MAY NOT exist.  It will only exist if a change
        /// has actually been added to the current diff (or it has been finalized).
        /// </remarks>
        public int CurrentDiffIndex { get; private set; }

        private List<Diff<T>> _diffs;
        /// <summary>
        /// All diffs recorded for the current map view, and their changes.
        /// </summary>
        public IReadOnlyList<Diff<T>> Diffs => _diffs.AsReadOnly();

        /// <summary>
        /// Whether or not to automatically compress diffs when the currently applied diff is changed.
        /// </summary>
        public bool AutoCompress;

        /// <summary>
        /// Constructs a diff-aware map view that wraps around an existing map view.
        /// </summary>
        /// <param name="baseMap">The map view whose changes are to be recorded in diffs.</param>
        /// <param name="autoCompress">
        /// Whether or not to automatically compress diffs when the currently applied diff is changed.
        /// </param>
        public DiffAwareMapView(ISettableMapView<T> baseMap, bool autoCompress = true)
        {
            _baseMap = baseMap;
            CurrentDiffIndex = 0;
            AutoCompress = autoCompress;
            _diffs = new List<Diff<T>>();
        }

        /// <summary>
        /// Constructs a diff-aware map view, whose base map will be a new <see cref="ArrayMap{T}"/>.
        /// </summary>
        /// <param name="width">Width of the base map view that will be created.</param>
        /// <param name="height">Height of the base map view that will be created.</param>
        /// <param name="autoCompress">
        /// Whether or not to automatically compress diffs when the currently applied diff is changed.
        /// </param>
        public DiffAwareMapView(int width, int height, bool autoCompress = true)
            : this(new ArrayMap<T>(width, height), autoCompress)
        { }

        /// <summary>
        /// Applies the next recorded diff, or throws exception if there is no future diffs recorded.
        /// </summary>
        public void ApplyNextDiff()
        {
            // Can't apply a diff if there is no next diff
            if (CurrentDiffIndex >= _diffs.Count - 1)
                throw new InvalidOperationException($"Cannot {nameof(ApplyNextDiff)} when the map is already " +
                                                    "synchronized with the most recent recorded diff.");

            // Compress the diff we're about to switch off if it needs it and auto-compression is on
            if (AutoCompress && CurrentDiffIndex != -1)
                _diffs[CurrentDiffIndex].Compress();

            // Modify state to reflect diff we're applying
            CurrentDiffIndex += 1;

            // Compress the diff we're about to apply if it needs it and auto-compression is on
            if (AutoCompress)
                _diffs[CurrentDiffIndex].Compress();

            // Apply diff's changes
            foreach (var change in _diffs[CurrentDiffIndex].Changes)
                _baseMap[change.Position] = change.NewValue;
        }

        /// <summary>
        /// Reverts the current diff's changes, so that the map will be in the state it was in at the end
        /// of the previous diff.  Throws exception if no diffs are applied.
        /// </summary>
        public void RevertToPreviousDiff()
        {
            if (CurrentDiffIndex == -1)
                throw new InvalidOperationException(
                    $"Cannot {nameof(RevertToPreviousDiff)} when there are no applied diffs.");

            if (CurrentDiffIndex != _diffs.Count) // If current diff has no changes, nothing to do
            {
                // Compress the diff we're about to switch off if it needs it and auto-compression is on
                if (AutoCompress)
                    _diffs[CurrentDiffIndex].Compress();

                // Revert current diff's changes
                foreach (var change in _diffs[CurrentDiffIndex].Changes)
                    _baseMap[change.Position] = change.OldValue;
            }

            // Modify state to reflect diff we're applying.  Exit if we're at beginning state, eg. there are no longer
            // any diffs applied
            CurrentDiffIndex -= 1;
            if (CurrentDiffIndex == -1)
                return;

            // If there is a previous diff, compress it if it needs it and auto-compression is on
            if (AutoCompress)
                _diffs[CurrentDiffIndex].Compress();
        }

        /// <summary>
        /// Finalizes the current diff, and creates a new one.  Throws exceptions if there are diffs that are not
        /// currently applied.
        /// </summary>
        public void FinalizeCurrentDiff()
        {
            if (CurrentDiffIndex < _diffs.Count - 1)
                throw new InvalidOperationException(
                    $"Cannot {nameof(FinalizeCurrentDiff)} if there are existing diffs that are not applied.");

            // No changes were ever added to the current diff, so create the empty one since it's been finalized
            if (CurrentDiffIndex == _diffs.Count)
                _diffs.Add(new Diff<T>());
            // No need to compress if diff we just added was empty
            else if (AutoCompress)
                _diffs[^1].Compress();

            // Add to index to record currently active diff
            CurrentDiffIndex += 1;
        }

        /// <summary>
        /// Convenience method that calls <see cref="ApplyNextDiff"/> if there are existing diffs to apply, and
        /// <see cref="FinalizeCurrentDiff"/> if there are no existing diffs to apply.  Returns whether or not an
        /// existing diff was applied.
        /// </summary>
        /// <returns>True if an existing diff is applied, false if a new one was created.</returns>
        public bool ApplyNextDiffOrFinalize()
        {
            if (CurrentDiffIndex >= _diffs.Count - 1)
            {
                FinalizeCurrentDiff();
                return false;
            }

            ApplyNextDiff();
            return true;
        }
    }
}
