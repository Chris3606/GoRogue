using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    [UsedImplicitly]
    internal class RegionRoutine : IRoutine
    {
        private class CenterOfRotation
        {
            internal Point Origin;

            public CenterOfRotation(Point origin)
            {
                Origin = origin;
            }
        }
        // Current amount to rotate _originalRegions by
        private double _rotation;

        // Original regions, and regions rotated by _rotation degrees, respectively
        private readonly List<RegionWithComponents> _originalRegions = new List<RegionWithComponents>();
        private readonly List<Region> _transformedRegions = new List<Region>();

        // _grid view set to indicate current state of each tile, so that it can be efficiently rendered.
        private readonly ArrayView<TileState> _map = new ArrayView<TileState>(80, 80);
        private readonly List<(string name, IGridView<char> view)> _views = new List<(string name, IGridView<char> view)>();
        /// <inheritdoc />
        public IReadOnlyList<(string name, IGridView<char> view)> Views => _views.AsReadOnly();

        /// <inheritdoc />
        public void InterpretKeyPress(int key) { } //

        /// <inheritdoc />
        public string Name => "Rotating Regions";

        /// <inheritdoc />
        public void CreateViews()
        {
            _views.Add(("Regions", new LambdaGridView<char>(_map.Width, _map.Height, RegionsView)));
        }

        /// <inheritdoc />
        public void NextTimeUnit()
        {
            // Revert all points in current regions back to walls
            RemoveRegionsFromMap();

            // Increase amount we rotate by
            _rotation += 5;

            // Remove transformed regions so we can transform them by a new amount
            _transformedRegions.Clear();

            // Rotate each original region by the new amount about its center, and add
            // it to the list of transformed regions
            foreach (RegionWithComponents region in _originalRegions)
            {
                var origin = region.GoRogueComponents.GetFirst<CenterOfRotation>().Origin;
                _transformedRegions.Add(region.Region.Rotate(_rotation, origin));
            }

            // Update map to reflect new regions
            ApplyRegionsToMap();
        }

        /// <inheritdoc />
        public void LastTimeUnit()
        {
            // Revert all points in current regions back to walls
            RemoveRegionsFromMap();

            // Decrease amount we rotate by
            _rotation -= 5;

            // Remove transformed regions so we can transform them by a new amount
            _transformedRegions.Clear();

            // Rotate each original region by the new amount about its center, and add
            // it to the list of transformed regions
            foreach (RegionWithComponents region in _originalRegions)
            {
                var origin = region.GoRogueComponents.GetFirst<CenterOfRotation>().Origin;
                _transformedRegions.Add(region.Region.Rotate(_rotation, origin));
            }
            // Update map to reflect new regions
            ApplyRegionsToMap();
        }

        /// <inheritdoc />
        public void GenerateMap()
        {
            // Initialize map for no regions
            foreach (var pos in _map.Positions())
                _map[pos] = TileState.Wall;

            for (int i = 0; i < 360; i += 45)
            {
                var center = (_map.Width / 2, _map.Height / 2);
                var region = Region.ParallelogramFromTopCorner(center, 15, 15).Rotate(i, center);
                var overRegion = new RegionWithComponents(region);
                overRegion.GoRogueComponents.Add(new CenterOfRotation(center));
                _originalRegions.Add(overRegion);
                _transformedRegions.Add(region);
            }

            // Update map values based on regions
            ApplyRegionsToMap();
        }

        // Update map to effectively revert all region positions back to walls
        private void RemoveRegionsFromMap()
        {
            foreach (var region in _transformedRegions)
                foreach (var point in region.Where(point => _map.Contains(point)))
                    _map[point] = TileState.Wall;
        }

        // Apply proper values to map, based on regions
        private void ApplyRegionsToMap()
        {
            foreach (var region in _transformedRegions)
            {
                foreach (var point in region.InnerPoints.Where(point => _map.Contains(point)))
                    _map[point] = TileState.InnerRegionPoint;

                foreach (var point in region.OuterPoints.Where(point => _map.Contains(point)))
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
