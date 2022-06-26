using Xunit;

namespace GoRogue.UnitTests
{
    public class DisjointSetTests
    {
        // Test that each item in a DisjointSet starts as a unique set
        [Fact]
        public void SimpleDisjointSetInitialization()
        {
            var items = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var set = new DisjointSet(items.Length);

            foreach (int item in items)
                Assert.Equal(item, set.Find(item));
        }

        // Test that each item in a DisjointSet<T> starts as a unique set
        [Fact]
        public void DisjointSetInitialization()
        {
            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var set = new DisjointSet<int>(items);
            
            foreach (int item in items)
                Assert.Equal(item, set.Find(item));
        }

        // Test DisjointSet.MakeUnion
        [Fact]
        public void SimpleDisjointSetMakeUnion()
        {
            var items = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var set = new DisjointSet(items.Length);
            int count = set.Count;

            for (int i = 0; i < items.Length - 1; i++)
            {
                set.MakeUnion(items[i], items[i] + 1);
                Assert.Equal(--count, set.Count);
            }

            // All items should have the same parent after unions; not necessarily the first one (that's up to the algorithm)
            var parent = set.Find(items[0]);
            
            Assert.Equal(1, set.Count);
            foreach (int item in items)
                Assert.Equal(parent, set.Find(item));
        }

        // Test DisjointSet<T>.MakeUnion
        [Fact]
        public void DisjointSetMakeUnion()
        {
            var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var set = new DisjointSet<int>(items);
            int count = set.Count;

            for (int i = 0; i < items.Length - 1; i++)
            {
                set.MakeUnion(items[i], items[i] + 1);
                Assert.Equal(--count, set.Count);
            }

            // All items should have the same parent after unions; not necessarily the first one (that's up to the algorithm)
            var parent = set.Find(items[0]);

            Assert.Equal(1, set.Count);
            foreach (int item in items)
                Assert.Equal(parent, set.Find(item));
        }
    }
}
