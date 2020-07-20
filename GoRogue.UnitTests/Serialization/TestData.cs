using System;
using System.Collections.Generic;
using SadRogue.Primitives;
using System.Linq;
using GoRogue.Components;
using GoRogue.DiceNotation;
using GoRogue.Factories;
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapGeneration.Steps.Translation;
using GoRogue.SerializedTypes.Components;
using GoRogue.SerializedTypes.DiceNotation;
using GoRogue.SerializedTypes.Factories;
using GoRogue.SerializedTypes.MapGeneration;
using GoRogue.SerializedTypes.MapGeneration.ContextComponents;
using GoRogue.SerializedTypes.MapGeneration.Steps.Translation;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives.SerializedTypes;

namespace GoRogue.UnitTests.Serialization
{
    public static class TestData
    {
        #region Original Data
        /// <summary>
        /// List of expressive versions of types.  The assumptions here are:
        ///     1. All types in this list are serializable via generic data contract serialization
        ///     2. All types in this list serialize to JSON objects (JObject) when using Newtonsoft.Json
        /// </summary>
        private static readonly IEnumerable<object> _expressiveTypes = new object[]
        {
            // Not expressive types, but if these don't serialize properly to binary and JSON it will break
            // tests using them
            new Component1() { Value = 100 },
            new Component2() { Value = 200 },
            // Simple structs; don't have expressive versions but meet the preconditions and serve the same purpose
            // ComponentTagPairs
            new ComponentTagPair(new Component1() { Value = 92 }, "tag"),
            new ComponentTagPair(new Component2() { Value = 91 }, null),
            new ItemStepPair<string>("MyItem", "MyStep"),
            new ItemStepPair<string>("MyItem2", "MyStep2"),
            // ComponentCollection
            new ComponentCollectionSerialized
            {
                Components = new List<ComponentTagPair>
                {
                    new ComponentTagPair(new Component1(), "tag"),
                    new ComponentTagPair(new Component2(), null),
                }
            },
            // DiceExpression
            new DiceExpressionSerialized { Expression = "3d(1d12)k2+4" },
            // Factories
            new FactorySerialized<FactoryItem>
            {
                Blueprints = new List<IFactoryBlueprint<FactoryItem>>
                {
                    new FactoryItemBlueprint("1"),
                    new FactoryItemBlueprint("2"),
                    new FactoryItemBlueprint("3")
                }
            },
            new AdvancedFactorySerialized<int, FactoryItem>
            {
                Blueprints = new List<IAdvancedFactoryBlueprint<int, FactoryItem>>
                {
                    new AdvancedFactoryItemBlueprint("1"),
                    new AdvancedFactoryItemBlueprint("2"),
                    new AdvancedFactoryItemBlueprint("3")
                }
            },
            // RoomDoors
            new RoomDoorsSerialized
            {
                Room = new RectangleSerialized { X = 1, Y = 2, Width = 20, Height = 10 },
                Doors = new List<ItemStepPair<PointSerialized>>
                {
                    new ItemStepPair<PointSerialized>(new PointSerialized {X = 1, Y = 3}, "SomeStep"),
                    new ItemStepPair<PointSerialized>(new PointSerialized {X = 5, Y = 2}, "OtherStep"),
                }
            },
            // DoorList
            new DoorListSerialized
            {
                RoomsAndDoors = new List<RoomDoorsSerialized>
                {
                    new RoomDoorsSerialized
                    {
                        Room = new RectangleSerialized { X = 1, Y = 2, Width = 20, Height = 10 },
                        Doors = new List<ItemStepPair<PointSerialized>>
                        {
                            new ItemStepPair<PointSerialized>(new PointSerialized {X = 1, Y = 3}, "SomeStep"),
                            new ItemStepPair<PointSerialized>(new PointSerialized {X = 5, Y = 2}, "OtherStep"),
                        }
                    },
                    new RoomDoorsSerialized
                    {
                        Room = new RectangleSerialized { X = 11, Y = 21, Width = 20, Height = 10 },
                        Doors = new List<ItemStepPair<PointSerialized>>
                        {
                            new ItemStepPair<PointSerialized>(new PointSerialized {X = 11, Y = 30}, "SomeStep"),
                            new ItemStepPair<PointSerialized>(new PointSerialized {X = 18, Y = 21}, "OtherStep"),
                        }
                    }
                }
            },
            // ItemList
            new ItemListSerialized<string>
            {
                Items = new List<ItemStepPair<string>>
                {
                    new ItemStepPair<string>("Item1", "SomeStep"),
                    new ItemStepPair<string>("Item2", "OtherStep"),
                }
            },
            // RectangleEdgePositionsList
            new RectangleEdgePositionsListSerialized
            {
                Rectangle = new RectangleSerialized { X = 10, Y = 12, Width = 20, Height = 10 },
                Positions = new List<PointSerialized>
                {
                    new PointSerialized { X = 10, Y = 15 },
                    new PointSerialized { X = 14, Y = 12 }
                }
            },
            // AppendItemLists
            new AppendItemListsSerialized<string>
            {
                Name = "MyName",
                BaseListTag = "BaseList",
                ListToAppendTag = "AppendList",
                RemoveAppendedComponent = true
            },
            // RectanglesToAreas
            new RectanglesToAreasSerialized
            {
                Name = "MyName",
                AreasComponentTag = "AreasComponent",
                RectanglesComponentTag = "RectanglesComponent",
                RemoveSourceComponent = true
            },
            // RemoveDuplicatePoints
            new RemoveDuplicatePointsSerialized
            {
                Name = "MyName",
                ModifiedAreaListTag = "ModifiedList",
                UnmodifiedAreaListTag = "UnmodifiedList"
            }
        };

