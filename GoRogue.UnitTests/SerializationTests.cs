using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace GoRogue.UnitTests
{
    public class SerializationTests
    {
        private bool DisjointSetEquality(DisjointSet t1, DisjointSet t2)
        {
            if (t1.Count != t2.Count)
                return false;

            for (var i = 0; i < t1.Count; i++)
                if (t1.Find(i) != t2.Find(i))
                    return false;

            return true;
        }

        private void TestSerialization<T>(T typeToSerialize, Func<T, T, bool> equality = null)
        {
            if (equality == null)
                equality = (t1, t2) => t1?.Equals(t2) ?? ReferenceEquals(t2, null);

            var name = $"{typeof(T)}.bin";

            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(name, FileMode.Create, FileAccess.Write))
                formatter.Serialize(stream, typeToSerialize);

            T reSerialized;
            using (var stream = new FileStream(name, FileMode.Open, FileAccess.Read))
                reSerialized = (T)formatter.Deserialize(stream);

            Assert.True(equality(typeToSerialize, reSerialized));
        }

        [Fact]
        public void SerializeDisjointSet()
        {
            var dj = new DisjointSet(10);
            dj.MakeUnion(1, 2);
            dj.MakeUnion(2, 6);

            TestSerialization(dj, DisjointSetEquality);
        }
    }
}
