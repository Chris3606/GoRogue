using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using GoRogue.PerformanceTests.MapAreaFinders.Implementations;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.PerformanceTests.MapAreaFinders
{
    public class FindMapAreasOpenMapTests
    {
        private ArrayView<bool> _rects = null!;

        [UsedImplicitly]
        [Params(10, 100)]
        public int Size;


        [UsedImplicitly]
        [Params(AdjacencyRule.Types.Cardinals, AdjacencyRule.Types.EightWay)]
        public AdjacencyRule.Types NeighborRule;

        [GlobalSetup]
        public void GlobalSetup()
        {

            // Make two rectangles
            _rects = new ArrayView<bool>(Size, Size);
            _rects.Fill(true);

            var sects = _rects.Bounds().BisectVertically().ToArray();

            foreach (var rect in sects)
            {
                foreach (var pos in rect.PerimeterPositions())
                    _rects[pos] = false;
            }
        }


        [Benchmark]
        public List<Area> GoRogueCurrentAreaFinder()
        {
            return MapAreaFinder.MapAreasFor(_rects, NeighborRule).ToList();
        }

        [Benchmark]
        public List<Area> OriginalAreaFinder()
        {
            return OriginalMapAreaFinder.MapAreasFor(_rects, NeighborRule).ToList();
        }

        [Benchmark]
        public List<Area> OriginalSizeHashAreaFinder()
        {
            return OriginalSizeHashMapAreaFinder.MapAreasFor(_rects, NeighborRule).ToList();
        }


        [Benchmark]
        public List<Area> AreaContainsDefaultHashAreaFinder()
        {
            return AreaContainsDefaultHashMapAreaFinder.MapAreasFor(_rects, NeighborRule).ToList();
        }


        [Benchmark]
        public List<Area> AreaContainsSizeHashAreaFinder()
        {
            return AreaContainsSizeHashMapAreaFinder.MapAreasFor(_rects, NeighborRule).ToList();
        }


        [Benchmark]
        public List<Area> HashSetDefaultHashAreaFinder()
        {
            return HashSetDefaultHashMapAreaFinder.MapAreasFor(_rects, NeighborRule).ToList();
        }

        [Benchmark]
        public List<Area> HashSetSizeHashAreaFinder()
        {
            return HashSetSizeHashMapAreaFinder.MapAreasFor(_rects, NeighborRule).ToList();
        }
    }
}
