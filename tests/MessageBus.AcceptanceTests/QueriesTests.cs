using System.Threading.Tasks;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.AcceptanceTests
{
    public abstract class QueriesTests
    {
        protected abstract IMessageBus CreateBus();

        [TestMethod]
        public async Task QueryHandlerReturnsQueryResult()
        {
            using IMessageBus bus = CreateBus();

            IMyQuery firedQuery = new MyQueryImpl("Test Value");

            bus.RegisterQueryDelegate<IMyQuery, IMyQueryResult>(received => new MyQueryResult(received.Value + "+Result", received.MessageId));

            IMyQueryResult receivedResult = await bus.FireQuery<IMyQuery, IMyQueryResult>(firedQuery, TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            Assert.AreNotSame(firedQuery, receivedResult);
            Assert.AreEqual(firedQuery.MessageId, receivedResult.MessageId);
            Assert.AreEqual(firedQuery.Value + "+Result", receivedResult.Value);
        }

        [TestMethod]
        public async Task QueryWithExceptionGetsForwarded()
        {
            using IMessageBus bus = CreateBus();

            IMyQuery firedQuery = new MyQueryImpl("Test Value");

            bus.RegisterQueryDelegate<IMyQuery, IMyQueryResult>(_ => throw new Exception("My exception"));

            Exception exception = await Assert.ThrowsExceptionAsync<MessageOperationFailedException>(() => bus.FireQuery<IMyQuery, IMyQueryResult>(firedQuery, TimeSpan.FromSeconds(2))).ConfigureAwait(false);
            Assert.AreEqual("My exception", exception.Message);
        }

        [Topic("command://-test/acceptance/MyQuery")]
        public interface IMyQuery : IMessageQuery<IMyQueryResult>
        {
            string Value { get; }
        }

        public interface IMyQueryResult : IMessageQueryResult
        {
            string Value { get; }
        }

        private class MyQueryImpl : IMyQuery
        {
            public MyQueryImpl(string value)
            {
                Value = value;
            }

            public string Value { get; }

            public MessageId MessageId { get; } = MessageId.NewId();
        }

        private class MyQueryResult : IMyQueryResult
        {
            public MyQueryResult(string value, MessageId queryMessageId)
            {
                Value = value;
                MessageId = queryMessageId;
            }

            public string Value { get; }

            public MessageId MessageId { get; }
        }
    }
}
