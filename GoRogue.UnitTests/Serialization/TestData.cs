using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Components;
using GoRogue.DiceNotation;
using GoRogue.SerializedTypes.Components;
using GoRogue.SerializedTypes.DiceNotation;
using GoRogue.UnitTests.Mocks;

namespace GoRogue.UnitTests.Serialization
{
    public static class TestData
    {
        #region Original Data
        /// <summary>
        /// List of expressive versions of types.  The assumptions here are:
        ///     1. All types in this list are serializable via binary serialization, and data contract serialization
        ///     2. ONLY the types in this list are serializable via binary serialization
        ///     2. All types in this list serialize to JSON objects (JObject)
        /// </summary>
        public static readonly IEnumerable<object> ExpressiveTypes = new object[]
        {
            // Not expressive types, but if these don't serialize properly to binary and JSON it will break
            // tests using them
            new Component1() { Value = 100 },
            new Component2() { Value = 200 },
            // ComponentTagPair
            new ComponentTagPairSerialized { Component = new Component1() { Value = 82 }, Tag = "tag" },
            new ComponentTagPairSerialized { Component = new Component2() { Value = 81 }, Tag = null },
            // ComponentCollection
            new ComponentCollectionSerialized
            {
                Components = new List<ComponentTagPairSerialized>
                {
                    new ComponentTagPairSerialized { Component = new Component1(), Tag = "tag" },
                    new ComponentTagPairSerialized { Component = new Component2(), Tag = null },
                }
            },
            // DiceExpression
            new DiceExpressionSerialized { Expression = "3d(1d12)k2+4" }
        };

        /// <summary>
        /// List of all non-expressive types that serialize to JSON objects (JObject)
        /// </summary>
        private static readonly object[] _nonExpressiveJsonObjects =
        {
            // ComponentTagPairs
            new ComponentTagPair(new Component1() { Value = 92 }, "tag"),
            new ComponentTagPair(new Component2() { Value = 91 }, null),
            // DiceExpressions
            Dice.Parse("(3d(1d12))k2+4")
        };

        /// <summary>
        /// Dictionary of object types to an unordered but complete list of fields that each object type should have
        /// serialized in its JSON object form.  All objects in SerializableValuesJsonObjects should have an entry here.
        /// </summary>
        public static readonly Dictionary<Type, string[]> TypeSerializedFields = new Dictionary<Type, string[]>
        {
            // Not GoRogue types, but we check them to verify they serialize as expected to ensure other tests work
            { typeof(Component1), new []{ "Value" } },
            { typeof(Component2), new []{ "Value" } },
            // GoRogue types
            { typeof(ComponentTagPair), new []{ "Component", "Tag" } },
            { typeof(ComponentTagPairSerialized), new []{ "Component", "Tag" } },
            { typeof(ComponentCollectionSerialized), new []{ "Components" } },
            { typeof(DiceExpression), new [] { "RootTerm" } },
            { typeof(DiceExpressionSerialized), new [] { "Expression" } }
        };

        /// <summary>
        /// Objects that are JSON serializable but should NOT serialize to JSON objects (instead, for example,
        /// JSON array)
        /// </summary>
        public static readonly IEnumerable<object> SerializableValuesNonJsonObjects = new object[]
        {
            // Component collection with serializable components
            new ComponentCollection
            {
                new Component1() { Value = 1 },
                { new Component2() { Value = 2 }, "MyTag" },
                new Component2() { Value = 3}
            },
        };

        /// <summary>
        /// Dictionary of non-expressive types to their expressive type
        /// </summary>
        public static readonly Dictionary<Type, Type> RegularToExpressiveTypes = new Dictionary<Type, Type>
        {
            { typeof(ComponentTagPair), typeof(ComponentTagPairSerialized) },
            { typeof(ComponentCollection), typeof(ComponentCollectionSerialized) },
            { typeof(DiceExpression), typeof(DiceExpressionSerialized) }
        };
        #endregion

        #region Combinatory Data
        /// <summary>
        /// All objects that should serialize to JSON objects.  All should have entries in TypeSerializedFields
        /// </summary>
        public static IEnumerable<object> SerializableValuesJsonObjects
            => ExpressiveTypes.Concat(_nonExpressiveJsonObjects);

        /// <summary>
        /// Objects that should have expressive versions of types.  Each item must have an entry in
        /// RegularToExpressiveTypes
        /// </summary>
        public static IEnumerable<object> AllNonExpressiveTypes
            => _nonExpressiveJsonObjects.Concat(SerializableValuesNonJsonObjects);

        /// <summary>
        /// All JSON objects for which we can test serialization equality
        /// </summary>
        public static IEnumerable<object> AllSerializableObjects =>
            SerializableValuesJsonObjects.Concat(SerializableValuesNonJsonObjects);
        #endregion
    }
}
