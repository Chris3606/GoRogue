using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using JetBrains.Annotations;
using SadRogue.Primitives;
using Xunit;
using Xunit.Abstractions;

namespace GoRogue.UnitTests.MapViews
{
    public class DiffTests
    {
        [Fact]
        public void Construction()
        {
            // Create object
            var diff = new Diff<bool>();

            // Should be no changes
            Assert.Empty(diff.Changes);
            // Compressed state should be true since there are no items
            Assert.True(diff.IsCompressed);
        }

        [Fact]
        public void AddDiffs()
        {
            // Create object and ensure starting point is as expected
            var diff = new Diff<bool>();
            Assert.Empty(diff.Changes);

            // Add new change and verify its addition
            var change1 = new ValueChange<bool>((1, 2), false, true);
            diff.Add(change1);
            Assert.False(diff.IsCompressed);
            Assert.Single(diff.Changes);
            Assert.Equal(change1, diff.Changes[0]);

            // Add another change (at same location) and verify its addition
            var change2 = new ValueChange<bool>((1, 2), false, true);
            diff.Add(change2);
            Assert.False(diff.IsCompressed);
            Assert.Equal(2, diff.Changes.Count);
            Assert.Equal(change2, diff.Changes[1]);
        }

        [Fact]
        public void CompressionBasic()
        {
            // Create diff and two uncompressable changes
            var diff = new Diff<int>();
            var initialChange1 = new ValueChange<int>((1, 2), 0, 1);
            var initialChange2 = new ValueChange<int>((5, 6), 0, 1);
            diff.Add(initialChange1);
            diff.Add(initialChange2);
            Assert.Equal(2, diff.Changes.Count);
            Assert.False(diff.IsCompressed);

            // Compress (which should not remove either change since they're not to the same position);
            // but it will mark the diff as fully compressed
            diff.Compress();
            Assert.Equal(2, diff.Changes.Count);
            Assert.True(diff.IsCompressed);

            // Add a compressable change
            var change1TrueToFalse = new ValueChange<int>((1, 2), 1, 0);
            diff.Add(change1TrueToFalse);
            Assert.Equal(3, diff.Changes.Count);
            Assert.False(diff.IsCompressed);

            // Compress (which should remove one position entirely since the changes offset)
            diff.Compress();
            Assert.Single(diff.Changes);
            Assert.True(diff.IsCompressed);
            Assert.Equal(diff.Changes[0], initialChange2);

            // Add a series of changes, 2 of which offset
            var uniquePositionChange = new ValueChange<int>((20, 20), 0, 1);
            var nonDuplicateForFirstPos = new ValueChange<int>((1, 2), 0, 2);
            diff.Add(initialChange1);
            diff.Add(uniquePositionChange);
            diff.Add(change1TrueToFalse);
            diff.Add(nonDuplicateForFirstPos);
            Assert.Equal(5, diff.Changes.Count);
            Assert.False(diff.IsCompressed);

            // Compress, which should remove the two offsetting changes, keep the most recent one, but not necessarily
            // preserve the relative ordering.
            diff.Compress();
            Assert.Equal(3, diff.Changes.Count);
            Assert.Equal(
                diff.Changes.ToHashSet(),
                new HashSet<ValueChange<int>>{initialChange2, uniquePositionChange, nonDuplicateForFirstPos});
            Assert.True(diff.IsCompressed);
        }

        [Fact]
        public void CompressionProducesMinimalSet()
        {

            // Create diff and add random changes that will have duplicates
            var diff = new Diff<int>();

            var positions = new Point[] { (1, 2), (5, 6), (20, 21) };
            var currentValues = new Dictionary<Point, int>();
            for (int i = 0; i < 100; i++)
            {
                foreach (var pos in positions)
                {
                    var oldVal = currentValues.GetValueOrDefault(pos);
                    var newVal = Random.GlobalRandom.DefaultRNG.Next(1, 6);
                    diff.Add(new ValueChange<int>(pos, oldVal, newVal));
                    currentValues[pos] = newVal;
                }
            }
            Assert.Equal(100 * positions.Length, diff.Changes.Count);

            Assert.False(diff.IsCompressed);

            // We know what the current values are; since they all started at 0 and got changed precisely once,
            // there will always be 3 changes
            diff.Compress();
            Assert.True(diff.IsCompressed);
            Assert.Equal(3, diff.Changes.Count);
            foreach (var change in diff.Changes)
            {
                Assert.True(currentValues.ContainsKey(change.Position));
                Assert.Equal(0, change.OldValue);
                Assert.Equal(currentValues[change.Position], change.NewValue);
            }
        }
    }

    public class DiffAwareMapViewTests
    {
        private ITestOutputHelper _output;

