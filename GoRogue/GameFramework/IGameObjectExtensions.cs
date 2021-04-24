using System;
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

        /// <summary>
        /// Sets the given field to the new value, and fires the corresponding event.  The value will be properly
        /// reverted to the old value if the event handler throws InvalidOperationException.
        /// </summary>
        /// <param name="self" />
        /// <param name="propertyField">Field to set.</param>
        /// <param name="newValue">New value to set to given field.</param>
        /// <param name="changedEvent">Event to fire when change occurs.</param>
        /// <typeparam name="T">Type of the property.</typeparam>
        public static void SafelySetProperty<T>(this IGameObject self, ref T propertyField, T newValue,
                                                EventHandler<GameObjectPropertyChanged<T>>? changedEvent)
            where T: notnull
        {
            // Nothing to do; the value hasn't changed
            if (propertyField.Equals(newValue))
                return;

            // Set new value and fire event
            var oldValue = propertyField;
            propertyField = newValue;
            try
            {
                changedEvent?.Invoke(self, new GameObjectPropertyChanged<T>(self, oldValue, newValue));
            }
            catch (InvalidOperationException)
            {
                // If exception, preserve old value for future
                propertyField = oldValue;
                throw;
            }
        }

        /// <summary>
        /// Sets the given map field to the given value, and fires the AddedToMap event and RemovedFromMap
        /// event as necessary.
        /// </summary>
        /// <remarks>
        /// This is a convenience function to set the backing field for <see cref="IGameObject.CurrentMap"/>
        /// correctly, ensuring exceptions are handled and that events are fired properly.  Generally, this should
        /// only be called from within an implementation of <see cref="IGameObject.OnMapChanged"/>.
        ///
        /// See the implementation of <see cref="GameObject.OnMapChanged"/> for an example of intended use.
        /// </remarks>
        /// <param name="self" />
        /// <param name="currentMapField">Map field to set.</param>
        /// <param name="newValue">New value to set to the map field.</param>
        /// <param name="addedEvent">The <see cref="IGameObject.AddedToMap"/> event for the object whose map is being changed.</param>
        /// <param name="removedEvent">The <see cref="IGameObject.RemovedFromMap"/> event for the object whose map is being changed.</param>
        public static void SafelySetCurrentMap(this IGameObject self, ref Map? currentMapField, Map? newValue,
            EventHandler<GameObjectCurrentMapChanged>? addedEvent, EventHandler<GameObjectCurrentMapChanged>? removedEvent)
        {
            // Nothing to do; the map hasn't changed
            if (ReferenceEquals(currentMapField, newValue))
                return;

            // Set new value and fire events as needed
            var oldValue = currentMapField;
            currentMapField = newValue;
            try
            {
                if (oldValue != null)
                    removedEvent?.Invoke(self, new GameObjectCurrentMapChanged(oldValue));

                if (newValue != null)
                    addedEvent?.Invoke(self, new GameObjectCurrentMapChanged(newValue));
            }
            catch (InvalidOperationException)
            {
                // If exception, preserve old value for future
                currentMapField = oldValue;
                throw;
            }
        }
    }
}
