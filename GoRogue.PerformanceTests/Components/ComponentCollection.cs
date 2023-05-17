using BenchmarkDotNet.Attributes;
using GoRogue.Components;
using JetBrains.Annotations;

namespace GoRogue.PerformanceTests.Components
{
    public interface IMockComponent
    {
        public int Value { get; }
    }

    public class MockComponentA : IMockComponent
    {
        public int Value { get; init; }
    }

    public class MockComponentB : IMockComponent
    {
        public int Value { get; init; }
    }
    public class MockComponentC : IMockComponent
    {
        public int Value { get; init; }
    }

    /// <summary>
    /// Tests the performance of the retrieve/contains type functions of ComponentCollection
    /// </summary>
    public class ComponentCollectionGet
    {
        [UsedImplicitly]
        //[Params(1, 2, 3)]
        [Params(1, 2)]
        public int TypesOfComponents;

        [UsedImplicitly]
        //[Params(2, 10, 100
        [Params(2, 10)]
        public int ComponentsPerType;

        private ComponentCollection _collection = null!;
        private IComponentCollection _iCollection = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _collection = new ComponentCollection();
            _iCollection = _collection;

            for (int i = 0; i < ComponentsPerType; i++)
                _collection.Add(new MockComponentA{ Value=i });

            if (TypesOfComponents >= 2)
                for (int i = 0; i < ComponentsPerType; i++)
                    _collection.Add(new MockComponentB{ Value = i });

            if (TypesOfComponents >= 3)
                for (int i = 0; i < ComponentsPerType; i++)
                    _collection.Add(new MockComponentC{ Value = i });
        }

        //[Benchmark]
        //public bool ContainsMockA() => _collection.Contains<MockComponentA>();

        //[Benchmark]
        //public bool ContainsIMock() => _collection.Contains<IMockComponent>();

        [Benchmark]
        public MockComponentA GetFirstMockA() => _collection.GetFirst<MockComponentA>();

        [Benchmark]
        public MockComponentA GetFirstMockAViaInterface() => _iCollection.GetFirst<MockComponentA>();

        [Benchmark]
        public IMockComponent GetFirstIMock() => _collection.GetFirst<IMockComponent>();

        [Benchmark]
        public IMockComponent GetFirstIMockViaInterface() => _iCollection.GetFirst<IMockComponent>();

        //[Benchmark]
        //public MockComponentA? GetFirstOrDefaultMockA() => _collection.GetFirstOrDefault<MockComponentA>();

        //[Benchmark]
        //public MockComponentA? GetFirstOrDefaultMockAViaInterface() => _iCollection.GetFirstOrDefault<MockComponentA>();

        //[Benchmark]
        //public IMockComponent? GetFirstOrDefaultIMock() => _collection.GetFirstOrDefault<IMockComponent>();

        //[Benchmark]
        //public IMockComponent? GetFirstOrDefaultIMockViaInterface() => _iCollection.GetFirstOrDefault<IMockComponent>();

        //[Benchmark]
        //public int GetAllMockA()
        //{
        //    int sum = 0;
        //    foreach (var component in _collection.GetAll<MockComponentA>())
        //        sum += component.Value;

        //    return sum;
        //}

        //[Benchmark]
        //public int GetAllMockAViaInterface()
        //{
        //    int sum = 0;
        //    foreach (var component in _iCollection.GetAll<MockComponentA>())
        //        sum += component.Value;

        //    return sum;
        //}

        //[Benchmark]
        //public int GetAllIMock()
        //{
        //    int sum = 0;
        //    foreach (var component in _collection.GetAll<IMockComponent>())
        //        sum += component.Value;

        //    return sum;
        //}

        //[Benchmark]
        //public int GetAllIMockViaInterface()
        //{
        //    int sum = 0;
        //    foreach (var component in _iCollection.GetAll<IMockComponent>())
        //        sum += component.Value;

        //    return sum;
        //}
    }
}
