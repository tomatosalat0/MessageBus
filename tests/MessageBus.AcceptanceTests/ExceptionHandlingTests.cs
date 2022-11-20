using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.AcceptanceTests
{
    public abstract class ExceptionHandlingTests
    {
        protected abstract IMessageBus CreateBus();

        [TestMethod]
        public async Task ExceptionsInRpcAreCapturedAndThrown()
        {
            using IMessageBus bus = CreateBus();

            const string exceptionMessage = "My custom message";
            IMyRpc firedRpc = new MyRcpImpl();
            bus.RegisterRpcDelegate<IMyRpc, IMyRpcResult>(received =>
            {
                throw new NotSupportedException(exceptionMessage);
            });

            try
            {
                IMyRpcResult receivedEvent = await bus.FireRpc<IMyRpc, IMyRpcResult>(firedRpc, TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                Assert.Fail("The thrown exception did not get propagated back.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(MessageOperationFailedException));
                MessageOperationFailedException m = (MessageOperationFailedException)ex;
                Assert.AreEqual(exceptionMessage, m.Message);
            }
        }

        [TestMethod]
        public async Task ExceptionsInQueryAreCapturedAndThrown()
        {
            using IMessageBus bus = CreateBus();

            const string exceptionMessage = "My custom message";
            IMyQuery firedQuery = new MyQueryImpl();
            bus.RegisterQueryDelegate<IMyQuery, IMyQueryResult>(received =>
            {
                throw new NotSupportedException(exceptionMessage);
            });

            try
            {
                IMyQueryResult receivedEvent = await bus.FireQuery<IMyQuery, IMyQueryResult>(firedQuery, TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                Assert.Fail("The thrown exception did not get propagated back.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(MessageOperationFailedException));
                MessageOperationFailedException m = (MessageOperationFailedException)ex;
                Assert.AreEqual(exceptionMessage, m.Message);
            }
        }

        [TestMethod]
        public async Task ExceptionsInCommandAreCapturedAndThrownIfAwaited()
        {
            using IMessageBus bus = CreateBus();

            const string exceptionMessage = "My custom message";
            IMyCommand firedCommand = new MyCommandImpl();

            bus.RegisterCommandDelegate<IMyCommand>(received =>
            {
                throw new NotSupportedException(exceptionMessage);
            });

            try
            {
                await bus.FireCommandAndWait(firedCommand, TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                Assert.Fail("The thrown exception did not get propagated back.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(MessageOperationFailedException));
                MessageOperationFailedException m = (MessageOperationFailedException)ex;
                Assert.AreEqual(exceptionMessage, m.Message);
            }
        }

        [TestMethod]
        public async Task ExceptionsInCommandWillNotTriggerRerunIfAwaited()
        {
            using IMessageBus bus = CreateBus();

            const string exceptionMessage = "My custom message";
            IMyCommand firedCommand = new MyCommandImpl();

            using ManualResetEventSlim resetEvent = new ManualResetEventSlim();
            int counter = 0;
            bus.RegisterCommandDelegate<IMyCommand>(received =>
            {
                if (++counter < 2)
                    throw new NotSupportedException(exceptionMessage);
                else
                    resetEvent.Set();
            });

            await Assert.ThrowsExceptionAsync<MessageOperationFailedException>(async () => await bus.FireCommandAndWait(firedCommand, TimeSpan.FromSeconds(2)).ConfigureAwait(false)).ConfigureAwait(false);

            Assert.IsFalse(resetEvent.Wait(TimeSpan.FromSeconds(2)));
        }

        [TestMethod]
        public async Task HandlerUnavailableExceptionWillRerunTheHandler()
        {
            using IMessageBus bus = CreateBus();

            const string exceptionMessage = "My custom message";
            IMyCommand firedCommand = new MyCommandImpl();

            int counter = 0;
            bus.RegisterCommandDelegate<IMyCommand>(received =>
            {
                if (++counter < 2)
                    throw new HandlerUnavailableException();
                else
                    throw new NotSupportedException(exceptionMessage);
            });

            MessageOperationFailedException ex = await Assert.ThrowsExceptionAsync<MessageOperationFailedException>(async () => await bus.FireCommandAndWait(firedCommand, TimeSpan.FromSeconds(2)).ConfigureAwait(false)).ConfigureAwait(false);
            Assert.AreEqual(2, counter);
            Assert.AreEqual(exceptionMessage, ex.Message);            
        }

        [TestMethod]
        public async Task ExceptionsInCommandAreNotCapturedIfNotAwaited()
        {
            using IMessageBus bus = CreateBus();

            const string exceptionMessage = "My custom message";
            IMyCommand firedCommand = new MyCommandImpl();

            using ManualResetEventSlim resetEvent = new ManualResetEventSlim();
            int counter = 0;
            bus.RegisterCommandDelegate<IMyCommand>(received =>
            {
                if (++counter < 2)
                    throw new NotSupportedException(exceptionMessage);
                else
                    resetEvent.Set();
            });

            await bus.FireCommand(firedCommand).ConfigureAwait(false);

            Assert.IsFalse(resetEvent.Wait(TimeSpan.FromSeconds(2)));
        }

        [Topic("rpc://-test/acceptance/exceptions/MyRpc")]
        public interface IMyRpc : IMessageRpc<IMyRpcResult> { }

        public interface IMyRpcResult : IMessageRpcResult { }

        private class MyRcpImpl : IMyRpc
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        [Topic("query://-test/acceptance/exceptions/MyQuery")]
        public interface IMyQuery : IMessageQuery<IMyQueryResult> { }

        public interface IMyQueryResult : IMessageQueryResult { }

        private class MyQueryImpl : IMyQuery
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        [Topic("command://-test/acceptance/exceptions/MyCommand")]
        public interface IMyCommand : IMessageCommand { }

        private class MyCommandImpl : IMyCommand
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }
    }
}
