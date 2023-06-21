using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Snippets.HowTos.MapGeneration;

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
