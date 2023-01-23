using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests
{
    public class LineAlgorithmTests
    {
        [UsedImplicitly]
        [ParamsSource(nameof(TestCases))]
        public (Point start, Point end) LineToDraw;

        private Consumer _consumer = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _consumer = new Consumer();
        }

        [Benchmark]
        [ArgumentsSource(nameof(GoRogueLineAlgorithms))]
        public void GetPointsOnLine(Lines.Algorithm algo)
            => Lines.Get(LineToDraw.start, LineToDraw.end, algo).Consume(_consumer);

        [Benchmark]
        public void GetPointsBresenhamAlt()
            => BresenhamAlt(LineToDraw.start, LineToDraw.end).Consume(_consumer);

        public static IEnumerable<(Point start, Point end)> TestCases()
        {
            yield return ((1, 1), (25, 50));
            yield return ((1, 1), (50, 25));
            yield return ((25, 50), (1, 1));
            yield return ((50, 25), (1, 1));
        }

        public static IEnumerable<Lines.Algorithm> GoRogueLineAlgorithms()
            => Enum.GetValues<Lines.Algorithm>();

        private static IEnumerable<Point> BresenhamAlt(Point start, Point end)
            => BresenhamAlt(start.X, start.Y, end.X, end.Y);

        private static IEnumerable<Point> BresenhamAlt(int startX, int startY, int endX, int endY)
        {
            int w = endX - startX;
            int h = endY - startY;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                yield return new Point(startX, startY);
                //plot(startX, startY);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    startX += dx1;
                    startY += dy1;
                }
                else
                {
                    startX += dx2;
                    startY += dy2;
                }
            }
        }
    }
}
