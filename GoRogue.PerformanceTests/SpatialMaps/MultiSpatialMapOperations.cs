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
        private readonly IDObject _trackedObject = new IDObject();
        private readonly int _width = 10;
        private MultiSpatialMap<IDObject> _moveMap = null!;

        [UsedImplicitly]
        [Params(1, 10, 50, 100)]
        public int NumEntities;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _moveMap = new MultiSpatialMap<IDObject> { { _trackedObject, _initialPosition } };

            // Put other entities on the map
            int idx = -1;
            while (_moveMap.Count < NumEntities)
            {
                idx += 1;
                _moveMap.Add(new IDObject(), Point.FromIndex(idx, _width));
            }
        }

        [Benchmark]
        public int MoveTwice()
        {
            _moveMap.Move(_trackedObject, _moveToPosition);
            _moveMap.Move(_trackedObject, _initialPosition); // Move it back to not spoil next benchmark
            return _moveMap.Count; // Ensure nothing is optimized out
        }

        // [Benchmark]
        // public int TryMoveTwiceOriginal()
        // {
        //     if (_moveMap.CanMove(_trackedObject, _moveToPosition))
        //     {
        //         _moveMap.Move(_trackedObject, _moveToPosition);
        //         _moveMap.Move(_trackedObject, _initialPosition);
        //     }
        //
        //     return _moveMap.Count;
        // }
        //
        // [Benchmark]
        // public int TryMoveTwice()
        // {
        //     _moveMap.TryMove(_trackedObject, _moveToPosition);
        //     _moveMap.TryMove(_trackedObject, _initialPosition);
        //
        //     return _moveMap.Count;
        // }
    }
}
