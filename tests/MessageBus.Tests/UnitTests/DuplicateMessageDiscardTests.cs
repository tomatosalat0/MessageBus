using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests
{
    [TestClass]
    public class DuplicateMessageDiscardTests
    {
        [TestMethod]
        public async Task MultipleIdenticalEventsWillGetDiscarded()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            EventCounterHandler handler = new EventCounterHandler();
            bus.RegisterEventHandler(handler
                .WithDuplicateMessageDetection()
            );

            MyEvent scheduledEvent = new MyEvent();
            await bus.FireEvent(scheduledEvent);
            await bus.FireEvent(scheduledEvent);
            await bus.FireEvent(scheduledEvent);

            // cheap way: just wait a little bit until the events got executed
            await Task.Delay(200);

            Assert.AreEqual(1, handler.CallCount);
        }

        [TestMethod]
        public async Task MultipleIdenticalCommandsWillGetDiscarded()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            CommandCounterHandler handler = new CommandCounterHandler();
            bus.RegisterCommandHandler(handler
                .WithDuplicateMessageDetection()
            );

            MyCommand scheduledCommand = new MyCommand();
            await bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2));
            await bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2));
            await bus.FireCommandAndWait(scheduledCommand, TimeSpan.FromSeconds(2));

            Assert.AreEqual(1, handler.CallCount);
        }

        [Topic("Events/MyEvent")]
        public class MyEvent : IMessageEvent
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        [Topic("Commands/MyCommand")]
        public class MyCommand : IMessageCommand
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        public class EventCounterHandler : IMessageEventHandler<MyEvent>
        {
            private int _callCount;

            public int CallCount => _callCount;

            public void Handle(MyEvent @event)
            {
                ++_callCount;
            }
        }

        public class CommandCounterHandler : IMessageCommandHandler<MyCommand>
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
