namespace GoRogue.UnitTests.Mocks
{
    internal class ComponentBase
    { }

    internal class Component1 : ComponentBase
    { }

    internal class Component2 : ComponentBase
    { }

    internal class SortedComponent : Component1, ISortedComponent
    {
        public SortedComponent(uint sortOrder) => SortOrder = sortOrder;

        public uint SortOrder { get; }
    }
}
