using SadRogue.Primitives;
using ShaiRandom.Generators;
using ShaiRandom.Wrappers;

// ReSharper disable UnusedVariable

namespace GoRogue.Snippets
{
    public static class UpgradeGuide2To3
    {
        public static void ExampleCode()
        {
            #region CustomEnumerators
            var rect = new Rectangle(1, 2, 3, 4);

            // Positions() returns a RectanglePositionsEnumerator, but you can use it exactly as if it
            // returned IEnumerable
            foreach (var pos in rect.Positions())
                Console.WriteLine(pos);

            // You can even use it with LINQ
            var positions = rect.Positions().ToArray();
            #endregion

            var myList = new List<int>{1, 2, 3, 4, 5};
            var myRNG = Random.GlobalRandom.DefaultRNG;

            #region ShuffleListExample
            myRNG.Shuffle(myList);
            #endregion

            var myRect = new Rectangle(1, 2, 3, 4);
            #region RNGExtensionsCustomTypes
            var itemFromList = myRNG.RandomElement(myList);
            var itemFromRect = myRNG.RandomPosition(myRect);
            #endregion
        }

        #region ArchivalWrapper
        // Assume we have an algorithm which uses an RNG, and has a bug
        // which causes it to sometimes fail (return false)
        public static bool AlgorithmThatSometimesFails(IEnhancedRandom rng)
        {
            // We simulate a failure by just returning false 50% of the time
            // for this example
            return rng.PercentageCheck(50f);
        }

        // Find an issue that occurs sometimes, and get a way to reproduce it
        public static string FindAndReproduceProblem()
        {
            KnownSeriesRandom? sequence = null;
            while (sequence == null)
            {
                var wrapper = new ArchivalWrapper(Random.GlobalRandom.DefaultRNG);
                var result = AlgorithmThatSometimesFails(wrapper);
                if (!result)
                    sequence = wrapper.MakeArchivedSeries();
            }

            // This string, when deserialized, produces a KnownSeriesRandom that will
            // always produce the problem we found on the first call to
            // AlgorithmThatSometimesFails
            return sequence.StringSerialize();
        }
        #endregion
    }
}
