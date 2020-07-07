using System;
using System.Linq;
using GoRogue.UnitTests.Mocks;
using Xunit;

namespace GoRogue.UnitTests
{
    public class ComponentContainerTests
    {
        public ComponentContainerTests()
        {
            _addedCount = 0;
            _componentContainer = new ComponentContainer();
            _componentContainer.ComponentAdded += (s, e) => _addedCount++;
            _componentContainer.ComponentRemoved += (s, e) => _addedCount--;
        }

        private readonly ComponentContainer _componentContainer;
        private int _addedCount;

        // We cannot test retrieval until we test GetComponent, but we will test exception-based behaviors
        [Fact]
        public void AddBasic()
        {
            var component = new Component1();
            var component2 = new Component1();

            // Should not throw
            _componentContainer.Add(component);
            Assert.Equal(1, _addedCount);

            // Should throw exception because it's a duplicate object
            Assert.Throws<ArgumentException>(() => _componentContainer.Add(component));
            Assert.Equal(1, _addedCount);

            // Multiple objects of same type, however, are allowed
            _componentContainer.Add(component2);
            Assert.Equal(2, _addedCount);
        }

        // We cannot test retrieval until we test GetComponent, but we will test exception-based behaviors
        [Fact]
        public void AddTag()
        {
            var component = new Component1();
            var component2 = new Component1();

            // Should not throw
            _componentContainer.Add(component, "Tag1");
            Assert.Equal(1, _addedCount);

            // Should throw exception because it's a duplicate object
            Assert.Throws<ArgumentException>(() => _componentContainer.Add(component, "Tag10"));
            Assert.Equal(1, _addedCount);

            // Should throw exception because it's a duplicate tag
            Assert.Throws<ArgumentException>(() => _componentContainer.Add(component2, "Tag1"));
            Assert.Equal(1, _addedCount);

            // Duplicate types are still allowed, however
            _componentContainer.Add(component2, "Tag2");
            Assert.Equal(2, _addedCount);
        }

        [Fact]
        public void ConstructionWithInitializerList()
        {
            var container = new ComponentContainer
            {
                new Component1(),
                new Component2(),
                { new Component2(), "TaggedComponent" }
            };

            Assert.Equal(3, container.Count());
            Assert.NotNull(container.GetFirstOrDefault<Component2>("TaggedComponent"));
        }

        [Fact]
        public void GetFirstBasic()
        {
            // Create and add a component
            var component = new Component1();
            _componentContainer.Add(component);

            // Component1 qualifies as both of these types, so both should return the object.
            Assert.Same(component, _componentContainer.GetFirst<Component1>());
            Assert.Same(component, _componentContainer.GetFirst<ComponentBase>());

            // Component1 is NOT this type, so this should throw
            Assert.Throws<InvalidOperationException>(() => _componentContainer.GetFirst<Component2>());

            // Create and add a component of a different type
            var component2 = new Component2();
            _componentContainer.Add(component2);

            // Component1 should return the same instance as before
            Assert.Same(component, _componentContainer.GetFirst<Component1>());

            var retrievedComponent = _componentContainer.GetFirst<ComponentBase>();

            // Should be one of the two, since both are the proper type.  Order is not enforced since we don't have priorities set to the components
            Assert.True(retrievedComponent == component || retrievedComponent == component2);

            // Should now return component2
            Assert.Same(component2, _componentContainer.GetFirst<Component2>());

            // Remove the original component
            _componentContainer.Remove(component);

            // Now component1 should throw because there's no more component1 instances in the container,
            // but the others should return component2
            Assert.Throws<InvalidOperationException>(() => _componentContainer.GetFirst<Component1>());
            Assert.Same(component2, _componentContainer.GetFirst<ComponentBase>()); // Component2 qualifies
            Assert.Same(component2, _componentContainer.GetFirst<Component2>());
        }

        [Fact]
        public void GetFirstOrDefaultBasic()
        {
            // Create and add a component
            var component = new Component1();
            _componentContainer.Add(component);

            // Component1 qualifies as both of these types, so both should return the object.
            Assert.Same(component, _componentContainer.GetFirstOrDefault<Component1>());
            Assert.Same(component, _componentContainer.GetFirstOrDefault<ComponentBase>());

            // Component1 is NOT this type, so this should be null
            Assert.Null(_componentContainer.GetFirstOrDefault<Component2>());

            // Create and add a component of a different type
            var component2 = new Component2();
            _componentContainer.Add(component2);

            // Component1 should return the same instance as before
            Assert.Same(component, _componentContainer.GetFirstOrDefault<Component1>());

            var retrievedComponent = _componentContainer.GetFirstOrDefault<ComponentBase>();

            // Should be one of the two, since both are the proper type.  Order is not enforced since we don't have priorities set to the components
            Assert.True(retrievedComponent == component || retrievedComponent == component2);

            // Should now return component2
            Assert.Same(component2, _componentContainer.GetFirstOrDefault<Component2>());

            // Remove the original component
            _componentContainer.Remove(component);

            // Now component1 should be null, but the others should return component2
            Assert.Null(_componentContainer.GetFirstOrDefault<Component1>());
            Assert.Same(component2, _componentContainer.GetFirstOrDefault<ComponentBase>()); // Component2 qualifies
            Assert.Same(component2, _componentContainer.GetFirstOrDefault<Component2>());
        }

