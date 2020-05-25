﻿using GoRogue.SpatialMaps;
using System;
using SadRogue.Primitives;

namespace GoRogue.GameFramework
{
    /// <summary>
    /// An interface that defines the entire public interface of <see cref="GameObject"/>.  Generally, you should NOT implement these functions
    /// yourself, however it can be used in conjunction with a private, backing field of type GameObject to store items in a map, where that
    /// object cannot directly inherit from GameObject.
    /// </summary>
    /// <remarks>
    /// Generally, you will never implement the items in this interface manually, but rather do so through a private, backing field of type
    /// <see cref="GameObject"/>. There is an example of this type of implementation
    /// <a href="https://chris3606.github.io/GoRogue/articles/game-framework.html#implementing-igameobject">here</a>.
    /// </remarks>
    public interface IGameObject : IHasID, IHasLayer, IHasComponents
    {
        /// <summary>
        /// The current <see cref="Map"/> which this object resides on.  Returns null if the object has not been added to a map.
        /// An IGameObject is allowed to reside on only one map.
        /// </summary>
        Map? CurrentMap { get; }

        /// <summary>
        /// Whether or not the object is "static".  Static objects CANNOT be moved, and only static objects may
        /// be placed on layer 0 of a <see cref="Map"/>.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Whether or not the object is considered "transparent", eg. whether or not light passes through it
        /// for the sake of calculating the FOV of a <see cref="Map"/>.
        /// </summary>
        bool IsTransparent { get; set; }

        /// <summary>
        /// Whether or not the object is to be considered "walkable", eg. whether or not the square it resides
        /// on can be traversed by other, non-walkable objects on the same <see cref="Map"/>.  Effectively, whether or not this
        /// object collides.
        /// </summary>
        bool IsWalkable { get; set; }

        /// <summary>
        /// The position of this object on the grid. Any time this value is changed, the <see cref="Moved"/> event is fired.
        /// </summary>
        /// <remarks>
        /// This property may be overriden to implement custom functionality, however it is highly recommended
        /// that you call the base set in the overridden setter, as it performs collision detection.
        /// </remarks>
        Point Position { get; set; }

        /// <summary>
        /// Event fired whenever this object's grid position is successfully changed.  Fired regardless of whether
        /// the object is part of a <see cref="Map"/>.
        /// </summary>
        event EventHandler<ItemMovedEventArgs<IGameObject>> Moved;

        /// <summary>
        /// Internal use only, do not call manually!  Must, at minimum, update the <see cref="CurrentMap"/> field of the
        /// IGameObject to reflect the change.
        /// </summary>
        /// <param name="newMap">New map to which the IGameObject has been added.</param>
        void OnMapChanged(Map? newMap);

        /// <summary>
        /// Returns true if the GameObject can be moved to the location specified; false otherwise.
        /// </summary>
        /// <remarks>
        /// This function should return false in exactly any case where setting the <see cref="Position"/> property to the value specified
        /// would fail.
        /// </remarks>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the object can be moved to the specified position; false otherwise.</returns>
        public bool CanMove(Point position);

        /// <summary>
        /// Returns true if the GameObject can move in the given direction; false otherwise.
        /// </summary>
        /// <remarks>
        /// See remarks in documentation for <see cref="CanMove(Point)"/> for details on when this function should return false.
        /// </remarks>
        /// <param name="direction">The direction of movement to check.</param>
        /// <returns>True if the object can be moved in the specified direction; false otherwise</returns>
        public bool CanMoveIn(Direction direction);

    }
}
