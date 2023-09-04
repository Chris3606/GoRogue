// ReSharper disable UnusedVariable
// ReSharper disable UnusedAutoPropertyAccessor.Global

#region Usings
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapGeneration.Steps;
using GoRogue.MapGeneration.Steps.Translation;
using SadRogue.Primitives.GridViews;
using SadRogue.Primitives;
#endregion

namespace GoRogue.Snippets.HowTos.MapGeneration;

public static class MapGeneration
{
    public static void GettingStarted()
    {
        #region GeneratingMap
        // The map will have a width of 60 and height of 40
        var generator = new Generator(60, 40);

        // Add the steps to generate a map using the DungeonMazeMap built-in algorithm,
        // and generate the map.
        generator.ConfigAndGenerateSafe(gen =>
        {
            gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps());
        });
        #endregion

        #region AccessingMap
        var wallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
        foreach (var pos in wallFloorValues.Positions())
            if (wallFloorValues[pos])
                Console.WriteLine($"{pos} is a floor.");
            else
                Console.WriteLine($"{pos} is a wall.");
        #endregion
    }

    public static void UsingGenerationSteps()
    {
        #region UsingGenerationSteps
        var generator = new Generator(60, 40);

        // Specify initial configuration for steps, and execute steps added.  If any of the steps
        // raise RegenerateMapException, the context and steps will be removed, then regenerated
        // according to the specified configuration, and generation will be re-run.
        generator.ConfigAndGenerateSafe(gen =>
        {
            gen.AddStep
                (
                    // Sets custom values for some parameters, leaves others at their default
                    new RoomsGeneration
                    {
                        MinRooms = 2,
                        MaxRooms = 8,
                        RoomMinSize = 3,
                        RoomMaxSize = 9
                    }
                )
                // According to the documentation, RoomsGenerationStep records the rooms it creates
                // in an ItemList<Room> component, with the tag "Rooms" (unless changed via
                // constructor parameter).  However, the area connection algorithm we want to run
                // operates on an ItemList<Area>, with the tag "Areas", by default.  So, we have a
                // "translation step" that creates areas from rectangles, and adds them to a new
                // component.  We specify the tags of components to input from and output to, to
                // match what the previous generation step creates the the next one expects
                .AddStep(new RectanglesToAreas("Rooms", "Areas"))
                // Connects areas together.  This component by default uses the component with the
                // tag "Areas" for areas to connect, so since we haven't changed it,  it will
                // connect the areas representing our rooms.
                .AddStep(new ClosestMapAreaConnection());
        });
        #endregion
    }

    public static void GenerateMapStageByStage()
    {
        #region GenerateMapStageByStage
        var generator = new Generator(60, 40);

        var iterator = generator.ConfigAndGetStageEnumeratorSafe(gen =>
        {
            // We use DungeonMazeMapSteps here but this could be any generation steps, including
            // custom ones
            gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps());
        });

        while (iterator.MoveNext()) { /* Stage complete */ }
        #endregion
    }
}

#region GenerationStepRequiringComponents
record RequiredComponentType1;
record RequiredComponentType2;

public class GenerationStepRequiringComponents : GenerationStep
{
    public GenerationStepRequiringComponents()
        // This generation step is requiring the context to have a component of type
        // RequiredComponentType1, that has the tag "Component1Tag".  Additionally,
        // it is required to have a component of type "RequiredComponentType2", with no
        // particular tag (any object of that type, with or without a tag, will suffice)
        : base(null,
            (typeof(RequiredComponentType1), "Component1Tag"),
            (typeof(RequiredComponentType2), null)) { }

    protected override IEnumerator<object?> OnPerform(GenerationContext context)
    {
        // Both of these functions are guaranteed to return a component successfully,
        // because the components were specified in the constructor as required
        var requiredComponent1 = context.GetFirst<RequiredComponentType1>("Component1Tag");
        var requiredComponent2 = context.GetFirst<RequiredComponentType2>();

        // Do generation

        yield break;
    }
}
#endregion

#region GenerationStepUseOrAddComponent
public class GenerationStepUseOrAddComponent : GenerationStep
{
    protected override IEnumerator<object?> OnPerform(GenerationContext context)
    {
        // Get an ItemList<Rectangle> component with the tag "Rooms" if it exists;
        // otherwise create a new one, add it, and return that
        var roomsList = context.GetFirstOrNew(
            () => new ItemList<Rectangle>(),
            "Rooms");
        // Get an ISettableGridView<bool> component with the tag "WallFloor" if it
        // exists; otherwise create a new ArrayView of the appropriate size
        // (which is a subclass of ISettableGridView), add it, and return that
        var wallFloor = context.GetFirstOrNew<ISettableGridView<bool>>(
            () => new ArrayView<bool>(context.Width, context.Height),
            "WallFloor");

        // Do generation

        yield break;
    }
}
#endregion

#region GenerationStepPauseViaEnumerable
public class GenerationStepPauseViaEnumerable : GenerationStep
{
    public int NumRooms { get; init; }

    protected override IEnumerator<object?> OnPerform(GenerationContext context)
    {
        for (int i = 0; i < NumRooms; i++)
        {
            /* Generate a room and modify context appropriately */

            // Use "yield" to break the algorithm up in between generated rooms
            yield return null;
        }
    }
}
#endregion GenerationStepPauseViaEnumerable
