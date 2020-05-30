using GoRogue;
using GoRogue.MapGeneration;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Xunit;
using SadRogue.Primitives;
using System;

namespace GoRogue.UnitTests
{
    public class SerializationTests
    {
        [Fact]
        public void SerializeAdjacencyRule() => TestSerialization(AdjacencyRule.EightWay);

        private void TestSerialization(object eIGHT_WAY)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void SerializeBoundedRectangle() => TestSerialization(new BoundedRectangle((1, 2, 3, 4), (10, 10, 10, 10)));

        [Fact]
        public void SerializeCoord() => TestSerialization<Point>((1, 2));

        [Fact]
        public void SerializeDirection() => TestSerialization(Direction.DownRight);

        [Fact]
        public void SerializeDisjointSet()
        {
            var dj = new DisjointSet(10);
            dj.MakeUnion(1, 2);
            dj.MakeUnion(2, 6);

            TestSerialization(dj, DisjointSetEquality);
        }

        [Fact]
        public void TestDistance() => TestSerialization(Distance.Chebyshev);


        [Fact]
        public void TestRadius() => TestSerialization(Radius.Circle);


        [Fact]
        public void TestRectangle() => TestSerialization(new Rectangle(2, 1, 5, 8));

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
                equality = (t1, t2) => t1.Equals(t2);

            var name = $"{typeof(T)}.bin";

            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(name, FileMode.Create, FileAccess.Write))
                formatter.Serialize(stream, typeToSerialize);

            T reSerialized = default;
            using (var stream = new FileStream(name, FileMode.Open, FileAccess.Read))
                reSerialized = (T)formatter.Deserialize(stream);

            Assert.True(equality(typeToSerialize, reSerialized));
        }
    }
}
