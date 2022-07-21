using System;
using System.Runtime.Serialization;
using GoRogue.Components;
using GoRogue.Components.ParentAware;

namespace GoRogue.UnitTests.Mocks
{
    [DataContract]
    [Serializable]
    internal class ComponentBase
    {
        [DataMember]
        public int Value;

        public override bool Equals(object? obj) => obj is ComponentBase component && Value == component.Value;

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => Value.GetHashCode();
    }

    [DataContract]
    [Serializable]
    internal class Component1 : ComponentBase
    { }

    [DataContract]
    [Serializable]
    internal class Component2 : ComponentBase
    { }

    internal class SortedComponent : Component1, ISortedComponent
    {
        public SortedComponent(uint sortOrder)
        {
            SortOrder = sortOrder;
        }

        public uint SortOrder { get; }
    }

    internal class MockObjectWithComponents : IObjectWithComponents
    {
        public IComponentCollection GoRogueComponents { get; }

        public MockObjectWithComponents()
        {
            GoRogueComponents = new ComponentCollection(this);
        }
    }

    internal class MockParentAwareComponentBase : ParentAwareComponentBase
    {
        public int TimesAddedCalled { get; private set; }
        public int TimesRemovedCalled { get; private set; }

        public MockParentAwareComponentBase()
        {
            Added += OnAdded;
            Removed += OnRemoved;
        }

        private void OnRemoved(object? sender, EventArgs e)
        {
            TimesRemovedCalled++;
        }

        private void OnAdded(object? sender, EventArgs e)
        {
            TimesAddedCalled++;
        }

        public void ClearHistory()
        {
            TimesAddedCalled = 0;
            TimesRemovedCalled = 0;
        }
    }
}
