using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using GoRogue.MapViews;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests
{
    internal struct Change
    {
        public Point Position;
        public int Value;
    }

    /// <summary>
    /// Performance tests for diff-aware map view operations.
    /// </summary>
    public class MapViewDiffMethods
    {
        // Number of changes per position to generate
        [UsedImplicitly]
        [Params(5, 10, 25, 50, 100, 1000, 10000)]
        public int NumberOfDiffs;

        // Number of positions to generate changes for
        [UsedImplicitly]
        [Params(1, 2, 4, 8, 16, 32)]
        public int ChangesPerPositionInEachDiff;

        // Size of maps to test with
        private readonly Point _mapSize = (500, 500);

        // Arbitrary positions to change values for
        private readonly List<Point> _positionsToChange = new List<Point> { (1, 2), (5, 6), (15, 20) };

        // Change sets to apply
        private readonly List<List<Change>> _changeSets = new List<List<Change>>();

        private DiffAwareGridView<int>? _regularStartingMapView;

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Generate change sets to use
            for (int i = 0; i < NumberOfDiffs; i++)
            {
                _changeSets.Add(new List<Change>());
                foreach (var pos in _positionsToChange)
                {
                    for (int j = 0; j < ChangesPerPositionInEachDiff; j++)
                        _changeSets[^1].Add(
                            new Change
                            {
                                Position = pos,
                                Value = GlobalRandom.DefaultRNG.Next(1, 6)
                            });
                }
            }

            // Pre-configure maps to use for non-add tests
            _regularStartingMapView = new DiffAwareGridView<int>(_mapSize.X, _mapSize.Y);
            foreach (var changeSet in _changeSets)
            {
                foreach (var change in changeSet)
                    _regularStartingMapView[change.Position] = change.Value;

                _regularStartingMapView.FinalizeCurrentDiff();
            }
        }

        [Benchmark]
        public IGridView<int> CreateViewAndDiffsCompressionMethod()
        {
            var view = new DiffAwareGridView<int>(_mapSize.X, _mapSize.Y);

            foreach (var changeSet in _changeSets)
            {
                foreach (var change in changeSet)
                    view[change.Position] = change.Value;

                view.FinalizeCurrentDiff();
            }

            return view;
        }

        [Benchmark]
        public IGridView<int> TraverseViewsCompressionMethod()
        {
            // Go from start to beginning
            while (_regularStartingMapView!.CurrentDiffIndex != -1)
                _regularStartingMapView.RevertToPreviousDiff();

            // Go from beginning to end again
            while (_regularStartingMapView!.CurrentDiffIndex != _regularStartingMapView.Diffs.Count - 1)
                _regularStartingMapView.ApplyNextDiff();

            return _regularStartingMapView;
        }
    }
}
