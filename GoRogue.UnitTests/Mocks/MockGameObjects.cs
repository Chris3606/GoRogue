using System;
using GoRogue.Components;
using GoRogue.GameFramework;
using SadRogue.Primitives;

namespace GoRogue.UnitTests.Mocks
{
    /// <summary>
    /// A GameObject subclass which mainly serves as a way to test casting-based item-retrieval functions in Map.
    /// </summary>
    internal class MockGameObject : GameObject
    {
        public MockGameObject(Point position, int layer, bool isWalkable = true, bool isTransparent = true, Func<uint>? idGenerator = null, IComponentCollection? customComponentCollection = null)
            : base(position, layer, isWalkable, isTransparent, idGenerator, customComponentCollection)
        { }

        public MockGameObject(int layer, bool isWalkable = true, bool isTransparent = true, Func<uint>? idGenerator = null, IComponentCollection? customComponentCollection = null)
            : base(layer, isWalkable, isTransparent, idGenerator, customComponentCollection)
        { }
    }
}
