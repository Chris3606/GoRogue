namespace GoRogue.UnitTests.Mocks
{
    internal interface IMockMessage
    { }

    internal class MockMessageBase : IMockMessage
    { }

    internal class MockMessage1 : IMockMessage
    { }

    internal class MockMessage2 : MockMessageBase
    { }
}
