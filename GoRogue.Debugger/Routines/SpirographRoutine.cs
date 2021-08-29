using System;
using System.Collections.Generic;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    public enum SpirographTileState
    {
        Blank, //Has nothing drawn on it
        Faintest, // `
        Fainter, // .
        Faint, // -
        Moderate, // +
        Bold, // *
        Bolder, // %
        Boldest, // #
    }

    public class SpirographRoutine : IRoutine
    {
        private const int _size = 100;
        private const double _twelfthOfCircle = 2 * Math.PI / 12;
        private readonly ArrayView<SpirographTileState> _map = new ArrayView<SpirographTileState>(_size, _size);
        private readonly List<(string name, IGridView<char> view)> _views = new List<(string name, IGridView<char> view)>();
        private double _theta = -100.0;
        private readonly List<Spirograph> _spirographs = new List<Spirograph>();
        public string Name => "Spirograph";
        public IReadOnlyList<(string name, IGridView<char> view)> Views => _views;

        public SpirographRoutine()
        {
            for (int i = 0; i < 12; i++)
            {
                double offset = i * _twelfthOfCircle;
                _spirographs.Add(
                new Spirograph(theta =>
                    {
                        theta += offset;
                        var innerPoint = new PolarCoordinate(25, theta * 0.75);
                        var outerPoint = new PolarCoordinate(7, theta * 10);
                        return innerPoint.ToCartesian() + outerPoint.ToCartesian();
                    }));

                _spirographs.Add(
                new Spirograph(theta =>
                    {
                        theta += offset;
                        var innerPoint = new PolarCoordinate(10, theta * -7.5);
                        var outerPoint = new PolarCoordinate(7, theta * -0.5);
                        return innerPoint.ToCartesian() + outerPoint.ToCartesian();
                    }));
            }
        }

        public void NextTimeUnit()
        {
            _theta += 0.025;
            GenerateMap();
        }

        public void LastTimeUnit()
        {
            _theta -= 0.025;
            GenerateMap();
        }
        public void GenerateMap()
        {
            foreach (var pos in _map.Positions())
            {
                if (_map[pos] > SpirographTileState.Blank)
                    _map[pos]--;
            }

            foreach (var spiro in _spirographs)
            {
                if (_map.Contains(spiro.Next(_theta) + _size / 2))
                    _map[spiro.Next(_theta) + _size / 2] = SpirographTileState.Boldest;
                if (_map.Contains(spiro.Now(_theta) + _size / 2))
                    _map[spiro.Next(_theta) + _size / 2] = SpirographTileState.Bolder;
                if (_map.Contains(spiro.Last(_theta) + _size / 2))
                    _map[spiro.Next(_theta) + _size / 2] = SpirographTileState.Bold;
            }
        }
        public void CreateViews()
        {
            _views.Add(("Regions", new LambdaGridView<char>(_map.Width, _map.Height, Spiroview)));
        }

        private char Spiroview(Point pos)
            => _map[pos] switch
            {
                SpirographTileState.Blank => ' ',
                SpirographTileState.Faintest => '`',
                SpirographTileState.Fainter => '.',
                SpirographTileState.Faint => '-',
                SpirographTileState.Moderate => '+',
                SpirographTileState.Bold => '*',
                SpirographTileState.Bolder => '%',
                SpirographTileState.Boldest => '#',
                _ => ' '
            };

        public void InterpretKeyPress(int key) { }
    }
}
