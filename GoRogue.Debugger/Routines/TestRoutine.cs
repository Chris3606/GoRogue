using System.Collections.Generic;
using System.Diagnostics;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using GoRogue.Debugger.Implementations.GameObjects;
using SadRogue.Primitives;

namespace GoRogue.Debugger.Routines
{
    internal class TestRoutine : IRoutine
    {
        public Map Map { get; private set; } = null!; // Null override because it won't be null after GenerateMap
        private double _rotation;

        private readonly List<Region> _originalRegions = new List<Region>();

        private readonly List<Region> _transformedRegions = new List<Region>();
        public IEnumerable<Region> Regions => _transformedRegions;
        public string Name => "TestRoutine";

        private readonly List<(string name, IMapView<char> view)> _views = new List<(string name, IMapView<char> view)>();
        public IReadOnlyList<(string name, IMapView<char> view)> Views => _views.AsReadOnly();


        public void ElapseTimeUnit()
        {
            // Set tiles in current regions back to start (so whole map is blank)
            RemoveRegionsFromMap();

            // Increase rotation
            _rotation += 5;

            // Remove transformed regions so we can replace them
            _transformedRegions.Clear();

            // Rotate each original region by the new amount and add to the list.
            foreach (Region region in _originalRegions)
                _transformedRegions.Add(region.Rotate(_rotation, false, region.Center));

            // Apply tile settings to tiles now in region
            ApplyRegionsToMap();
        }

        public void GenerateMap()
        {
            // Create map and initialize tiles
            Map = new Map(500, 500, 31, Distance.Euclidean);
            foreach (var pos in Map.Positions())
                Map.SetTerrain(new Terrain(pos, true, false));


            // Generate regions
            Region region = Region.Rectangle("square", (256, 256), 48, 48);
            _originalRegions.Add(region);
            _transformedRegions.Add(region);
            /*
            region = new Region("rhombus", (256,256),(202, 128),(2,2),(128, 202));
            _originalRegions.Add(region);
            _transformedRegions.Add(region);
            region = Region.RegularParallelogram("parallelogram", (256,256),40,40, 75);
            _originalRegions.Add(region);
            _transformedRegions.Add(region);
            */

            // Apply regions to map data
            ApplyRegionsToMap();
        }

        public void CreateViews()
        {
            Debug.Assert(Map != null);

            _views.Add(("Regions", new LambdaMapView<char>(Map.Width, Map.Height, RegionsView)));
        }

        // Apply proper settings to terrain tiles in regions
        private void ApplyRegionsToMap()
        {
            Debug.Assert(Map != null);

            foreach (var region in _transformedRegions)
            {
                foreach (var point in region.InnerPoints)
                {
                    Map.Terrain[point]!.IsWalkable = true;
                    Map.Terrain[point]!.IsTransparent = true;
                }

                foreach (var point in region.OuterPoints)
                {
                    Map.Terrain[point]!.IsWalkable = false;
                    Map.Terrain[point]!.IsTransparent = false;
                }
            }
        }

        // Revert all tiles in current regions back to start
        private void RemoveRegionsFromMap()
        {
            Debug.Assert(Map != null);

            foreach (var region in _transformedRegions)
                foreach (var point in region.Points)
                {
                    Map.Terrain[point]!.IsWalkable = true;
                    Map.Terrain[point]!.IsTransparent = false;
                }
        }

        private char RegionsView(Point pos)
        {
            Debug.Assert(Map != null);
            var terrain = Map.Terrain[pos];
            if (terrain == null)
                return '?';

            return terrain.IsWalkable ? (terrain.IsTransparent ? '.' : 'I') : terrain.IsTransparent ? '-' : '#';
        }
    }
}
