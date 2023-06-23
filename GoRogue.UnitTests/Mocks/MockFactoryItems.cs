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
    internal class FactoryItemBlueprint : IFactoryBlueprint<string, FactoryItem>
    {
        public string ID { get; }

        public FactoryItemBlueprint(string id) => ID = id;

        public FactoryItem Create() => new FactoryItem();

        public override bool Equals(object? obj) => obj is FactoryItemBlueprint f && f.ID == ID;

        public override int GetHashCode() => ID.GetHashCode();
    }

    [Serializable]
    internal class AdvancedFactoryItemBlueprint : IAdvancedFactoryBlueprint<string, int, FactoryItem>
    {
        public string ID { get; }

        public AdvancedFactoryItemBlueprint(string id) => ID = id;
        public FactoryItem Create(int config) => new FactoryItem { Value = config };

        public override bool Equals(object? obj) => obj is AdvancedFactoryItemBlueprint f && f.ID == ID;

        public override int GetHashCode() => ID.GetHashCode();
    }
}
