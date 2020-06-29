using System;
using Xunit;

namespace GoRogue.UnitTests
{
    internal class ComponentBase
    { }

    internal class Component1 : ComponentBase
    { }

    internal class Component2 : ComponentBase
    { }

    internal class SortedComponent : Component1, ISortedComponent
    {
        public SortedComponent(uint sortOrder) => SortOrder = sortOrder;

        public uint SortOrder { get; }
    }


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

        [Fact]
        public void
            AddComponentBasic() // We cannot test retrieval until we test GetComponent, but we will test exception-based behaviors
        {
            var component = new Component1();
            var component2 = new Component1();

            // Should not throw
            _componentContainer.AddComponent(component);
            Assert.Equal(1, _addedCount);
            // Should throw exception because it's a duplicate object
            Assert.Throws<ArgumentException>(() => _componentContainer.AddComponent(component));
            Assert.Equal(1, _addedCount);

            // Multiple objects of same type, however, are allowed
            _componentContainer.AddComponent(component2);
            Assert.Equal(2, _addedCount);
        }

        [Fact]
        public void
            AddComponentTag() // We cannot test retrieval until we test GetComponent, but we will test exception-based behaviors
        {
            var component = new Component1();
            var component2 = new Component1();

            // Should not throw
            _componentContainer.AddComponent(component, "Tag1");
            Assert.Equal(1, _addedCount);

            // Should throw exception because it's a duplicate object
            Assert.Throws<ArgumentException>(() => _componentContainer.AddComponent(component, "Tag10"));
            Assert.Equal(1, _addedCount);

            // Should throw exception because it's a duplicate tag
            Assert.Throws<ArgumentException>(() => _componentContainer.AddComponent(component2, "Tag1"));
            Assert.Equal(1, _addedCount);

            // Duplicate types are still allowed, however
            _componentContainer.AddComponent(component2, "Tag2");
            Assert.Equal(2, _addedCount);
        }

        [Fact]
        public void GetComponentBasic()
        {
            // Create and add a component
            var component = new Component1();
            _componentContainer.AddComponent(component);

            // Component1 qualifies as both of these types, so both should return the object.
            Assert.Same(component, _componentContainer.GetComponent<Component1>());
            Assert.Same(component, _componentContainer.GetComponent<ComponentBase>());
            // Component1 is NOT this type, so this should be null
            Assert.Null(_componentContainer.GetComponent<Component2>());

            // Create and add a component of a different type
            var component2 = new Component2();
            _componentContainer.AddComponent(component2);

            // Component1 should return the same instance as before
            Assert.Same(component, _componentContainer.GetComponent<Component1>());

            var retrievedComponent = _componentContainer.GetComponent<ComponentBase>();
            // Should be one of the two, since both are the proper type.  Order is not enforced since we don't have priorities set to the components
            Assert.True(retrievedComponent == component || retrievedComponent == component2);
            // Should now return component2
            Assert.Same(component2, _componentContainer.GetComponent<Component2>());

            // Remove the original component
            _componentContainer.RemoveComponent(component);

            // Now component1 should be null, but the others should return component2
            Assert.Null(_componentContainer.GetComponent<Component1>());
            Assert.Same(component2, _componentContainer.GetComponent<ComponentBase>()); // Component2 qualifies
            Assert.Same(component2, _componentContainer.GetComponent<Component2>());
        }

        [Fact]
        public void GetComponentPriority()
        {
            var component = new SortedComponent(1);
            var component2 = new SortedComponent(2);
            var component3 = new SortedComponent(4);

            var component4 = new Component1(); // Not a sorted component

            // Ensure to add out of order
            _componentContainer.AddComponent(component2);
            _componentContainer.AddComponent(component);
            _componentContainer.AddComponent(component4);
            _componentContainer.AddComponent(component3);

            // Lowest priority of all the components of a type is always returned here
            Assert.Same(component, _componentContainer.GetComponent<ComponentBase>());
            Assert.Same(component, _componentContainer.GetComponent<Component1>());
            Assert.Same(component, _componentContainer.GetComponent<SortedComponent>());

            // In order of priority, with objects that have no priority at the end
            Assert.Equal(TestUtils.Enumerable(component, component2, component3, component4),
                _componentContainer.GetComponents<Component1>());
            Assert.Equal(TestUtils.Enumerable(component, component2, component3, component4),
                _componentContainer.GetComponents<ComponentBase>());

            // Same difference but we omit the Component1 that does not meet the type requirement
            Assert.Equal(TestUtils.Enumerable(component, component2, component3),
                _componentContainer.GetComponents<SortedComponent>());
        }