        /// <summary>
        /// List of all non-expressive types that serialize to JSON objects (JObject)
        /// </summary>
        private static readonly object[] _nonExpressiveJsonObjects =
        {
            // DiceExpressions
            Dice.Parse("(3d(1d12))k2+4"),
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
            { typeof(ComponentCollectionSerialized), new []{ "Components" } },
            { typeof(DiceExpression), new [] { "RootTerm" } },
            { typeof(DiceExpressionSerialized), new [] { "Expression" } },
            { typeof(FactorySerialized<FactoryItem>), new [] { "Blueprints" } },
            { typeof(AdvancedFactorySerialized<int, FactoryItem>), new [] { "Blueprints" } },
            { typeof(ItemStepPair<string>), new []{ "Item", "Step" } },
            { typeof(ItemListSerialized<string>), new []{ "Items" } },
            { typeof(DoorListSerialized), new []{ "RoomsAndDoors" } },
            { typeof(RoomDoorsSerialized), new []{ "Room", "Doors" } },
            { typeof(RectangleEdgePositionsListSerialized), new [] { "Rectangle", "Positions" } },
            { typeof(AppendItemListsSerialized<string>), new [] { "Name", "BaseListTag", "ListToAppendTag", "RemoveAppendedComponent" } },
            { typeof(RectanglesToAreasSerialized), new []{ "Name", "AreasComponentTag", "RectanglesComponentTag", "RemoveSourceComponent" } },
            { typeof(RemoveDuplicatePointsSerialized), new []{ "Name", "UnmodifiedAreaListTag", "ModifiedAreaListTag" } }
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
            // Factories with serializable blueprints
            new Factory<FactoryItem>
            {
                new FactoryItemBlueprint("1"),
                new FactoryItemBlueprint("2"),
                new FactoryItemBlueprint("3")
            },
            new AdvancedFactory<int, FactoryItem>
            {
                new AdvancedFactoryItemBlueprint("1"),
                new AdvancedFactoryItemBlueprint("2"),
                new AdvancedFactoryItemBlueprint("3")
            },
            // ItemList
            new ItemList<string>
            {
                {"Item1", "Step1"},
                {"Item2", "Step2"},
            }
        };

