namespace GoRogue.UnitTests.Mocks
{
    public class MockSpatialMapItem : IHasID, IHasLayer
    {
        private static readonly IDGenerator _idGen = new IDGenerator();

        public MockSpatialMapItem(int layer)
        {
            ID = _idGen.UseID();
            Layer = layer;
        }

        public uint ID { get; }
        public int Layer { get; }

        public override string ToString() => $"[{ID}, {Layer}]";
    }
}
