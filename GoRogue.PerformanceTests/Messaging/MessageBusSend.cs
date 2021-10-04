using BenchmarkDotNet.Attributes;
using GoRogue.Messaging;
using JetBrains.Annotations;

namespace GoRogue.PerformanceTests.Messaging
{
    public class BenchmarkSubscriber : ISubscriber<string>
    {
        [UsedImplicitly]
        private string _mostRecentMessage = "";

        public void Handle(string message)
        {
            _mostRecentMessage = message;
        }
    }

    public class MessageBusSend
    {
        [UsedImplicitly]
        [Params(10, 100, 1000, 10000)]
        public int Messages;

        [UsedImplicitly]
        [Params(1000)]
        public int Subscribers;

        [UsedImplicitly]
        [Params("message")]
        public string Message = null!;

        private MessageBus _originalBus = null!;
        private NoForEachMessageBus _noForeachBus = null!;
        private ExpressionMessageBus _expressionBus = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _originalBus = new MessageBus();
            _noForeachBus = new NoForEachMessageBus();
            _expressionBus = new ExpressionMessageBus();

            for (int i = 0; i < Subscribers; i++)
            {
                _originalBus.RegisterSubscriber(new BenchmarkSubscriber());
                _noForeachBus.RegisterSubscriber(new BenchmarkSubscriber());
                _expressionBus.RegisterSubscriber(new BenchmarkSubscriber());
            }

        }

        [Benchmark]
        public void Original()
        {
            for (int i = 0; i < Messages; i++)
                _originalBus.Send(Message);
        }

        [Benchmark]
        public void NoForEach()
        {
            for (int i = 0; i < Messages; i++)
                _noForeachBus.Send(Message);
        }

        [Benchmark]
        public void Expression()
        {
            for (int i = 0; i < Messages; i++)
                _expressionBus.Send(Message);
        }

    }
}
