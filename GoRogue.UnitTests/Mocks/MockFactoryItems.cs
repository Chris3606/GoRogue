using System;
using GoRogue.Factories;

namespace GoRogue.UnitTests.Mocks
{
    [Serializable]
    class FactoryItem
    {
        public int Value;
    }

    [Serializable]
    internal class FactoryItemBlueprint : IFactoryBlueprint<FactoryItem>
    {
        public string Id { get; }

        public FactoryItemBlueprint(string id) => Id = id;

        public FactoryItem Create() => new FactoryItem();

        public override bool Equals(object? obj) => obj is FactoryItemBlueprint f && f.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();
    }

    [Serializable]
    internal class AdvancedFactoryItemBlueprint : IAdvancedFactoryBlueprint<int, FactoryItem>
    {
        public string Id { get; }

        public AdvancedFactoryItemBlueprint(string id) => Id = id;
        public FactoryItem Create(int config) => new FactoryItem { Value = config };

        public override bool Equals(object? obj) => obj is AdvancedFactoryItemBlueprint f && f.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
