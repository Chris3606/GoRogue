using BenchmarkDotNet.Running;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters;

namespace GoRogue.PerformanceTests
{

    internal static class Program
    {
        private static void Main(string[] args)
            => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args,
                ManualConfig.CreateMinimumViable()
                .AddExporter(JsonExporter.Full, CsvMeasurementsExporter.Default, HtmlExporter.Default));
    }
}
