using GoRogue;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class MathHelperTests
    {
        [TestMethod]
        public void RoundToMultiple()
        {
            int result = MathHelpers.RoundToMultiple(3, 3);
            Assert.AreEqual(3, result);

            result = MathHelpers.RoundToMultiple(2, 3);
            Assert.AreEqual(3, result);

            result = MathHelpers.RoundToMultiple(4, 3);
            Assert.AreEqual(6, result);

            result = MathHelpers.RoundToMultiple(-1, 3);
            Assert.AreEqual(0, result);
        }
    }
}
