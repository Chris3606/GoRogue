using System.Collections.Generic;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A interface adding onto IReadOnlyArea, intended to represent an area consisting of multiple other areas.
    /// Exposes a <see cref="SubAreas"/> field which lists the constituent areas.
    /// </summary>
    public interface IReadOnlyMultiArea : IReadOnlyArea
    {
        /// <summary>
        /// List of all sub-areas in the MultiArea.
        /// </summary>
        IReadOnlyList<IReadOnlyArea> SubAreas { get; }
    }
}
