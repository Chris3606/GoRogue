using System.Collections.Generic;
using GoRogue.MapViews;

namespace GoRogue.Debugger
{
    /// <summary>
    /// Effectively, a set of map generation rules, map transformation
    /// rules, and views of the map that allow you to debug one or more
    /// concepts.
    /// </summary>
    public interface IRoutine
    {
        /// <summary>
        /// Name of the routine.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Elapses a single unit of time; should apply one or more transformations to the map, that ultimately
        /// affect one or more views.
        /// </summary>
        void ElapseTimeUnit();

        /// <summary>
        /// Initially generates the map.
        /// </summary>
        void GenerateMap();

        /// <summary>
        /// Populates <see cref="Views"/> with one or more map views (after <see cref="GenerateMap"/> is called).
        /// Each view generated must have the same width and height.
        /// </summary>
        void CreateViews();

        /// <summary>
        /// Different views of the map, each with a name.  Each view must have the same width and height.
        /// </summary>
        IReadOnlyList<(string name, IMapView<char> view)> Views { get; }

    }
}
