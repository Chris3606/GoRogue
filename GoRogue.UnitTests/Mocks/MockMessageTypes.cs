using System;
using System.Collections.Generic;
using System.Text;

namespace GoRogue.UnitTests.Mocks
{
    interface IMockMessage { }

    class MockMessageBase : IMockMessage { }

    class MockMessage1 : IMockMessage { }
    class MockMessage2 : MockMessageBase { }
}