        [Fact]
        public void GetFirstPriority()
        {
            var component = new SortedComponent(1);
            var component2 = new SortedComponent(2);
            var component3 = new SortedComponent(4);

            var component4 = new Component1(); // Not a sorted component

            // Ensure to add out of order
            _componentContainer.Add(component2);
            _componentContainer.Add(component);
            _componentContainer.Add(component4);
            _componentContainer.Add(component3);

            // Lowest priority of all the components of a type is always returned here
            Assert.Same(component, _componentContainer.GetFirst<ComponentBase>());
            Assert.Same(component, _componentContainer.GetFirst<Component1>());
            Assert.Same(component, _componentContainer.GetFirst<SortedComponent>());
        }

        [Fact]
        public void GetFirstOrDefaultPriority()
        {
            var component = new SortedComponent(1);
            var component2 = new SortedComponent(2);
            var component3 = new SortedComponent(4);

            var component4 = new Component1(); // Not a sorted component

            // Ensure to add out of order
            _componentContainer.Add(component2);
            _componentContainer.Add(component);
            _componentContainer.Add(component4);
            _componentContainer.Add(component3);

            // Lowest priority of all the components of a type is always returned here
            Assert.Same(component, _componentContainer.GetFirstOrDefault<ComponentBase>());
            Assert.Same(component, _componentContainer.GetFirstOrDefault<Component1>());
            Assert.Same(component, _componentContainer.GetFirstOrDefault<SortedComponent>());
        }

        [Fact]
        public void GetAllPriority()
        {
            var component = new SortedComponent(1);
            var component2 = new SortedComponent(2);
            var component3 = new SortedComponent(4);

            var component4 = new Component1(); // Not a sorted component

            // Ensure to add out of order
            _componentContainer.Add(component2);
            _componentContainer.Add(component);
            _componentContainer.Add(component4);
            _componentContainer.Add(component3);

            // Returned in order of priority, with objects that have no priority at the end
            Assert.Equal(TestUtils.Enumerable(component, component2, component3, component4),
                _componentContainer.GetAll<Component1>());
            Assert.Equal(TestUtils.Enumerable(component, component2, component3, component4),
                _componentContainer.GetAll<ComponentBase>());

            // Same difference but we omit the Component1 that does not meet the type requirement
            Assert.Equal(TestUtils.Enumerable(component, component2, component3),
                _componentContainer.GetAll<SortedComponent>());
        }

        [Fact]
        public void GetFirstOrDefaultTag()
        {
            string tag1 = "Component1";
            string tag2 = "NotComponent1";

            // Create and add a component
            var component = new Component1();
            _componentContainer.Add(component, tag1);

            // Component1 qualifies as both of these types, so both should return the component we added when we're not looking for tags.
            Assert.Same(component, _componentContainer.GetFirstOrDefault<Component1>());
            Assert.Same(component, _componentContainer.GetFirstOrDefault<ComponentBase>());

            // Component1 is NOT this type, so this is null
            Assert.Null(_componentContainer.GetFirstOrDefault<Component2>());

            // Should return the component because the component has the tag we're looking for
            Assert.Same(component, _componentContainer.GetFirstOrDefault<Component1>(tag1));
            Assert.Same(component, _componentContainer.GetFirstOrDefault<ComponentBase>(tag1));

            // Null because the object with the tag doesn't match the type requirement
            Assert.Null(_componentContainer.GetFirstOrDefault<Component2>(tag1));

            // False because the object with the correct type doesn't have the tag
            Assert.Null(_componentContainer.GetFirstOrDefault<Component1>(tag2));

            // Create and add a component of a different type, with no tag
            var component2 = new Component2();
            _componentContainer.Add(component2);

            // Null because the object we added has no tag
            Assert.Null(_componentContainer.GetFirstOrDefault<Component2>(tag2));
        }

