using BenchmarkDotNet.Attributes;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;

namespace GoRogue.PerformanceTests.SpatialMaps;

/// <summary>
/// Performance test for basic operations on layer masks.
/// </summary>
public class LayerMaskerOperations
{
    [UsedImplicitly]
    [Params(2, 8, 16, 32)]
    public int NumberOfLayers;

    private LayerMasker _masker = null!;
    private uint _allMask;
    private uint _halfMask;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _masker = new LayerMasker(NumberOfLayers);
        _allMask = _halfMask = 0;

        for (int i = 0; i < NumberOfLayers; i++)
        {
            _allMask = _masker.AddLayers(_allMask, i);
            if (i % 2 == 0)
                _halfMask = _masker.AddLayers(_halfMask, i);
        }
    }

    [Benchmark]
    public int SumLayersInAllMask()
    {
        int sum = 0;
        foreach (int layer in _masker.Layers(_allMask))
            sum += layer;
        return sum;
    }

    [Benchmark]
    public int SumLayersInAllMaskNoForEach()
    {
        uint mask = _allMask << (32 - NumberOfLayers);
        int sum = 0;
        int currentLayer = NumberOfLayers - 1;
        while (mask != 0)
        {
            if ((mask & 0x80000000) != 0)
                sum += currentLayer;
            mask <<= 1;
            currentLayer--;
        }

        return sum;
    }

    [Benchmark]
    public int SumLayersInHalfMask()
    {
        int sum = 0;
        foreach (int layer in _masker.Layers(_halfMask))
            sum += layer;
        return sum;
    }

    [Benchmark]
    public int SumLayersInHalfMaskNoForEach()
    {
        uint mask = _halfMask << (32 - NumberOfLayers);
        int sum = 0;
        int currentLayer = NumberOfLayers - 1;
        while (mask != 0)
        {
            if ((mask & 0x80000000) != 0)
                sum += currentLayer;
            mask <<= 1;
            currentLayer--;
        }

        return sum;
    }
}
