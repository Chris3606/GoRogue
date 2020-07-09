using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Components;
using GoRogue.SerializedTypes.Components;

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
            };

        public static Func<object, object, bool> GetComparisonFunc(object obj)
            => _equalityMethods.GetValueOrDefault(obj.GetType(), (o1, o2) => o1.Equals(o2))!;


        private static bool CompareComponentCollections(object o1, object o2)
        {
            var c1 = (ComponentCollection)o1;
            var c2 = (ComponentCollection)o2;

            var hash1 = c1.ToHashSet();
            var hash2 = c2.ToHashSet();

            foreach (var value in hash1)
                if (!hash2.Contains(value))
                    return false;

            foreach (var value in hash2)
                if (!hash1.Contains(value))
                    return false;

            return true;
        }

        private static bool CompareComponentCollectionSerialized(object o1, object o2)
        {
            var c1 = (ComponentCollectionSerialized)o1;
            var c2 = (ComponentCollectionSerialized)o2;

            return ElementWiseEquality(c1.Components, c2.Components);
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
