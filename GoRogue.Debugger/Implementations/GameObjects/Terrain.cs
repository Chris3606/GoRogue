using SadRogue.Primitives;

namespace GoRogue.Debugger.Implementations.GameObjects
{
    public class Terrain : EntityBase
    {
        // Terrain can only have one of four values depending on IsWalkable and IsTransparent
        public override int Glyph => IsWalkable ? (IsTransparent ? '.' : 'I') : IsTransparent ? '-' : '#';
        public Terrain(Point position, bool walkable, bool transparent)
            : base(position, walkable, transparent, 0, true)
        { }
    }
}
