using System;
using System.Net.NetworkInformation;
using GoRogue.MapGeneration;
using Xunit;

namespace GoRogue.UnitTests.MapGeneration
{

    public class GenerationStepTests
    {
        private int _timesOnPerformCalled = 0;

        public GenerationStepTests()
        {
            _timesOnPerformCalled = 0; // Reset before each test
        }

        [Fact]
        public void ConstructionWithName()
        {
            var step = new TestGenerationStep(() => _timesOnPerformCalled++, name: "name");
            Assert.Equal("name", step.Name);
        }

        [Fact]
        public void ConstructionDefaultName()
        {
            // Name should be the class name
            var step = new TestGenerationStep(() => _timesOnPerformCalled++);
            Assert.Equal(nameof(TestGenerationStep), step.Name);
            Assert.Equal(0, _timesOnPerformCalled); // Does not perform the step
        }

        [Fact]
        public void PerformNoComponentRequirements()
        {
            var context = new GenerationContext(10, 15);

            // Requires no components so should not throw and should call OnPerform once
            var stepNoComponents = new TestGenerationStep(() => _timesOnPerformCalled++);
            stepNoComponents.PerformStep(context);
            Assert.Equal(1, _timesOnPerformCalled);
        }

        [Fact]
        public void PerformComponentRequirements()
        {
            var context = new GenerationContext(10, 15);

            // Requires a single component that isn't present so will throw and will not call OnPerform
            var stepSingleComponent = new TestGenerationStep(() => _timesOnPerformCalled++, null, typeof(MapContextComponent1));
            Assert.Throws<InvalidOperationException>(() => stepSingleComponent.PerformStep(context));
            Assert.Equal(0, _timesOnPerformCalled);

            // When the required component is there, it runs as normal
            context.AddComponent(new MapContextComponent1());
            stepSingleComponent.PerformStep(context);
            Assert.Equal(1, _timesOnPerformCalled);

            // Requires 2 components, one of which isn't there so will throw and not call OnPerform
            var stepMultipleComponents = new TestGenerationStep(() => _timesOnPerformCalled++, null, typeof(MapContextComponent1), typeof(MapContextComponent2));
            Assert.Throws<InvalidOperationException>(() => stepMultipleComponents.PerformStep(context));
            Assert.Equal(1, _timesOnPerformCalled);

            // Add the second component and it runs appropriately.  Components with tags count if the requirement is for no particular tag
            context.AddComponent(new MapContextComponent2(), "tag1");
            stepMultipleComponents.PerformStep(context);
            Assert.Equal(2, _timesOnPerformCalled);
        }

        [Fact]
        public void PerformWithComponentRequirementsAndTags()
        {
            var context = new GenerationContext(10, 15);

            string tag1 = "component1";
            string tag2 = "component2";
            string tag3 = "component3";

            // Add a component that has no tag
            context.AddComponent(new MapContextComponent1());

            // Requires a single component with a tag that isn't present so will throw and will not call OnPerform
            var stepSingleComponent = new TestGenerationStep(() => _timesOnPerformCalled++, null, (typeof(MapContextComponent1), tag1));
            Assert.Throws<InvalidOperationException>(() => stepSingleComponent.PerformStep(context));
            Assert.Equal(0, _timesOnPerformCalled);

            // When the required component is there, it runs as normal
            context.AddComponent(new MapContextComponent1(), tag1);
            stepSingleComponent.PerformStep(context);
            Assert.Equal(1, _timesOnPerformCalled);

            // Requires 2 components, one of which is a type/tag combo that isn't there so will throw and not call OnPerform
            var stepMultipleComponents = new TestGenerationStep(() => _timesOnPerformCalled++, null, (typeof(MapContextComponent1), tag1), (typeof(MapContextComponent2), tag2));
            Assert.Throws<InvalidOperationException>(() => stepMultipleComponents.PerformStep(context));
            Assert.Equal(1, _timesOnPerformCalled);

            // Both of these objects have the wrong tag (or no tag) so we'll again throw exception
            context.AddComponent(new MapContextComponent2(), tag3);
            context.AddComponent(new MapContextComponent2());
            Assert.Throws<InvalidOperationException>(() => stepMultipleComponents.PerformStep(context));
            Assert.Equal(1, _timesOnPerformCalled);

            // Add the second component with the proper tag and it runs appropriately.
            context.AddComponent(new MapContextComponent2(), tag2);
            stepMultipleComponents.PerformStep(context);
            Assert.Equal(2, _timesOnPerformCalled);
        }
    }
}
