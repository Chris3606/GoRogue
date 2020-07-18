using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SadRogue.Primitives.SerializedTypes;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.SerializedTypes.MapGeneration
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="RectangleEdgePositionsList"/>
    /// </summary>
    [PublicAPI]
    [Serializable]
    [SuppressMessage("ReSharper", "CA1815")] // Type should only be used for serialization
    public struct RectangleEdgePositionsListSerialized
    {
        /// <summary>
        /// The rectangle whose edge positions are being stored.
        /// </summary>
        public RectangleSerialized Rectangle;

        /// <summary>
        /// Positions being stored.
        /// </summary>
        public List<PointSerialized> Positions;

        /// <summary>
        /// Converts <see cref="RectangleEdgePositionsList"/> to <see cref="RectangleEdgePositionsListSerialized"/>.
        /// </summary>
        /// <param name="list"/>
        /// <returns/>
        public static implicit operator RectangleEdgePositionsListSerialized(RectangleEdgePositionsList list)
            => FromRectangleEdgePositionsList(list);

        /// <summary>
        /// Converts <see cref="RectangleEdgePositionsListSerialized"/> to <see cref="RectangleEdgePositionsList"/>.
        /// </summary>
        /// <param name="list"/>
        /// <returns/>
        public static implicit operator RectangleEdgePositionsList(RectangleEdgePositionsListSerialized list)
            => list.ToRectangleEdgePositionsList();

        /// <summary>
        /// Converts <see cref="RectangleEdgePositionsList"/> to <see cref="RectangleEdgePositionsListSerialized"/>.
        /// </summary>
        /// <param name="list"/>
        /// <returns/>
        public static RectangleEdgePositionsListSerialized FromRectangleEdgePositionsList(
            RectangleEdgePositionsList list)
            => new RectangleEdgePositionsListSerialized()
            {
                Rectangle = list.Rectangle, Positions = list.Select(point => (PointSerialized)point).ToList()
            };

        /// <summary>
        /// Converts <see cref="RectangleEdgePositionsListSerialized"/> to <see cref="RectangleEdgePositionsList"/>.
        /// </summary>
        /// <returns/>
        public RectangleEdgePositionsList ToRectangleEdgePositionsList()
        {
            var result = new RectangleEdgePositionsList(Rectangle);
            result.AddRange(Positions.Select(pointSerialized => (Point)pointSerialized));

            return result;
        }
    }
}
