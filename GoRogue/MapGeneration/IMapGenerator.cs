namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Abstract interface for an algorithm that generates a map.
    /// </summary>
    public interface IMapGenerator
    {
        /// <summary>
        /// Should generate the map.
        /// </summary>
        void Generate();
    }
}