        /// <summary>
        /// Dictionary of non-expressive types to their expressive type
        /// </summary>
        public static readonly Dictionary<Type, Type> RegularToExpressiveTypes = new Dictionary<Type, Type>
        {
            { typeof(ComponentCollection), typeof(ComponentCollectionSerialized) },
            { typeof(DiceExpression), typeof(DiceExpressionSerialized) },
            { typeof(Factory<FactoryItem>), typeof(FactorySerialized<FactoryItem>) },
            { typeof(AdvancedFactory<int, FactoryItem>), typeof(AdvancedFactorySerialized<int, FactoryItem>) },
            { typeof(DoorList), typeof(DoorListSerialized) },
            { typeof(ItemList<string>), typeof(ItemListSerialized<string>) },
            { typeof(RoomDoors), typeof(RoomDoorsSerialized) },
            { typeof(RectangleEdgePositionsList), typeof(RectangleEdgePositionsListSerialized) },
            { typeof(AppendItemLists<string>), typeof(AppendItemListsSerialized<string>) },
            { typeof(RectanglesToAreas), typeof(RectanglesToAreasSerialized) },
            { typeof(RemoveDuplicatePoints), typeof(RemoveDuplicatePointsSerialized) }
        };

        /// <summary>
        /// List of non-serializable objects that do have serializable equivalents (expressive types).
        /// </summary>
        private static readonly object[] _nonSerializableValuesWithExpressiveTypes =
        {
            // DoorList
            GenerateDoorList(),
            // RoomDoors
            GenerateRoomDoors(),
            // RectangleEdgePositionsList
            new RectangleEdgePositionsList((1, 2, 10, 20)) { (1, 4), (1, 7), (5, 2), (3, 2) },
            // AppendItemLists
            new AppendItemLists<string>("MyName", "BaseList", "AppendList")
            {
                RemoveAppendedComponent = true
            },
            // RectanglesToAreas
            new RectanglesToAreas("MyName", "RectanglesList", "AreasList")
            {
                RemoveSourceComponent = true
            },
            // RemoveDuplicatePoints
            new RemoveDuplicatePoints("MyName", "UnmodifiedAreaList", "ModifiedAreaList")
        };
        #endregion

        #region Combinatory Data
        /// <summary>
        /// All objects that should serialize to JSON objects.  All should have entries in TypeSerializedFields
        /// </summary>
        public static IEnumerable<object> SerializableValuesJsonObjects
            => _expressiveTypes.Concat(_nonExpressiveJsonObjects);

        /// <summary>
        /// Objects that should have expressive versions of types.  Each item must have an entry in
        /// RegularToExpressiveTypes
        /// </summary>
        public static IEnumerable<object> AllNonExpressiveTypes
            => _nonExpressiveJsonObjects.Concat(SerializableValuesNonJsonObjects).Concat(_nonSerializableValuesWithExpressiveTypes);

        /// <summary>
        /// All JSON objects for which we can test serialization equality
        /// </summary>
        public static IEnumerable<object> AllSerializableObjects =>
            SerializableValuesJsonObjects.Concat(SerializableValuesNonJsonObjects);
        #endregion

        #region Data Generation Functions
        private static RoomDoors GenerateRoomDoors()
        {
            var roomDoors = new RoomDoors((1, 2, 10, 20));

            // Add step1 doors
            roomDoors.AddDoor("Step1", (0, 5));
            roomDoors.AddDoor("Step1", (4, 1));

            // Add step 2 doors
            roomDoors.AddDoor("Step2", (0, 7));
            roomDoors.AddDoor("Step2", (3, 1));

            return roomDoors;
        }

        private static DoorList GenerateDoorList()
        {
            var doorList = new DoorList();
            var room1 = new Rectangle(1, 2, 10, 20);
            var room2 = new Rectangle(30, 34, 7, 15);

            // Add room1 doors
            doorList.AddDoor("Step1", room1, (0, 5));
            doorList.AddDoor("Step2", room1, (4, 1));

            // Add room2 doors
            doorList.AddDoor("Step1", room2, (29, 40));
            doorList.AddDoor("Step2", room2, (33, 33));

            return doorList;
        }
        #endregion
    }
}
