using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration.ConnectionPointSelectors;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapGeneration.TunnelCreators;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Connects areas of the map by connecting each area to its closest neighboring area, with distance between areas based on
    /// the connection point selector specified.
    /// Context Components Required:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Component</term>
    ///         <description>Default Tag</description>
    ///     </listheader>
    ///     <item>
    ///         <term>
    ///             <see cref="ItemList{Area}" />
    ///         </term>
    ///         <description>"Areas"</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ISettableGridView{T}" /> where T is bool</term>
    ///         <description>"WallFloor"</description>
    ///     </item>
    /// </list>
    /// Context Components Added/Used:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Component</term>
    ///         <description>Default Tag</description>
    ///     </listheader>
    ///     <item>
    ///         <term>
    ///             <see cref="ItemList{Area}" />
    ///         </term>
    ///         <description>"Tunnels"</description>
    ///     </item>
    /// </list>
    /// In the case of the tunnels component, an existing component is used if an appropriate one is present; a new one is
    /// added if not.
    /// </summary>
    /// <remarks>
    /// This generation steps takes as input a <see cref="ItemList{Area}" /> context component (with the tag "Areas", by
    /// default) containing areas to connect, and "WallFloor" map view context component
    /// that indicates wall/floor status for each location on the map.  It then connects the map areas in the list, generating
    /// tunnels in the process.  Each location comprising
    /// the generated tunnels is set to "true" in the "WallFloor" component.  Additionally, an <see cref="Area" /> representing
    /// each tunnel created is added to the <see cref="ItemList{Area}" /> context
    /// component (with the tag "Tunnels", by default).
    /// If an appropriate component with the specified tag exists for the resulting tunnels, the Areas are added to that
    /// component.  Otherwise, a new component is created.
    /// Areas are connected by drawing a tunnel between each Area and its closest neighboring area, based on distance
    /// between the points selected by the given <see cref="ConnectionPointSelector"/>.  The actual points selected in
    /// each area to connect, as well as the method for drawing tunnels between those areas, is customizable via the
    /// <see cref="ConnectionPointSelector" /> and <see cref="TunnelCreator" /> parameters.
    /// </remarks>
    [PublicAPI]
    public class ClosestMapAreaConnection : GenerationStep
    {
        /// <summary>
        /// Optional tag that must be associated with the component used to store map areas connected by this algorithm.
        /// </summary>
        public readonly string? AreasComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component created/used to store the tunnels created by this connection
        /// method.
        /// </summary>
        public readonly string? TunnelsComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component used to set wall/floor status of tiles changed by this
        /// algorithm.
        /// </summary>
        public readonly string? WallFloorComponentTag;

        /// <summary>
        /// The area connection strategy to use. Not all methods function on maps with concave areas
        /// -- see respective class documentation for details.
        /// </summary>
        public IConnectionPointSelector ConnectionPointSelector = new RandomConnectionPointSelector();

        /// <summary>
        /// The distance calculation that defines distance/neighbors.
        /// </summary>
        public Distance DistanceCalc = Distance.Manhattan;

        /// <summary>
        /// The tunnel creation strategy to use. Defaults to <see cref="DirectLineTunnelCreator" /> with
        /// the cardinal adjacency rules.
        /// </summary>
        public ITunnelCreator TunnelCreator = new DirectLineTunnelCreator(Distance.Manhattan);

        private List<MultiArea>? _multiAreas;

        /// <summary>
        /// Creates a new closest area connection step.
        /// </summary>
        /// <param name="name">>The name of the generation step.  Defaults to <see cref="ClosestMapAreaConnection" />.</param>
        /// <param name="wallFloorComponentTag">
        /// Optional tag that must be associated with the map view component used to store/set
        /// floor/wall status.  Defaults to "WallFloor".
        /// </param>
        /// <param name="areasComponentTag">
        /// Optional tag that must be associated with the component used to store map areas
        /// connected by this algorithm.  Defaults to "Areas".
        /// </param>
        /// <param name="tunnelsComponentTag">
        /// Optional tag that must be associated with the component created/used to store the
        /// tunnels created by this connection method.  Defaults to "Tunnels".
        /// </param>
        public ClosestMapAreaConnection(string? name = null, string? wallFloorComponentTag = "WallFloor",
                                        string? areasComponentTag = "Areas", string? tunnelsComponentTag = "Tunnels")
            : base(name, (typeof(ISettableGridView<bool>), wallFloorComponentTag),
                (typeof(ItemList<Area>), areasComponentTag))
        {
            WallFloorComponentTag = wallFloorComponentTag;
            AreasComponentTag = areasComponentTag;
            TunnelsComponentTag = tunnelsComponentTag;
        }

        /// <inheritdoc />
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Get required components; guaranteed to exist because enforced by required components list
            var areasToConnect = context.GetFirst<ItemList<Area>>(AreasComponentTag);
            var wallFloor = context.GetFirst<ISettableGridView<bool>>(WallFloorComponentTag);

            // Get/create tunnel component
            var tunnels = context.GetFirstOrNew(() => new ItemList<Area>(), TunnelsComponentTag);

            // Create set of multi-areas for the areas we're joining, and a disjoint set based off of them
            _multiAreas = new List<MultiArea>(areasToConnect.Select(a => new MultiArea(a.Item)));
            var ds = new DisjointSet(_multiAreas.Count);
            ds.SetsJoined += DSOnSetsJoined;

            while (ds.Count > 1) // Haven't unioned all sets into one
                for (var i = 0; i < _multiAreas.Count; i++)
                {
                    // We finished early
                    if (ds.Count == 1) break;

                    // Make sure we operate on the parent set (since it contains the true points
                    int iParent = ds.Find(i);

                    // Find nearest area (area calculated based on point selector, and return selected connection points)
                    var (iClosest, area1Position, area2Position) = FindNearestMapArea(_multiAreas, DistanceCalc, ConnectionPointSelector, iParent, ds);

                    // Create a tunnel between the two points
                    var tunnel = TunnelCreator.CreateTunnel(wallFloor, area1Position, area2Position);
                    tunnels.Add(tunnel, Name);

                    // Mark the sets as unioned in the disjoint set
                    ds.MakeUnion(iParent, iClosest);

                    yield return null; // One stage per connection
                }
        }

        private void DSOnSetsJoined(object? sender, JoinedEventArgs e)
        {
            // Can ignore the nullability mismatch because the event handler is only used from OnPerform, after
            // multiAreas is initialized.
            _multiAreas![e.LargerSetID].AddRange(_multiAreas![e.SmallerSetID].SubAreas);
        }

        private static (int areaIndex, Point area1Position, Point area2Position) FindNearestMapArea(
            IReadOnlyList<IReadOnlyArea> mapAreas, Distance distanceCalc, IConnectionPointSelector pointSelector,
            int mapAreaIndex, DisjointSet ds)
        {
            // Record minimum distance and pair of points found based on selection
            int closestIndex = mapAreaIndex;
            double closestDistance = double.MaxValue;
            AreaConnectionPointPair closestPointPair = (Point.None, Point.None);

            for (var i = 0; i < mapAreas.Count; i++)
            {
                // Don't check against ourselves or anything in our set
                if (i == mapAreaIndex)
                    continue;

                if (ds.InSameSet(i, mapAreaIndex))
                    continue;

                // Ensure we operate on the parent of the neighbor (which cannot be our parent due to the checks
                // above), since the parents are the only areas that have the "true" list of points
                int iParentNeighbor = ds.Find(i);

                // Select connection points to check between the two areas
                var currentPointPair = pointSelector.SelectConnectionPoints(mapAreas[mapAreaIndex],
                    mapAreas[iParentNeighbor]);

                // Calculate distance between the selected connection points
                double distance = distanceCalc.Calculate(currentPointPair.Area1Position, currentPointPair.Area2Position);
                if (distance < closestDistance)
                {
                    closestIndex = iParentNeighbor;
                    closestDistance = distance;
                    closestPointPair = currentPointPair;
                }
            }

            // Return index, along with connection points found
            return (closestIndex, closestPointPair.Area1Position, closestPointPair.Area2Position);
        }
    }
}
