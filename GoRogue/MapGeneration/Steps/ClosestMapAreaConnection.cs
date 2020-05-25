using System.Collections.Generic;
using GoRogue.MapGeneration.ConnectionPointSelectors;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapGeneration.TunnelCreators;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Connects areas of the map by connecting each area to its closest neighboring area, with distance between areas based on the center-points of the areas.
    /// 
    /// Context Components Required:
    /// <list type="table">
    /// <listheader>
    /// <term>Component</term>
    /// <description>Default Tag</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="ItemList{Area}"/></term>
    /// <description>"Areas"</description>
    /// </item>
    /// <item>
    /// <term><see cref="ISettableMapView{T}"/> where T is bool</term>
    /// <description>"WallFloor"</description>
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
    /// <description>"Tunnels"</description>
    /// </item>
    /// </list>
    ///
    /// In the case of the tunnels component, an existing component is used if an apppropriate one is present; a new one is added if not.
    /// </summary>
    /// <remarks>
    /// This generation steps takes as input a <see cref="ItemList{Area}"/> context component (with the tag "Areas", by default) containing areas to connect, and "WallFloor" map view context component
    /// that indicates wall/floor status for each location on the map.  It then connects the map areas in the list, generating tunnels in the process.  Each location comprising
    /// the generated tunnels is set to "true" in the "WallFloor" component.  Additionally, an <see cref="Area"/> representing each tunnel created is added to the <see cref="ItemList{Area}"/> context
    /// component (with the tag "Tunnels", by default).
    ///
    /// If an appropriate component with the specified tag exists for the resulting tunnels, the Areas are added to that component.  Otherwise, a new component is created.
    ///
    /// Areas are connected by drawing a tunnel between each Area and its closest neighboring area, based on based on distance between the center points of the areas.  The actual points selected in each area
    /// to connect, as well as the method for drawing tunnels between those areas, is customizable via the <see cref="ConnectionPointSelector"/> and <see cref="TunnelCreator"/> parameters.
    /// </remarks>
    public class ClosestMapAreaConnection : GenerationStep
    {
        /// <summary>
        /// The distance calculation that defines distance/neighbors.
        /// </summary>
        public Distance DistanceCalc = Distance.Manhattan;

        /// <summary>
        /// The area connection strategy to use. Not all methods function on maps with concave areas
        /// -- see respective class documentation for details.
        /// </summary>
        public IConnectionPointSelector ConnectionPointSelector = new RandomConnectionPointSelector();

        /// <summary>
        /// The tunnel creation strategy to use. Defaults to <see cref="DirectLineTunnelCreator"/> with
        /// the cardinal adjacency rules.
        /// </summary>
        public ITunnelCreator TunnelCreator = new DirectLineTunnelCreator(Distance.Manhattan);

        /// <summary>
        /// Optional tag that must be associated with the component used to store map areas connected by this algorithm.
        /// </summary>
        public readonly string? AreasComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component used to set wall/floor status of tiles changed by this algorithm.
        /// </summary>
        public readonly string? WallFloorComponentTag;

        /// <summary>
        /// Optional tag that must be associated wiht the component created/used to store the tunnels created by this connection method.
        /// </summary>
        public readonly string? TunnelsComponentTag;

        /// <summary>
        /// Creates a new closest area connection step.
        /// </summary>
        /// <param name="name">>The name of the generation step.  Defaults to <see cref="ClosestMapAreaConnection"/>.</param>
        /// <param name="wallFloorComponentTag">Optional tag that must be associated with the map view component used to store/set floor/wall status.  Defaults to "WallFloor".</param>
        /// <param name="areasComponentTag">Optional tag that must be associated with the component used to store map areas connected by this algorithm.  Defaults to "Areas".</param>
        /// <param name="tunnelsComponentTag">Optional tag that must be associated wiht the component created/used to store the tunnels created by this connection method.  Defaults to "Tunnels".</param>
        public ClosestMapAreaConnection(string? name = null, string? wallFloorComponentTag = "WallFloor", string? areasComponentTag = "Areas", string? tunnelsComponentTag = "Tunnels")
            : base(name, (typeof(ISettableMapView<bool>), wallFloorComponentTag), (typeof(ItemList<Area>), areasComponentTag))
        {
            WallFloorComponentTag = wallFloorComponentTag;
            AreasComponentTag = areasComponentTag;
            TunnelsComponentTag = tunnelsComponentTag;
        }

        /// <inheritdoc/>
        protected override void OnPerform(GenerationContext context)
        {
            // Get required existing components
            var areasToConnect = context.GetComponent<ItemList<Area>>(AreasComponentTag)!; // Known to not be null since Perform checked for us
            var wallFloor = context.GetComponent<ISettableMapView<bool>>(WallFloorComponentTag)!; // Known to not be null since Perform checked for us

            // Get/create tunnel component
            var tunnels = context.GetComponentOrNew(() => new ItemList<Area>(), TunnelsComponentTag);

            var ds = new DisjointSet(areasToConnect.Items.Count);
            while (ds.Count > 1) // Haven't unioned all sets into one
            {
                for (int i = 0; i < areasToConnect.Items.Count; i++)
                {
                    int iClosest = findNearestMapArea(areasToConnect.Items, DistanceCalc, i, ds);

                    var (area1Position, area2Position) = ConnectionPointSelector.SelectConnectionPoints(areasToConnect.Items[i], areasToConnect.Items[iClosest]);

                    var tunnel = TunnelCreator.CreateTunnel(wallFloor, area1Position, area2Position);
                    tunnels.AddItem(tunnel, Name);

                    ds.MakeUnion(i, iClosest);
                }
            }
        }

        private static int findNearestMapArea(IReadOnlyList<IReadOnlyArea> mapAreas, Distance distanceCalc, int mapAreaIndex, DisjointSet ds)
        {
            int closestIndex = mapAreaIndex;
            double distance = double.MaxValue;

            for (int i = 0; i < mapAreas.Count; i++)
            {
                if (i == mapAreaIndex)
                    continue;

                if (ds.InSameSet(i, mapAreaIndex))
                    continue;

                double distanceBetween = distanceCalc.Calculate(mapAreas[mapAreaIndex].Bounds.Center, mapAreas[i].Bounds.Center);
                if (distanceBetween < distance)
                {
                    distance = distanceBetween;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
    }
}
