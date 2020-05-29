using System;
using System.Collections.Generic;
using System.Text;
using GoRogue.GameFramework;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.UnitTests.Mocks
{
    enum TestPattern
    {
        Blank,
        Spiral,
        Disconnected,
        CardinalBisection,
        CardinalDoubleBisection,
        DiagonalBisection,
        LowDensityNoise,
        HighDensityNoise,
    }
    public class MockMap : Map
    {
        public string Name;
        static readonly int _width  = Console.WindowWidth;
        static readonly int _height = Console.WindowHeight;
        public int ExpectedMinimum;
        public int ExpectedMaximum;
        public MockMap() : base(_width, _height, 1, Distance.Manhattan)
        {
            Name = "blank space";

            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    SetTerrain(Walkable(i, j));
                }
            }

            ExpectedMinimum = 1;
            ExpectedMaximum = 1;
        }
        public int Width => _width;
        public int Height => _height;

        public void SetExpectations(AdjacencyRule rule)
        {

        }
        public override string ToString()
        { //for getting a smaller glimpse into what the map looks like
            string answer = Name + base.ToString();
            //for (int x = 0; x < _width; x++)
            //{
            //    for (int y = 0; y < _height; y++)
            //    {
            //        if (GetTerrainAt(x, y).IsWalkable)
            //            answer += " ";
            //        else
            //            answer += "X";
            //    }
            //    answer += Environment.NewLine;
            //}

            return answer;
        }
        private static GameObject Unwalkable(int x, int y) => new GameObject((x, y), 0, null, isWalkable: false, isStatic: true);
        private static GameObject Walkable(int x, int y) => new GameObject((x, y), 0, null, isWalkable: true, isStatic: true);
        internal MockMap Spiral()
        {
            Name = "spiral";
            int center = _width / 2;
            int dx = 0;
            int dy = 1;
            int x = 0;
            int y = 0;

            for (var i = 0; i < _width; i++)
            {
                for (var j = 0; j < _height; j++)
                {
                    if (-center / 2 < i && i < center / 2)
                        if (-center / 2 < j && j < center / 2)
                            if (this.Contains(x, y))
                                SetTerrain(Unwalkable(x, y));

                    if (x == y || x < 0 && x == -y || x > 0 && x == 1 - y)
                    {
                        var temp = dx;
                        dx = -dy;
                        dy = temp;
                    }
                    x += dx;
                    y += dy;
                }
            }
            ExpectedMinimum = 4;
            ExpectedMaximum = 50; //???
            return this;
        }

        internal MockMap DisconnectedSquares()
        {
            Name = "innaccessible";
            var r1 = new Rectangle(3, 3, 10, 10);
            var r2 = new Rectangle(30, 3, 10, 10);
            var r3 = new Rectangle(3, 30, 10, 10);
            var r4 = new Rectangle(30, 30, 10, 10);

            for (var i = 0; i < _width; i++)
                for (var j = 0; j < _height; j++)
                {
                    if (i == r1.MinExtentX || i == r1.MaxExtentX || j == r1.MinExtentY || j == r1.MaxExtentY)
                        SetTerrain(Unwalkable(i, j));
                    if (i == r2.MinExtentX || i == r2.MaxExtentX || j == r2.MinExtentY || j == r2.MaxExtentY)
                        SetTerrain(Unwalkable(i, j));
                    if (i == r3.MinExtentX || i == r3.MaxExtentX || j == r3.MinExtentY || j == r3.MaxExtentY)
                        SetTerrain(Unwalkable(i, j));
                    if (i == r4.MinExtentX || i == r4.MaxExtentX || j == r4.MinExtentY || j == r4.MaxExtentY)
                        SetTerrain(Unwalkable(i, j));

                }
            return this;
        }

        internal MockMap CardinalBisection(int timesToBisect)
        {
            Name = "cardinally bisected";
            for (var i = 0; i < _width; i++)
                for (var j = 0; j < _height; j++)
                {
                    SetTerrain(Walkable(i, j));
                    if (j == 25)
                    {
                        SetTerrain(Unwalkable(i, j));
                    }
                    if (timesToBisect > 1 && i == 25)
                    {
                        SetTerrain(Unwalkable(i, j));
                        Name += " twice";
                    }
                }
            return this;
        }

        internal MockMap DiagonalBisection(int timesToBisect)
        {
            Name = "diagonally bisected";
            for (var i = 0; i < _width; i++)
            {
                for (var j = 0; j < _height; j++)
                {
                    SetTerrain(Walkable(i, j));
                    if (j == i)
                    {
                        SetTerrain(Unwalkable(i, j));
                    }
                    if (timesToBisect > 1 && j - 1 == 0)
                    {
                        SetTerrain(Unwalkable(i, j));
                    }
                }
            }
            return this;
        }
    }
}
