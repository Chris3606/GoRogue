using System;
using BenchmarkDotNet.Attributes;
using GoRogue.FOV;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.Pathing;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.PerformanceTests.GameFramework
{
    /// <summary>
    /// Performance tests for various Map operations
    /// </summary>
    public class OpenMapTests
    {
        [UsedImplicitly]
        [Params(50, 100, 200)]
        public int Size;

        [UsedImplicitly]
        [Params(2, 10, 30)]
        public int NumEntitySpawnLocations;

        [UsedImplicitly]
        [Params(1, 2, 3)]
        public int NumEntitiesPerLocation;

        [UsedImplicitly]
        [Params(1, 2, 3)]
        public int NumEntityLayers;

        private Map _map = null!;
        private Point _positionWithEntities;
        private Point _positionWithoutEntities;
        private Point _center;

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Use GoRogue map generation to generate terrain data
            var wallFloor = new Generator(Size, Size)
                .ConfigAndGenerateSafe(gen => gen.AddSteps(DefaultAlgorithms.RectangleMapSteps()))
                .Context.GetFirst<IGridView<bool>>("WallFloor");

            // Create real map and apply terrain
            _map = new Map(Size, Size, NumEntityLayers, Distance.Chebyshev);
            _map.ApplyTerrainOverlay(wallFloor, (pos, val) => new GameObject(pos, 0, val, val));

            // Spawn correct number of entities
            float spawnRegion = (float)Size - 2; // Range (1, width - 1)
            float increment = spawnRegion / NumEntitySpawnLocations;

            int spawns = 0;
            for (float i = increment; i < Size - 1; i += increment)
            {
                int val = (int)i;
                var position = new Point(val, val);
                for (int j = 0; j < NumEntitiesPerLocation; j++)
                {
                    var entity = new GameObject(position, j % NumEntityLayers + 1, j != 0);
                    _map.AddEntity(entity);
                }

                // Record two positions (with and without entities) we can test against
                _positionWithEntities = position;
                _positionWithoutEntities = position - 1;

                // Sanity check
                spawns++;
            }

            if (spawns != NumEntitySpawnLocations)
                throw new Exception($"Incorrect number of entity spawn locations.  Got: {spawns}, but wanted: {NumEntitySpawnLocations}");

            // Record center for caching purposes
            _center = _map.Bounds().Center;
        }

        [Benchmark]
        public bool WalkabilityWithEntities()
        {
            return _map.WalkabilityView[_positionWithEntities];
        }

        [Benchmark]
        public bool WalkabilityWithoutEntities()
        {
            return _map.WalkabilityView[_positionWithoutEntities];
        }

        [Benchmark]
        public bool TransparencyWithEntities()
        {
            return _map.TransparencyView[_positionWithEntities];
        }

        [Benchmark]
        public bool TransparencyWithoutEntities()
        {
            return _map.TransparencyView[_positionWithoutEntities];
        }

        [Benchmark]
        public IFOV FOVFromCenter()
        {
            _map.PlayerFOV.Calculate(_center, 10, _map.DistanceMeasurement);
            return _map.PlayerFOV;
        }

        [Benchmark]
        public Path? AStarAcrossMap()
        {
            return _map.AStar.ShortestPath(new Point(1, 1), new Point(_map.Width - 1, _map.Height - 1));
        }

        [Benchmark]
        public uint GetObjectsAtWithEntities()
        {
            uint sum = 0;
            unchecked
            {
                foreach (var entity in _map.GetObjectsAt(_positionWithEntities))
                    sum += entity.ID;
            }

            return sum;
        }

        [Benchmark]
        public uint GetObjectsAtWithoutEntities()
        {
            uint sum = 0;
            unchecked
            {
                foreach (var entity in _map.GetObjectsAt(_positionWithoutEntities))
                    sum += entity.ID;
            }

            return sum;
        }
    }
}
