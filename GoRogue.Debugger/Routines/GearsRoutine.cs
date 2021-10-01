using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    public enum GearTileState
    {
        Blank, //nothing on it
        Outline, //outer point
        Solid, //inner point
        ReflectingDim, //special
        Reflecting, //special
        ReflectingBright, //special
    }

    public class GearsRoutine : IRoutine
    {
        public string Name => "Gears in Motion";
        private int _rotationDegrees = 0;
        public IReadOnlyList<(string name, IGridView<char> view)> Views => _views.AsReadOnly();
        private readonly List<(string name, IGridView<char> view)> _views = new List<(string name, IGridView<char> view)>();
        private readonly ArrayView<GearTileState> _map = new ArrayView<GearTileState>(100, 100);
        private readonly Point _center = (50, 50);
        private readonly List<PolygonArea> _originalClockWiseGearPositions = new List<PolygonArea>();
        private readonly List<PolygonArea> _originalCounterClockWiseGearPositions = new List<PolygonArea>();
        private readonly List<PolygonArea> _originalClockWiseWindowPositions = new List<PolygonArea>();
        private readonly List<PolygonArea> _originalCounterClockWiseWindowPositions = new List<PolygonArea>();
        private readonly List<PolygonArea> _currentGearPositions = new List<PolygonArea>();
        private readonly List<PolygonArea> _currentWindowPositions = new List<PolygonArea>();

        public void GenerateMap()
        {
            //one large gear
            int outerRadius = 27;
            int innerRadius = 25;
            var gear = Gear(_center, innerRadius, outerRadius);
            _originalClockWiseGearPositions.Add(gear);
            _currentGearPositions.Add(gear);
            var windows = Windows(_center, 5, outerRadius - 8);
            _originalClockWiseWindowPositions.AddRange(windows);
            _currentWindowPositions.AddRange(windows);

            var triangle = PolygonArea.RegularPolygon(_center, 3, 41);

            outerRadius = 14;
            innerRadius = 12;
            foreach (var corner in triangle.Corners)
            {
                gear = Gear(corner, innerRadius, outerRadius);
                _originalCounterClockWiseGearPositions.Add(gear);
                _currentGearPositions.Add(gear);
                windows = Windows(corner, 3, outerRadius - 4);
                // _originalCounterClockWiseWindowPositions.AddRange(windows);
            }

            Draw();
        }

        private IEnumerable<PolygonArea> Windows(Point center, int innerRadius, int outerRadius)
        {
            var corners = new List<Point>(5);
            var start = center + (innerRadius, -innerRadius);
            corners.Add(start);


            for (int i = 0; i < 4; i++)
            {
                var radius = outerRadius;
                var theta = SadRogue.Primitives.MathHelpers.ToRadian(i * 15);
                var pc = new PolarCoordinate(radius, theta);
                var pos = center + pc.ToCartesian();
                corners.Add(pos);
            }

            var window = new PolygonArea(ref corners);
            yield return window;
            yield return window.Rotate(90, center);
            yield return window.Rotate(180, center);
            yield return window.Rotate(270, center);
        }

        private PolygonArea Gear(Point center, int innerRadius, int outerRadius)
        {
            List<Point> corners = new List<Point>();
            for (int i = 0; i < 360; i += 15)
            {
                var theta = SadRogue.Primitives.MathHelpers.ToRadian(i);
                if (i % 2 == 0)
                {
                    corners.Add(center + new PolarCoordinate(innerRadius, theta).ToCartesian());
                    corners.Add(center + new PolarCoordinate(outerRadius, theta).ToCartesian());
                }
                else
                {
                    corners.Add(center + new PolarCoordinate(outerRadius, theta));
                    corners.Add(center + new PolarCoordinate(innerRadius, theta));
                }
            }

            return new PolygonArea(ref corners);
        }

        private void Erase()
        {
            foreach (var gear in _currentGearPositions)
                foreach (var point in gear.Where(point => _map.Contains(point)))
                    _map[point] = GearTileState.Blank;


            foreach (var gear in _currentWindowPositions)
                foreach (var point in gear.Where(point => _map.Contains(point)))
                    _map[point] = GearTileState.Blank;
        }

        private void Draw()
        {
            foreach (var gear in _currentGearPositions)
            {
                foreach (var point in gear.OuterPoints.Where(point => _map.Contains(point)))
                    _map[point] = GearTileState.Outline;

                var startVal = gear.Center.X + gear.Center.Y;
                var maxVal = gear.Bottom + gear.Right;
                var centerVal = (maxVal + startVal) / 2;
                foreach (var point in gear.InnerPoints.Where(point => _map.Contains(point)))
                {
                    var hereVal = point.X + point.Y;
                    if (hereVal < startVal)
                        _map[point] = GearTileState.Solid;
                    else if (hereVal < centerVal)
                        _map[point] = GearTileState.ReflectingDim;
                    else if (hereVal < maxVal)
                        _map[point] = GearTileState.Reflecting;
                    else
                        _map[point] = GearTileState.ReflectingBright;
                }
            }
            foreach (var window in _currentWindowPositions)
            {
                foreach (var point in window.OuterPoints.Where(point => _map.Contains(point)))
                    _map[point] = GearTileState.Blank;

                foreach (var point in window.InnerPoints.Where(point => _map.Contains(point)))
                    _map[point] = GearTileState.Blank;
            }
        }

        public void NextTimeUnit()
        {
            Erase();
            _currentGearPositions.Clear();
            _currentWindowPositions.Clear();
            _rotationDegrees += 5;

            foreach(var gear in _originalClockWiseGearPositions)
                _currentGearPositions.Add(gear.Rotate(_rotationDegrees, _center));

            foreach(var gear in _originalCounterClockWiseGearPositions)
                _currentGearPositions.Add(gear.Rotate(-_rotationDegrees));

            foreach(var window in _originalClockWiseWindowPositions)
                _currentWindowPositions.Add(window.Rotate(_rotationDegrees, _center));

            // foreach(var window in _originalCounterClockWiseWindowPositions)
            //     _currentWindowPositions.Add(window.Rotate(-_rotationDegrees));
            Draw();
        }

        public void LastTimeUnit()
        {
            Erase();
            _currentGearPositions.Clear();
            _rotationDegrees -= 5;
            foreach(var gear in _originalClockWiseGearPositions)
                _currentGearPositions.Add(gear.Rotate(_rotationDegrees));

            Draw();
        }

        public void CreateViews() => _views.Add(("Gears", new LambdaGridView<char>(_map.Width, _map.Height, View)));
        private char View(Point pos)
            => _map[pos] switch
            {
                GearTileState.Solid => '.',
                GearTileState.Outline => '#',
                GearTileState.ReflectingDim => '-',
                GearTileState.Reflecting => '+',
                GearTileState.ReflectingBright => '*',
                _ => ' ',
            };
        public void InterpretKeyPress(int key) { }
    }
}
