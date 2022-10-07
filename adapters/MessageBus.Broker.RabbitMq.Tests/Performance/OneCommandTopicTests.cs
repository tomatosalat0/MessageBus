using System.Collections.Generic;
using System.Threading.Tasks;
using MessageBus.Messaging;
using MessageBus.Serialization.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

namespace MessageBus.Broker.RabbitMq.Tests.Performance
{
    [TestClass]
    public class OneCommandTopicTests
    {
        public long CommandsToFire { get; } = 500_000;

        private IMessageBroker CreateBroker()
        {
            ConnectionFactory connection = new ConnectionFactory()
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost"
            };
            return new RabbitMqBroker(connection)
                .UseMessageSerialization(new JsonMessageSerializer().WithInterfaceDeserializer());
        }

        [TestMethod]
        public async Task MessageBusWithOneCommandTopicAndOneListener()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(CreateBroker(), NoExceptionNotification.Instance);

            await RunTest(bus, numberOfSubscribers: 1);
        }

        [TestMethod]
        public async Task MessageBusWithOneCommandTopicAndFiveListeners()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(CreateBroker(), NoExceptionNotification.Instance);

            await RunTest(bus, numberOfSubscribers: 5);
        }

        private async Task RunTest(IMessageBus bus, int numberOfSubscribers)
        {
            using Counter counter = new Counter(CommandsToFire);
            for (int i = 0; i < numberOfSubscribers; i++)
            {
                bus.RegisterCommandHandler(new TestCommandHandler(counter));
            }

            Task t = Task.Run(async () =>
            {
                for (int i = 0; i < CommandsToFire; i++)
                {
                    await bus.FireCommand(new TestCommand());
                }
            });

            while (!counter.Wait(System.TimeSpan.FromSeconds(1)))
            {
            }
            await t;
            Assert.AreEqual(CommandsToFire, counter.Value);
        }

        [Topic("command://-test/commands/performancetest")]
        [TopicOptions(typeof(TopicOptionsProvider))]
        private class TestCommand : IMessageCommand
        {
            public MessageId MessageId { get; } = MessageId.NewId();

            private class TopicOptionsProvider
            {
                public static ISubscriptionOptions GetTopicOptions()
                {
                    return new Options();
                }

                private class Options : ISubscriptionOptions
                {
                    public IReadOnlyDictionary<string, object> Attributes { get; } = new Dictionary<string, object>()
                    {
                        ["-rabbitmq-qos-prefetchcount"] = 100
                    };
                }
            }
        }

        private class TestCommandHandler : IMessageCommandHandler<TestCommand>
        {
            private readonly Counter _counter;

            public TestCommandHandler(Counter counter)
            {
                _counter = counter;
            }

            public void Handle(TestCommand command)
            {
                _counter.Increment();
            }
        }
    }
}
