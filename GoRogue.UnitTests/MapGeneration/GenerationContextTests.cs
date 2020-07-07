using System;
using GoRogue.MapGeneration;
using GoRogue.UnitTests.Mocks;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.MapGeneration
{
    public class GenerationContextTests
    {
        public GenerationContextTests() => _timesNewCalled = 0;

        public static (int width, int height)[] ValidSizes = { (1, 2), (1, 1), (5, 6), (10, 20), (1000, 987) };

        public static (int width, int height)[] InvalidSizes =
        {
            (0, 5), (4, 0), (0, 0), (-1, 6), (7, -1), (-7, 0), (0, -2), (-3, -9)
        };

        private int _timesNewCalled;

        [Theory]
        [MemberDataTuple(nameof(ValidSizes))]
        public void ConstructionValidSizes(int width, int height)
        {
            var context = new GenerationContext(width, height);
            Assert.Equal(width, context.Width);
            Assert.Equal(height, context.Height);
        }

        [Theory]
        [MemberDataTuple(nameof(InvalidSizes))]
        public void ConstructionInvalidSizes(int width, int height)
        {
            GenerationContext? context = null;
            Assert.Throws<ArgumentException>(() => context = new GenerationContext(width, height));
            Assert.Null(context);
        }

        [Fact]
        public void GetComponentOrNewWithExistingComponent()
        {
            var context = new GenerationContext(10, 15);
            var component = new MapContextComponent1();
            context.Add(component); // Inherited so tested in the Component system.

            // Existing component should prevent creation
            var component2 = context.GetComponentOrNew(CreateFunc);
            Assert.Equal(0, _timesNewCalled);
            Assert.Same(component, component2);

            var component3 = context.GetComponentOrNew<IMapContextComponent>(CreateFunc);
            Assert.Equal(0, _timesNewCalled);
            Assert.Same(component, component3);
        }

        [Fact]
        public void GetComponentOrNewWithNewComponent()
        {
            var context = new GenerationContext(10, 15);

            // No existing instance so it should be created
            MapContextComponent1 component = context.GetComponentOrNew(CreateFunc);
            Assert.Equal(1, _timesNewCalled);
            Assert.NotNull(component);

            // Existing instance should prevent creation
            MapContextComponent1 component2 = context.GetComponentOrNew(CreateFunc);
            Assert.Equal(1, _timesNewCalled);
            Assert.Same(component, component2);

            // No instance with the tag specified exists, so should create one
            component2 = context.GetComponentOrNew(CreateFunc, "tag1");
            Assert.Equal(2, _timesNewCalled);
            Assert.NotSame(component, component2);

            // Existing instance should prevent creation
            MapContextComponent1 component3 = context.GetComponentOrNew(CreateFunc, "tag1");
            Assert.Equal(2, _timesNewCalled);
            Assert.Same(component2, component3);

            // This call can return either since no tag was specified and no priorities were used.
            component3 = context.GetComponentOrNew(CreateFunc);
            Assert.Equal(2, _timesNewCalled);
            Assert.True(component3 == component2 || component3 == component);
        }

        private MapContextComponent1 CreateFunc()
        {
            _timesNewCalled++;
            return new MapContextComponent1();
        }
    }
}
