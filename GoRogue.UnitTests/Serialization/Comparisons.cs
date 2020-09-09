using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Components;
using GoRogue.DiceNotation;
using GoRogue.Factories;
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using GoRogue.MapGeneration.Steps.Translation;
using GoRogue.SerializedTypes.Components;
using GoRogue.SerializedTypes.Factories;
using GoRogue.SerializedTypes.MapGeneration;
using GoRogue.SerializedTypes.MapGeneration.ContextComponents;
using GoRogue.UnitTests.Mocks;

namespace GoRogue.UnitTests.Serialization
{
    public static class Comparisons
    {
        // Dictionary of object types mapping them to custom methods to use in order to determine equality.
        private static readonly Dictionary<Type, Func<object, object, bool>> _equalityMethods =
            new Dictionary<Type, Func<object, object, bool>>()
            {
                { typeof(ComponentCollection), CompareComponentCollections },
                { typeof(ComponentCollectionSerialized), CompareComponentCollectionSerialized },
                { typeof(DiceExpression), CompareDiceExpressions },
                { typeof(Factory<FactoryItem>), CompareFactory },
                { typeof(FactorySerialized<FactoryItem>), CompareFactorySerialized },
                { typeof(AdvancedFactory<int, FactoryItem>), CompareAdvancedFactory },
                { typeof(AdvancedFactorySerialized<int, FactoryItem>), CompareAdvancedFactorySerialized },
                { typeof(DoorList), CompareDoorList },
                { typeof(DoorListSerialized), CompareDoorListSerialized },
                { typeof(ItemList<string>), CompareItemList },
                { typeof(ItemListSerialized<string>), CompareItemListSerialized },
                { typeof(RoomDoors), CompareRoomDoors },
                { typeof(RoomDoorsSerialized), CompareRoomDoorsSerialized },
                { typeof(RectangleEdgePositionsList), CompareRectangleEdgePositionsList },
                { typeof(RectangleEdgePositionsListSerialized), CompareRectangleEdgePositionsListSerialized },
                { typeof(AppendItemLists<string>), CompareAppendItemLists },
                { typeof(RectanglesToAreas), CompareRectanglesToAreas },
                { typeof(RemoveDuplicatePoints), CompareRemoveDuplicatePoints }
            };

        public static Func<object, object, bool> GetComparisonFunc(object obj)
            => _equalityMethods.GetValueOrDefault(obj.GetType(), (o1, o2) => o1.Equals(o2))!;


        private static bool CompareDiceExpressions(object o1, object o2)
        {
            var e1 = (DiceExpression)o1;
            var e2 = (DiceExpression)o2;

            // ToString returns parsable expressions so this should suffice
            return e1.ToString() == e2.ToString();
        }

        private static bool CompareComponentCollections(object o1, object o2)
        {
            var c1 = (ComponentCollection)o1;
            var c2 = (ComponentCollection)o2;

            var hash1 = c1.ToHashSet();
            var hash2 = c2.ToHashSet();

            return HashSetEquality(hash1, hash2);
        }

        private static bool CompareComponentCollectionSerialized(object o1, object o2)
        {
            var c1 = (ComponentCollectionSerialized)o1;
            var c2 = (ComponentCollectionSerialized)o2;

            return ElementWiseEquality(c1.Components, c2.Components);
        }

        private static bool CompareFactory(object o1, object o2)
        {
            var f1 = (Factory<FactoryItem>)o1;
            var f2 = (Factory<FactoryItem>)o2;

            return HashSetEquality(f1.ToHashSet(), f2.ToHashSet());
        }

        private static bool CompareFactorySerialized(object o1, object o2)
        {
            var f1 = (FactorySerialized<FactoryItem>)o1;
            var f2 = (FactorySerialized<FactoryItem>)o2;

            return HashSetEquality(f1.Blueprints.ToHashSet(), f2.Blueprints.ToHashSet());
        }

        private static bool CompareAdvancedFactory(object o1, object o2)
        {
            var f1 = (AdvancedFactory<int, FactoryItem>)o1;
            var f2 = (AdvancedFactory<int, FactoryItem>)o2;

            return HashSetEquality(f1.ToHashSet(), f2.ToHashSet());
        }

        private static bool CompareAdvancedFactorySerialized(object o1, object o2)
        {
            var f1 = (AdvancedFactorySerialized<int, FactoryItem>)o1;
            var f2 = (AdvancedFactorySerialized<int, FactoryItem>)o2;

            return HashSetEquality(f1.Blueprints.ToHashSet(), f2.Blueprints.ToHashSet());
        }

        private static bool CompareDoorList(object o1, object o2)
        {
            var d1 = (DoorList)o1;
            var d2 = (DoorList)o2;

            if (d1.DoorsPerRoom.Count != d2.DoorsPerRoom.Count)
                return false;

            foreach (var key in d1.DoorsPerRoom.Keys)
            {
                if (!d2.DoorsPerRoom.ContainsKey(key))
                    return false;

                // Get comparison func for value
                var equalityFunc = GetComparisonFunc(d1.DoorsPerRoom[key]);
                if (!equalityFunc(d1.DoorsPerRoom[key], d2.DoorsPerRoom[key]))
                    return false;
            }

            return true;
        }

