using GoRogue.Components;
using GoRogue.Debugger.Implementations.Components;
using GoRogue.GameFramework;
using GoRogue.GameFramework.Components;
using SadRogue.Primitives;

namespace GoRogue.Debugger.Implementations.GameObjects
{
    public abstract class EntityBase : GameObject
    {
        public EntityBase(Point position = default,
                          bool isWalkable = true,
                          bool isTransparent = true,
                          int layer = 1,
                          bool isStatic = false)
            : base(position, layer, null, isStatic, isWalkable, isTransparent)
        { }

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

        //public void SetGlyph(int glyph)
        //{
        //    _glyph = glyph;
        //}

        public void ElapseTimeUnit()
        {
            // TODO: Unused
            foreach (ComponentTagPair componentTagPair in GoRogueComponents)
            {
                ComponentBase component = (ComponentBase)componentTagPair.Component;
            }
        }
    }
}
