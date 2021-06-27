using System;
using GoRogue.UnitTests.Mocks;
using Xunit;

namespace GoRogue.UnitTests.Components.ParentAware
{
    public class ParentAwareComponentTests
    {
        [Fact]
        public void ParentUpdatedByComponentCollection()
        {
            var obj = new MockObjectWithComponents();
            var comp = new MockParentAwareComponentBase();

            // Should have null parent to start
            Assert.Null(comp.Parent);

            // Should be updated automatically when added to a collection
            obj.GoRogueComponents.Add(comp);
            Assert.Same(obj, comp.Parent);

            // When component is removed, the Parent should also update to null
            obj.GoRogueComponents.Remove(comp);
            Assert.Null(comp.Parent);
        }

        [Fact]
        public void AddedAndRemovedFire()
        {
            var obj = new MockObjectWithComponents();
            var comp = new MockParentAwareComponentBase();

            // Added (and only added) should fire when component is added to an object
            obj.GoRogueComponents.Add(comp);
            Assert.Equal(1, comp.TimesAddedCalled);
            Assert.Equal(0, comp.TimesRemovedCalled);

            // When removed, ONLY removed should fire
            comp.ClearHistory();
            obj.GoRogueComponents.Remove(comp);
            Assert.Equal(0, comp.TimesAddedCalled);
            Assert.Equal(1, comp.TimesRemovedCalled);
        }

        [Fact]
        public void AttachToMultipleObjectsNotAllowed()
        {
            var obj1 = new MockObjectWithComponents();
            var obj2 = new MockObjectWithComponents();
            var comp = new MockParentAwareComponentBase();

            obj1.GoRogueComponents.Add(comp);

            comp.ClearHistory();
            Assert.Throws<ArgumentException>(() => obj2.GoRogueComponents.Add(comp));
            Assert.Same(obj1, comp.Parent);
            Assert.Equal(0, comp.TimesAddedCalled);
            Assert.Equal(0, comp.TimesRemovedCalled);
        }
    }
}
