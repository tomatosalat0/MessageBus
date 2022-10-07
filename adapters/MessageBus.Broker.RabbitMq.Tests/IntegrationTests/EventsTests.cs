using MessageBus.Messaging;
using MessageBus.Serialization.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

namespace MessageBus.Broker.RabbitMq.Tests.IntegrationTests
{
    [TestClass]
    public class EventsTests : AcceptanceTests.EventsTests
    {
        protected override IMessageBus CreateBus()
        {
            return new MessageBrokerMessageBus(CreateBroker(), NoExceptionNotification.Instance);
        }

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
    }
}
