using GoRogue.MapGeneration;
using GoRogue.MapGeneration.Steps;
using GoRogue.MapGeneration.Steps.Translation;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Snippets.HowTos.MapGeneration;

public static class Basic
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
