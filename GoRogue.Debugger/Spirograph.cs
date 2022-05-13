using System;
using SadRogue.Primitives;

namespace GoRogue.Debugger
{
    public class Spirograph
    {
        public readonly Func<double, Point> Graph;

        public Spirograph(Func<double, Point> graph)
        {
            Graph = graph;
        }

        public Point Next(double theta)
        {
            Point here = Graph(theta);
            while (Graph(theta) == here)
            {
                theta += 0.001;
            }
            return Graph(theta);
        }

        public Point Now(double theta)
        {
            return Graph(theta);
        }

        public Point Last(double theta)
        {
            Point here = Graph(theta);
            while (Graph(theta) == here)
            {
                theta -= 0.0002;
            }

            return Graph(theta);
        }
    }
}
