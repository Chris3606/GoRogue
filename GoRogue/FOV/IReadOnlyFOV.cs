using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.FOV
{
    /// <summary>
    /// Read-only interface of <see cref="IFOV" />.
    /// </summary>
    [PublicAPI]
    public interface IReadOnlyFOV
    {
        /// <summary>
        /// The values used to calculate field of view. Values of true are considered
        /// non-blocking (transparent) to line of sight, while false values are considered
        /// to be blocking.
        /// </summary>
        public IGridView<bool> TransparencyView { get; }

        /// <summary>
        /// A view of the calculation results in boolean form, where true indicates a location is in
        /// field of view, and false indicates it is not.
        /// </summary>
        IGridView<bool> BooleanResultView { get; }

        /// <summary>
        /// A view of the calculation results in double form, where a value greater than 0.0 indicates that the value
        /// is inside of FOV, and a value of 1.0 indicates the center point.  All other values vary between 0.0 and
        /// 1.0, decreasing as positions get farther away from the center point.
        /// </summary>
        IGridView<double> DoubleResultView { get; }

        /// <summary>
        /// IEnumerable of only positions currently in the field of view.
        /// </summary>
        IEnumerable<Point> CurrentFOV { get; }

        /// <summary>
        /// IEnumerable of positions that ARE in field of view as of the most current Calculate call, but were
        /// NOT in field of view after the previous time Calculate was called.
        /// </summary>
        IEnumerable<Point> NewlySeen { get; }

        /// <summary>
        /// IEnumerable of positions that are NOT in field of view as of the most current Calculate call, but
        /// WERE in field of view after the previous time Calculate was called.
        /// </summary>
        IEnumerable<Point> NewlyUnseen { get; }

        /// <summary>
        /// A list of the parameters passed to each call to Calculate/CalculateAppend that has been performed since the last reset, in the order in which
        /// they were called.
        /// </summary>
        public IReadOnlyList<FOVCalculateParameters> CalculationsPerformed { get; }

        /// <summary>
        /// Returns a read-only representation of the field of view.
        /// </summary>
        /// <returns>This FOV object, as an <see cref="IReadOnlyFOV" /> instance.</returns>
        public IReadOnlyFOV AsReadOnly();

        /// <summary>
        /// ToString overload that customizes the characters used to represent the map.
        /// </summary>
        /// <param name="normal">The character used for any location not in FOV.</param>
        /// <param name="sourceValue">The character used for any location that is in FOV.</param>
        /// <returns>The string representation of FOV, using the specified characters.</returns>
        public string ToString(char normal = '-', char sourceValue = '+');

        /// <summary>
        /// Returns a string representation of the map, with the actual values in the FOV, rounded to
        /// the given number of decimal places.
        /// </summary>
        /// <param name="decimalPlaces">The number of decimal places to round to.</param>
        /// <returns>A string representation of FOV, rounded to the given number of decimal places.</returns>
        public string ToString(int decimalPlaces);
    }
}
