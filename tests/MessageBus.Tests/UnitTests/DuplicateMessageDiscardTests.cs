using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests
{
    [TestClass]
    public class DuplicateMessageDiscardTests
    {
        [TestMethod]
        public async Task DiscardDuplicateExtensionWillDiscardDuplicates()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            CounterHandler handler = new CounterHandler();
            bus.RegisterCommandHandler(handler
                .WithDuplicateMessageDetection()
            );

            MyCommand scheduledCommand = new MyCommand();
            await bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2));
            await bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2));
            await bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2));

            Assert.AreEqual(1, handler.CallCount);
        }


        [Topic("Commands/MyCommand")]
        public class MyCommand : IMessageCommand
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        public class CounterHandler : IMessageCommandHandler<MyCommand>
        {
            private int _callCount;

            public int CallCount => _callCount;

            public void Handle(MyCommand command)
            {
                ++_callCount;
            }
        }
    }
}
