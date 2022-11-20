using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.AcceptanceTests
{
    public abstract class CommandsTests
    {
        protected abstract IMessageBus CreateBus();

        [TestMethod]
        public async Task CommandHandlerWithoutWaitPasses()
        {
            using IMessageBus bus = CreateBus();

            IMyCommand firedCommand = new MyCommandImpl("Test Value");
            IMyCommand? receivedCommand = null;
            using ManualResetEventSlim notifyEvent = new ManualResetEventSlim();

            bus.RegisterCommandDelegate<IMyCommand>(received =>
            {
                receivedCommand = received;
                notifyEvent.Set();
            });

            await bus.FireCommand(firedCommand).ConfigureAwait(false);

            bool waitComplete = notifyEvent.Wait(TimeSpan.FromSeconds(2));
            Assert.IsTrue(waitComplete);

            Assert.IsNotNull(receivedCommand);
            Assert.AreEqual(firedCommand.MessageId, receivedCommand.MessageId);
            Assert.AreEqual(firedCommand.Value, receivedCommand.Value);
        }

        [TestMethod]
        public async Task CommandHandlerWithWaitPasses()
        {
            using IMessageBus bus = CreateBus();

            IMyCommand firedCommand = new MyCommandImpl("Test Value");
            IMyCommand? receivedCommand = null;

            bus.RegisterCommandDelegate<IMyCommand>(received =>
            {
                receivedCommand = received;
            });

            await bus.FireCommandAndWait(firedCommand, TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            Assert.IsNotNull(receivedCommand);
            Assert.AreEqual(firedCommand.MessageId, receivedCommand.MessageId);
            Assert.AreEqual(firedCommand.Value, receivedCommand.Value);
        }

        [TestMethod]
        public async Task CommandHandlerWithExceptionGetsPropagated()
        {
            using IMessageBus bus = CreateBus();

            IMyCommand firedCommand = new MyCommandImpl("Test Value");
            bus.RegisterCommandDelegate<IMyCommand>(received =>
            {
                throw new Exception("My custom exception");
            });

            Exception ex = await Assert.ThrowsExceptionAsync<MessageOperationFailedException>(() => bus.FireCommandAndWait(firedCommand, TimeSpan.FromSeconds(2))).ConfigureAwait(false);
            Assert.AreEqual("My custom exception", ex.Message);
        }

        [TestMethod]
        public async Task ReceivedCommandWillGetRescheduledIfNotAvailableExceptionIsThrown()
        {
            using IMessageBus bus = CreateBus();

            IMyCommand firedCommand = new MyCommandImpl("Test Value");
            IMyCommand? receivedCommand = null;

            int executeCounter = 0;
            bus.RegisterCommandDelegate<IMyCommand>(received =>
            {
                executeCounter++;
                if (executeCounter < 2)
                    throw new HandlerUnavailableException();
                receivedCommand = received;
            });

            await bus.FireCommandAndWait(firedCommand, TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            Assert.IsNotNull(receivedCommand);
            Assert.AreEqual(firedCommand.MessageId, receivedCommand.MessageId);
            Assert.AreEqual(firedCommand.Value, receivedCommand.Value);
        }

        [Topic("command://-test/acceptance/MyCommand")]
        public interface IMyCommand : IMessageCommand
        {
            string Value { get; }
        }

        private class MyCommandImpl : IMyCommand
        {
            public MyCommandImpl(string value)
            {
                Value = value;
            }

            public string Value { get; }

            public MessageId MessageId { get; } = MessageId.NewId();
        }
    }
}
