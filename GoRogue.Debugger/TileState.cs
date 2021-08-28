namespace GoRogue.Debugger
{
    /// <summary>
    /// Used to indicate the current state of each tile (whether it is within a region or not),
    /// for the sake of efficiently generating a view.
    /// </summary>
    public enum TileState
    {
        Wall,
        Floor,
        Door,
    }
}
