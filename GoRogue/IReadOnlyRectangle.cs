using System;
using System.Collections.Generic;
using Troschuetz.Random;

namespace GoRogue
{
    public interface IReadOnlyRectangle
    {
        Coord Center { get; }
        int Height { get; }
        bool IsEmpty { get; }
        Coord MaxCorner { get; }
        int MaxX { get; }
        int MaxY { get; }
        Coord MinCorner { get; }
        int Width { get; }
        int X { get; }
        int Y { get; }
        bool Contains(IReadOnlyRectangle rectangle);
        bool Contains(int x, int y);
        bool Contains(Coord position);

        bool Intersects(IReadOnlyRectangle rectangle);
        IEnumerable<Coord> Positions();
        Coord RandomPosition(Func<Coord, bool> selector, IGenerator rng = null);
        Coord RandomPosition(IGenerator rng = null);
    }
}
