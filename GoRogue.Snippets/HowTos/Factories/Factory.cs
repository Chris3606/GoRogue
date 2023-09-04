// ReSharper disable NotAccessedPositionalProperty.Local
// ReSharper disable UnusedVariable

namespace GoRogue.Snippets.HowTos.Factories
{
    #region BasicExample
    using GoRogue.Factories;

    public static class BasicExample
    {
        // Arbitrary class we want to create instances of.  Implementing the IFactoryObject
        // interface is optional, however when we do the DefinitionID field will automatically
        // be set to the ID of the blueprint used to create it when a factory's Create method
        // is called.
        record Terrain(int Glyph, bool IsWalkable, bool IsTransparent) : IFactoryObject<string>
        {
            public string DefinitionID { get; set; } = "";
        }

        public static void ExampleCode()
        {
            // We'll identify the blueprints with strings in this instance, but this could be
            // an enum or any hashable type
            var factory = new Factory<string, Terrain>()
            {
                new LambdaFactoryBlueprint<string, Terrain>(
                    "Floor",
                    () => new Terrain('.', true, true)),
                new LambdaFactoryBlueprint<string, Terrain>(
                    "Wall",
                    () => new Terrain('#', false, false))
            };

            var floorTile = factory.Create("Floor");
            var wallTile = factory.Create("Wall");
        }
    }

    #endregion
}

namespace GoRogue.Snippets.HowTos.Factories
{
    #region SubclassExample
    using GoRogue.Factories;

    public static class SubclassExample
    {
        record Terrain(int Glyph, bool IsWalkable, bool IsTransparent) : IFactoryObject<string>
        {
            public string DefinitionID { get; set; } = "";
        }

        class MyFactory : Factory<string, Terrain>
        {
            public MyFactory()
            {
                Add(new LambdaFactoryBlueprint<string, Terrain>("Floor", Floor));
                Add(new LambdaFactoryBlueprint<string, Terrain>("Wall", Wall));
            }

            private Terrain Floor() => new('.', true, true);
            private Terrain Wall() => new('#', false, false);
        }

        public static void ExampleCode()
        {
            var factory = new MyFactory();

            var floorTile = factory.Create("Floor");
            var wallTile = factory.Create("Wall");
        }

    }

    #endregion
}

namespace GoRogue.Snippets.HowTos.Factories
{
    #region CustomBlueprintExample
    using GoRogue.Factories;

    public static class CustomBlueprintExample
    {
        record Terrain(int Glyph, bool IsWalkable, bool IsTransparent) : IFactoryObject<string>
        {
            public string DefinitionID { get; set; } = "";
        }

        // A blueprint for terrain which counts the number of times each item type is
        // instantiated.
        record TerrainBlueprint(string ID, int Glyph, bool IsWalkable, bool IsTransparent)
            : IFactoryBlueprint<string, Terrain>
        {
            private static readonly Dictionary<string, int> s_countingDictionary = new();

            public Terrain Create()
            {
                s_countingDictionary[ID] = s_countingDictionary.GetValueOrDefault(ID, 0) + 1;
                return new Terrain(Glyph, IsWalkable, IsTransparent);
            }
        }

        public static void ExampleCode()
        {
            var factory = new Factory<string, Terrain>()
            {
                new TerrainBlueprint(
                    "Floor", '.', true, true),
                new TerrainBlueprint(
                    "Wall", '#', false, false)
            };

            var floorTile = factory.Create("Floor");
            var wallTile = factory.Create("Wall");
        }
    }

    #endregion
}
