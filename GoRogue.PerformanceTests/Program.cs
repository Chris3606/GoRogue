using BenchmarkDotNet.Running;

namespace GoRogue.PerformanceTests
{
	internal static class Program
	{
		private static void Main()
		{
            BenchmarkRunner.Run<MapGenDefaultAlgorithms>();
        }
	}
}
