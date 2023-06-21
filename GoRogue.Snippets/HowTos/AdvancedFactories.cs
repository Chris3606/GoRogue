using GoRogue.Factories;
using SadRogue.Primitives;

namespace GoRogue.Snippets.HowTos;

#region AdvancedFactoryBasicExample
public static class AdvancedFactoryBasicExample
{
    record Terrain(Point Position, int Glyph, bool IsWalkable, bool IsTransparent)
        : IFactoryObject<string>
    {
        public string DefinitionID { get; set; } = "";
    }

    public static void ExampleCode()
    {
        // LambdaAdvancedFactoryBlueprint is the same as LambdaFactoryBlueprint but
        // implements IAdvancedFactoryBlueprint instead, which allows its creation
        // function to take parameters. This is useful, for example, to create objects
        // that require parameters to be passed to their constructor.
        var factory = new AdvancedFactory<string, Point, Terrain>
        {
            new LambdaAdvancedFactoryBlueprint<string, Point, Terrain>(
                "Floor",
                pos => new Terrain(pos, '.', true, true)),
            new LambdaAdvancedFactoryBlueprint<string, Point, Terrain>(
                "Wall",
                pos => new Terrain(pos, '#', false, false))
        };

        var floorTile = factory.Create("Floor", new Point(1, 2));
        var wallTile = factory.Create("Wall", new Point(3, 4));
    }
}
#endregion