        private static bool CompareDoorListSerialized(object o1, object o2)
        {
            var d1 = (DoorListSerialized)o1;
            var d2 = (DoorListSerialized)o2;



            return ElementWiseEquality(d1.RoomsAndDoors.Cast<object>(), d2.RoomsAndDoors.Cast<object>(),
                _equalityMethods[typeof(RoomDoorsSerialized)]);
        }

        private static bool CompareItemList(object o1, object o2)
        {
            var l1 = (ItemList<string>)o1;
            var l2 = (ItemList<string>)o2;

            return ElementWiseEquality(l1, l2);
        }

        private static bool CompareItemListSerialized(object o1, object o2)
        {
            var l1 = (ItemListSerialized<string>)o1;
            var l2 = (ItemListSerialized<string>)o2;

            return ElementWiseEquality(l1.Items, l2.Items);
        }

        private static bool CompareRoomDoors(object o1, object o2)
        {
            var d1 = (RoomDoors)o1;
            var d2 = (RoomDoors)o2;

            return d1.Room.Equals(d2.Room) && ElementWiseEquality(d1, d2);
        }

        private static bool CompareRoomDoorsSerialized(object o1, object o2)
        {
            var d1 = (RoomDoorsSerialized)o1;
            var d2 = (RoomDoorsSerialized)o2;

            return d1.Room.Equals(d2.Room) && ElementWiseEquality(d1.Doors, d2.Doors);
        }

        private static bool CompareRectangleEdgePositionsList(object o1, object o2)
        {
            var d1 = (RectangleEdgePositionsList)o1;
            var d2 = (RectangleEdgePositionsList)o2;

            return d1.Rectangle.Equals(d2.Rectangle) && ElementWiseEquality(d1.Positions, d2.Positions);
        }

        private static bool CompareRectangleEdgePositionsListSerialized(object o1, object o2)
        {
            var d1 = (RectangleEdgePositionsListSerialized)o1;
            var d2 = (RectangleEdgePositionsListSerialized)o2;

            return d1.Rectangle.Equals(d2.Rectangle) && ElementWiseEquality(d1.Positions, d2.Positions);
        }

        private static bool CompareGenerationSteps(GenerationStep g1, GenerationStep g2)
        {
            var c1 = g1.RequiredComponents.ToList();
            var c2 = g2.RequiredComponents.ToList();

            if (c1.Count != c2.Count)
                return false;

            for (int i = 0; i < c1.Count; i++)
            {
                if (c1[i].ComponentType.FullName != c2[i].ComponentType.FullName)
                    return false;

                if (c1[i].Tag != c2[i].Tag)
                    return false;
            }

            return g1.Name == g2.Name;
        }

        private static bool CompareAppendItemLists(object o1, object o2)
        {
            var s1 = (AppendItemLists<string>)o1;
            var s2 = (AppendItemLists<string>)o2;

            if (!CompareGenerationSteps(s1, s2))
                return false;

            return s1.BaseListTag == s2.BaseListTag &&
                   s1.ListToAppendTag == s2.ListToAppendTag &&
                   s1.RemoveAppendedComponent == s2.RemoveAppendedComponent;
        }

        private static bool CompareRectanglesToAreas(object o1, object o2)
        {
            var s1 = (RectanglesToAreas)o1;
            var s2 = (RectanglesToAreas)o2;

            if (!CompareGenerationSteps(s1, s2))
                return false;

            return s1.AreasComponentTag == s2.AreasComponentTag &&
                   s1.RectanglesComponentTag == s2.RectanglesComponentTag &&
                   s1.RemoveSourceComponent == s2.RemoveSourceComponent;
        }

        private static bool CompareRemoveDuplicatePoints(object o1, object o2)
        {
            var s1 = (RemoveDuplicatePoints)o1;
            var s2 = (RemoveDuplicatePoints)o2;

            if (!CompareGenerationSteps(s1, s2))
                return false;

            return s1.ModifiedAreaListTag == s2.ModifiedAreaListTag &&
                   s1.UnmodifiedAreaListTag == s2.UnmodifiedAreaListTag;
        }

        private static bool HashSetEquality<T>(HashSet<T> h1, HashSet<T> h2)
        {
            foreach (var value in h1)
                if (!h2.Contains(value))
                    return false;

            foreach (var value in h2)
                if (!h1.Contains(value))
                    return false;

            return true;
        }

        private static bool ElementWiseEquality<T>(IEnumerable<T> e1, IEnumerable<T> e2,
                                                   Func<T, T, bool>? compareFunc = null)
        {
            compareFunc ??= (o1, o2) => o1?.Equals(o2) ?? o2 == null;

            var l1 = e1.ToList();
            var l2 = e2.ToList();

            if (l1.Count != l2.Count)
                return false;

            for (int i = 0; i < l1.Count; i++)
                if (!compareFunc(l1[i], l2[i]))
                    return false;

            return true;
        }
    }
}
