namespace GoRogue.PerformanceTests.MultiAreas
{
    /// <summary>
    /// Contains fields which act as data sources for test parameters that are shared across multiple tests.
    /// </summary>
    public static class SharedTestParams
    {
        /// <summary>
        /// Contains all the values of the "Size" test parameter that will be tested when the tests are run.
        /// </summary>
        public static readonly int[] Sizes = { 10, 100, 256 };

        /// <summary>
        /// Contains all the values of the "NumAreas" test parameter that will be tested when the tests are run.
        /// </summary>
        public static readonly int[] NumAreas = { 1, 2, 5, 10 };
    }
}
