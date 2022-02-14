namespace GoRogue.PerformanceTests.SpatialMaps
{
    public class IDObject: IHasID
    {
        private static readonly IDGenerator s_idGenerator = new IDGenerator();

        public IDObject()
        {
            ID = s_idGenerator.UseID();
        }

        public uint ID { get; }
    }
}
