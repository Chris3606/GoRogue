using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.MapGeneration
{
    public class MultiAreaTests
    {
        #region Test Data

        private static readonly IEnumerable<Area> _subAreas = new[]
        {
            new Area((1, 2), (3, 4), (5, 6)), new Area((9, 10), (11, 12), (14, 13))
        };

        private static readonly Rectangle _boundsOutsideAreas = new Rectangle(0, 0, 50, 50);

        public static readonly IEnumerable<MultiArea> Areas = new[]
        {
            new MultiArea(_subAreas.First()),
            new MultiArea(_subAreas.Take(2)),
            new MultiArea(_subAreas.First().Yield().Append(_subAreas.Last())),
            new MultiArea(_subAreas)
        };
        #endregion

        [Fact]
        public void CreateEmpty()
        {
            var area = new MultiArea();

            Assert.Empty(area.SubAreas);
            Assert.Equal(Rectangle.Empty, area.Bounds);
            Assert.Empty(area);
        }

        [Fact]
        public void CreateFromSingleArea()
        {
            // Create area for testing
            var subArea = new Area((1, 2), (3, 4), (5, 6));
            var area = new MultiArea(subArea);

            // Should have one sub-area
            Assert.Single(area.SubAreas);
            Assert.Equal(area.Count, subArea.Count);
            Assert.Equal(subArea, area.SubAreas[0]);
            Assert.NotEmpty(area);
            TestUtils.Matches<IReadOnlyArea>(subArea, area);
            TestUtils.Matches<IReadOnlyArea>(area, subArea);
        }

        [Fact]
        public void CreateFromMultipleAreas()
        {
            // Create area for testing
            var area = new MultiArea(_subAreas);

            // Should have one sub-area
            Assert.Equal(2, area.SubAreas.Count);
            Assert.Equal(area.Count, _subAreas.Sum(a => a.Count));
            Assert.Equal(_subAreas, area.SubAreas);
            Assert.NotEmpty(area);
        }

        [Theory]
        [MemberDataEnumerable(nameof(Areas))]
        public void ContainsPoints(MultiArea area)
        {
            foreach (var point in _boundsOutsideAreas.Positions())
            {
                bool subAreasContain = area.SubAreas.Any(i => i.Contains(point));
                Assert.Equal(subAreasContain, area.Contains(point));
            }
        }

        [Theory]
        [MemberDataEnumerable(nameof(Areas))]
        public void ContainsIntPoints(MultiArea area)
        {
            foreach (var point in _boundsOutsideAreas.Positions())
            {
                bool subAreasContain = area.SubAreas.Any(i => i.Contains(point.X, point.Y));
                Assert.Equal(subAreasContain, area.Contains(point.X, point.Y));
            }
        }

        [Theory]
        [MemberDataEnumerable(nameof(Areas))]
        public void ContainsArea(MultiArea area)
        {
            foreach (var point in _boundsOutsideAreas.Positions())
            {
                var compArea = new Area(point);
                bool subAreasContain = area.SubAreas.Any(i => i.Contains(compArea));
                Assert.Equal(subAreasContain, area.Contains(compArea));
            }
        }

        [Theory]
        [MemberDataEnumerable(nameof(Areas))]
        public void IntersectsArea(MultiArea area)
        {
            foreach (var point in _boundsOutsideAreas.Positions())
            {
                var compArea = new Area(point);
                bool subAreasIntersect = area.SubAreas.Any(i => i.Intersects(compArea));
                Assert.Equal(subAreasIntersect, area.Intersects(compArea));
            }
        }

        [Theory]
        [MemberDataEnumerable(nameof(Areas))]
        public void GetPoint(MultiArea area)
        {
            var points = new HashSet<Point>(area);

            // Test requires all unique areas and that all points are included in the returned areas
            Assert.Equal(area.SubAreas.Sum(i => i.Count), points.Count);

            // Check that we get the right point back, based on its calculated index
            foreach (var point in points)
            {
                int index = FindIndexOf(area.SubAreas, point);
                Assert.Equal(point, area[index]);
            }
        }

        [Fact]
        public void AddSubArea()
        {
            var area = new MultiArea();
            Assert.Empty(area.SubAreas);
            Assert.Equal(0, area.Count);
            Assert.Empty(area);

            int i = 1;
            foreach (var subArea in _subAreas)
            {
                area.Add(subArea);
                Assert.Equal(_subAreas.Take(i), area.SubAreas);
                Assert.Equal(_subAreas.Take(i).Sum(a => a.Count), area.Count);
                Assert.Equal(_subAreas.Take(i).Flatten(), area);

                i++;
            }
        }

        [Fact]
        public void RemoveSubArea()
        {
            var area = new MultiArea(_subAreas);

            Assert.Equal(_subAreas, area.SubAreas);
            Assert.Equal(_subAreas.Sum(a => a.Count), area.Count);
            Assert.Equal(_subAreas.Flatten(), area);

            int i = 1;
            foreach (var subArea in _subAreas)
            {
                area.Remove(subArea);
                Assert.Equal(_subAreas.Skip(i), area.SubAreas);
                Assert.Equal(_subAreas.Skip(i).Sum(a => a.Count), area.Count);
                Assert.Equal(_subAreas.Skip(i).Flatten(), area);

                i++;
            }
        }

        // Returns an index of the point, where index 0 is the first index
        // of the first area, index area1.Count is the first index of the second
        // area, etc (mimicking the functionality of MultiArea's indexer in a more
        // primitive way)
        private static int FindIndexOf(IEnumerable<IReadOnlyArea> areas, Point point)
        {
            int sum = 0;
            foreach (var area in areas)
            {
                foreach (var p in area)
                {
                    if (p == point) return sum;
                    sum++;
                }
            }

            throw new ArgumentException("Cannot find point in any of the given areas.", nameof(point));
        }
    }
}
