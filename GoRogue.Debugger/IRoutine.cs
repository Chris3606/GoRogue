using System.Collections.Generic;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapViews;

namespace GoRogue.Debugger
{
    /// <summary>
    /// Effectively, a set of map generation rules, map transformation
    /// rules, and views of the map that allow you to debug multiple
    /// concepts in the same session
    /// </summary>
    public interface IRoutine
    {
        /// <summary>
        /// Regions in the map.
        /// </summary>
        IEnumerable<Region> Regions { get; }
        /// <summary>
        /// Name of the routine: TODO: Replace w/class name?
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Map that is generated/transformed.
        /// </summary>
        Map Map { get; }
        /// <summary>
        /// Elapses a single unit of time; should apply one or more transformations to the map.
        /// </summary>
        void ElapseTimeUnit();
        /// <summary>
        /// Initially generates the map.
        /// </summary>
        void GenerateMap();

        /// <summary>
        /// After map generation, populates <see cref="Views"/> with one or more map views.
        /// </summary>
        void CreateViews();

        /// <summary>
        /// Different views of the map, each with a name.
        /// </summary>
        IReadOnlyList<(string name, IMapView<char> view)> Views { get; }
    }
}
