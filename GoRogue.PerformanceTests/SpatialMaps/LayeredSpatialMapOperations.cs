using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.SpatialMaps
{
    /// <summary>
    /// Basic benchmarks for LayeredSpatialMap, roughly equivalent to the benchmarks for other spatial map implementations.
    /// </summary>
    /// <remarks>
    /// These benchmarks make all layers MultiSpatialMaps (eg. support multiple items), to represent a more or less
    /// "worst case" for memory usage performance of many functions, and secondarily to replicate what is found by
    /// default in GoRogue's Map.  There is some benefit to testing SpatialMap as well (to see how much the overhead
    /// of LayeredSpatialMap affects an underlying layer implementation that takes less overall time than MultiSpatialMap,
    /// however for simplicity, these tests ignore that case.
    /// </remarks>
    public class LayeredSpatialMapOperations
    {
        private readonly Point _initialPosition = (0, 1);
        private readonly Point _moveToPosition = (5, 6);
        private readonly Point _addPosition = (1, 1);
        private readonly IDLayerObject _addedObject = new(0);
        private readonly IDLayerObject _trackedObject = new(0);
        private readonly int _width = 10;
        private LayeredSpatialMap<IDLayerObject> _testMap = null!;

        [UsedImplicitly]
        [Params(1, 10, 50, 100)]
        public int NumEntities;

        [UsedImplicitly]
        [Params(1, 2, 3)]
        public int NumLayers;

        [GlobalSetup(Targets = new []{nameof(MoveTwice), nameof(TryMoveTwiceOriginal), nameof(TryMoveTwice),nameof(AddAndRemove), nameof(TryAddAndRemoveOriginal), nameof(TryAddAndRemove)})]
        public void GlobalSetupObjectsAtMoveToLocation()
        {
            _testMap = new LayeredSpatialMap<IDLayerObject>(NumLayers, layersSupportingMultipleItems: uint.MaxValue) { { _trackedObject, _initialPosition } };

            // Put other entities on the map (on each layer)
            for (int i = 0; i < NumLayers; i++)
            {
                int idx = -1;
                var layer = _testMap.GetLayer(i);
                while (layer.Count < NumEntities)
                {
                    idx += 1;
                    _testMap.Add(new IDLayerObject(i), Point.FromIndex(idx, _width));
                }
            }

        }

        [GlobalSetup(Targets = new []{nameof(MoveAllTwice), nameof(MoveValidTwice)})]
        public void GlobalSetupNoObjectsAtMoveToLocation()
        {
            _testMap = new LayeredSpatialMap<IDLayerObject>(NumLayers, layersSupportingMultipleItems: uint.MaxValue) { { _trackedObject, _initialPosition } };

            // Put other entities on the map, avoiding the starting point (on each layer)
            for (int i = 0; i < NumLayers; i++)
            {
                int idx = -1;
                var layer = _testMap.GetLayer(i);
                while (layer.Count < NumEntities)
                {
                    idx += 1;
                    var point = Point.FromIndex(idx, _width);
                    if (point != _moveToPosition)
                        _testMap.Add(new IDLayerObject(i), Point.FromIndex(idx, _width));
                }
            }
        }

        [Benchmark]
        public int MoveTwice()
        {
            _testMap.Move(_trackedObject, _moveToPosition);
            _testMap.Move(_trackedObject, _initialPosition); // Move it back to not spoil next benchmark
            return _testMap.Count; // Ensure nothing is optimized out
        }

        [Benchmark]
        public int TryMoveTwiceOriginal()
        {
            if (_testMap.CanMove(_trackedObject, _moveToPosition))
            {
                _testMap.Move(_trackedObject, _moveToPosition);
                _testMap.Move(_trackedObject, _initialPosition);
            }

            return _testMap.Count;
        }

        [Benchmark]
        public int TryMoveTwice()
        {
            _testMap.TryMove(_trackedObject, _moveToPosition);
            _testMap.TryMove(_trackedObject, _initialPosition);

            return _testMap.Count;
        }

        [Benchmark]
        public int MoveAllTwice()
        {
            _testMap.MoveAll(_initialPosition, _moveToPosition);
            // Move it back to not spoil next benchmark.  Valid since the GlobalSetup function used for this benchmark
            // doesn't put anything at _moveToPosition in the initial state.
            _testMap.MoveAll(_moveToPosition, _initialPosition);
            return _testMap.Count; // Ensure nothing is optimized out
        }

        [Benchmark]
        public (List<IDLayerObject> l1, List<IDLayerObject> l2) MoveValidTwice()
        {
            var list1 = _testMap.MoveValid(_initialPosition, _moveToPosition);
            // Move it back to not spoil next benchmark.  Valid since the GlobalSetup function used for this benchmark
            // doesn't put anything at _moveToPosition in the initial state.
            var list2 = _testMap.MoveValid(_moveToPosition, _initialPosition);
            return (list1, list2); // Ensure nothing is optimized out
        }

        [Benchmark]
        public int AddAndRemove()
        {
            _testMap.Add(_addedObject, _addPosition);
            _testMap.Remove(_addedObject); // Must remove as well to avoid spoiling next invocation

            return _testMap.Count;
        }

        [Benchmark]
        public int TryAddAndRemoveOriginal()
        {
            if (_testMap.CanAdd(_addedObject, _addPosition))
            {
                _testMap.Add(_addedObject, _addPosition);
                _testMap.Remove(_addedObject);
            }

            return _testMap.Count;
        }

        [Benchmark]
        public int TryAddAndRemove()
        {
            _testMap.TryAdd(_addedObject, _addPosition);
            _testMap.TryRemove(_addedObject);

            return _testMap.Count;
        }
    }
}
