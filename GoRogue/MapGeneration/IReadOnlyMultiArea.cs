using System.Collections.Generic;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A IReadOnlyArea with SubAreas
    /// </summary>
    public interface IReadOnlyMultiArea : IReadOnlyArea
    {
        /// <summary>
        /// The Sub-Areas which have been added to this Area
        /// </summary>
        IReadOnlyList<IReadOnlyArea> SubAreas { get; }
    }
}
