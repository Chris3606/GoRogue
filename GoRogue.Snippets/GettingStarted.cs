namespace GoRogue.Snippets
{
    public static class GettingStarted
    {
        public static void ExampleCode()
        {
            #region ExampleMainFunction
            Console.WriteLine(new SadRogue.Primitives.Point(1, 2));

            // Used to stop window from closing until a key is pressed.
            int c = Console.Read();
            #endregion
        }
    }
}
