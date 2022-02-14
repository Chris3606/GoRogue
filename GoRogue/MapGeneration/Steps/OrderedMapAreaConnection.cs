using System.Collections.Generic;
using GoRogue.MapGeneration.ConnectionPointSelectors;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapGeneration.TunnelCreators;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Connects areas of the map by connecting each area specified to a random other area, or connecting the areas
    /// in a specific order specified.
    /// </summary>
    [PublicAPI]
    public class OrderedMapAreaConnection : GenerationStep
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
        /// Whether or not to randomize the order of the areas before connecting them.  If false, the areas will
        /// be connected to the next area in the list specified by <see cref="AreasComponentTag"/>.
        /// </summary>
        public bool RandomizeOrder;

        /// <summary>
        /// The tunnel creation strategy to use. Defaults to <see cref="HorizontalVerticalTunnelCreator" /> using
        /// <see cref="GlobalRandom.DefaultRNG"/>.
        /// </summary>
        public ITunnelCreator TunnelCreator = new HorizontalVerticalTunnelCreator();

        /// <summary>
        /// RNG to use for randomization or room order (if randomization is enabled).
        /// </summary>
        public IEnhancedRandom RNG = GlobalRandom.DefaultRNG;

        /// <summary>
        /// Creates a new ordered area connection step.
        /// </summary>
        /// <param name="name">>The name of the generation step.  Defaults to <see cref="OrderedMapAreaConnection" />.</param>
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
        public OrderedMapAreaConnection(string? name = null, string? wallFloorComponentTag = "WallFloor",
                                        string? areasComponentTag = "Areas", string? tunnelsComponentTag = "Tunnels")
            : base(name, (typeof(ISettableGridView<bool>), wallFloorComponentTag),
                (typeof(ItemList<Area>), areasComponentTag))
        {
            WallFloorComponentTag = wallFloorComponentTag;
            AreasComponentTag = areasComponentTag;
            TunnelsComponentTag = tunnelsComponentTag;
        }

        /// <inheritdoc/>
        protected override IEnumerator<object?> OnPerform(GenerationContext context)
        {
            // Get required components; guaranteed to exist because enforced by required components list
            var areasToConnectOriginal = context.GetFirst<ItemList<Area>>(AreasComponentTag);
            var wallFloor = context.GetFirst<ISettableGridView<bool>>(WallFloorComponentTag);

            // Get/create tunnel component
            var tunnels = context.GetFirstOrNew(() => new ItemList<Area>(), TunnelsComponentTag);

            // Randomize order of connected areas if we need to
            IReadOnlyList<Area> areasToConnect;
            if (RandomizeOrder)
            {
                var list = new List<Area>(areasToConnectOriginal.Items);
                RNG.Shuffle(list);
                areasToConnect = list;
            }
            else
                areasToConnect = areasToConnectOriginal.Items;

            // Connect each area to the next one in the list
            for (int i = 1; i < areasToConnect.Count; i++)
            {
                var (point1, point2) =
                    ConnectionPointSelector.SelectConnectionPoints(areasToConnect[i], areasToConnect[i - 1]);

                var tunnel = TunnelCreator.CreateTunnel(wallFloor, point1, point2);
                tunnels.Add(tunnel, Name);

                yield return null;
            }
        }
    }
}
