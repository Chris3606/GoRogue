using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using JetBrains.Annotations;

namespace GoRogue.PerformanceTests
{
    record MockItem(int Value);
    
    public class ListCastEnumerators
    {
        [UsedImplicitly]
        [Params(10, 25, 100, 200)]
        public int Size;

        private List<object> _list = null!;
        private IReadOnlyList<object> _readOnlyList = null!;

        [GlobalSetup]
        public void Setup()
        {
            _list = new List<object>(Size);
            _readOnlyList = _list.AsReadOnly();

            for (int i = 0; i < Size; i++)
                _list.Add(new MockItem(i));
        }

        [Benchmark]
        public int LinqIEnumerableList()
        {
            int sum = 0;
            foreach (var item in _list.Cast<MockItem>())
                sum += item.Value;
            return sum;
        }

        [Benchmark]
        public int ManualCastList()
        {
            int sum = 0;
            foreach (var item in _list)
                sum += ((MockItem)item).Value;
            return sum;
        }

        [Benchmark]
        public int ListCastEnumeratorList()
        {
            int sum = 0;
            foreach (var item in new ListCastEnumerator<object, MockItem>(_list))
                sum += item.Value;
            return sum;
        }

        [Benchmark]
        public int LinqIEnumerableReadOnlyList()
        {
            int sum = 0;
            foreach (var item in _readOnlyList.Cast<MockItem>())
                sum += item.Value;
            return sum;
        }

        [Benchmark]
        public int ManualCastReadOnlyList()
        {
            int sum = 0;
            foreach (var item in _readOnlyList)
                sum += ((MockItem)item).Value;
            return sum;
        }

        [Benchmark]
        public int ListCastEnumeratorReadOnlyList()
        {
            int sum = 0;
            foreach (var item in new ReadOnlyListCastEnumerator<object, MockItem>(_readOnlyList))
                sum += item.Value;
            return sum;
        }
    }
}
