using System;
using GoRogue.Components;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.GameFramework
{
    /// <summary>
    /// Event arguments for en event fired when IGameObject properties are changed.
    /// </summary>
    /// <typeparam name="T">Type of the property changed.</typeparam>
    [PublicAPI]
    public class GameObjectPropertyChanged<T> : EventArgs
    {
        /// <summary>
        /// Game object whose property was changed.
        /// </summary>
        public readonly IGameObject Item;

        /// <summary>
        /// Previous value of property.
        /// </summary>
        public readonly T OldValue;

        /// <summary>
        /// New value of property.
        /// </summary>
        public readonly T NewValue;

        /// <summary>
        /// Creates a new change description object.
        /// </summary>
        /// <param name="item">Game object whose property was changed.</param>
        /// <param name="oldValue">Previous value of property.</param>
        /// <param name="newValue">New value of property.</param>
        public GameObjectPropertyChanged(IGameObject item, T oldValue, T newValue)
        {
            Item = item;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Base interface required for any object that has a grid position and can be added to a <see cref="Map" />.
    /// Implements basic attributes generally common to all objects on a map, as well as properties/methods that
    /// <see cref="Map"/> needs to function.  It also provides a container that you may attach arbitrary components to.
    ///
    /// For a concrete implementation, see <see cref="GameObject"/>.
    /// </summary>
    /// <remarks>
    /// Generally, you can use <see cref="GameObject"/> instead of implementing this interface directly.  However, if
    /// the need to avoid inheritance or change that implementation arises, please note that the interface contains
    /// events that you must fire appropriately when the corresponding property implementations are changed.
    /// GoRogue defines helper methods for this; it is recommended that you use GameObject as an example.
    /// </remarks>
    [PublicAPI]
    public interface IGameObject : IHasID, IHasLayer
    {
        /// <summary>
        /// The current <see cref="Map" /> which this object resides on.  Returns null if the object has not been added to a map.
        /// An IGameObject is allowed to reside on only one map.
        /// </summary>
        Map? CurrentMap { get; }

        /// <summary>
        /// Whether or not the object is considered "transparent", eg. whether or not light passes through it
        /// for the sake of calculating FOV.
        /// </summary>
        bool IsTransparent { get; set; }

        /// <summary>
        /// Fired when <see cref="IsTransparent"/> is changed.
        /// </summary>
        public event EventHandler<GameObjectPropertyChanged<bool>>? TransparencyChanged;

        /// <summary>
        /// Container holding components that have been attached to this object.
        /// </summary>
        public ITaggableComponentCollection GoRogueComponents { get; }

        /// <summary>
        /// Whether or not the object is to be considered "walkable", eg. whether or not the square it resides
        /// on can be traversed by other, non-walkable objects on the same <see cref="Map" />.  Effectively, whether or
        /// not this object collides.
        /// </summary>
        bool IsWalkable { get; set; }

        /// <summary>
        /// Fired when <see cref="IsWalkable"/> is changed.
        /// </summary>
        public event EventHandler<GameObjectPropertyChanged<bool>>? WalkabilityChanged;

        /// <summary>
        /// The position of this object on the grid. Any time this value is changed, the <see cref="Moved" /> event is
        /// fired.
        /// </summary>
        Point Position { get; set; }

        /// <summary>
        /// Event fired whenever this object's grid position is successfully changed.  Fired regardless of whether
        /// the object is part of a <see cref="Map" />.
        /// </summary>
        event EventHandler<GameObjectPropertyChanged<Point>>? Moved;

        /// <summary>
        /// Internal use only, do not call manually!  Must, at minimum, update the <see cref="CurrentMap" /> field of the
        /// IGameObject to reflect the change.
        /// </summary>
        /// <param name="newMap">New map to which the IGameObject has been added.</param>
        void OnMapChanged(Map? newMap);
    }
}
