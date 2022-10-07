using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.IntegrationTests
{
    [TestClass]
    public class EventsTests : AcceptanceTests.EventsTests
    {
        protected override IMessageBus CreateBus()
        {
            return new MessageBrokerMessageBus(
                MemoryMessageBrokerBuilder.InProcessBroker(),
                NoExceptionNotification.Instance);
        }
    }
}
