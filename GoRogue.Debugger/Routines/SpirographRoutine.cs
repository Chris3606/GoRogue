using System;
using System.Collections.Generic;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    public class SpirographRoutine : IRoutine
    {
        private const int _size = 500;
        private readonly ArrayView<TileState> _map = new ArrayView<TileState>(_size, _size);
        private readonly List<(string name, IGridView<char> view)> _views = new List<(string name, IGridView<char> view)>();
        private double _theta = -100.0;
        private readonly List<Spirograph> _spirographs = new List<Spirograph>();
        public string Name => "Spirographs";
        public IReadOnlyList<(string name, IGridView<char> view)> Views => _views;

        public SpirographRoutine()
        {
            foreach (var spirograph in PolarCoordinate.Functions.Values)
            {
                _spirographs.Add(new Spirograph(spirograph));
            }
        }

        public void NextTimeUnit()
        {
            _theta += 0.0025;
            GenerateMap();
        }

        public void LastTimeUnit()
        {
            _theta -= 0.0025;
            GenerateMap();
        }
        public void GenerateMap()
        {
            foreach (var pos in _map.Positions())
            {
                _map[pos] = TileState.Wall;
            }

            foreach (var spiro in _spirographs)
            {
                if(_map.Contains(spiro.Next(_theta) + _size / 2))
                    _map[spiro.Next(_theta) + _size / 2] = TileState.SpiroNext;

                if(_map.Contains(spiro.Now(_theta)))
                    _map[spiro.Now(_theta) + _size / 2] = TileState.SpiroNow;

                if(_map.Contains(spiro.Last(_theta)))
                    _map[spiro.Last(_theta) + _size / 2] = TileState.SpiroLast;
            }
        }
        public void CreateViews()
        {
            _views.Add(("Regions", new LambdaGridView<char>(_map.Width, _map.Height, Spiroview)));
        }

        private char Spiroview(Point pos)
            => _map[pos] switch
            {
                TileState.Wall => ' ',
                TileState.SpiroLast => '.',
                TileState.SpiroNow => '+',
                TileState.SpiroNext => '#',
                _ => throw new Exception("Regions view encountered unsupported tile settings.")
            };

        public void InterpretKeyPress(int key) { }
    }
}
