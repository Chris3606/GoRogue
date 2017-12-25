namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Specifies the method of connecting two areas.
    /// </summary>
    public enum AreaConnectionStrategy
    {
        /// <summary>
        /// Connects two areas by choosing random points from the list of coordinates in each area, and connecting
        /// those two points.
        /// </summary>
        RANDOM_POINT,
        /// <summary>
        /// Connects two areas by connecting the center points of the bounding rectangle for each area.
        /// On concave shapes, the center point of the bounding rectangle is not guaranteed to have a walkable
        /// connection to the rest of the area, so when concave map areas are present this may not connect areas properly.
        /// </summary>
        CENTER_BOUNDS
    };
}
