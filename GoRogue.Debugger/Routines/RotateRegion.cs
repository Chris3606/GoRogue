using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.Debugger.Routines
{
    /// <summary>
    /// Used to indicate the current state of each tile (whether it is within a region or not),
    /// for the sake of efficiently generating a view.
    /// </summary>
    internal enum TileState
    {
        InnerRegionPoint,
        OuterRegionPoint,
        Wall
    }

    [UsedImplicitly]
    internal class RotateRegion : IRoutine
    {
        // Current amount to rotate _originalRegions by
        private double _rotation;

        // Original regions, and regions rotated by _rotation degrees, respectively
        private readonly List<Region> _originalRegions = new List<Region>();
        private readonly List<Region> _transformedRegions = new List<Region>();

        // Map view set to indicate current state of each tile, so that it can be efficiently rendered.
        private readonly ArrayMap<TileState> _map = new ArrayMap<TileState>(500, 500);

        private readonly List<(string name, IMapView<char> view)> _views = new List<(string name, IMapView<char> view)>();
        /// <inheritdoc />
        public IReadOnlyList<(string name, IMapView<char> view)> Views => _views.AsReadOnly();

        /// <inheritdoc />
        public string Name => "Rotating Regions";

        /// <inheritdoc />
        public void CreateViews()
        {
            _views.Add(("Regions", new LambdaMapView<char>(_map.Width, _map.Height, RegionsView)));
        }

        /// <inheritdoc />
        public void ElapseTimeUnit()
        {
            // Revert all points in current regions back to walls
            RemoveRegionsFromMap();

            // Increase amount we rotate by
            _rotation += 5;

            // Remove transformed regions so we can transform them by a new amount
            _transformedRegions.Clear();

            // Rotate each original region by the new amount about its center, and add
            // it to the list of transformed regions
            foreach (Region region in _originalRegions)
                _transformedRegions.Add(region.Rotate(_rotation, region.Center));

            // Update map to reflect new regions
            ApplyRegionsToMap();
        }

        /// <inheritdoc />
        public void GenerateMap()
        {
            // Initialize map for no regions
            foreach (var pos in _map.Positions())
                _map[pos] = TileState.Wall;

            // Generate some regions of various shapes
            for (int x = 0; x < _map.Width; x += 50)
            {
                for (int y = 0; y < _map.Height; y += 50)
                {
                    Point here = (x, y);
                    Region region = GlobalRandom.DefaultRNG.Next(4) switch
                    {
                        0 => new Region("arbitrary", (2, 2) + here, (35, 35) + here, (49, 49) + here, (35, 14) + here),
                        1 => Region.RegularParallelogram("parallelogram", (2, 2) + here, 40, 40, 75),
                        2 => Region.Rectangle("square", (2, 2) + here, 48, 48),
                        3 => new Region("triangle", (2, 2) + here, (2, 2) + here, (2, 49) + here, (42, 24) + here),
                        _ => throw new Exception("Invalid selection for region type.")
                    };
                    _originalRegions.Add(region);
                }
            }

            // The transformed regions are the same as the original to start with
            _transformedRegions.AddRange(_originalRegions);

            // Update map values based on regions
            ApplyRegionsToMap();
        }

        // Update map to effectively revert all region positions back to walls
        private void RemoveRegionsFromMap()
        {
            foreach (var region in _transformedRegions)
                foreach (var point in region.Points.Where(point => _map.Contains(point)))
                    _map[point] = TileState.Wall;
        }

        // Apply proper values to map, based on regions
        private void ApplyRegionsToMap()
        {
            foreach (var region in _transformedRegions)
            {
                foreach (var point in region.InnerPoints.Positions.Where(point => _map.Contains(point)))
                    _map[point] = TileState.InnerRegionPoint;

                foreach (var point in region.OuterPoints.Positions.Where(point => _map.Contains(point)))
                    _map[point] = TileState.OuterRegionPoint;
            }
        }

        // Translate point to character based on whether or not the point is in the InnerPoints of a region,
        // in the OuterPoints of a region, or not in any region
        private char RegionsView(Point pos)
            => _map[pos] switch
            {
                TileState.Wall => ' ',
                TileState.InnerRegionPoint => '.',
                TileState.OuterRegionPoint => '#',
                _ => throw new Exception("Regions view encountered unsupported tile settings.")
            };
    }
}
