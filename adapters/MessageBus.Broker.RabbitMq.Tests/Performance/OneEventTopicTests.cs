using System.Threading.Tasks;
using MessageBus.Messaging;
using MessageBus.Serialization.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

namespace MessageBus.Broker.RabbitMq.Tests.Performance
{
    [TestClass]
    public class OneEventTopicTests
    {
        public long EventsToFire { get; } = 500_000;

        private IMessageBroker CreateBroker()
        {
            ConnectionFactory connection = new ConnectionFactory()
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost"
            };
            return new RabbitMqBroker(connection)
                .UseMessageSerialization(new JsonMemoryMessageSerializer().WithInterfaceDeserializer());
        }

        [TestMethod]
        public async Task MessageBusWithOneEventTopicAndOneListener()
        {
            using var bus = new MessageBrokerMessageBus(CreateBroker(), NoExceptionNotification.Instance);

            await RunTest(bus, numberOfSubscribers: 1);
        }

        [TestMethod]
        public async Task MessageBusWithOneEventTopicAndFiveListeners()
        {
            using var bus = new MessageBrokerMessageBus(CreateBroker(), NoExceptionNotification.Instance);

            await RunTest(bus, numberOfSubscribers: 5);
        }

        [TestMethod]
        public async Task MessageBusWithOneEventTopicAndTwentyListeners()
        {
            using var bus = new MessageBrokerMessageBus(CreateBroker(), NoExceptionNotification.Instance);

            await RunTest(bus, numberOfSubscribers: 20);
        }

        private async Task RunTest(IMessageBus bus, int numberOfSubscribers)
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
                await bus.FireEvent(new TestEvent(i));
            }


            while (!counter.Wait(System.TimeSpan.FromSeconds(1)))
            {
            }
            Assert.AreEqual(EventsToFire * (long)numberOfSubscribers, counter.Value);
        }

        [Topic("event://-test/events/performancetest")]
        private class TestEvent : IMessageEvent
        {
            public TestEvent(int index)
            {
                Index = index;
            }

            public int Index { get; }

            public MessageId MessageId { get; } = MessageId.NewId();
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
                    throw new System.Exception();
                _lastId = @event.Index;
                _counter.Increment();
            }
        }
    }
}
