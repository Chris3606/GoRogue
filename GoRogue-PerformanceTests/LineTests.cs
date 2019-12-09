using GoRogue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GoRogue_PerformanceTests
{
	public static class LineTests
	{
		public static TimeSpan TimeForLineGeneration(Point start, Point end, Lines.Algorithm algorithm, int iterations)
		{
			var s = new Stopwatch();

			// Caching
			List<Point> line = Lines.Get(start, end, algorithm).ToList();

			s.Start();
			for (int i = 0; i < iterations; i++)
				line = Lines.Get(start, end, algorithm).ToList();
			s.Stop();

			return s.Elapsed;
		}
	}
}
