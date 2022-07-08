using System.Linq;
using GoRogue.FOV;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.FOV
{
    public class RecursiveShadowcastingEquivalenceTests
    {
        private const int MapSize = 50;
        private const int FOVRadius = 30;

        // Basic rectangle LOS map
        private static readonly IGridView<bool> s_losMap = MockMaps.Rectangle(MapSize, MapSize);

        // LOS map with double-thick walls
        private static readonly IGridView<bool> s_losMapDoubleThickWalls =
            MockMaps.DoubleThickRectangle(MapSize, MapSize);

        // Radius shapes to test
        private static readonly Radius[] s_radii = TestUtils.GetEnumValues<Radius.Types>().Select(i => (Radius)i).ToArray();

        // Center point of the map
        private static readonly Point s_center = new Point(MapSize / 2, MapSize / 2);

        // Combinations of positions, radius values, radius types, and maps to test on
        public static (Point position, int radius, Radius radiusType, IGridView<bool> losMap)[] TestData
            = s_center.Yield()
                .Combinate(FOVRadius.Yield())
                .Combinate(s_radii).ToArray().
                Combinate(TestUtils.Enumerable(s_losMap, s_losMapDoubleThickWalls))
                .ToArray();


        [Theory]
        [MemberDataTuple(nameof(TestData))]
        public void FOVDoubleAndBooleanEquivalent(Point position, int radius, Radius radiusType, IGridView<bool> losMap)
        {
            // Create double and boolean variants of recursive shadowcasting with the same source map
            var doubleBased = new RecursiveShadowcastingFOV(losMap);
            var boolBased = new RecursiveShadowcastingBooleanBasedFOV(losMap);

            // Calculate both maps from the center point
            doubleBased.Calculate(position, radius, radiusType);
            boolBased.Calculate(position, radius, radiusType);

            // Values should be the same across the entire map
            foreach (var pos in losMap.Positions())
            {
                Assert.Equal(doubleBased.BooleanResultView[pos], boolBased.BooleanResultView[pos]);
                Assert.Equal(doubleBased.DoubleResultView[pos], boolBased.DoubleResultView[pos]);
            }
        }
    }
}
