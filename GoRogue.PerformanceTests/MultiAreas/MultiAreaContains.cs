using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.PointHashers;

namespace GoRogue.PerformanceTests.MultiAreas
{
    public class MultiAreaContains
    {
        public IEnumerable<int> SizeData => SharedTestParams.Sizes;
        public IEnumerable<int> NumAreasData => SharedTestParams.NumAreas;

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
        public int CheckForExisting()
        {
            int sum = 0;
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    if (_multiArea.Contains(new Point(x, y)))
                        sum++;

            return sum;
        }

        [Benchmark]
        public int CheckForNonExisting()
        {
            int sum = 0;
            for (int y = Size; y < Size * 2; y++)
                for (int x = Size; x < Size * 2; x++)
                    if (_multiArea.Contains(new Point(x, y)))
                        sum++;

            return sum;
        }
    }
}
