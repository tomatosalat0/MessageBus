using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.AcceptanceTests
{
    public abstract class RcpTests
    {
        protected abstract IMessageBus CreateBus();

        [TestMethod]
        public async Task RpcHandlerReturnsQueryResult()
        {
            using IMessageBus bus = CreateBus();

            IMyRpc firedRpc = new MyRcpImpl("Test Value");

            bus.RegisterRpcDelegate<IMyRpc, IMyRcpResult>(received => new MyRpcResultImpl(received.Value + "+Result", received.MessageId));

            IMyRcpResult receivedResult = await bus.FireRpc<IMyRpc, IMyRcpResult>(firedRpc, TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            Assert.AreNotSame(firedRpc, receivedResult);
            Assert.AreEqual(firedRpc.MessageId, receivedResult.MessageId);
            Assert.AreEqual(firedRpc.Value + "+Result", receivedResult.Value);
        }

        [TestMethod]
        public async Task QueryWithExceptionGetsForwarded()
        {
            using IMessageBus bus = CreateBus();

            IMyRpc firedRpc = new MyRcpImpl("Test Value");

            bus.RegisterRpcDelegate<IMyRpc, IMyRcpResult>(received => throw new Exception("My exception"));

            Exception exception = await Assert.ThrowsExceptionAsync<MessageOperationFailedException>(() => bus.FireRpc<IMyRpc, IMyRcpResult>(firedRpc, TimeSpan.FromSeconds(2))).ConfigureAwait(false);
            Assert.AreEqual("My exception", exception.Message);
        }

        [Topic("rpc://-test/acceptance/MyRcp")]
        public interface IMyRpc : IMessageRpc<IMyRcpResult>
        {
            string Value { get; }
        }

        public interface IMyRcpResult : IMessageRpcResult
        {
            string Value { get; }
        }

        private class MyRcpImpl : IMyRpc
        {
            public MyRcpImpl(string value)
            {
                Value = value;
            }

            public string Value { get; }

            public MessageId MessageId { get; } = MessageId.NewId();
        }

        private class MyRpcResultImpl : IMyRcpResult
        {
            public MyRpcResultImpl(string value, MessageId queryMessageId)
            {
                Value = value;
                MessageId = queryMessageId;
            }

            public string Value { get; }

            public MessageId MessageId { get; }
        }
    }
}
