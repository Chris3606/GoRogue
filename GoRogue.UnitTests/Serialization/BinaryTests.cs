using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.Serialization
{
    public class BinaryTests
    {
        public static IEnumerable<object> ExpressiveTypes => TestData.ExpressiveTypes;

        [Theory]
        [MemberDataEnumerable(nameof(ExpressiveTypes))]
        public void SerializeToDeserializeEquivalence(object objToSerialize)
        {
            Func<object, object, bool> equalityFunc = Comparisons.GetComparisonFunc(objToSerialize);

            string name = $"{objToSerialize.GetType().FullName}.bin";

            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(name, FileMode.Create, FileAccess.Write))
                formatter.Serialize(stream, objToSerialize);

            object reSerialized;
            using (var stream = new FileStream(name, FileMode.Open, FileAccess.Read))
                reSerialized = formatter.Deserialize(stream);

            File.Delete(name);
            Assert.True(equalityFunc(objToSerialize, reSerialized));
        }
    }
}
