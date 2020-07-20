using GoRogue.GameFramework;
using GoRogue.GameFramework.Components;

namespace GoRogue.Debugger.Implementations.Components
{
    public abstract class ComponentBase : IGameObjectComponent
    {
        public IGameObject? Parent { get; set; }
    }
}
