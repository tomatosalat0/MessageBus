using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.IntegrationTests
{
    [TestClass]
    public class RcpTests : AcceptanceTests.RcpTests
    {
        protected override IMessageBus CreateBus()
        {
            return new MessageBrokerMessageBus(
                MemoryMessageBrokerBuilder.InProcessBroker(),
                NoExceptionNotification.Instance);
        }
    }
}
