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

        private MessageBus _currentGoRogueBus = null!;
        private OriginalMessageBus _originalBus = null!;
        private NoForEachMessageBus _noForeachBus = null!;
        private ExpressionMessageBus _expressionBus = null!;
        private OptimizedAndCacheSubsMessageBus _cachedSubsBus = null!;
        private OptimizedAndToArrayMessageBus _toArrayBus = null!;
        private OptimizedAndToArrayAllMessageBus _toArrayAllBus = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _currentGoRogueBus = new MessageBus();
            _originalBus = new OriginalMessageBus();
            _noForeachBus = new NoForEachMessageBus();
            _expressionBus = new ExpressionMessageBus();
            _cachedSubsBus = new OptimizedAndCacheSubsMessageBus();
            _toArrayBus = new OptimizedAndToArrayMessageBus();
            _toArrayAllBus = new OptimizedAndToArrayAllMessageBus();

            for (int i = 0; i < Subscribers; i++)
            {
                _currentGoRogueBus.RegisterSubscriber(new BenchmarkSubscriber());
                _originalBus.RegisterSubscriber(new BenchmarkSubscriber());
                _noForeachBus.RegisterSubscriber(new BenchmarkSubscriber());
                _expressionBus.RegisterSubscriber(new BenchmarkSubscriber());
                _cachedSubsBus.RegisterSubscriber(new BenchmarkSubscriber());
                _toArrayBus.RegisterSubscriber(new BenchmarkSubscriber());
                _toArrayAllBus.RegisterSubscriber(new BenchmarkSubscriber());
            }
        }

        [Benchmark]
        public void CurrentGoRogue()
        {
            for (int i = 0; i < Messages; i++)
                _currentGoRogueBus.Send(Message);
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

        [Benchmark]
        public void OptimizeCachedSubs()
        {
            for (int i = 0; i < Messages; i++)
                _cachedSubsBus.Send(Message);
        }

        [Benchmark]
        public void OptimizeToArray()
        {
            for (int i = 0; i < Messages; i++)
                _toArrayBus.Send(Message);
        }

        [Benchmark]
        public void OptimizeToArrayAll()
        {
            for (int i = 0; i < Messages; i++)
                _toArrayAllBus.Send(Message);
        }

    }
}
