using GoRogue.Random;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.UnitTests.Mocks
{
    /// <summary>
    /// A class that creates Mock Items to use in tests.
    /// </summary>
    internal static class MockMaps
    {
        public static IGridView<double> RectangleResMap(int width, int height)
        {
            var map = Rectangle(width, height);
            return new LambdaTranslationGridView<bool, double>(map, val => val ? 0.0 : 1.0);
        }

        public static IGridView<bool> EmptyFOVMap(int width, int height)
            => new LambdaGridView<bool>(width, height, _ => true);

        public static ISettableGridView<double> TestResMap(int width, int height)
        {
            var map = new ArrayView<double>(width, height);

            foreach (var pos in map.Positions())
                map[pos] = GlobalRandom.DefaultRNG.NextDouble();

            return map;
        }

        public static ISettableGridView<bool> Rectangle(int width, int height)
        {
            ISettableGridView<bool> map = new ArrayView<bool>(width, height);
            for (var i = 0; i < map.Width; i++)
                for (var j = 0; j < map.Height; j++)
                    if (i == 0 || i == map.Width - 1 || j == 0 || j == map.Height - 1)
                        map[i, j] = false;
                    else
                        map[i, j] = true;

            return map;
        }

        // Generates a rectangle with a double-thick border.
        public static ISettableGridView<bool> DoubleThickRectangle(int width, int height)
        {
            var map = Rectangle(width, height);
            var innerBounds = map.Bounds().Expand(-1, -1);
            foreach (var pos in innerBounds.PerimeterPositions())
                map[pos] = false;

            return map;
        }

        public static IGridView<double> RectangleDoubleThickResMap(int width, int height)
        {
            var map = DoubleThickRectangle(width, height);
            return new LambdaTranslationGridView<bool, double>(map, val => val ? 0.0 : 1.0);
        }

        //internal static ISettableGridView<bool> Spiral(int width, int height)
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
        internal static ISettableGridView<bool> DisconnectedSquares(int width, int height)
        {
            var r1 = new Rectangle(3, 3, 10, 10);
            var r2 = new Rectangle(30, 3, 10, 10);
            var r3 = new Rectangle(3, 30, 10, 10);
            var r4 = new Rectangle(30, 30, 10, 10);
            ISettableGridView<bool> map = new ArrayView<bool>(width, height);

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

        internal static ISettableGridView<bool> CardinalBisection(int width, int height, int timesToBisect)
        {
            ISettableGridView<bool> map = new ArrayView<bool>(width, height);

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

        internal static ISettableGridView<bool> DiagonalBisection(int width, int height, int timesToBisect)
        {
            ISettableGridView<bool> map = new ArrayView<bool>(width, height);

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
