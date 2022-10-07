using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Serialization.Json.Tests.IntegrationTests
{
    [TestClass]
    public class CommandsTests : AcceptanceTests.CommandsTests
    {
        protected override IMessageBus CreateBus()
        {
            return new MessageBrokerMessageBus(
                MemoryMessageBrokerBuilder.InProcessBroker().UseMessageSerialization(new JsonMessageSerializer().WithInterfaceDeserializer()),
                NoExceptionNotification.Instance);
        }
    }
}
