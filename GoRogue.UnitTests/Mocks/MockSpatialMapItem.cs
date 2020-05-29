using System;
using System.Collections.Generic;
using System.Text;

namespace GoRogue.UnitTests.Mocks
{

    public class MockSpatialMapItem : IHasID, IHasLayer
    {
        public int Layer { get; }
        public uint ID { get; }

        private static IDGenerator _idGen = new IDGenerator();

        public MockSpatialMapItem(int layer)
        {
            ID = _idGen.UseID();
            Layer = layer;
        }

        public override string ToString()
        {
            return $"[{ID}, {Layer}]";
        }
    }
}
