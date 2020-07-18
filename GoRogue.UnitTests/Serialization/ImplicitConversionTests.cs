using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.Serialization
{
    public class ImplicitConversionTests
    {
        // Required to be in-class so we just forward
        public static IEnumerable<object> AllNonExpressiveTypes = TestData.AllNonExpressiveTypes;

        private ITestOutputHelper _output;

        public ImplicitConversionTests(ITestOutputHelper output) => _output = output;

        [Theory]
        [MemberDataEnumerable(nameof(AllNonExpressiveTypes))]
        public void RegularToSerializedConversion(object original)
        {
            var originalType = original.GetType();
            var serializedType = TestData.RegularToExpressiveTypes[originalType];
            var equalityFunc = Comparisons.GetComparisonFunc(original);

            _output.WriteLine($"Testing expressive conversion from {originalType} to {serializedType}.");

            // Retrieve each implicit conversion operator.  We must use reflection because there is no way to add type
            // constraints that tell the compiler that the object must have these implicit conversions without adding
            // unnecessary things to the GoRogue source code.  The conversion operators must be there or it's an error
            // in the test case, so we check.
            var toExpressive = serializedType.GetMethod("op_Implicit", new[] { originalType });
            var toOriginal = serializedType.GetMethod("op_Implicit", new[] { serializedType });
            // Make sure the conversion functions exist
            TestUtils.NotNull(toExpressive);
            TestUtils.NotNull(toOriginal);

            // Convert to expressive type and assert it returns a valid object of the correct type
            var expressive = toExpressive.Invoke(null, new[] { original });
            TestUtils.NotNull(expressive);
            Assert.Equal(serializedType, expressive.GetType());

            // Convert from the expressive type back to the original and assert it returns a valid object
            // of the correct type
            var converted = toOriginal.Invoke(null, new[] { expressive });
            TestUtils.NotNull(converted);
            Assert.Equal(originalType, converted.GetType());

            // Assert the type is equivalent after conversion as compared to before
            Assert.True(equalityFunc(original, converted));
        }
    }
}
