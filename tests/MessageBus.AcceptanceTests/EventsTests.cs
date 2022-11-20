using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.AcceptanceTests
{
    public abstract class EventsTests
    {
        protected abstract IMessageBus CreateBus();

        [TestMethod]
        public async Task NormalEventHandlerPasses()
        {
            using IMessageBus bus = CreateBus();

            IMyEvent firedEvent = new MyEventImpl("Test Value");
            IMyEvent? receivedEvent = null;
            using ManualResetEventSlim notifyEvent = new ManualResetEventSlim();

            bus.RegisterEventDelegate<IMyEvent>(received =>
            {
                receivedEvent = received;
                notifyEvent.Set();
            });

            await bus.FireEvent(firedEvent).ConfigureAwait(false);

            bool waitComplete = notifyEvent.Wait(TimeSpan.FromSeconds(2));
            Assert.IsTrue(waitComplete);

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(firedEvent.MessageId, receivedEvent.MessageId);
            Assert.AreEqual(firedEvent.Value, receivedEvent.Value);
        }

        [Topic("event://-test/acceptance/MyEvent")]
        public interface IMyEvent : IMessageEvent
        {
            string Value { get; }
        }

        private class MyEventImpl : IMyEvent
        {
            public MyEventImpl(string value)
            {
                Value = value;
            }

            public string Value { get; }

            public MessageId MessageId { get; } = MessageId.NewId();
        }
    }
}
