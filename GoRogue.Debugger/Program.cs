namespace GoRogue.Debugger
{
    internal static class Program
    {
        private static void Main()
        {
            Interpreter.Init();
            Interpreter.Run();
        }
    }
}
