using GoRogue.GameFramework;
using GoRogue.MapViews;
using Xunit;

namespace GoRogue.UnitTests.MapViews
{
    public class IMapViewExtensionsTests
    {
        [Fact]
        public void ToArrayTest()
        {
            ISettableMapView<int> view = new ArrayMap<int>(15,15);
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    view[i, j] = j;
                }
            }

            int[] answer = view.ToArray();

            int last = 0;
            for (int i = 0; i < answer.Length; i++)
            {
                Assert.True(answer[i] >= last);
                last = answer[i];
            }
        }
    }
}
