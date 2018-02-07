using System;
using System.Collections.Generic;

namespace GoRogue.Random
{
    /// <summary>
    /// "RNG" that takes in a series of integers, and simply returns the next one in the list. For
    /// example, for a series of numbers 3 long, calling Next the first time returns the first
    /// number, the second time it returns the second number, the third time the third number. This
    /// might be used for testing, where you want to test code involving random numbers on a specific
    /// number or set of numbers.
    /// </summary>
    public class KnownSeriesRandom : IRandom
    {
        //private long _numberGenerated;
        private Queue<int> series;

        /// <summary>
        /// Creates a new known series generator, with params indicating what the integers will be
        /// that it should use as the series.
        /// </summary>
        /// <param name="series">
        /// The sequence of numbers to return, in order, when next is called.
        /// </param>
        public KnownSeriesRandom(params int[] series)
        {
            if (series == null)
            {
                throw new ArgumentNullException(nameof(series), "Series cannot be null");
            }

            this.series = new Queue<int>();
            foreach (int number in series)
            {
                this.series.Enqueue(number);
            }
        }

        /// <summary>
        /// Gets the next number in the underlying series. If the value is less than 0 or over
        /// maxValue, throws an exception.
        /// </summary>
        /// <param name="maxValue">
        /// Maximum allowable number that can be returned.
        /// </param>
        /// <returns>
        /// The appropriate number from the series.
        /// </returns>
        public int Next(int maxValue) => Next(0, maxValue);

        /// <summary>
        /// Gets the next number in the underlying series. If the value is less than minValue or over
        /// maxValue, throws an exception.
        /// </summary>
        /// <param name="minValue">
        /// Minimum allowable number that can be returned.
        /// </param>
        /// <param name="maxValue">
        /// Maximum allowable number that can be returned.
        /// </param>
        /// <returns>
        /// The appropriate number from the series.
        /// </returns>
        public int Next(int minValue, int maxValue)
        {
            int value = series.Dequeue();
            if (value < minValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), "Next value in series is smaller than the minValue parameter");
            }
            if (value > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue), "Next value in series is larger than the maxValue parameter");
            }
            series.Enqueue(value);
            //_numberGenerated++;
            return value;
        }
    }
}