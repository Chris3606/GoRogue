using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapViews;
using GoRogue.Random;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Steps
{
    // TODO: Add adjacency rule support (3 walls assumes manhattan)
    /// <summary>
    /// Searches for tunnels that don't lead anywhere (eg. are surrounded by 3 walls), and removes them from the map.
    ///
    /// Context Components Required:
    /// <list type="table">
    /// <listheader>
    /// <term>Component</term>
    /// <description>Default Tag</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="ItemList{Area}"/></term>
    /// <description>"Tunnels"</description>
    /// </item>
    /// <item>
    /// <term><see cref="ISettableMapView{T}"/> where T is bool</term>
    /// <description>"WallFloor"</description>
    /// </item>
    /// </list>
    ///
    /// Context Components Added:
    ///     - None
    /// </summary>
    /// <remarks>
    /// This algorithm iterates over all map areas in the <see cref="ItemList{Area}"/> context component with the given tag.  For each area, it scans for dead ends
    /// (locations that, according to the "WallFloor" component given, are surrounded by 3 walls).  For each dead end, if that dead end is not currently
    /// and hasn't previously been selected as "saved", based on percentage checks, it proceeds to fill it in.  It will remove the dead end location, from the approprate
    /// area, and set the location in the "WallFloor" map to true.
    ///
    /// It proceeds in this manner until either no more (non-saved) dead ends are found, or the given maximum iterations is reached, then proceeds to the next area in the ItemList
    /// until it has processed all of the areas.
    /// </remarks>
    public class TunnelDeadEndTrimming : GenerationStep
    {
        /// <summary>
        /// The chance out of 100 that a dead end is left alone.  Defaults to 40.
        /// </summary>
        public ushort SaveDeadEndChance = 40;

        /// <summary>
        /// Maximum number of passes to make looking for dead ends per area.  Defaults to infinity.
        /// </summary>
        public int MaxTrimIterations = -1;

        /// <summary>
        /// RNG to use for percentage checks.  Defaults to <see cref="GlobalRandom.DefaultRNG"/>.
        /// </summary>
        public IGenerator RNG = GlobalRandom.DefaultRNG;

        /// <summary>
        /// Optional tag that must be associated with the component used to set wall/floor status of tiles changed by this algorithm.
        /// </summary>
        public readonly string? WallFloorComponentTag;

        /// <summary>
        /// Optional tag that must be associated with the component used to record areas representing tunnels that this algorithm will trim dead ends from.
        /// </summary>
        public readonly string? TunnelsComponentTag;

        /// <summary>
        /// Creates a new dead end trimming generation step.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="TunnelDeadEndTrimming"/>.</param>
        /// <param name="wallFloorComponentTag">Optional tag that must be associated with the component used to set wall/floor status of tiles changed by this algorithm.  Defaults to "WallFloor".</param>
        /// <param name="tunnelsComponentTag">Optional tag that must be associated with the component used to record areas representing tunnels that this algorithm will trim dead ends from.  Defaults to "Tunnels".</param>
        public TunnelDeadEndTrimming(string? name = null, string? wallFloorComponentTag = "WallFloor", string? tunnelsComponentTag = "Tunnels")
            : base(name, (typeof(ISettableMapView<bool>), wallFloorComponentTag), (typeof(ItemList<Area>), tunnelsComponentTag))
        {
            WallFloorComponentTag = wallFloorComponentTag;
            TunnelsComponentTag = tunnelsComponentTag;
        }

        /// <inheritdoc/>
        protected override void OnPerform(GenerationContext context)
        {
            // Validate configuration
            if (SaveDeadEndChance > 100)
                throw new InvalidConfigurationException(this, nameof(SaveDeadEndChance), "The value must be a valid percent (between 0 and 100).");

            if (MaxTrimIterations < -1)
                throw new InvalidConfigurationException(this, nameof(MaxTrimIterations), "The value must be 0 or above, or -1 for no iteration limit.");

            // Get required existing components
            var wallFloor = context.GetComponent<ISettableMapView<bool>>(WallFloorComponentTag)!; // Known to not be null since Perform checked for us
            var tunnels = context.GetComponent<ItemList<Area>>(TunnelsComponentTag)!; // Known to not be null since Perform checked for us

            // For each area, find dead ends up to the maximum number of iterations and prune them, unless they're saved
            foreach (var area in tunnels.Items)
            {
                HashSet<Point> safeDeadEnds = new HashSet<Point>();
                HashSet<Point> deadEnds = new HashSet<Point>();

                int iteration = 1;
                while (MaxTrimIterations == -1 || iteration <= MaxTrimIterations)
                {
                    foreach (var point in area.Positions)
                    {
                        foreach (var direction in AdjacencyRule.Cardinals.DirectionsOfNeighborsClockwise())
                        {
                            Point neighbor = point + direction;

                            if (wallFloor[neighbor])
                            {   
                                var oppositeNeighborDir = direction + 4;
                                bool found = false;

                                // If we get here, source direction is a floor, opposite direction
                                // should be wall
                                if (!wallFloor[point + oppositeNeighborDir])
                                {
                                    // Check for a wall pattern in the map. Where X is a wall,
                                    // checks the appropriately rotated version of:
                                    // XXX
                                    // X X
                                    found = oppositeNeighborDir.Type switch
                                    {
                                        Direction.Types.Up =>    !wallFloor[point + Direction.UpLeft] &&
                                                                 !wallFloor[point + Direction.UpRight] &&
                                                                 !wallFloor[point + Direction.Left] &&
                                                                 !wallFloor[point + Direction.Right],

                                        Direction.Types.Down =>  !wallFloor[point + Direction.DownLeft] &&
                                                                 !wallFloor[point + Direction.DownRight] &&
                                                                 !wallFloor[point + Direction.Left] &&
                                                                 !wallFloor[point + Direction.Right],

                                        Direction.Types.Right => !wallFloor[point + Direction.UpRight] &&
                                                                 !wallFloor[point + Direction.DownRight] &&
                                                                 !wallFloor[point + Direction.Up] &&
                                                                 !wallFloor[point + Direction.Down],

                                        Direction.Types.Left =>  !wallFloor[point + Direction.UpLeft] &&
                                                                 !wallFloor[point + Direction.DownLeft] &&
                                                                 !wallFloor[point + Direction.Up] &&
                                                                 !wallFloor[point + Direction.Down],

                                        _ => throw new System.Exception("Cannot occur since original neighbor direction was a cardinal.")
                                    };
                                }

                                // If we found a dead end and it's not already safe, then add it to the list
                                if (found && !safeDeadEnds.Contains(point))
                                    deadEnds.Add(point);

                                break; // Even if it is already saved, we know it's a dead end so we can stop processing this point
                            }
                        }
                    }

                    // No dead ends to process
                    if (deadEnds.Count == 0)
                        break;

                    // Process cancel chance for each dead end
                    foreach (var point in deadEnds)
                        if (RNG.PercentageCheck(SaveDeadEndChance))
                            safeDeadEnds.Add(point);

                    // Remove newly cancelled dead ends
                    deadEnds.ExceptWith(safeDeadEnds);

                    // Fill in all the selected dead ends on the wall-floor map
                    foreach (var point in deadEnds)
                        wallFloor[point] = false;

                    // Remove dead ends from the list of points in the tunnel
                    area.Remove(deadEnds);

                    // Clear dead ends for next pass and record the completed iteration
                    deadEnds.Clear();
                    iteration++;
                }
            }
        }
    }
}
