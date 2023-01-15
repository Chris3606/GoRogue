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
            CalculateFOV();
        }

        public void LastTimeUnit()
        {
            _targetPositionsIdx = MathHelpers.WrapAround(_targetPositionsIdx - 1, _targetPositions.Length);
            _currentAngle = Point.BearingOfLine(_fov.TransparencyView.Bounds().Center,
                _targetPositions[_targetPositionsIdx]);
            CalculateFOV();
        }

        public void GenerateMap()
        {
            var map = new ArrayView<bool>(30, 30);
            map.Fill(true);

            _fov = new RecursiveShadowcastingFOV(map);
            _targetPositions = _fov.BooleanResultView.Bounds().PerimeterPositions().ToArray();
            _targetPositionsIdx = 0;
            _currentAngle = 0;
            CalculateFOV();
        }

        public void CreateViews()
        {
            _views.Add(("In FOV",
                new LambdaTranslationGridView<bool, char>(_fov.BooleanResultView, i => i ? '#' : '.')));
        }

        public void InterpretKeyPress(int key) { }

        private void CalculateFOV()
        {
            _targetPositionsIdx = MathHelpers.WrapAround(_targetPositionsIdx + 1, _targetPositions.Length);
            _fov.Calculate(_fov.TransparencyView.Bounds().Center, 10, Distance.Chebyshev, _currentAngle, 30);
        }
    }
}
