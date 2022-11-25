using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests
{
    [TestClass]
    public class MessageAsInterfaceTests
    {
        [TestMethod]
        public async Task HandlerWillGetExecutedIfRegisteredForInterfaceAndInterfaceFired()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            CommandCounterHandler handler = new CommandCounterHandler();
            bus.RegisterCommandHandler(handler);

            IMyCommand scheduledCommand = new MyCommandImpl();
            await bus.FireCommandAndWait<IMyCommand>(scheduledCommand, TimeSpan.FromSeconds(2));

            Assert.AreEqual(1, handler.CallCount);
        }

        [TestMethod]
        public async Task HandlerWillGetExecutedIfRegisteredForInterfaceAndImplementationFired()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            CommandCounterHandler handler = new CommandCounterHandler();
            bus.RegisterCommandHandler(handler);

            MyCommandImpl scheduledCommand = new MyCommandImpl();
            await bus.FireCommandAndWait<MyCommandImpl>(scheduledCommand, TimeSpan.FromSeconds(2));

            Assert.AreEqual(1, handler.CallCount);
        }

        [Topic("Commands/IMyCommand")]
        private interface IMyCommand : IMessageCommand
        {
        }

        private class MyCommandImpl : IMyCommand
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        private class CommandCounterHandler : IMessageCommandHandler<IMyCommand>
        {
            private int _callCount;

            public int CallCount => _callCount;

            public void Handle(IMyCommand command)
            {
                ++_callCount;
            }
        }
    }
}
