using System.Threading;
using System;
using MessageBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json.Serialization;

namespace MessageBus.Serialization.Json.Tests.Performance
{
    [TestClass]
    public class PerformanceTests
    {
        public long EventsToFire { get; } = 500_000;

        private IMessageBroker CreateBroker()
        {
            return MemoryMessageBrokerBuilder.InProcessBroker()
                .UseMessageSerialization(new JsonMessageSerializer());
        }

        [TestMethod]
        public void MessageBusWithOneEventTopicAndOneListener()
        {
            using var bus = new MessageBrokerMessageBus(CreateBroker(), NoExceptionNotification.Instance);

            RunTest(bus, numberOfSubscribers: 1);
        }

        [TestMethod]
        public void MessageBusWithOneEventTopicAndFiveListeners()
        {
            using var bus = new MessageBrokerMessageBus(CreateBroker(), NoExceptionNotification.Instance);

            RunTest(bus, numberOfSubscribers: 5);
        }

        [TestMethod]
        public void MessageBusWithOneEventTopicAndHundredListeners()
        {
            using var bus = new MessageBrokerMessageBus(CreateBroker(), NoExceptionNotification.Instance);

            RunTest(bus, numberOfSubscribers: 100);
        }

        private void RunTest(IMessageBus bus, int numberOfSubscribers)
        {
            using Counter counter = new Counter(EventsToFire * numberOfSubscribers);

            if (numberOfSubscribers > 1)
            {
                for (int i = 0; i < numberOfSubscribers; i++)
                {
                    bus.RegisterEventHandler(new TestEventHandler(counter));
                }
            }
            else
                bus.RegisterEventHandler(new VerifyTestEventHandler(counter));

            for (int i = 0; i < EventsToFire; i++)
            {
                bus.FireEvent(new TestEvent(i));
            }


            while (!counter.Wait(TimeSpan.FromSeconds(1)))
            {
            }
            Assert.AreEqual(EventsToFire * numberOfSubscribers, counter.Value);
        }

        [Topic("Events/Test")]
        private class TestEvent : IMessageEvent
        {
            public TestEvent(int index)
            {
                Index = index;
            }

            [JsonInclude]
            public int Index { get; private init; }

            [JsonInclude]
            public MessageId MessageId { get; private init; } = MessageId.NewId();
        }

        private class TestEventHandler : IMessageEventHandler<TestEvent>
        {
            private readonly Counter _counter;

            public TestEventHandler(Counter counter)
            {
                _counter = counter;
            }

            public void Handle(TestEvent @event)
            {
                _counter.Increment();
            }
        }

        private class VerifyTestEventHandler : IMessageEventHandler<TestEvent>
        {
            private readonly Counter _counter;
            private int _lastId = -1;

            public VerifyTestEventHandler(Counter counter)
            {
                _counter = counter;
            }

            public void Handle(TestEvent @event)
            {
                if (_lastId != @event.Index - 1)
                    throw new Exception();
                _lastId = @event.Index;
                _counter.Increment();
            }
        }
    }

    internal class Counter : IDisposable
    {
        private readonly ManualResetEventSlim _event = new ManualResetEventSlim();
        private readonly long _expectedCount;
        private long _value;
        private bool disposedValue;

        public Counter(long expectedCount)
        {
            _expectedCount = expectedCount;
        }

        public long Value => _value;

        public void Increment()
        {
            long newValue = Interlocked.Increment(ref _value);
            if (newValue == _expectedCount)
                _event.Set();
        }

        public bool Wait(TimeSpan timeout)
        {
            if (_value == _expectedCount)
                return true;
            return _event.Wait(timeout);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _event.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
