using System;
using System.Collections.Generic;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using GoRogue.Debugger.Implementations.GameObjects;
using SadRogue.Primitives;

namespace GoRogue.Debugger.Routines
{
    internal class TestRoutine : IRoutine
    {
        private Map _baseMap;
        private Map _transformedMap;
        private List<Region> _currentRegions = new List<Region>();
        private List<Region> _originalRegions = new List<Region>();
        public Map BaseMap => _baseMap;
        private double _rotation = 0;
        public Map TransformedMap => _transformedMap;
        public IEnumerable<Region> Regions => _currentRegions;
        public string Name { get; set; }

        public TestRoutine()
        {
            Name = "TestRoutine";
        }

        public Map ElapseTimeUnit()
        {
            _rotation += 5;
            _transformedMap = new Map(500, 500, 31, Distance.Euclidean);
            _currentRegions.Clear();
            foreach (Region region in _originalRegions)
            {
                _currentRegions.Add(region.Rotate(_rotation, false, region.Center));
            }

            SetTerrain(_transformedMap);
            return _transformedMap;
        }

        public Map GenerateMap()
        {
            _baseMap = new Map(500, 500, 31, Distance.Euclidean);

            // Region region = new Region("rhombus", (256,256),(202, 128),(2,2),(128, 202));
            // _originalRegions.Add(region);
            // region = Region.RegularParallelogram("parallelogram", (256,256),40,40, 75);
            // _originalRegions.Add(region);
            Region region = Region.Rectangle("square", (256, 256), 48, 48);
            _originalRegions.Add(region);
            SetTerrain(_baseMap);

            _transformedMap = _baseMap;
            return _baseMap;
        }

        private void SetTerrain(Map map)
        {
            foreach (Point p in BaseMap.Positions())
                map.SetTerrain(new Terrain(p, true, false));

            foreach (Region region in _currentRegions)
            {
                foreach (Point p in region.InnerPoints)
                    map.SetTerrain(new Terrain(p, true, true));

                foreach (Point p in region.OuterPoints)
                    map.SetTerrain(new Terrain(p, false, false));
            }
        }
    }
}
