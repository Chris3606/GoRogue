using GoRogue.FOV;
using System;
using GoRogue.SenseMapping.Sources;
using JetBrains.Annotations;

namespace GoRogue.SenseMapping
{
    [PublicAPI]
    public interface ISenseMap : IReadOnlySenseMap
    {
        /// <summary>
        /// Fired whenever the SenseMap is recalculated.
        /// </summary>
        event EventHandler? Recalculated;

        /// <summary>
        /// Fired when the existing SenseMap is reset prior to calculating a new one.
        /// </summary>
        event EventHandler? SenseMapReset;

        /// <summary>
        /// Calculates the map.  For each enabled source in the source list, it calculates
        /// the source's spreading, and puts them all together in the sense map's output.
        /// </summary>
        public void Calculate();

        /// <summary>
        /// Adds the given source to the list of sources. If the source has its
        /// <see cref="ISenseSource.Enabled" /> flag set when <see cref="Calculate" /> is next called, then
        /// it will be counted as a source.
        /// </summary>
        /// <param name="senseSource">The source to add.</param>
        void AddSenseSource(ISenseSource senseSource);

        /// <summary>
        /// Removes the given source from the list of sources. Generally, use this if a source is permanently removed
        /// from a map. For temporary disabling, you should generally use the <see cref="ISenseSource.Enabled" /> flag.
        /// </summary>
        /// <remarks>
        /// The source values that this sense source was responsible for are NOT removed from the sensory output values
        /// until <see cref="Calculate" /> is next called.
        /// </remarks>
        /// <param name="senseSource">The source to remove.</param>
        public void RemoveSenseSource(ISenseSource senseSource);

        /// <summary>
        /// Resets the given sense map by erasing the current recorded result values.
        /// </summary>
        /// <remarks>
        /// After this function is called, any value in <see cref="IReadOnlySenseMap.ResultView"/> will be 0.
        /// Additionally,<see cref="IReadOnlySenseMap.CurrentSenseMap"/> will be blank.
        /// </remarks>
        public void Reset();
    }
}
