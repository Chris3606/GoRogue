using System.Collections.Generic;
using GoRogue.MapGeneration.ContextComponents;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Steps.Translation
{
    /// <summary>
    /// Removes all points from an area list that are in any of the areas present in another list.
    /// Context Components Required:
    /// - <see cref="ItemList{TItem}" /> (tag <see cref="UnmodifiedAreaListTag" />: The list of areas that will not be
    /// modified, but will serve as a basis for points to remove from areas in
    /// the other list.
    /// - <see cref="ItemList{Area}" /> (tag <see cref="ModifiedAreaListTag" />: The list of areas that will be modified; all
    /// areas in this list will have any points that also appear in
    /// areas in the other list removed.  If an area ends up with no remaining points, it is removed from the list.
    /// </summary>
    /// <remarks>
    /// This component will removes all positions from any areas in the the modified area list, that are also present in one or
    /// more areas in
    /// the unmodified area list.  If an area is modified such that it has no remaining points, it is removed from the list
    /// entirely.
    /// This ensures that the two lists do not contain any positions that overlap with each other.
    /// </remarks>
    [PublicAPI]
    public class RemoveDuplicatePoints : GenerationStep
    {
        /// <summary>
        /// Tag that must be associated with the component used as the area list from which duplicates are removed.
        /// </summary>
        public readonly string ModifiedAreaListTag;

        /// <summary>
        /// Tag that must be associated with the component used as the unmodified area list.
        /// </summary>
        public readonly string UnmodifiedAreaListTag;

        /// <summary>
        /// Creates a new area duplicate point remover step.
        /// </summary>
        /// <param name="name">The name of the generation step.</param>
        /// <param name="unmodifiedAreaListTag">Tag that must be associated with the component used as the unmodified area list.</param>
        /// <param name="modifiedAreaListTag">
        /// Tag that must be associated with the component used as the area list from which
        /// duplicates are removed.
        /// </param>
        public RemoveDuplicatePoints(string? name, string unmodifiedAreaListTag, string modifiedAreaListTag)
            : base(name, (typeof(ItemList<Area>), unmodifiedAreaListTag), (typeof(ItemList<Area>), modifiedAreaListTag))
        {
            UnmodifiedAreaListTag = unmodifiedAreaListTag;
            ModifiedAreaListTag = modifiedAreaListTag;

            // Validate here because it was given in the constructor
            if (ModifiedAreaListTag == UnmodifiedAreaListTag)
                throw new InvalidConfigurationException(this, nameof(ModifiedAreaListTag),
                    $"The value must be different than the value of {nameof(UnmodifiedAreaListTag)}.");
        }

        /// <summary>
        /// Creates a new area duplicate point remover step, with the name <see cref="RemoveDuplicatePoints" />.
        /// </summary>
        /// <param name="unmodifiedAreaListTag">Tag that must be associated with the component used as the unmodified area list.</param>
        /// <param name="modifiedAreaListTag">
        /// Tag that must be associated with the component used as the area list from which
        /// duplicates are removed.
        /// </param>
        public RemoveDuplicatePoints(string unmodifiedAreaListTag, string modifiedAreaListTag)
            : this(null, unmodifiedAreaListTag, modifiedAreaListTag)
        { }

        /// <inheritdoc />
        protected override void OnPerform(GenerationContext context)
        {
            // Get required components
            var areaList1 =
                context.GetComponent<ItemList<Area>>(
                    ModifiedAreaListTag)!; // Not null because is in required components list
            var areaList2 =
                context.GetComponent<ItemList<Area>>(
                    UnmodifiedAreaListTag)!; // Not null because is in required components list

            // Cache all positions in any area of area1List
            var areaList1Positions = new HashSet<Point>();
            foreach (var area in areaList1.Items)
                foreach (var point in area.Positions)
                    areaList1Positions.Add(point);

            // Remove any position in second list's areas that is already in the first
            foreach (var area in areaList2.Items)
                area.Remove(pos => areaList1Positions.Contains(pos));

            // Remove any areas that now contain no positions
            areaList2.RemoveItems(area => area.Count == 0);
        }
    }
}
