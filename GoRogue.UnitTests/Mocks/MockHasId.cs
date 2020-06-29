namespace GoRogue.UnitTests.Mocks
{
    internal class MyIDImpl : IHasID
    {
        private static readonly IDGenerator idGen = new IDGenerator();

        public MyIDImpl(int myInt)
        {
            ID = idGen.UseID();
            MyInt = myInt;
        }

        public int MyInt { get; }

        public uint ID { get; }

        public override string ToString() => "Thing " + MyInt;
    }
}
