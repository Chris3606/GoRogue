using System.Collections.Generic;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;

namespace GoRogue.Debugger
{
    /// <summary>
    /// Effectively, a set of map generation rules and map transformation
    /// rules that allow you to debug multiple concepts in the same session
    /// </summary>
    public interface IRoutine
    {
        IEnumerable<Region> Regions { get; }
        string Name { get; }
        Map BaseMap { get; }
        Map TransformedMap { get; }
        Map ElapseTimeUnit();
        Map GenerateMap();
    }
}
