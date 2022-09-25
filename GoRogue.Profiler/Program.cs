using GoRogue.MapGeneration;
using GoRogue.SenseMapping;
using GoRogue.SenseMapping.Sources;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Profiler;

interface ITest
{
    public void Perform();
}

class SenseMapRippleTest : ITest
{
    private const int MapSize = 100;

    private readonly SenseMap _senseMap;
    public SenseMapRippleTest()
    {
        _senseMap = CreateSenseMap();
        _senseMap.AddSenseSource(new RippleSenseSource((MapSize / 2, MapSize / 2), 10, Distance.Chebyshev));
    }

    public void Perform()
    {
        _senseMap.Calculate();
    }

    private static SenseMap CreateSenseMap()
    {
        // Create sense map of rectangular area
        var wallFloor = new Generator(MapSize, MapSize)
            .ConfigAndGenerateSafe(gen => gen.AddSteps(DefaultAlgorithms.RectangleMapSteps()))
            .Context.GetFirst<IGridView<bool>>("WallFloor");

        var resMap = new ArrayView<double>(wallFloor.Width, wallFloor.Height);
        resMap.ApplyOverlay(pos => wallFloor[pos] ? 0.0 : 1.0);
        return new SenseMap(resMap);
    }
}

static class Program
{
    private static ITest Prepare()
    {
        var test = new SenseMapRippleTest();

        for (int i = 0; i < 100; i++)
            test.Perform();

        return test;
    }

    private static void PerformTest(ITest test)
    {
        for (int i = 0; i < 1000; i++)
            test.Perform();
    }

    public static void Main()
    {
        var test = Prepare();
        PerformTest(test);
    }
}
