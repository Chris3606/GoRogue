using GoRogue;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
	interface IInterface { }

	class Base : IInterface { }

	class Derived : Base { }

	[TestClass]
	public class ReflectionTests
	{
		[TestMethod]
		public void TestGetRuntimeTree()
		{
			Base inst = new Derived();

			List<Type> types = Reflection.GetRuntimeTypeTree(inst).ToList();

			Assert.AreEqual(true, types.Contains(typeof(Base)));
			Assert.AreEqual(true, types.Contains(typeof(Derived)));
			Assert.AreEqual(true, types.Contains(typeof(IInterface)));
		}
	}
}
