﻿using System.Collections.Generic;
using GoRogue.SenseMapping.Sources;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.SenseMapping
{
    /// <summary>
    /// Read-only interface of an <see cref="ISenseMap" />.
    /// </summary>
    [PublicAPI]
    public interface IReadOnlySenseMap
    {
        /// <summary>
        /// IEnumerable of only positions currently "in" the sense map, eg. all positions that have a
        /// value other than 0.0.
        /// </summary>
        IEnumerable<Point> CurrentSenseMap { get; }

        /// <summary>
        /// IEnumerable of positions that DO have a non-zero value in the sense map as of the most
        /// current Calculate call, but DID NOT have a non-zero value after the previous time
        /// Calculate was called.
        /// </summary>
        IEnumerable<Point> NewlyInSenseMap { get; }

        /// <summary>
        /// IEnumerable of positions that DO NOT have a non-zero value in the sense map as of the
        /// most current Calculate call, but DID have a non-zero value after the previous time
        /// Calculate was called.
        /// </summary>
        IEnumerable<Point> NewlyOutOfSenseMap { get; }

        /// <summary>
        /// Read-only list of all sources currently considered part of the sense map. Some may have their
        /// <see cref="ISenseSource.Enabled" /> flag set to false, so all of these may or may not be counted
        /// when Calculate is called.
        /// </summary>
        IReadOnlyList<ISenseSource> SenseSources { get; }

        /// <summary>
        /// The resistance map used to perform calculations.
        /// </summary>
        public IGridView<double> ResistanceView { get; }

        /// <summary>
        /// A view of the sense map's calculation results.
        /// </summary>
        public IGridView<double> ResultView { get; }

        /// <summary>
        /// Returns a read-only representation of the sensory map.
        /// </summary>
        /// <returns>This sensory map object as <see cref="IReadOnlySenseMap" />.</returns>
        public IReadOnlySenseMap AsReadOnly();
    }
}
