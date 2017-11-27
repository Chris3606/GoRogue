namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Generates a simple rectangular box -- walls will be impassable, everything else will be passable.
    /// </summary>
    /// <remarks>
    /// Takes as a constructor parameter a settable map -- after generation, impassable tiles will be set to false, whereas
    /// passable ones will be set to true.
    /// </remarks>
    public class RectangleMapGenerator : IMapGenerator
    {
        private ISettableMapOf<bool> map;

        /// <summary>
        /// Constructor.  Takes map to set values to.
        /// </summary>
        /// <param name="map">The map that Generate will set values to.</param>
        public RectangleMapGenerator(ISettableMapOf<bool> map)
        {
            this.map = map;
        }

        /// <summary>
        /// Generates the map, setting the map given in the constructor as a "walkability map".  Wall tiles
        /// (the edges of the map) will have a value of false set in the given map, whereas true will be set
        /// to all non-wall tiles.
        /// </summary>
        public void Generate()
        {
            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                {
                    if (x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1)
                        map[x, y] = false;
                    else
                        map[x, y] = true;
                }
        }
    }
}