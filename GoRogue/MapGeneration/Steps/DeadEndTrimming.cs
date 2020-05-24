using System;
using System.Collections.Generic;
using System.Text;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapViews;
using GoRogue.Random;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Steps
{
    public class TunnelDeadEndTrimming : GenerationStep
    {
        public ushort SaveDeadEndChance = 70;
        public int MaxTrimIterations = -1;
        IGenerator RNG = GlobalRandom.DefaultRNG;

        public readonly string? WallFloorComponentTag;
        public readonly string? TunnelsComponentTag;

        public TunnelDeadEndTrimming(string? name = null, string? wallFloorComponentTag = "WallFloor", string? tunnelsComponentTag = "Tunnels")
            : base(name, (typeof(ISettableMapView<bool>), wallFloorComponentTag), (typeof(ItemList<Area>), tunnelsComponentTag))
        {
            WallFloorComponentTag = wallFloorComponentTag;
            TunnelsComponentTag = tunnelsComponentTag;
        }

        protected override void OnPerform(GenerationContext context)
        {
            // Validate configuration
            if (SaveDeadEndChance > 100)
                throw new ArgumentException("Chance to save dead ends must be in range [0, 100].");
        }
    }
}
