using System;
using System.Threading.Tasks;
using MessageBus.Decorations.Versioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests
{
    [TestClass]
    public class OldVersionDiscardTests
    {
        [TestMethod]
        public async Task OldMessageVersionsWillGetDiscarded()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);
            
            CounterHandler handler = new CounterHandler();
            bus.RegisterCommandHandler(handler
                .WithDiscardOldMessageVersion<MyCommand, int>()
            );

            await bus.FireCommandAndWait(new MyCommand(2), TimeSpan.FromSeconds(2));
            await bus.FireCommandAndWait(new MyCommand(1), TimeSpan.FromSeconds(2));
            await bus.FireCommandAndWait(new MyCommand(3), TimeSpan.FromSeconds(2));

            Assert.AreEqual(2, handler.CallCount);
        }

        [Topic("Commands/MyCommand")]
        public class MyCommand : IMessageCommand, IHasMessageVersion<int>
        {
            public MyCommand(int messageVersion)
            {
                MessageVersion = messageVersion;
            }

            public MessageId MessageId { get; } = MessageId.NewId();

            public int MessageVersion { get; }
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
