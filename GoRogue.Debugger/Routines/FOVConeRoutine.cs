using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.FOV;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    public class FOVConeRoutine : IRoutine
    {
        public string Name => "FOV Cone";

        public IReadOnlyList<(string name, IGridView<char> view)> Views => _views.AsReadOnly();
        private readonly List<(string name, IGridView<char> view)> _views = new List<(string name, IGridView<char> view)>();
        private IFOV _fov = null!;

        private Point[] _targetPositions = null!;
        private int _targetPositionsIdx;

        private double _currentAngle;

        public void NextTimeUnit()
        {
            _targetPositionsIdx = MathHelpers.WrapAround(_targetPositionsIdx + 1, _targetPositions.Length);
            _currentAngle = Point.BearingOfLine(_fov.TransparencyView.Bounds().Center,
                _targetPositions[_targetPositionsIdx]);
            UpdateCone();
        }

        public void LastTimeUnit()
        {
            _targetPositionsIdx = MathHelpers.WrapAround(_targetPositionsIdx - 1, _targetPositions.Length);
            UpdateCone();
        }

        public void GenerateMap()
        {
            var map = new ArrayView<bool>(30, 30);
            map.Fill(true);

            _fov = new RecursiveShadowcastingFOV(map);
            _targetPositions = _fov.BooleanResultView.Bounds().PerimeterPositions().ToArray();

            // Find position at angle 0, since it won't be the first position at the rectangle.
            var point = _fov.BooleanResultView.Bounds()
                .PositionsOnSide(Direction.Up).First(i => i.X == _fov.BooleanResultView.Bounds().Center.X);
            _targetPositionsIdx = Array.FindIndex(_targetPositions, i => i == point);
            UpdateCone();
        }

        public void CreateViews()
        {
            _views.Add(("In FOV",
                new LambdaTranslationGridView<bool, char>(_fov.BooleanResultView, i => i ? '#' : '.')));
        }

        public void InterpretKeyPress(int key) { }

        private void UpdateCone()
        {
            var center = _fov.TransparencyView.Bounds().Center;
            _currentAngle = Point.BearingOfLine(center, _targetPositions[_targetPositionsIdx]);
            _fov.Calculate(center, 10, Distance.Chebyshev, _currentAngle, 30);
        }
    }
}
