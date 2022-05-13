using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    //Enums for quickly swapping between the map data at a place and a char
    public enum RegionTileState
    {
        Exterior, //exterior to the entire region
        Inner,
        Outer,
        Corner,
    }

    [UsedImplicitly]
    internal class RotationRoutine : IRoutine
    {
        // Current amount to rotate _originalRegions by
        private double _rotation;

        // Original regions, and regions rotated by _rotation degrees, respectively
        private readonly List<PolygonArea> _originalRegions = new List<PolygonArea>();
        private readonly List<PolygonArea> _transformedRegions = new List<PolygonArea>();

        // _grid view set to indicate current state of each tile, so that it can be efficiently rendered.
        private readonly ArrayView<RegionTileState> _map = new ArrayView<RegionTileState>(80, 80);
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
            foreach (var polygon in _originalRegions)
            {
                var rotated = polygon.Rotate(_rotation, (40,40));
                _transformedRegions.Add(rotated);
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
            foreach (var polygon in _originalRegions)
                _transformedRegions.Add(polygon.Rotate(_rotation, (40,40)));

            // Update map to reflect new regions
            ApplyRegionsToMap();
        }

        /// <inheritdoc />
        public void GenerateMap()
        {
            // Initialize map for no regions
            foreach (var pos in _map.Positions())
                _map[pos] = RegionTileState.Exterior;

            for (int i = 0; i < 360; i += 45)
            {
                var center = (_map.Width / 2, _map.Height / 2);
                var region = PolygonArea.Parallelogram(center, 15, 15).Rotate(i, center);
                _originalRegions.Add(region);
            }

            // Update map values based on regions
            ApplyRegionsToMap();
        }

        // Update map to effectively revert all region positions back to walls
        private void RemoveRegionsFromMap()
        {
            foreach (var region in _transformedRegions)
                foreach (var point in region.Where(point => _map.Contains(point)))
                    _map[point] = RegionTileState.Exterior;
        }

        // Apply proper values to map, based on regions
        private void ApplyRegionsToMap()
        {
            foreach (var region in _transformedRegions)
            {
                foreach (var point in region.OuterPoints.Where(point => _map.Contains(point)))
                    _map[point] = RegionTileState.Outer;

                foreach (var point in region.InnerPoints.Where(point => _map.Contains(point)))
                    _map[point] = RegionTileState.Inner;

            }
        }

        // Translate point to character based on whether or not the point is in the InnerPoints of a region,
        // in the OuterPoints of a region, or not in any region
        private char RegionsView(Point pos)
            => _map[pos] switch
            {
                RegionTileState.Inner => '.',
                RegionTileState.Outer => '+',
                RegionTileState.Corner => '*',
                _ => ' '
            };
    }
}