        [Fact]
        public void GetComponentsPriority()
        {
            var component = new SortedComponent(1);
            var component2 = new SortedComponent(2);
            var component3 = new SortedComponent(4);

            var component4 = new Component1(); // Not a sorted component

            // Ensure to add out of order
            _componentContainer.AddComponent(component2);
            _componentContainer.AddComponent(component);
            _componentContainer.AddComponent(component4);
            _componentContainer.AddComponent(component3);

            // Returned in order of priority, with objects that have no priority at the end
            Assert.Equal(TestUtils.Enumerable(component, component2, component3, component4),
                _componentContainer.GetComponents<Component1>());
            Assert.Equal(TestUtils.Enumerable(component, component2, component3, component4),
                _componentContainer.GetComponents<ComponentBase>());

            // Same difference but we omit the Component1 that does not meet the type requirement
            Assert.Equal(TestUtils.Enumerable(component, component2, component3),
                _componentContainer.GetComponents<SortedComponent>());
        }

        [Fact]
        public void GetComponentTag()
        {
            string tag1 = "Component1";
            string tag2 = "NotComponent1";

            // Create and add a component
            var component = new Component1();
            _componentContainer.AddComponent(component, tag1);

            // Component1 qualifies as both of these types, so both should return the component we added when we're not looking for tags.
            Assert.Same(component, _componentContainer.GetComponent<Component1>());
            Assert.Same(component, _componentContainer.GetComponent<ComponentBase>());
            // Component1 is NOT this type, so this is null
            Assert.Null(_componentContainer.GetComponent<Component2>());
            // Should return the component because the component has the tag we're looking for
            Assert.Same(component, _componentContainer.GetComponent<Component1>(tag1));
            Assert.Same(component, _componentContainer.GetComponent<ComponentBase>(tag1));
            // Null because the object with the tag doesn't match the type requirement
            Assert.Null(_componentContainer.GetComponent<Component2>(tag1));
            // False because the object with the correct type doesn't have the tag
            Assert.Null(_componentContainer.GetComponent<Component1>(tag2));

            // Create and add a component of a different type, with no tag
            var component2 = new Component2();
            _componentContainer.AddComponent(component2);

            // Null because the object we added has no tag
            Assert.Null(_componentContainer.GetComponent<Component2>(tag2));
        }

        [Fact]
        public void HasComponentBasic()
        {
            // Create and add a component
            var component = new Component1();
            _componentContainer.AddComponent(component);

            // Component1 qualifies as both of these types, so both should be true.
            Assert.True(_componentContainer.HasComponent<Component1>());
            Assert.True(_componentContainer.HasComponent<ComponentBase>());
            // Component1 is NOT this type, so this is false
            Assert.False(_componentContainer.HasComponent<Component2>());

            // Create and add a component of a different type
            var component2 = new Component2();
            _componentContainer.AddComponent(component2);

            // Now all 3 should be true
            Assert.True(_componentContainer.HasComponent<Component1>());
            Assert.True(_componentContainer.HasComponent<ComponentBase>());
            Assert.True(_componentContainer.HasComponent<Component2>());

            _componentContainer.RemoveComponent(component);

            Assert.False(_componentContainer.HasComponent<Component1>());
            Assert.True(_componentContainer.HasComponent<ComponentBase>()); // Component2 qualifies
            Assert.True(_componentContainer.HasComponent<Component2>());
        }

