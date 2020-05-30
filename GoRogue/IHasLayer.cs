using JetBrains.Annotations;

namespace GoRogue
{
    /// <summary>
    /// Interface to be implemented by objects that will be added to a <see cref="SpatialMaps.LayeredSpatialMap{T}"/>.
    /// </summary>
    [PublicAPI]
    public interface IHasLayer
    {
        /// <summary>
        /// The layer on which the object should reside. Higher numbers indicate layers closer to the
        /// "top".
        /// </summary>
        /// <remarks>
        /// This value is assumed to remain constant while the object is within a data structure
        /// that uses this interface -- if it is modified, that data structure will become out of sync.
        /// </remarks>
        int Layer { get; }
    }
}
