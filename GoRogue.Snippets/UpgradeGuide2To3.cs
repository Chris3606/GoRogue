using SadRogue.Primitives;
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
        }
    }
}
