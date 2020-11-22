using System.Collections.Generic;
using SadRogue.Primitives.GridViews;

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
        /// Different views of the map, each with a name.  Each view must have the same width and height.
        /// </summary>
        IReadOnlyList<(string name, IGridView<char> view)> Views { get; }

        /// <summary>
        /// Elapses a single unit of time; should apply one or more transformations to the map, that ultimately
        /// affect one or more views.
        /// </summary>
        void NextTimeUnit();

        /// <summary>
        /// goes back one time unit. Only supported by some routines.
        /// </summary>
        void LastTimeUnit();

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
        /// Allows the routine to perform some actions when the user presses a key
        /// </summary>
        /// <param name="key">the key being pressed</param>
        void InterpretKeyPress(int key);
    }
}
