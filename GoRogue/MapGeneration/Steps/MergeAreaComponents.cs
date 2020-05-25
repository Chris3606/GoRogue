using System.Collections.Generic;
using GoRogue.MapGeneration.ContextComponents;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Takes two <see cref="ItemList{Area}"/> components, and merges them into one, with an option
    /// to remove points from the areas during merge that overlap.
    ///
    /// // TODO: Docs here
    /// </summary>
    public class MergeAreaComponents : GenerationStep
    {
        public readonly string AreaList1Tag;
        public readonly string AreaList2Tag;
        public readonly string? AreaListResultTag;

        public bool RemoveDuplicatePoints = true;

        public MergeAreaComponents(string? name = null, string areaList1Tag = "AreaList1", string areaList2Tag = "AreaList2", string? areaListResultTag = "AreaListResult")
            : base(name, (typeof(ItemList<Area>), areaList1Tag), (typeof(ItemList<Area>), areaList2Tag))
        {
            AreaList1Tag = areaList1Tag;
            AreaList2Tag = areaList2Tag;
            AreaListResultTag = areaListResultTag;
        }

        protected override void OnPerform(GenerationContext context)
        {
            // Get required components
            var areaList1 = context.GetComponent<ItemList<Area>>(AreaList1Tag)!; // Not null because is in required components list
            var areaList2 = context.GetComponent<ItemList<Area>>(AreaList2Tag)!; // Not null because is in required components list

            // Remove them from the context
            context.RemoveComponents(areaList1, areaList2);

            // Add the old component as the new one (shortcut to avoid work)
            context.AddComponent(areaList1, AreaListResultTag);

            // Remove any duplicate points from the areas we're adding if needed
            if (RemoveDuplicatePoints)
            {
                // Cache all positions in any area of area1List
                var areaList1Positions = new HashSet<Point>();
                foreach (var area in areaList1.Items)
                    foreach (var point in area.Positions)
                        areaList1Positions.Add(point);

                // Remove any position in second list's areas that is already in the first
                foreach (var area in areaList2.Items)
                    area.Remove(pos => areaList1Positions.Contains(pos));
            }

            // Iterate over each individual position and add to original list, so we keep the original generation step that created it with it
            foreach (var area in areaList2.Items)
                areaList1.AddItem(area, areaList2.ItemToStepMapping[area]);
        }
    }
}
