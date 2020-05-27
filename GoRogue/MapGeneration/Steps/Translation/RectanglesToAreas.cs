using GoRogue.MapGeneration.ContextComponents;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Steps.Translation
{
    /// <summary>
    /// "Translation" step that takes as input an <see cref="ItemList{Rectangle}"/>, and transforms it into an <see cref="ItemList{Area}"/>.
    /// Can optionally remove the <see cref="ItemList{Rectangle}"/> from the context.
    ///
    /// Context Components Required:
    ///     - <see cref="ItemList{Rectangle}"/> (tag <see cref="RectanglesComponentTag"/>): The list of rectangles to translate to areas
    ///
    /// Context Components Added/Used
    ///     - <see cref="ItemList{Area}"/> (tag <see cref="AreasComponentTag"/>): The list of areas to add the areas representing the rectangles to.  If it does not exist, it will be created.
    /// </summary>
    public class RectanglesToAreas : GenerationStep
    {
        /// <summary>
        /// Tag that must be associated with the component used as input rectangles.
        /// </summary>
        public readonly string RectanglesComponentTag;

        /// <summary>
        /// Tag that must be associated with the component used to store the resulting areas.
        /// </summary>
        public readonly string AreasComponentTag;

        /// <summary>
        /// Whether or not to remove the input list of rectangles from the context.  Defaults to false.
        /// </summary>
        public bool RemoveSourceComponent = false;

        /// <summary>
        /// Creates a new step for translation of <see cref="Rectangle"/> lists to <see cref="Area"/> lists.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="RectanglesToAreas"/>.</param>
        /// <param name="rectanglesComponentTag">Tag that must be associated with the component used as input rectangles.</param>
        /// <param name="areasComponentTag">Tag that must be associated with the component used to store the resulting areas.</param>
        public RectanglesToAreas(string? name, string rectanglesComponentTag, string areasComponentTag)
            : base(name, (typeof(ItemList<Rectangle>), rectanglesComponentTag))
        {
            RectanglesComponentTag = rectanglesComponentTag;
            AreasComponentTag = areasComponentTag;
        }

        /// <summary>
        /// Creates a new step for translation of <see cref="Rectangle"/> lists to <see cref="Area"/> lists, with the name <see cref="RectanglesToAreas"/>.
        /// </summary>
        /// <param name="rectanglesComponentTag">Tag that must be associated with the component used as input rectangles.</param>
        /// <param name="areasComponentTag">Tag that must be associated with the component used to store the resulting areas.</param>
        public RectanglesToAreas(string rectanglesComponentTag, string areasComponentTag)
            : this(null, rectanglesComponentTag, areasComponentTag) { }

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
