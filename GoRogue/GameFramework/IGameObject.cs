using System;
using GoRogue.Components;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.GameFramework
{
    /// <summary>
    /// Event arguments for <see cref="IGameObject.WalkabilityChanged"/> event.
    /// </summary>
    [PublicAPI]
    public class ItemWalkabilityChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Object whose walkability was changed.
        /// </summary>
        public readonly IGameObject Item;

        /// <summary>
        /// Previous value of walkability.
        /// </summary>
        public readonly bool OldWalkability;

        /// <summary>
        /// New value of walkability.
        /// </summary>
        public readonly bool NewWalkability;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="item">Object whose walkability was changed.</param>
        /// <param name="oldWalkability">Previous value of walkability.</param>
        /// <param name="newWalkability">New value of walkability.</param>
        public ItemWalkabilityChangedEventArgs(IGameObject item, bool oldWalkability, bool newWalkability)
        {
            Item = item;
            OldWalkability = oldWalkability;
            NewWalkability = newWalkability;
        }
    }

    /// <summary>
    /// Event arguments for <see cref="IGameObject.TransparencyChanged"/> event.
    /// </summary>
    [PublicAPI]
    public class ItemTransparencyChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Object whose transparency was changed.
        /// </summary>
        public readonly IGameObject Item;

        /// <summary>
        /// Previous value of transparency.
        /// </summary>
        public readonly bool OldTransparency;

        /// <summary>
        /// New value of transparency.
        /// </summary>
        public readonly bool NewTransparency;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="item">Object whose walkability was changed.</param>
        /// <param name="oldTransparency">Previous value of transparency.</param>
        /// <param name="newTransparency">New value of transparency.</param>
        public ItemTransparencyChangedEventArgs(IGameObject item, bool oldTransparency, bool newTransparency)
        {
            Item = item;
            OldTransparency = oldTransparency;
            NewTransparency = newTransparency;
        }
    }

    /// <summary>
    /// An interface that defines the entire public interface of <see cref="GameObject" />.  Generally, you should NOT
    /// implement these functions yourself, however it can be used in conjunction with a private, backing field of type
    /// GameObject to store items in a map, for cases where that object cannot directly inherit from GameObject.
    /// </summary>
    /// <remarks>
    /// Generally, you will never implement the items in this interface manually, but rather do so through a private,
    /// backing field of type <see cref="GameObject" />. There is an example of this type of implementation
    /// <a href="https://chris3606.github.io/GoRogue/articles/game-framework.html#implementing-igameobject">here</a>.
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
        /// for the sake of calculating the FOV of a <see cref="Map" />.
        /// </summary>
        bool IsTransparent { get; set; }

        /// <summary>
        /// Fired when <see cref="IsTransparent"/> is changed.
        /// </summary>
        public event EventHandler<ItemTransparencyChangedEventArgs>? TransparencyChanged;

        /// <summary>
        /// Container holding components that have been attached to this object.
        /// </summary>
        public ITaggableComponentCollection GoRogueComponents { get; }

        /// <summary>
        /// Whether or not the object is to be considered "walkable", eg. whether or not the square it resides
        /// on can be traversed by other, non-walkable objects on the same <see cref="Map" />.  Effectively, whether or not this
        /// object collides.
        /// </summary>
        bool IsWalkable { get; set; }

        /// <summary>
        /// Fired when <see cref="IsWalkable"/> is changed.
        /// </summary>
        public event EventHandler<ItemWalkabilityChangedEventArgs>? WalkabilityChanged;

        /// <summary>
        /// The position of this object on the grid. Any time this value is changed, the <see cref="Moved" /> event is fired.
        /// </summary>
        /// <remarks>
        /// This property may be overriden to implement custom functionality, however it is highly recommended
        /// that you call the base set in the overridden setter, as it performs collision detection.
        /// </remarks>
        Point Position { get; set; }

        /// <summary>
        /// Event fired whenever this object's grid position is successfully changed.  Fired regardless of whether
        /// the object is part of a <see cref="Map" />.
        /// </summary>
        event EventHandler<ItemMovedEventArgs<IGameObject>>? Moved;

        /// <summary>
        /// Internal use only, do not call manually!  Must, at minimum, update the <see cref="CurrentMap" /> field of the
        /// IGameObject to reflect the change.
        /// </summary>
        /// <param name="newMap">New map to which the IGameObject has been added.</param>
        void OnMapChanged(Map? newMap);
    }
}
