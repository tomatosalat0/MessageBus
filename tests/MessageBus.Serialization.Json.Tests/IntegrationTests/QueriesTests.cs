using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Serialization.Json.Tests.IntegrationTests
{
    [TestClass]
    public class QueriesTests : AcceptanceTests.QueriesTests
    {
        protected override IMessageBus CreateBus()
        {
            return new MessageBrokerMessageBus(
                MemoryMessageBrokerBuilder.InProcessBroker().UseMessageSerialization(new JsonMessageSerializer().WithInterfaceDeserializer()),
                NoExceptionNotification.Instance);
        }
    }
}
