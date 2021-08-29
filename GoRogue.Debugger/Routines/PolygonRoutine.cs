﻿using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    internal enum PolygonTileState
    {
        Exterior, //Exterior to the polygon
        InnerPoint,
        OuterPoint,
        Corner
    }

    [UsedImplicitly]
    internal class PolygonRoutine : IRoutine
    {
        // The Polygons to display
        private readonly List<PolygonArea> _polygons = new List<PolygonArea>();

        private int _cornerAmount = 3;

        // Grid view set to indicate current state of each tile, so that it can be efficiently rendered.
        private readonly ArrayView<PolygonTileState> _map = new ArrayView<PolygonTileState>(100, 30);
        private readonly List<(string name, IGridView<char> view)> _views = new List<(string name, IGridView<char> view)>();

        /// <inheritdoc />
        public IReadOnlyList<(string name, IGridView<char> view)> Views => _views.AsReadOnly();

        /// <inheritdoc />
        public void InterpretKeyPress(int key) { } //

        /// <inheritdoc />
        public string Name => "Polygons";

        /// <inheritdoc />
        public void CreateViews()
        {
            _views.Add(("Polygons", new LambdaGridView<char>(_map.Width, _map.Height, RegionsView)));
        }

        /// <inheritdoc />
        public void NextTimeUnit()
        {
            // Revert all points in current regions back to walls
            RemovePolygonsFromMap();

            _polygons.Clear();
            _polygons.Add(Polygon(++_cornerAmount));

            // Update map to reflect new regions
            ApplyPolygonsToMap();
        }

        /// <inheritdoc />
        public void LastTimeUnit()
        {
            // Revert all points in current regions back to walls
            RemovePolygonsFromMap();

            if (_cornerAmount > 3)
            {
                _polygons.Clear();
                _polygons.Add(Polygon(--_cornerAmount));
            }

            // Update map to reflect new regions
            ApplyPolygonsToMap();
        }

        /// <inheritdoc />
        public void GenerateMap()
        {
            // Initialize map for no regions
            foreach (var pos in _map.Positions())
                _map[pos] = PolygonTileState.Exterior;

            _polygons.Add(Polygon(_cornerAmount));

            // Update map values based on regions
            ApplyPolygonsToMap();
        }

        private PolygonArea Polygon(int cornerCount)
        {
            var midPoint = new Point(_map.Width / 2 - 5, _map.Height / 2 - 5);
            var dim = _map.Width < _map.Height ? _map.Width : _map.Height;
            var corners = new List<Point>();
            bool star = false;
            bool inner = false;
            int radius;

            //draw simple shapes up to hexagons
            if(cornerCount >= 7)
            {
                cornerCount *= 2;
                star = true;
            }

            for (int i = 0; i < 360; i += 360 / cornerCount)
            {
                //each full rotation, increase the distance from the center we're placing our polygons
                if (star)
                {
                    if (inner)
                        radius = dim / 4;
                    else
                        radius = dim * 3 / 4;

                    inner = !inner;
                }
                else
                    radius = dim * 3 / 4;

                var corner = new PolarCoordinate(radius, SadRogue.Primitives.MathHelpers.ToRadian(i)).ToCartesian();
                corner += midPoint;
                corners.Add(corner);
            }

            return new PolygonArea(corners);
        }

        // Update map to effectively revert all region positions back to walls
        private void RemovePolygonsFromMap()
        {
            foreach (var polygon in _polygons)
                foreach (var point in polygon.Where(point => _map.Contains(point)))
                    _map[point] = PolygonTileState.Exterior;
        }

        // Apply proper values to map, based on regions
        private void ApplyPolygonsToMap()
        {
            foreach (var polygon in _polygons)
            {
                foreach (var point in polygon.InnerPoints.Where(point => _map.Contains(point)))
                    _map[point] = PolygonTileState.InnerPoint;

                foreach (var point in polygon.OuterPoints.Where(point => _map.Contains(point)))
                    _map[point] = PolygonTileState.OuterPoint;

                foreach (var point in polygon.Corners.Where(point => _map.Contains(point)))
                    _map[point] = PolygonTileState.Corner;
            }
        }

        private char RegionsView(Point pos)
            => _map[pos] switch
            {
                PolygonTileState.InnerPoint => '.',
                PolygonTileState.OuterPoint => '+',
                PolygonTileState.Corner => '#',
                _ => ' ',
            };
    }
}