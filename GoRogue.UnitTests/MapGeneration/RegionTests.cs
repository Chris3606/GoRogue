using System;
using System.Linq;
using System.Text;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Xunit;
using Xunit.Abstractions;

namespace GoRogue.UnitTests.MapGeneration
{
    public sealed class RegionTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        /*   0 1 2 3 4 5 6 7
         * 0        /--*
         * 1   *+--+    \
         * 2     \       \
         * 3      \    +--+*
         * 4       *--/
         * 5
         */
        private readonly Point _sw = new Point(3, 4);
        private readonly Point _nw = new Point(1, 1);
        private readonly Point _ne = new Point(5, 0);
        private readonly Point _se = new Point(7, 3);
        private readonly Region _area;

        public RegionTests(ITestOutputHelper output)
        {
            _output = output;
            _area = new Region(_nw, _ne, _se, _sw);

            _output.WriteLine("Test Region:");
            _output.WriteLine(GetRegionString(_area));
        }

        private string GetRegionString(Region region)
        {
            var bounds = region.Bounds;
            var gv = new LambdaGridView<char>(region.Width, region.Height, pos =>
            {
                // Offset to grid view coords
                pos += region.Bounds.Position;

                // Print proper char
                if (region.Area.Corners.Contains(pos)) return 'C';
                if (region.Area.OuterPoints.Contains(pos)) return 'O';
                if (region.Area.InnerPoints.Contains(pos)) return 'I';

                return '.';
            });

            var lines = gv.ToString().Split('\n');

            var final = new StringBuilder();

            // Generate x scale
            final.Append(' ');
            for (int i = 0; i < bounds.Width; i++)
                final.Append($" {i + bounds.MinExtentX}");
            final.Append('\n');

            // Add each line with y-scale value
            for (int i = 0; i < lines.Length; i++)
                final.Append($"{i + bounds.MinExtentY} {lines[i]}\n");

            return final.ToString();
        }

        [Fact]
        public void RegionTest()
        {
            Assert.Equal(18, _area.Area.OuterPoints.Count);
            Assert.Equal(7, _area.Area.InnerPoints.Count);
        }
        [Fact]
        public void ToStringOverrideTest()
        {
            Assert.Equal("Region: (1,1) => (5,0) => (7,3) => (3,4) => ", _area.ToString());
        }

        public void Dispose()
        {

        }
    }
}
