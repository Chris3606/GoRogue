using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests
{
    public class LineReturnMethodTests
    {
        [UsedImplicitly]
        [ParamsSource(nameof(TestCases))]
        public (Point start, Point end) LineToDraw;

        [Benchmark]
        public int BresenhamEnum()
        {
            int sum = 0;
            foreach (var point in BresenhamEnumerable(LineToDraw.start, LineToDraw.end))
                sum += point.X + point.Y;

            return sum;
        }

        [Benchmark]
        public int BresenhamFunc()
        {
            int sum = 0;
            BresenhamFuncPtr(LineToDraw.start, LineToDraw.end, p =>
            {
                sum += p.X + p.Y;
                return true;
            });

            return sum;
        }

        [Benchmark]
        public int BresenhamAction()
        {
            int sum = 0;
            BresenhamAction(LineToDraw.start, LineToDraw.end, p => sum += p.X + p.Y);

            return sum;
        }

        public static IEnumerable<(Point start, Point end)> TestCases()
        {
            yield return ((1, 1), (250, 500));
            yield return ((1, 1), (500, 250));
            yield return ((250, 500), (1, 1));
            yield return ((500, 250), (1, 1));
        }

        private static void BresenhamAction(Point start, Point end, Action<Point> action)
            => BresenhamActionPtr(start.X, start.Y, end.X, end.Y, action);

        private static void BresenhamActionPtr(int startX, int startY, int endX, int endY, Action<Point> action)
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
                action(new Point(startX, startY));

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

        private static void BresenhamFuncPtr(Point start, Point end, Func<Point, bool> action)
            => BresenhamFuncPtr(start.X, start.Y, end.X, end.Y, action);

        private static void BresenhamFuncPtr(int startX, int startY, int endX, int endY, Func<Point, bool> action)
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
                if (!action(new Point(startX, startY))) return;

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

        private static IEnumerable<Point> BresenhamEnumerable(Point start, Point end)
            => BresenhamEnumerable(start.X, start.Y, end.X, end.Y);

        private static IEnumerable<Point> BresenhamEnumerable(int startX, int startY, int endX, int endY)
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
