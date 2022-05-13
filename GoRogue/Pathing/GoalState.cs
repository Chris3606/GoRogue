using JetBrains.Annotations;

namespace GoRogue.Pathing
{
    /// <summary>
    /// Used to determine the status of a tile for goal-mapping purposes.
    /// </summary>
    [PublicAPI]
    public enum GoalState
    {
        /// <summary>
        /// A tile that can't be entered and has to be routed around
        /// </summary>
        Obstacle,

        /// <summary>
        /// A tile that can be entered
        /// </summary>
        Clear,

        /// <summary>
        /// A destination on the goal map.
        /// </summary>
        Goal
    }
}