        [Fact]
        public void ContainsBasic()
        {
            // Create and add a component
            var component = new Component1();
            _componentContainer.Add(component);

            // Component1 qualifies as both of these types, so both should be true.
            Assert.True(_componentContainer.Contains<Component1>());
            Assert.True(_componentContainer.Contains<ComponentBase>());

            // Component1 is NOT this type, so this is false
            Assert.False(_componentContainer.Contains<Component2>());

            // Create and add a component of a different type
            var component2 = new Component2();
            _componentContainer.Add(component2);

            // Now all 3 should be true
            Assert.True(_componentContainer.Contains<Component1>());
            Assert.True(_componentContainer.Contains<ComponentBase>());
            Assert.True(_componentContainer.Contains<Component2>());

            _componentContainer.Remove(component);

            Assert.False(_componentContainer.Contains<Component1>());
            Assert.True(_componentContainer.Contains<ComponentBase>()); // Component2 qualifies
            Assert.True(_componentContainer.Contains<Component2>());
        }

        [Fact]
        public void ContainsTag()
        {
            string tag1 = "Component1";
            string tag2 = "NotComponent1";

            // Create and add a component
            var component = new Component1();
            _componentContainer.Add(component, tag1);

            // Component1 qualifies as both of these types, so both should be true when we're not looking for tags.
            Assert.True(_componentContainer.Contains<Component1>());
            Assert.True(_componentContainer.Contains<ComponentBase>());

            // Component1 is NOT this type, so this is false
            Assert.False(_componentContainer.Contains<Component2>());

            // Should be true because the component has the tag we're looking for
            Assert.True(_componentContainer.Contains<Component1>(tag1));
            Assert.True(_componentContainer.Contains<ComponentBase>(tag1));

            // False because the object with the tag doesn't match the type requirement
            Assert.False(_componentContainer.Contains<Component2>(tag1));

            // False because the object with the correct type doesn't have the tag
            Assert.False(_componentContainer.Contains<Component1>(tag2));

            // Create and add a component of a different type, with no tag
            var component2 = new Component2();
            _componentContainer.Add(component2);

            // False because the object we added has no tag
            Assert.False(_componentContainer.Contains<Component2>(tag2));
        }

        [Fact]
        public void RemoveBasic()
        {
            var component = new Component1();
            var component2 = new Component2();

            // Ensure _addedCount increments so we're actually testing on decrement
            _componentContainer.Add(component);
            _componentContainer.Add(component2);
            Assert.Equal(2, _addedCount);

            _componentContainer.Remove(component);
            Assert.Equal(1, _addedCount);

            // Should throw because object isn't in the component list
            Assert.Throws<ArgumentException>(() => _componentContainer.Remove(component));
            Assert.Equal(1, _addedCount);

            _componentContainer.Remove(component2);
            Assert.Equal(0, _addedCount);
        }

        [Fact]
        public void RemoveMultipleBasic()
        {
            var component = new Component1();
            var component2 = new Component2();
            var component3 = new Component1();

            // Ensure _addedCount increments so we're actually testing on decrement
            _componentContainer.Add(component);
            _componentContainer.Add(component2);
            _componentContainer.Add(component3);
            Assert.Equal(3, _addedCount);

            _componentContainer.Remove(component, component3);
            Assert.Equal(1, _addedCount);

            // Should throw because one of the components doesn't exist in the list
            Assert.Throws<ArgumentException>(() => _componentContainer.Remove(component2, component));

            // But, the other one should have been removed nonetheless (since it was in the list before the one that was not present)
            Assert.Equal(0, _addedCount);
        }

        [Fact]
        public void RemoveMultipleTags()
        {
            var component = new Component1();
            var component2 = new Component2();
            var component3 = new Component1();

            // Ensure _addedCount increments so we're actually testing on decrement
            _componentContainer.Add(component, "tag1");
            _componentContainer.Add(component2, "tag2");
            _componentContainer.Add(component3, "tag3");
            Assert.Equal(3, _addedCount);

            _componentContainer.Remove("tag1", "tag3");
            Assert.Equal(1, _addedCount);

            // Should throw because one of the tags doesn't exist in the list
            Assert.Throws<ArgumentException>(() => _componentContainer.Remove("tag2", "tag3"));

            // But, the other one should have been removed nonetheless (since it was in the list before the one that was not present)
            Assert.Equal(0, _addedCount);
        }

        [Fact]
        public void RemoveTag()
        {
            var component = new Component1();
            var component2 = new Component2();
            var component3 = new Component2();

            // Ensure _addedCount increments so we're actually testing on decrement
            _componentContainer.Add(component, "tag1");
            _componentContainer.Add(component2);
            _componentContainer.Add(component3, "tag3");
            Assert.Equal(3, _addedCount);


            // Remove should remove component
            _componentContainer.Remove("tag1");
            Assert.Equal(2, _addedCount);
            Assert.Null(_componentContainer.GetFirstOrDefault<Component1>());

            // Should throw because no such tag exists after the previous remove
            Assert.Throws<ArgumentException>(() => _componentContainer.Remove("tag1"));

            // Tag exists so we just leave the one without tag
            _componentContainer.Remove("tag3");
            Assert.Equal(1, _addedCount);
        }
    }
}
