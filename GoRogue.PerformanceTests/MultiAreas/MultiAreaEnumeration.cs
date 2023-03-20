using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives.PointHashers;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.MultiAreas
{
    public class MultiAreaEnumeration
    {
        [UsedImplicitly]
#pragma warning disable CA1822 // Mark members as static
        public IEnumerable<int> SizeData => SharedTestParams.Sizes;

        [UsedImplicitly]
        public IEnumerable<int> NumAreasData => SharedTestParams.NumAreas;
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        /// An area of Size x Size will be used for partitioning into areas.
        /// </summary>
        [UsedImplicitly]
        [ParamsSource(nameof(SizeData))]
        public int Size;

        /// <summary>
        /// Number of areas to split the map area into, to place in the MultiAreas for testing.
        /// </summary>
        [UsedImplicitly]
        [ParamsSource(nameof(NumAreasData))]
        public int NumAreas;

        private MultiArea _multiArea = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Use KnownSizeHasher so that the hashing algorithm is very efficient for the use case
            var areas = new Area[NumAreas];
            for (int i = 0; i < areas.Length; i++)
                areas[i] = new Area(new KnownSizeHasher(Size));

            var divisor = Size * Size / NumAreas;
            for (int i = 0; i < Size * Size; i++)
            {
                // Ensure we cap at the max index, to avoid rounding error if the divisor doesn't evenly distribute
                int idx = Math.Min(i / divisor, NumAreas - 1);
                areas[idx].Add(Point.FromIndex(i, Size));
            }

            _multiArea = new MultiArea(areas);
        }

        [Benchmark]
        public int EnumeratePositions()
        {
            int sum = 0;
            foreach (var pos in _multiArea)
                sum += pos.X + pos.Y;

            return sum;
        }

        [Benchmark]
        public int EnumeratePositionsFastEnumerator()
        {
            int sum = 0;
            foreach (var pos in new ReadOnlyAreaPositionsEnumerator(_multiArea))
                sum += pos.X + pos.Y;

            return sum;
        }
    }
}
