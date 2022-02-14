using BenchmarkDotNet.Attributes;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.SpatialMaps
{
    public class SpatialMapOperations
    {
        private readonly Point _initialPosition = (0, 1);
        private readonly Point _moveToPosition = (5, 6);
        private readonly Point _addPosition = (1, 1);
        private readonly IDObject _addedObject = new IDObject();
        private readonly IDObject _trackedObject = new IDObject();
        private readonly int _width = 10;
        private SpatialMap<IDObject> _testMap = null!;

        [UsedImplicitly]
        [Params(1, 10, 50, 100)]
        public int NumEntities;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _testMap = new SpatialMap<IDObject> { { _trackedObject, _initialPosition } };

            // Put other entities on the map, steering clear of the three points we need to remain clear to support
            // benchmarked adds/removes.
            int idx = -1;
            while (_testMap.Count < NumEntities)
            {
                idx += 1;

                if (idx == _initialPosition.ToIndex(_width) || idx == _moveToPosition.ToIndex(_width) || idx == _addPosition.ToIndex(_width)) continue;
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