        public DiffAwareMapViewTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConstructionViaWrapper(bool autoCompress)
        {
            // Create the wrapper
            var arrayMap = new ArrayView<bool>(80, 25);
            var diffAwareMap = new DiffAwareGridView<bool>(arrayMap, autoCompress);

            // Validate beginning state
            Assert.Equal(autoCompress, diffAwareMap.AutoCompress);
            Assert.Equal(arrayMap, diffAwareMap.BaseGrid);
            Assert.Equal(arrayMap.Width, diffAwareMap.Width);
            Assert.Equal(arrayMap.Height, diffAwareMap.Height);
            Assert.Equal(0, diffAwareMap.CurrentDiffIndex);
            Assert.Empty(diffAwareMap.Diffs);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConstructionViaWidthHeight(bool autoCompress)
        {
            // Create the wrapper
            var diffAwareMap = new DiffAwareGridView<bool>(80, 25, autoCompress);

            // Validate beginning state
            Assert.Equal(autoCompress, diffAwareMap.AutoCompress);
            Assert.IsType<ArrayView<bool>>(diffAwareMap.BaseGrid);
            Assert.Equal(80, diffAwareMap.BaseGrid.Width);
            Assert.Equal(25, diffAwareMap.BaseGrid.Height);
            Assert.Equal(80, diffAwareMap.Width);
            Assert.Equal(25, diffAwareMap.Height);
            Assert.Equal(0, diffAwareMap.CurrentDiffIndex);
            Assert.Empty(diffAwareMap.Diffs);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetAndSetValuesToMap(bool autoCompress)
        {
            // Create a new map view and change 1 value
            var view = new DiffAwareGridView<bool>(80, 25, autoCompress)
            {
                [1, 2] = true
            };

            // Verify appropriate value changed
            Assert.True(view[1, 2]);
            Assert.False(view[5, 6]);

            // Verify change recorded appropriately
            Assert.Single(view.Diffs);
            Assert.Single(view.Diffs[0].Changes);
            var change = view.Diffs[0].Changes[0];
            Assert.Equal<Point>((1, 2), change.Position);
            Assert.False(change.OldValue);
            Assert.True(change.NewValue);

            // Change value 2
            view[5, 6] = true;

            // Verify appropriate value changed
            Assert.True(view[1, 2]);
            Assert.True(view[5, 6]);

            // Verify change recorded appropriately
            Assert.Single(view.Diffs);
            Assert.Equal(2, view.Diffs[0].Changes.Count);
            change = view.Diffs[0].Changes[1];
            Assert.Equal<Point>((5, 6), change.Position);
            Assert.False(change.OldValue);
            Assert.True(change.NewValue);

            // Change value 1 again
            view[1, 2] = false;

            // Verify appropriate value changed
            Assert.False(view[1, 2]);
            Assert.True(view[5, 6]);

            // Verify change recorded appropriately
            Assert.Single(view.Diffs);
            Assert.Equal(3, view.Diffs[0].Changes.Count);
            change = view.Diffs[0].Changes[2];
            Assert.Equal<Point>((1, 2), change.Position);
            Assert.True(change.OldValue);
            Assert.False(change.NewValue);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DiffRecordingAndTraversal(bool autoCompress)
        {
            // Create a new map view
            var view = new DiffAwareGridView<int>(80, 25, autoCompress);

            // Create 3 states for which we will record diffs
            var stateChangeSet1 = new Dictionary<Point, int>
            {
                [(1, 2)] = 1,
                [(5, 6)] = 2,
                [(15, 20)] = 3,
            };
            _output.WriteLine("State 1: " + stateChangeSet1.ExtendToString());

            var stateChangeSet2 = new Dictionary<Point, int>(stateChangeSet1)
            {
                [(25, 10)] = 4
            };
            _output.WriteLine("State 2: " + stateChangeSet2.ExtendToString());
            var stateChangeSet3 = new Dictionary<Point, int>(stateChangeSet2)
            {
                [(1, 2)] = 10,
                [(7, 8)] = 100,
            };
            _output.WriteLine("State 3: " + stateChangeSet3.ExtendToString());

            var forwardOrderStates = new List<Dictionary<Point, int>>
            {
                stateChangeSet1,
                stateChangeSet2,
                stateChangeSet3
            };

            // Apply each state to the view and record diffs as appropriate
            foreach (var state in forwardOrderStates)
            {
                foreach (var (pos, val) in state)
                    view[pos] = val;
                view.FinalizeCurrentDiff();
            }
            // 3 diffs with changes, but the active one hasn't been created yet
            Assert.Equal(3, view.Diffs.Count);
            Assert.Equal(3, view.CurrentDiffIndex);

            // Going to next diff should fail since there is no next diff
            Assert.Throws<InvalidOperationException>(view.ApplyNextDiff);
            CheckMapState(view, forwardOrderStates[^1]);

            // We should be able to go to all the previous diffs back to the start without exception
            // and the states should match the expected
            foreach (var prevState in Enumerable.Reverse(forwardOrderStates).Append(new Dictionary<Point, int>()))
            {
                view.RevertToPreviousDiff();
                CheckMapState(view, prevState);
            }

            // Validate we reverted through the last previous state
            Assert.Equal(-1, view.CurrentDiffIndex);

            // Since there are no previous states, the next call should fail
            Assert.Throws<InvalidOperationException>(view.RevertToPreviousDiff);

            // Next, traverse the states in the other direction (forward), using the next-or-finalize function
            foreach (var nextState in forwardOrderStates)
            {
                Assert.True(view.ApplyNextDiffOrFinalize());
                CheckMapState(view, nextState);
            }
            // One more will not apply any state, instead finalizing the last one that actually has changes
            Assert.False(view.ApplyNextDiffOrFinalize());

            // No new diffs should have been created, but we should have the one active that hasn't been created yet
            // (no changes)
            Assert.Equal(3, view.Diffs.Count);
            Assert.Equal(3, view.CurrentDiffIndex);

            // This call will not fail but should instead create a new diff, and finalize the current one (with no
            // changes)
            Assert.False(view.ApplyNextDiffOrFinalize());
            Assert.Equal(4, view.Diffs.Count);
            Assert.Equal(4, view.CurrentDiffIndex);
        }

        [AssertionMethod]
        private static void CheckMapState(IGridView<int> map, Dictionary<Point, int> fullChanges)
        {
            foreach (var pos in map.Positions())
                Assert.Equal(fullChanges.GetValueOrDefault(pos), map[pos]);
        }
    }


}
