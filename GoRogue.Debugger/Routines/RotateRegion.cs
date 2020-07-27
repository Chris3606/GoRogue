using System.Collections.Generic;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.Debugger.Routines
{
    internal class RotateRegion : IRoutine
    {
        private Map? _baseMap;
        private Map? _transformedMap;
        private List<Region> _currentRegions = new List<Region>();
        private List<Region> _originalRegions = new List<Region>();
        public Map? BaseMap => _baseMap;
        private double _rotation = 0;
        public Map? TransformedMap => _transformedMap;
        public IEnumerable<Region> Regions => _currentRegions;
        public string Name { get; set; }

        public RotateRegion()
        {
            Name = "Rotating Regions";
        }

        public Map ElapseTimeUnit()
        {
            _rotation += 5;
            _transformedMap = new Map(500, 500, 31, Distance.Euclidean);
            _currentRegions.Clear();
            foreach (Region region in _originalRegions)
            {
                _currentRegions.Add(region.Rotate(_rotation, region.Center));
            }

            SetTerrain(_transformedMap);
            return _transformedMap;
        }

        public Map GenerateMap()
        {
            _baseMap = new Map(500, 500, 31, Distance.Euclidean);

            System.Random r = new System.Random();
            for (int i = 0; i < _baseMap.Width; i += 50)
            {
                for (int j = 0; j < _baseMap.Height; j += 50)
                {
                    Point here = (i, j);
                    switch (r.Next(1,4))
                    {
                        case 0:
                            _originalRegions.Add(new Region("arbitrary",(2,2) + here,(35, 35) + here, (49,49) + here, (35, 14) + here));
                            break;
                        case 1:
                            _originalRegions.Add(Region.RegularParallelogram("parallelogram", (2,2) + here,40,40, 75));
                            break;
                        case 2:
                            _originalRegions.Add(Region.Rectangle("square", (2, 2) + here, 48, 48));
                            break;
                        case 3:
                            _originalRegions.Add(new Region("triangle",(2,2) + here,(2,2) + here, (2,49) + here, (42, 24) + here));
                            break;
                    }
                }
            }
            SetTerrain(_baseMap);
            _transformedMap = _baseMap;
            return _baseMap;
        }

        private void SetTerrain(Map map)
        {
            foreach (Region region in _currentRegions)
            {
                foreach (Point p in region.InnerPoints.Positions)
                {
                    if (map.Contains(p))
                        map.SetTerrain(new GameObject(p, 0, null, true, true));
                }

                foreach (Point p in region.OuterPoints.Positions)
                {
                    if(map.Contains(p))
                        map.SetTerrain(new GameObject(p, 0, null, false, false));
                }
            }
        }
    }
}