        [Fact]
        public void HasComponentTag()
        {
            string tag1 = "Component1";
            string tag2 = "NotComponent1";

            // Create and add a component
            var component = new Component1();
            _componentContainer.AddComponent(component, tag1);

            // Component1 qualifies as both of these types, so both should be true when we're not looking for tags.
            Assert.True(_componentContainer.HasComponent<Component1>());
            Assert.True(_componentContainer.HasComponent<ComponentBase>());
            // Component1 is NOT this type, so this is false
            Assert.False(_componentContainer.HasComponent<Component2>());
            // Should be true because the component has the tag we're looking for
            Assert.True(_componentContainer.HasComponent<Component1>(tag1));
            Assert.True(_componentContainer.HasComponent<ComponentBase>(tag1));
            // False because the object with the tag doesn't match the type requirement
            Assert.False(_componentContainer.HasComponent<Component2>(tag1));
            // False because the object with the correct type doesn't have the tag
            Assert.False(_componentContainer.HasComponent<Component1>(tag2));

            // Create and add a component of a different type, with no tag
            var component2 = new Component2();
            _componentContainer.AddComponent(component2);

            // False because the object we added has no tag
            Assert.False(_componentContainer.HasComponent<Component2>(tag2));
        }

        [Fact]
        public void RemoveComponentBasic()
        {
            var component = new Component1();
            var component2 = new Component2();

            // Ensure _addedCount increments so we're actually testing on decrement
            _componentContainer.AddComponent(component);
            _componentContainer.AddComponent(component2);
            Assert.Equal(2, _addedCount);

            _componentContainer.RemoveComponent(component);
            Assert.Equal(1, _addedCount);
            // Should throw because object isn't in the component list
            Assert.Throws<ArgumentException>(() => _componentContainer.RemoveComponent(component));
            Assert.Equal(1, _addedCount);

            _componentContainer.RemoveComponent(component2);
            Assert.Equal(0, _addedCount);
        }

        [Fact]
        public void RemoveComponentsBasic()
        {
            var component = new Component1();
            var component2 = new Component2();
            var component3 = new Component1();

            // Ensure _addedCount increments so we're actually testing on decrement
            _componentContainer.AddComponent(component);
            _componentContainer.AddComponent(component2);
            _componentContainer.AddComponent(component3);
            Assert.Equal(3, _addedCount);

            _componentContainer.RemoveComponents(component, component3);
            Assert.Equal(1, _addedCount);

            // Should throw because one of the components doesn't exist in the list
            Assert.Throws<ArgumentException>(() => _componentContainer.RemoveComponents(component2, component));

            // But, the other one should have been removed nonetheless (since it was in the list before the one that was not present)
            Assert.Equal(0, _addedCount);
        }

        [Fact]
        public void RemoveComponentsTags()
        {
            var component = new Component1();
            var component2 = new Component2();
            var component3 = new Component1();

            // Ensure _addedCount increments so we're actually testing on decrement
            _componentContainer.AddComponent(component, "tag1");
            _componentContainer.AddComponent(component2, "tag2");
            _componentContainer.AddComponent(component3, "tag3");
            Assert.Equal(3, _addedCount);

            _componentContainer.RemoveComponents("tag1", "tag3");
            Assert.Equal(1, _addedCount);

            // Should throw because one of the tags doesn't exist in the list
            Assert.Throws<ArgumentException>(() => _componentContainer.RemoveComponents("tag2", "tag3"));

            // But, the other one should have been removed nonetheless (since it was in the list before the one that was not present)
            Assert.Equal(0, _addedCount);
        }

        [Fact]
        public void RemoveComponentTag()
        {
            var component = new Component1();
            var component2 = new Component2();
            var component3 = new Component2();

            // Ensure _addedCount increments so we're actually testing on decrement
            _componentContainer.AddComponent(component, "tag1");
            _componentContainer.AddComponent(component2);
            _componentContainer.AddComponent(component3, "tag3");
            Assert.Equal(3, _addedCount);


            // Remove should remove component
            _componentContainer.RemoveComponent("tag1");
            Assert.Equal(2, _addedCount);
            Assert.Null(_componentContainer.GetComponent<Component1>());

            // Should throw because no such tag exists after the previous remove
            Assert.Throws<ArgumentException>(() => _componentContainer.RemoveComponent("tag1"));

            // Tag exists so we just leave the one without tag
            _componentContainer.RemoveComponent("tag3");
            Assert.Equal(1, _addedCount);
        }
    }
}
