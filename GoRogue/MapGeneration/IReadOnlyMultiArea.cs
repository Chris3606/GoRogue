using System.Collections.Generic;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    public interface IReadOnlyMultiArea : IReadOnlyArea
    {
        IReadOnlyList<IReadOnlyArea> SubAreas { get; }
    }
}
