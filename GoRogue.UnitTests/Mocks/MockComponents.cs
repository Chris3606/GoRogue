using System;
using System.Runtime.Serialization;
using GoRogue.Components;

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
}
