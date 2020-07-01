using GoRogue.MapViews;
using GoRogue.Random;
using SadRogue.Primitives;

namespace GoRogue.UnitTests.Mocks
{
    /// <summary>
    /// A class that creates Mock Items to use in tests.
    /// </summary>
    internal class MockFactory
    {
        public static IMapView<double> BoxResMap(int width, int height)
        {
            var map = Rectangle(width, height);
            return new LambdaTranslationMap<bool, double>(map, val => val ? 0.0 : 1.0);
        }

        public static IMapView<bool> EmptyFOVMap(int width, int height)
            => new LambdaMapView<bool>(width, height, _ => true);

        public static ISettableMapView<double> TestResMap(int width, int height)
        {
            var map = new ArrayMap<double>(width, height);

            foreach (var pos in map.Positions())
                map[pos] = GlobalRandom.DefaultRNG.NextDouble();

            return map;
        }

        internal static ISettableMapView<bool> Rectangle(int width, int height)
        {
            ISettableMapView<bool> map = new ArrayMap<bool>(width, height);
            for (var i = 0; i < map.Width; i++)
                for (var j = 0; j < map.Height; j++)
                    if (i == 0 || i == map.Width - 1 || j == 0 || j == map.Height - 1)
                        map[i, j] = false;
                    else
                        map[i, j] = true;

            return map;
        }
        //internal static ISettableMapView<bool> Spiral(int width, int height)
        //{
        //    int center = map.Width / 2;
        //    int dx = 0;
        //    int dy = 1;
        //    int x = 0;
        //    int y = 0;

        //    for (var i = 0; i < map.Width; i++)
        //    {
        //        for (var j = 0; j < map.Height; j++)
        //        {

        //            if (-center / 2 < i && i < center / 2)
        //                if (-center / 2 < j && j < center / 2)
        //                    if (map.Contains(x, y))
        //                        map[x, y] = false;

        //            if (x == y || (x < 0 && x == -y) || (x > 0 && x == 1 - y))
        //            {
        //                var temp = dx;
        //                dx = -dy;
        //                dy = temp;
        //            }
        //            x += dx;
        //            y += dy;
        //        }
        //    }
        //    return map;
        //}
        internal static ISettableMapView<bool> DisconnectedSquares(int width, int height)
        {
            var r1 = new Rectangle(3, 3, 10, 10);
            var r2 = new Rectangle(30, 3, 10, 10);
            var r3 = new Rectangle(3, 30, 10, 10);
            var r4 = new Rectangle(30, 30, 10, 10);
            ISettableMapView<bool> map = new ArrayMap<bool>(width, height);

            for (var i = 0; i < map.Width; i++)
                for (var j = 0; j < map.Height; j++)
                    if (i == r1.MinExtentX || i == r1.MaxExtentX || j == r1.MinExtentY || j == r1.MaxExtentY)
                        map[i, j] = false;
                    else if (i == r2.MinExtentX || i == r2.MaxExtentX || j == r2.MinExtentY || j == r2.MaxExtentY)
                        map[i, j] = false;
                    else if (i == r3.MinExtentX || i == r3.MaxExtentX || j == r3.MinExtentY || j == r3.MaxExtentY)
                        map[i, j] = false;
                    else if (i == r4.MinExtentX || i == r4.MaxExtentX || j == r4.MinExtentY || j == r4.MaxExtentY)
                        map[i, j] = false;
                    else
                        map[i, j] = true;
            return map;
        }

        internal static ISettableMapView<bool> CardinalBisection(int width, int height, int timesToBisect)
        {
            ISettableMapView<bool> map = new ArrayMap<bool>(width, height);

            for (var i = 0; i < map.Width; i++)
                for (var j = 0; j < map.Height; j++)
                    if (j == 25)
                        map[i, j] = false;

                    else if (timesToBisect > 1 && i == 25)
                        map[i, j] = false;

                    else
                        map[i, j] = true;
            return map;
        }

        internal static ISettableMapView<bool> DiagonalBisection(int width, int height, int timesToBisect)
        {
            ISettableMapView<bool> map = new ArrayMap<bool>(width, height);

            for (var i = 0; i < map.Width; i++)
                for (var j = 0; j < map.Height; j++)
                    if (j == i)
                        map[i, j] = false;
                    else if (timesToBisect > 1 && j + i == map.Width - 1)
                        map[i, j] = false;
                    else
                        map[i, j] = true;
            return map;
        }
    }
}
