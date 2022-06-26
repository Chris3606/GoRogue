namespace GoRogue.PerformanceTests.SpatialMaps
{
    public class IDObject : IHasID
    {
        private static readonly IDGenerator s_idGenerator = new IDGenerator();

        public IDObject()
        {
            ID = s_idGenerator.UseID();
        }

        public uint ID { get; }
    }

    public class IDLayerObject : IHasID, IHasLayer
    {
        private static readonly IDGenerator s_idGenerator = new IDGenerator();

        public IDLayerObject(int layer)
        {
            ID = s_idGenerator.UseID();
            Layer = layer;
        }

        public uint ID { get; }

        public int Layer { get; }
    }
}
