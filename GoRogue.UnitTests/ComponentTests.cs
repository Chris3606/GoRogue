using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace GoRogue.UnitTests
{
    class OrderedComponent : ISortedComponent
    {
        public uint SortOrder { get; }

        public OrderedComponent(uint sortOrder)
        {
            SortOrder = sortOrder;
        }
    }

    class UnorderedComponent
    {
        public int Value;
    }

    public class ComponentTests
    {
        [Fact]
        public void OrderingOnlyOrdered()
        {
            var components = new List<OrderedComponent>();
            for (uint i = 0; i < 10; i++)
                components.Add(new OrderedComponent(i));

            components.FisherYatesShuffle();

            var container = new ComponentContainer();
            foreach (var component in components)
                container.AddComponent(component);

            components = components.OrderBy(c => c.SortOrder).ToList();
            int compIndex = 0;
            foreach (var comp in container.GetComponents<ISortedComponent>())
            {
                Assert.True(components[compIndex] == comp);
                compIndex++;
            }
        }

        [Fact]
        public void OrderingOrderedAndUnordered()
        {
            var components = new List<OrderedComponent>();
            var unorderedComponents = new List<UnorderedComponent>();
            for (uint i = 0; i < 10; i++)
            {
                components.Add(new OrderedComponent(i));
                unorderedComponents.Add(new UnorderedComponent());
            }

            components.FisherYatesShuffle();

            var container = new ComponentContainer();

            int compIndex = 0;
            for (int i = 0; i < components.Count; i++)
            {
                container.AddComponent(components[i]);
                container.AddComponent(unorderedComponents[i]);
            }

            components = components.OrderBy(c => c.SortOrder).ToList();
            compIndex = 0;

            var objects = new List<object>();
            objects.AddRange(components);
            objects.AddRange(unorderedComponents);

            foreach (var comp in container.GetComponents<object>())
            {
                Assert.True(objects[compIndex] == comp);
                compIndex++;
            }
        }
    }
}
