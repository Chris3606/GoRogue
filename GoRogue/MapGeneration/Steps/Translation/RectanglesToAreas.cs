using GoRogue.MapGeneration.ContextComponents;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Steps.Translation
{
    /// <summary>
    /// "Translation" step that takes as input an <see cref="ItemList{Rectangle}"/>, and transforms it into an <see cref="ItemList{Area}"/>.
    /// Can optionally remove the <see cref="ItemList{Rectangle}"/> from the context.
    ///
    /// Context Components Required:
    /// <list type="table">
    /// <listheader>
    /// <term>Component</term>
    /// <description>Default Tag</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="ItemList{Rectangle}"/></term>
    /// <description>"Rooms"</description>
    /// </item>
    /// </list>
    ///
    /// Context Components Added/Used:
    /// <list type="table">
    /// <listheader>
    /// <term>Component</term>
    /// <description>Default Tag</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="ItemList{Area}"/></term>
    /// <description>"Areas"</description>
    /// </item>
    /// </list>
    /// </summary>
    public class RectanglesToAreas : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the component used as input rectangles.
        /// </summary>
        public readonly string? RectanglesComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component used to store the resulting areas.
        /// </summary>
        public readonly string? AreasComponentTag;

        /// <summary>
        /// Whether or not to remove the input list of rectangles from the context.  Defaults to false.
        /// </summary>
        public bool RemoveSourceComponent = false;

        /// <summary>
        /// Creates a new step for translation of <see cref="Rectangle"/> lists to <see cref="Area"/> lists.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="RectanglesToAreas"/>.</param>
        /// <param name="rectanglesComponentTag">Optional tag that must be associated with the component used as input rectangles.  Defaults to "Rooms".</param>
        /// <param name="areasComponentTag">Optional tag that must be associated with the component used to store the resulting areas.  Defaults to "Areas".</param>
        public RectanglesToAreas(string? name = null, string? rectanglesComponentTag = "Rooms", string? areasComponentTag = "Areas")
            : base(name, (typeof(ItemList<Rectangle>), rectanglesComponentTag))
        {
            RectanglesComponentTag = rectanglesComponentTag;
            AreasComponentTag = areasComponentTag;
        }

        /// <inheritdoc/>
        protected override void OnPerform(GenerationContext context)
        {
            // Get required components
            var rectangles = context.GetComponent<ItemList<Rectangle>>(RectanglesComponentTag)!; // Not null because is in required components list

            // Get/create output component as needed
            var areas = context.GetComponentOrNew(() => new ItemList<Area>(), AreasComponentTag);

            if (RemoveSourceComponent)
                context.RemoveComponent(rectangles);

            foreach (var rect in rectangles.Items)
            {
                var area = new Area();
                area.Add(rect.Positions());
                areas.AddItem(area, Name);
            }
        }
    }
}
