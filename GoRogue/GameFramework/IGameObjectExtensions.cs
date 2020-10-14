using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.GameFramework
{
    /// <summary>
    /// A collection of helpful extension methods for <see cref="IGameObject"/> instances.
    /// </summary>
    [PublicAPI]
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Returns true if the object can be moved to the location specified; false otherwise.
        /// </summary>
        /// <remarks>
        /// This function should return false in exactly any case where setting the <see cref="IGameObject.Position" />
        /// property to the value specified would fail.
        /// </remarks>
        /// <param name="self" />
        /// <param name="position">The position to check.</param>
        /// <returns>True if the object can be moved to the specified position; false otherwise.</returns>
        public static bool CanMove(this IGameObject self, Point position)
            => self.CurrentMap == null || self.CurrentMap.GameObjectCanMove(self, position);

        /// <summary>
        /// Returns true if the object can move in the given direction; false otherwise.
        /// </summary>
        /// <remarks>
        /// See remarks in documentation for <see cref="GameObjectExtensions.CanMove(IGameObject, Point)" /> for details
        /// on when this function should return false.
        /// </remarks>
        /// <param name="self" />
        /// <param name="direction">The direction of movement to check.</param>
        /// <returns>True if the object can be moved in the specified direction; false otherwise</returns>
        public static bool CanMoveIn(this IGameObject self, Direction direction)
            => self.CanMove(self.Position + direction);

        /// <summary>
        /// Returns true if the object can set its <see cref="IGameObject.IsWalkable"/> property to the specified
        /// value; false otherwise.
        /// </summary>
        /// <remarks>
        /// This returns true unless the new value would violate collision detection rules of a map.
        /// </remarks>
        /// <param name="self" />
        /// <param name="value">The new value for walkability to check.</param>
        /// <returns>
        /// True if the object can set its walkability to the given value without violating a map's collision detection,
        /// false otherwise.</returns>
        public static bool CanSetWalkability(this IGameObject self, bool value)
            => self.CurrentMap == null || self.CurrentMap.GameObjectCanSetWalkability(self, value);

        /// <summary>
        /// Returns true if the object can toggle its <see cref="IGameObject.IsWalkable"/> property; false otherwise.
        /// </summary>
        /// <param name="self" />
        /// <returns>
        /// True if the object can toggle its walkability without violating a map's collision detection,
        /// false otherwise.
        /// </returns>
        public static bool CanToggleWalkability(this IGameObject self) => self.CanSetWalkability(!self.IsWalkable);
    }
}
