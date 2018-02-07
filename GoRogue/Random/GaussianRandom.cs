using System;

namespace GoRogue.Random
{
    /// <summary>
    /// Pseudo-random number generator that uses a Box-Muller transformation to generate random
    /// numbers. Basically this will give you "random" numbers that distribute on a bell curve
    /// centered on the proper range. TODO: Port this to use any random source.
    /// </summary>
    public class GaussianRandom : IRandom
    {
        //private long numberGenerated;
        private double nextGaussian;

        private System.Random random;
        private int seed;
        private bool uselast = true;

        /// <summary>
        /// Constructs a new Gaussian pseudo-random number generator with a seed based on the number
        /// of milliseconds elapsed since the system started.
        /// </summary>
        public GaussianRandom()
           : this(Environment.TickCount)
        {
        }

        /// <summary>
        /// Constructs a new Gaussian pseudo-random number generator with the specified seed.
        /// </summary>
        /// <param name="seed">
        /// An integer used to calculate a starting value for the pseudo-random number sequence.
        /// </param>
        public GaussianRandom(int seed)
        {
            this.seed = seed;
            random = new System.Random(seed);
        }

        /// <summary>
        /// Will approximately give the next Gaussian pseudo-random integer between 0 and that
        /// specified max value, inclusive, so that min and max are at 3.5 deviations from the mean
        /// (half-way of min and max).
        /// </summary>
        /// <param name="maxValue">
        /// Inclusive maximum result.
        /// </param>
        /// <returns>
        /// A Gaussian pseudo-random integer between 0 and the specified maxValue, inclusive.
        /// </returns>
        public int Next(int maxValue) => Next(0, maxValue);

        /// <summary>
        /// Will approximately give the next random Gaussian integer between the specified min and
        /// max values, inclusive, so that min and max are at 3.5 deviations from the mean (half-way
        /// of min and max).
        /// </summary>
        /// <param name="minValue">
        /// Inclusive minimum result.
        /// </param>
        /// <param name="maxValue">
        /// Inclusive maximum result.
        /// </param>
        /// <returns>
        /// Returns a pseudo-random integer between the specified minValue and maxValue, inclusive.
        /// </returns>
        public int Next(int minValue, int maxValue)
        {
            //_numberGenerated++;
            double deviations = 3.5;
            // var r = (int)boxMuller(minValue + (maxValue - minValue) / 2.0, (maxValue - minValue) /
            // 2.0 / deviations);
            var r = minValue - 1;

            while (r < minValue || r > maxValue)
                r = (int)boxMuller(minValue + (maxValue - minValue) / 2.0, (maxValue - minValue) / 2.0 / deviations);

            /*
            if (r > maxValue)
            {
                r = maxValue;
            }
            else if (r < minValue)
            {
                r = minValue;
            }
            */

            return r;
        }

        private double boxMuller()
        {
            if (uselast)
            {
                uselast = false;
                return nextGaussian;
            }
            else
            {
                double v1, v2, s;
                do
                {
                    v1 = 2.0 * random.NextDouble() - 1.0;
                    v2 = 2.0 * random.NextDouble() - 1.0;
                    s = v1 * v1 + v2 * v2;
                }
                while (s >= 1.0 || Math.Abs(s) < 0.0000000001);

                s = Math.Sqrt((-2.0 * Math.Log(s)) / s);

                nextGaussian = v2 * s;
                uselast = true;
                return v1 * s;
            }
        }

        private double boxMuller(double mean, double standardDeviation) => mean + boxMuller() * standardDeviation;
    }
}