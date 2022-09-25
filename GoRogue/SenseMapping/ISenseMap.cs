using System;
using GoRogue.SenseMapping.Sources;
using JetBrains.Annotations;

namespace GoRogue.SenseMapping
{
    /// <summary>
    /// Interface for calculating a map representing senses (sound, light, etc), or generally anything
    /// that can be modeled as sources propagating through a map that has degrees of resistance to spread.
    /// </summary>
    /// <remarks>
    /// If you're looking for an existing implementation of this interface to use, see <see cref="SenseMap"/>.  If you instead want
    /// to implement your own, you may want to consider using <see cref="SenseMapBase"/> as your base class, as it simplifies the
    /// implementation considerably.
    /// 
    /// This interface functions on the concept of having one or more <see cref="ISenseSource"/> instances, which are capable
    /// of using some algorithm to propagate the source through a map.  The map, and therefore each source, uses a grid view
    /// of doubles as its map representation, where each double represents the "resistance" that location has to the passing of
    /// source values through it. The values must be >= 0.0, where 0.0 means that a location has no resistance to spreading of
    /// source values, and greater values represent greater resistance.  The scale of this resistance is arbitrary, and is
    /// related to the <see cref="ISenseSource.Intensity" /> of your sources.
    ///
    /// Other than the constraint that 0.0 means a cell has no resistance, the interfaces/APIs themselves impose no strict limitations
    /// on the definition of the intensity and resistance values.  The default implementations of these interfaces in GoRogue treat
    /// the resistance view value as a value which is subtracted from source's remaining intensity as they propagate through that cell;
    /// so as a source spreads through a given location, a value equal to the resistance value of that location is subtracted from the
    /// source's value (plus the normal fall-of for distance).  However, if some other method is needed, a custom <see cref="ISenseSource"/>
    /// can be implemented; see that interface's documentation for details.
    ///
    /// Generally, usage involves performing and aggregating calculations for all sense sources by calling the <see cref="ISenseMap.Calculate" />
    /// function, then accessing the results via <see cref="IReadOnlySenseMap.ResultView"/>.  These values will be 0.0 if no source spread to that
    /// location, and greater than 0.0 to indicate the strength of the combined sources which spread there.
    /// </remarks>
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
        /// Calculates the map.  For each enabled source in the source list, it calculates
        /// the source's spreading, and puts them all together in the sense map's output.
        /// </summary>
        public void Calculate();

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
