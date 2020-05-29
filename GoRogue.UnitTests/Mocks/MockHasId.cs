using System;
using System.Collections.Generic;
using System.Text;

namespace GoRogue.UnitTests.Mocks
{
    internal class MyIDImpl : IHasID
    {
        private static IDGenerator idGen = new IDGenerator();

        public MyIDImpl(int myInt)
        {
            ID = idGen.UseID();
            MyInt = myInt;
        }

        public uint ID { get; private set; }
        public int MyInt { get; private set; }

        public override string ToString() => "Thing " + MyInt;
    }
}
