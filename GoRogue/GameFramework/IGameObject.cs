using System;
using GoRogue.Components;
using GoRogue.Components.ParentAware;
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
    /// Event arguments for events fired when an <see cref="IGameObject"/> is added
    /// to/removed from a <see cref="Map"/>.
    /// </summary>
    [PublicAPI]
    public class GameObjectCurrentMapChanged : EventArgs
    {
        /// <summary>
        /// The map that the object was added to/removed from.
        /// </summary>
        public readonly Map Map;

        /// <summary>
        /// Creates a new argument for events.
        /// </summary>
        /// <param name="map">Map the object was added to/removed from.</param>
        public GameObjectCurrentMapChanged(Map map)
        {
            Map = map;
        }
    }

    /// <summary>
    /// Base interface required for any object that has a grid position and can be added to a <see cref="Map" />.
    /// Implements basic attributes generally common to all objects on a map, as well as properties/methods that
    /// <see cref="Map"/> needs to function.  It also provides a container that you may attach arbitrary components to
    /// (via the <see cref="IObjectWithComponents"/> interface).
    ///
    /// For a concrete implementation, see <see cref="GameObject"/>.
    /// </summary>
    /// <remarks>
    /// Generally, you can use <see cref="GameObject"/> instead of implementing this interface directly.  However, if
    /// the need to avoid inheritance or change that implementation arises, please note that the interface contains
    /// events that you must fire appropriately when the corresponding property implementations are changed.
    /// GoRogue defines helper methods for this; it is recommended that you use GameObject as an example.
    ///
    /// Regardless of what implementation is used, however, if <see cref="ComponentCollection"/> (or some other custom
    /// collection implementing the proper functionality) is used, this object provides support for its components to
    /// (optionally) implement <see cref="IParentAwareComponent"/>, or inherit from <see cref="ParentAwareComponentBase"/>.
    /// In this case, the <see cref="IParentAwareComponent.Parent"/> will be updated automatically as components are added/
    /// removed.  Typically, you will want to inherit your components from <see cref="ParentAwareComponentBase{TParent}"/>,
    /// where TParent would be IGameObject or some class implementing that interface.
    /// </remarks>
    [PublicAPI]
    public interface IGameObject : IHasID, IHasLayer, IObjectWithComponents
    {
        /// <summary>
        /// The current <see cref="Map" /> which this object resides on.  Returns null if the object has not been added to a map.
        /// An IGameObject is allowed to reside on only one map.
        /// </summary>
        Map? CurrentMap { get; }

        /// <summary>
        /// Fired when the object is added to a map.
        /// </summary>
        public event EventHandler<GameObjectCurrentMapChanged>? AddedToMap;

        /// <summary>
        /// Fired when the object is removed from a map.
        /// </summary>
        public event EventHandler<GameObjectCurrentMapChanged>? RemovedFromMap;

        /// <summary>
        /// Whether or not the object is considered "transparent", eg. whether or not light passes through it
        /// for the sake of calculating FOV.
        /// </summary>
        bool IsTransparent { get; set; }

        /// <summary>
        /// Fired when <see cref="IsTransparent"/> is about to be changed.
        /// </summary>
        public event EventHandler<GameObjectPropertyChanged<bool>>? TransparencyChanging;

        /// <summary>
        /// Fired when <see cref="IsTransparent"/> is changed.
        /// </summary>
        public event EventHandler<GameObjectPropertyChanged<bool>>? TransparencyChanged;

        /// <summary>
        /// Whether or not the object is to be considered "walkable", eg. whether or not the square it resides
        /// on can be traversed by other, non-walkable objects on the same <see cref="Map" />.  Effectively, whether or
        /// not this object collides.
        /// </summary>
        bool IsWalkable { get; set; }

        /// <summary>
        /// Fired when <see cref="IsWalkable"/> is about to changed.
        /// </summary>
        public event EventHandler<GameObjectPropertyChanged<bool>>? WalkabilityChanging;

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
        /// Internal use only, do not call manually!  Must, at minimum, call <see cref="GameObjectExtensions.SafelySetCurrentMap"/>
        /// which will update the <see cref="CurrentMap" /> field of the IGameObject to reflect the change and fire map
        /// added/removed events as appropriate (or provide equivalent functionality).
        /// </summary>
        /// <param name="newMap">New map to which the IGameObject has been added.</param>
        void OnMapChanged(Map? newMap);
    }
}
