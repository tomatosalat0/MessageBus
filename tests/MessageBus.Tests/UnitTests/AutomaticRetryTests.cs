using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests
{
    [TestClass]
    public class AutomaticRetryTests
    {
        [TestMethod]
        public async Task AutomaticRetryExtensionWillRetryHandler()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            AlwaysTimeoutOnFirstCallHandler handler = new AlwaysTimeoutOnFirstCallHandler();
            bus.RegisterCommandHandler(handler
                .WithRetryOnException()
            );

            MyCommand scheduledCommand = new MyCommand();
            await bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2));

            Assert.AreEqual(2, handler.CallCount);
            Assert.IsNotNull(handler.HandledMessageId);
            Assert.AreEqual(scheduledCommand.MessageId, handler.HandledMessageId.Value);
        }
        

        [Topic("Commands/MyCommand")]
        public class MyCommand : IMessageCommand
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        public class AlwaysTimeoutOnFirstCallHandler : IMessageCommandHandler<MyCommand>
        {
            private int _callCount;

            public MessageId? HandledMessageId { get; private set; }

            public int CallCount => _callCount;

            public void Handle(MyCommand command)
            {
                if (++_callCount < 2)
                    throw new TimeoutException("Something bad happened");
                else
                    HandledMessageId = command.MessageId;
            }
        }
    }
}
