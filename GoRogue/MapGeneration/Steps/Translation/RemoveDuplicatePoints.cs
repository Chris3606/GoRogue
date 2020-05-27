using System.Collections.Generic;
using GoRogue.MapGeneration.ContextComponents;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Steps.Translation
{
    /// <summary>
    /// Removes all points from an area list that are in any of the areas present in another list.
    ///
    ///  Context Components Required:
    /// <list type="table">
    /// <listheader>
    /// <term>Component</term>
    /// <description>Default Tag</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="ItemList{Area}"/></term>
    /// <description>"UnmodifiedAreaList"</description>
    /// </item>
    /// <item>
    /// <term><see cref="ItemList{Area}"/></term>
    /// <description>"ModifiedAreaList"</description>
    /// </item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// This component will removes all positions from any areas in the the modified area list, that are also present in one or more areas in
    /// the unmodified area list.  This ensures that the two lists do not contain any positions that overlap with each other.
    /// </remarks>
    public class RemoveDuplicatePoints : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the component used as the unmodified area list.  Defaults to "AreaList1".
        /// </summary>
        public readonly string? UnmodifiedAreaListTag;

        /// <summary>
        /// Optional tag that must be associated with the component used as the area list from which duplicates are removed.  Defaults to "AreaList2".
        /// </summary>
        public readonly string? ModifiedAreaListTag;

        /// <summary>
        /// Creates a new area duplicate point remover step.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="RemoveDuplicatePoints"/>.</param>
        /// <param name="unmodifiedAreaListTag">Optional tag that must be associated with the component used as the unmodified area list.  Defaults to "AreaList1".</param>
        /// <param name="modifiedAreaListTag">Optional tag that must be associated with the component used as the area list from which duplicates are removed.  Defaults to "AreaList2".</param>
        public RemoveDuplicatePoints(string? name = null, string? unmodifiedAreaListTag = "UnmodifiedAreaList", string? modifiedAreaListTag = "ModifiedAreaList")
            : base(name, (typeof(ItemList<Area>), unmodifiedAreaListTag), (typeof(ItemList<Area>), modifiedAreaListTag))
        {
            UnmodifiedAreaListTag = unmodifiedAreaListTag;
            ModifiedAreaListTag = modifiedAreaListTag;

            // Validate here because it was given in the constructor
            if (ModifiedAreaListTag == UnmodifiedAreaListTag)
                throw new InvalidConfigurationException(this, nameof(ModifiedAreaListTag), $"The value must be different than the value of {nameof(UnmodifiedAreaListTag)}.");
        }

        /// <inheritdoc/>
        protected override void OnPerform(GenerationContext context)
        {
            // Get required components
            var areaList1 = context.GetComponent<ItemList<Area>>(ModifiedAreaListTag)!; // Not null because is in required components list
            var areaList2 = context.GetComponent<ItemList<Area>>(UnmodifiedAreaListTag)!; // Not null because is in required components list

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
