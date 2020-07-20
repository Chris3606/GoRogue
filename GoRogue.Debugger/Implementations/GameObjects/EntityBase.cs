using System;
using GoRogue.Components;
using GoRogue.Debugger.Implementations.Components;
using GoRogue.GameFramework;
using GoRogue.GameFramework.Components;
using GoRogue.SpatialMaps;
using SadRogue.Primitives;

namespace GoRogue.Debugger.Implementations.GameObjects
{
    public abstract class EntityBase : IGameObject
    {
        public virtual int Glyph => _glyph;
        private int _glyph = 0;
        public uint ID { get; }
        public int Layer { get; }

        public void OnMapChanged(Map? newMap)
        {
        }

        public bool CanMove(Point position)
        {
            throw new NotImplementedException();
        }
        public bool CanMoveIn(Direction direction)
        {
            throw new NotImplementedException();
        }

        public Map? CurrentMap { get; }
        public bool IsStatic { get; }
        public bool IsTransparent { get; set; }
        public ITaggableComponentCollection GoRogueComponents { get; }
        public bool IsWalkable { get; set; }
        public Point Position { get; set; }
        public event EventHandler<ItemMovedEventArgs<IGameObject>> Moved;

        public EntityBase(Point position = default, bool isWalkable = true, bool isTransparent = true, int layer = 1, Map map = null, bool isStatic = false, int glyph = 0)
        {
            System.Random initRand = new System.Random();
            ID = (uint) initRand.Next();
            Layer = layer;
            Position = position;
            IsWalkable = isWalkable;
            IsTransparent = isTransparent;
            IsStatic = isStatic;
            GoRogueComponents = new ComponentCollection();
            _glyph = glyph;
        }

        public void AddComponent(IGameObjectComponent component)
        {
            GoRogueComponents.Add(component);
        }

        public void RemoveComponent(IGameObjectComponent component)
        {
            try
            {
                GoRogueComponents.Remove(component);
            }
            catch
            {
                //don't care for now
            }
        }

        public void SetGlyph(int glyph)
        {
            _glyph = glyph;
        }

        public void ElapseTimeUnit()
        {
            foreach (ComponentTagPair componentTagPair in GoRogueComponents)
            {
                ComponentBase component = (ComponentBase)componentTagPair.Component;
            }
        }
    }
}
