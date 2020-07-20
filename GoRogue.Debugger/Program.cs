using System;

namespace GoRogue.Debugger
{
    class Program
    {
        static void Main(string[] args)
        {
            Interpreter.Init();
            Interpreter.Run();
        }
    }
}
