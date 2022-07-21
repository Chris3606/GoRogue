using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.SpatialMaps
{
    public class MultiSpatialMapOperations
    {
        private readonly Point _initialPosition = (0, 1);
        private readonly Point _moveToPosition = (5, 6);
        private readonly Point _addPosition = (1, 1);
        private readonly IDObject _addedObject = new IDObject();
        private readonly IDObject _trackedObject = new IDObject();
        private readonly int _width = 10;
        private MultiSpatialMap<IDObject> _testMap = null!;

        [UsedImplicitly]
        [Params(1, 10, 50, 100)]
        public int NumEntities;

        [GlobalSetup(Targets = new[] { nameof(MoveTwice), nameof(TryMoveTwiceOriginal), nameof(TryMoveTwice), nameof(AddAndRemove), nameof(TryAddAndRemoveOriginal), nameof(TryAddAndRemove) })]
        public void GlobalSetupObjectsAtMoveToLocation()
        {
            _testMap = new MultiSpatialMap<IDObject> { { _trackedObject, _initialPosition } };

            // Put other entities on the map
            int idx = -1;
            while (_testMap.Count < NumEntities)
            {
                idx += 1;
                _testMap.Add(new IDObject(), Point.FromIndex(idx, _width));
            }
        }

        [GlobalSetup(Targets = new[] { nameof(MoveAllTwice), nameof(MoveValidTwice), nameof(TryMoveAllTwice) })]
        public void GlobalSetupNoObjectsAtMoveToLocation()
        {
            _testMap = new MultiSpatialMap<IDObject> { { _trackedObject, _initialPosition } };

            // Put other entities on the map, avoiding the starting point
            int idx = -1;
            while (_testMap.Count < NumEntities)
            {
                idx += 1;
                var point = Point.FromIndex(idx, _width);
                if (point != _moveToPosition)
                    _testMap.Add(new IDObject(), Point.FromIndex(idx, _width));
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
        public int TryMoveAllTwice()
        {
            _testMap.TryMoveAll(_initialPosition, _moveToPosition);
            // Move it back to not spoil next benchmark.  Valid since the GlobalSetup function used for this benchmark
            // doesn't put anything at _moveToPosition in the initial state.
            _testMap.TryMoveAll(_moveToPosition, _initialPosition);
            return _testMap.Count; // Ensure nothing is optimized out
        }

        [Benchmark]
        public (List<IDObject> l1, List<IDObject> l2) MoveValidTwice()
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
