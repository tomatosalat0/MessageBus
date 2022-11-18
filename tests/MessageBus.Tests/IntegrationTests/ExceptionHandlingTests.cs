using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.IntegrationTests
{
    [TestClass]
    public class ExceptionHandlingTests : AcceptanceTests.ExceptionHandlingTests
    {
        protected override IMessageBus CreateBus()
        {
            return new MessageBrokerMessageBus(
                MemoryMessageBrokerBuilder.InProcessBroker(),
                NoExceptionNotification.Instance);
        }
    }
}
