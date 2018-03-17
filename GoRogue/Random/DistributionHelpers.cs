using Troschuetz.Random;
using Troschuetz.Random.Distributions.Continuous;
using Troschuetz.Random.Generators;

namespace GoRogue.Random
{
    /// <summary>
    /// Defines functions that assist in dealing with RNG distributions.
    /// </summary>
    static public class DistributionHelpers
    {
        /// <summary>
        /// Creates a normal distribution based on a lower and upper value, and a distance to place those points from the mean.  The mean is placed precisely in between
        /// the upper and lower values given, and the upper and lower values will both be exactly deviationsFromMean away from the mean. 
        /// </summary>
        /// <param name="lower">Lower value by which to define the distribution.</param>
        /// <param name="upper">Upper value by which to define the distribution.</param>
        /// <param name="deviationsFromMean">Number of deviations from the mean at which to place the lower and upper values given.</param>
        /// <returns>A NormalDistribution constructed such that the mean is precisely in between the lower and upper values given, and the lower and upper values
        /// are exatly the specified number of deviations away from the mean.</returns>
        static public NormalDistribution CreateNormalDistribution(double lower = -1.0, double upper = 1.0, double deviationsFromMean = 3.5)
            => CreateNormalDistribution(new XorShift128Generator(), lower, upper, deviationsFromMean);

        /// <summary>
        /// Creates a normal distribution based on a lower and upper value, and a distance to place those points from the mean.  The mean is placed precisely in between
        /// the upper and lower values given, and the upper and lower values will both be exactly deviationsFromMean away from the mean. 
        /// </summary>
        /// <param name="seed">The seed to pass the default XorShift128Generator that is created.</param>
        /// <param name="lower">Lower value by which to define the distribution.</param>
        /// <param name="upper">Upper value by which to define the distribution.</param>
        /// <param name="deviationsFromMean">Number of deviations from the mean at which to place the lower and upper values given.</param>
        /// <returns>A NormalDistribution constructed such that the mean is precisely in between the lower and upper values given, and the lower and upper values
        /// are exatly the specified number of deviations away from the mean.</returns>
        static public NormalDistribution CreateNormalDistribution(uint seed, double lower = -1.0, double upper = 1.0, double deviationsFromMean = 3.5)
            => CreateNormalDistribution(new XorShift128Generator(seed), lower, upper, deviationsFromMean);

        /// <summary>
        /// Creates a normal distribution based on a lower and upper value, and a distance to place those points from the mean.  The mean is placed precisely in between
        /// the upper and lower values given, and the upper and lower values will both be exactly deviationsFromMean away from the mean. 
        /// </summary>
        /// <param name="generator">Generator to use. If null is specified, the default RNG will be used.</param>
        /// <param name="lower">Lower value by which to define the distribution.</param>
        /// <param name="upper">Upper value by which to define the distribution.</param>
        /// <param name="deviationsFromMean">Number of deviations from the mean at which to place the lower and upper values given.</param>
        /// <returns>A NormalDistribution constructed such that the mean is precisely in between the lower and upper values given, and the lower and upper values
        /// are exatly the specified number of deviations away from the mean.</returns>
        static public NormalDistribution CreateNormalDistribution(IGenerator generator = null, double lower = -1.0, double upper = 1.0, double deviationsFromMean = 3.5)
        {
            if (generator == null)
                generator = SingletonRandom.DefaultRNG;

            return new NormalDistribution(generator, lower + (upper - lower) / 2.0, (upper - lower) / 2.0 / deviationsFromMean);
        }
    }
}
