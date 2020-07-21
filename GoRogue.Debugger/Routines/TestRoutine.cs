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
        public Map? Map { get; private set; }
        private double _rotation = 0;

        private readonly List<Region> _regions = new List<Region>();
        public IEnumerable<Region> Regions => _regions;
        public string Name { get; }

        public TestRoutine()
        {
            Name = "TestRoutine";
        }

        public void ElapseTimeUnit()
        {
            // Set tiles in current regions back to start (so whole map is blank)
            RemoveRegionsFromMap();

            // Rotate all regions
            _rotation += 5;
            foreach (Region region in _regions)
                region.Rotate(_rotation, true, region.Center);

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
            _regions.Add(region);
            /*
            region = new Region("rhombus", (256,256),(202, 128),(2,2),(128, 202));
            _regions.Add(region);
            region = Region.RegularParallelogram("parallelogram", (256,256),40,40, 75);
            _regions.Add(region);
            */

            // Apply regions to map data
            ApplyRegionsToMap();
        }

        // Apply proper settings to terrain tiles in regions
        private void ApplyRegionsToMap()
        {
            Debug.Assert(Map != null);

            foreach (var region in _regions)
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

            foreach (var region in _regions)
                foreach (var point in region.Points)
                {
                    Map.Terrain[point]!.IsWalkable = true;
                    Map.Terrain[point]!.IsTransparent = false;
                }
        }
    }
}
