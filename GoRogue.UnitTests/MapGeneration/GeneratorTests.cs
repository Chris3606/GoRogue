using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using Xunit;
using XUnit.ValueTuples;

namespace GoRogue.UnitTests.MapGeneration
{
    public class GeneratorTests
    {
        #region Test Data
        public static IEnumerable<(int width, int height)> ValidSizes => TestUtils.Enumerable(
            (1, 2),
            (1, 1),
            (5, 6),
            (10, 20),
            (1000, 987)
        );

        public static IEnumerable<(int width, int height)> InvalidSizes => TestUtils.Enumerable(
            (0, 5),
            (4, 0),
            (0, 0),
            (-1, 6),
            (7, -1),
            (-7, 0),
            (0, -2),
            (-3, -9)
       );
        #endregion

        private readonly Generator _generator;
        private int _addedCount;
        private readonly IncrementOnlyValue _orderChecker;

        public GeneratorTests()
        {
            _generator = new Generator(10, 15);
            _addedCount = 0;
            _generator.Context.ComponentAdded += (s, e) => _addedCount++;

            _orderChecker = new IncrementOnlyValue();
        }

        [Theory]
        [MemberDataTuple(nameof(ValidSizes))]
        public void ConstructionValidSizes(int width, int height)
        {
            var generator = new Generator(width, height);
            Assert.Equal(width, generator.Context.Width);
            Assert.Equal(height, generator.Context.Height);

            Assert.NotNull(generator.GenerationSteps);
            Assert.Equal(0, generator.GenerationSteps.Count);
        }

        [Theory]
        [MemberDataTuple(nameof(InvalidSizes))]
        public void ConstructionInvalidSizes(int width, int height)
        {
            Generator? generator = null;
            Assert.Throws<ArgumentException>(() => generator = new Generator(width, height));
            Assert.Null(generator);
        }

        [Fact]
        public void AddComponentReturnsThis()
        {
            // Verify AddComponent returns its instance (for chaining)
            var ret = _generator.AddComponent(new MapContextComponent1());
            Assert.Same(_generator, ret);
        }

        [Fact]
        public void AddComponentBasic()
        {
            var component = new MapContextComponent1();
            var component2 = new MapContextComponent2();

            // Should not throw
            _generator.AddComponent(component);
            Assert.Equal(1, _addedCount);
            // Should throw exception because it's a duplicate object
            Assert.Throws<ArgumentException>(() => _generator.AddComponent(component));
            Assert.Equal(1, _addedCount);

            // Multiple objects of same type, however, are allowed
            _generator.AddComponent(component2);
            Assert.Equal(2, _addedCount);
        }

        [Fact]
        public void AddComponentTag()
        {
            var component = new MapContextComponent1();
            var component2 = new MapContextComponent1();

            // Should not throw
            _generator.AddComponent(component, "Tag1");
            Assert.Equal(1, _addedCount);

            // Should throw exception because it's a duplicate object
            Assert.Throws<ArgumentException>(() => _generator.AddComponent(component, "Tag10"));
            Assert.Equal(1, _addedCount);

            // Should throw exception because it's a duplicate tag

            Assert.Throws<ArgumentException>(() => _generator.AddComponent(component2, "Tag1"));
            Assert.Equal(1, _addedCount);

            // Duplicate types are still allowed, however
            var component3 = new MapContextComponent1();
            _generator.AddComponent(component2, "Tag2");
            Assert.Equal(2, _addedCount);
        }

        [Fact]
        public void AddStepReturnsThis()
        {
            // Verify AddStep returns its instance (for chaining)
            var ret = _generator.AddStep(new TestGenerationStep(null));
            Assert.Same(_generator, ret);
        }

        [Fact]
        public void AddStepAddsToList()
        {
            var step = new TestGenerationStep(null);

            _generator.AddStep(step);
            Assert.Equal(1, _generator.GenerationSteps.Count);
            Assert.Same(step, _generator.GenerationSteps[0]);

            var step2 = new TestGenerationStep(null);
            _generator.AddStep(step2);
            Assert.Equal(TestUtils.Enumerable(step, step2), _generator.GenerationSteps);
        }

        [Fact]
        public void GenerateCompletesStepsInOrder()
        {
            // Add steps that will trigger an exception if they're completed out of order
            _generator.AddStep(new TestGenerationStep(GetOnPerform(1)))
                .AddStep(new TestGenerationStep(GetOnPerform(2)))
                .AddStep(new TestGenerationStep(GetOnPerform(3)));

            // We directly assert that all the steps were called; IncrementOnlyValue will throw if any were executed out of order or duplicated.
            _generator.Generate();
            Assert.Equal(3, _addedCount); 
        }

        private Action GetOnPerform(int id)
        {

            return () =>
            {
                _addedCount++;
                _orderChecker.Value = id;
            };
        }
    }
}
