using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.Serialization
{
    public class DataContractTests
    {
        // Settings used for serialization/deserialization.  Ensures that things stored as polymorphic types (ex.
        // object in ComponentCollection) deserialize properly
        private static readonly JsonSerializerSettings s_settings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        #region Test Data
        // Tests require the properties used for theories to be in the same class, so we refer to TestData
        public static IEnumerable<object> AllSerializableObjects => TestData.AllSerializableObjects;
        public static IEnumerable<object> SerializableValuesJsonObjects => TestData.SerializableValuesJsonObjects;
        public static IEnumerable<object> SerializableValuesNonJsonObjects => TestData.SerializableValuesNonJsonObjects;
        #endregion

        // Useful for viewing output
        private readonly ITestOutputHelper _output;

        public DataContractTests(ITestOutputHelper output) => _output = output;

        /// <summary>
        /// Tests that if we serialize an object to JSON, then deserialize it back to an object,
        /// that the deserialized object is equal to the one that we serialized.  Equality is compared
        /// via <see cref="Comparisons.GetComparisonFunc"/>.
        /// </summary>
        /// <param name="objToSerialize"/>
        [Theory]
        [MemberDataEnumerable(nameof(AllSerializableObjects))]
        public void SerializeToDeserializeEquivalence(object objToSerialize)
        {
            var objType = objToSerialize.GetType();
            _output.WriteLine($"Type is: {objType.Name}");

            // Set equality to custom comparer if we have one, otherwise default to .Equals
            Func<object, object, bool> equality = Comparisons.GetComparisonFunc(objToSerialize);

            // Serialize to JSON string
            string json = JsonConvert.SerializeObject(objToSerialize, s_settings);

            _output.WriteLine("Json:");
            _output.WriteLine(JToken.Parse(json).ToString(Formatting.Indented));

            // Deserialize to object
            object? deSerialized = JsonConvert.DeserializeObject(json, objType, s_settings);
            TestUtils.NotNull(deSerialized);

            // Make sure the deserialized object is equivalent to the one we serialized
            Assert.True(equality(objToSerialize, deSerialized));
        }

        /// <summary>
        /// For objects that are serialized to JSON objects, tests that the fields in the serialized JSON
        /// are exactly the fields that are expected to be.  This ensures that no fields are being serialized unintentionally.
        /// </summary>
        /// <param name="objToSerialize"/>
        [Theory]
        [MemberDataEnumerable(nameof(SerializableValuesJsonObjects))]
        public void ExpectedFieldsSerialized(object objToSerialize)
        {
            // Serialize to JSON string
            string json = JsonConvert.SerializeObject(objToSerialize, s_settings);

            // Get fields in hash set
            var fields = JObject.Parse(json).Properties().Select(i => i.Name).ToHashSet();

            // Make hash set from specified fields that _should_ be there
            var expectedFields = TestData.TypeSerializedFields[objToSerialize.GetType()].ToHashSet();

            // Ensure expected fields are what we got (in arbitrary order)
            Assert.Equal(expectedFields, fields);
        }

        // We assume that there are no fields to validate on the objects in the non-objects list,
        // so we check to make sure they're arrays and thus have no fields.  There may be other types we want to
        // check for here later
        [Theory]
        [MemberDataEnumerable(nameof(SerializableValuesNonJsonObjects))]
        public void NonJsonObjectsSerializeToJsonArray(object objToSerialize)
        {
            // Serialize to JSON string
            string json = JsonConvert.SerializeObject(objToSerialize, s_settings);

            var array = JArray.Parse(json);
            Assert.NotNull(array);
            Assert.NotEmpty(array);
        }
    }
}
