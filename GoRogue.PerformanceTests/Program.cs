using BenchmarkDotNet.Running;

namespace GoRogue.PerformanceTests
{
    internal static class Program
    {
        private static void Main(string[] args)
            => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
