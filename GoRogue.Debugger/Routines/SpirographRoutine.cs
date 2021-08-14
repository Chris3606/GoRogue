using System;
using System.Collections.Generic;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    public class SpirographRoutine : IRoutine
    {
        private const int _size = 100;
        private const double _twelfthOfCircle = 2 * Math.PI / 12;
        private readonly List<Point> _trail = new List<Point>();
        private readonly ArrayView<TileState> _map = new ArrayView<TileState>(_size, _size);
        private readonly List<(string name, IGridView<char> view)> _views = new List<(string name, IGridView<char> view)>();
        private double _theta = -100.0;
        private readonly List<Spirograph> _spirographs = new List<Spirograph>();
        public string Name => "Spirographs";
        public IReadOnlyList<(string name, IGridView<char> view)> Views => _views;

        public SpirographRoutine()
        {
            for (int i = 0; i < 12; i++)
            {
                double offset = i * _twelfthOfCircle;
                double innerVariance = 18;
                double outerVariance = 6;
                if (i % 3 == 0)
                {
                    innerVariance = 6;
                    outerVariance = 3;
                }
                _spirographs.Add(
                new Spirograph(theta =>
                    {
                        theta += offset;
                        var innerPoint = new PolarCoordinate(outerVariance, theta * 10);
                        var outerPoint = new PolarCoordinate(innerVariance, theta * 1.5);
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
                if (_trail.Contains(pos))
                    _map[pos] = NextTrailingState(pos);
                else
                    _map[pos] = TileState.Wall;
            }

            foreach (var spiro in _spirographs)
            {
                if (_map.Contains(spiro.Next(_theta) + _size / 2))
                    _map[spiro.Next(_theta) + _size / 2] = TileState.SpiroNext;

                if(_map.Contains(spiro.Now(_theta) + _size / 2))
                    _map[spiro.Now(_theta) + _size / 2] = TileState.SpiroNow;

                if (_map.Contains(spiro.Last(_theta) + _size / 2))
                {
                    _map[spiro.Last(_theta) + _size / 2] = TileState.SpiroLast;
                    _trail.Add(spiro.Last(_theta) + _size / 2);
                }
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
                TileState.InnerRegionPoint => '.',
                TileState.OuterRegionPoint => '-',
                TileState.Door => '+',
                TileState.SpiroLast => '*',
                TileState.SpiroNow => '%',
                TileState.SpiroNext => '#',
                _ => throw new Exception("Regions view encountered unsupported tile settings.")
            };

        private TileState NextTrailingState(Point pos)
            => _map[pos] switch
            {
                TileState.InnerRegionPoint => TileState.Wall,
                TileState.OuterRegionPoint => TileState.InnerRegionPoint,
                TileState.Door => TileState.OuterRegionPoint,
                TileState.SpiroLast => TileState.Door,
                TileState.SpiroNow => TileState.SpiroLast,
                TileState.SpiroNext => TileState.SpiroNow,
                _ => TileState.Wall,
            };

        public void InterpretKeyPress(int key) { }
    }
}
