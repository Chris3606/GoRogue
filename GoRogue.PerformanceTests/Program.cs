using BenchmarkDotNet.Running;

namespace GoRogue.PerformanceTests
{
	class Program
	{
		static void Main(string[] args)
		{
            var summary = BenchmarkRunner.Run<MapGenDefaultAlgorithms>();
        }
	}
}
