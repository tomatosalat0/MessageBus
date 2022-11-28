using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageBus.Messaging.InProcess;
using MessageBus.Messaging.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Messaging.Tests.UnitTests
{
    [TestClass]
    public class BrokerLoggingTests
    {
        [TestMethod]
        public async Task LoggerCapturesEventFireing()
        {
            LogCollector collector = new LogCollector();
            using IMessageBroker broker = new InProcessMessageBroker(MessageBrokerOptions.Default())
                .WrapWithLogging(collector);

            await broker.PublishEvent("Hello", "Topic");

            var actualMessages = collector.ToArray();
            AssertMessages(new[] { "[Topic] type {System.String}: Publish event with payload: Hello" }, actualMessages);
        }

        [TestMethod]
        public async Task LoggerCapturesCommandFireing()
        {
            LogCollector collector = new LogCollector();
            using IMessageBroker broker = new InProcessMessageBroker(MessageBrokerOptions.Default())
                .WrapWithLogging(collector);

            await broker.PublishCommand("Hello", "Topic");

            var actualMessages = collector.ToArray();
            AssertMessages(new[] { "[Topic] type {System.String}: Publish command with payload: Hello" }, actualMessages);
        }

        [TestMethod]
        public async Task LoggerCapturesEventFlow()
        {
            LogCollector collector = new LogCollector();
            using IMessageBroker broker = new InProcessMessageBroker(MessageBrokerOptions.Default())
                .WrapWithLogging(collector);

            using ManualResetEventSlim readyEvent = new ManualResetEventSlim();

            broker.Events("Topic").Subscribe<string>(m =>
            {
                m.Ack();
                readyEvent.Set();
            });
            await broker.PublishEvent("Hello", "Topic");

            Assert.IsTrue(readyEvent.Wait(TimeSpan.FromSeconds(2)));
            await Task.Delay(100);

            var actualMessages = collector.ToArray();
            var expectedMessages = new[]
            {
                "[Topic]: Adding typed subscriber with type System.String",
                "[Topic] type {System.String}: Publish event with payload: Hello",
                "[Topic] type {System.String}: Begin handle message",
                "[Topic] type {System.String}: Acknowledged",
                "[Topic] type {System.String}: Completed handle message",
            };
            AssertMessages(expectedMessages, actualMessages);
        }

        private void AssertMessages(IReadOnlyList<string> expectedMessages, IReadOnlyList<string> actualMessages)
        {
            Assert.AreEqual(expectedMessages.Count, actualMessages.Count);
            for (int i = 0; i < expectedMessages.Count; i++)
            {
                Assert.IsTrue(actualMessages[i].StartsWith(expectedMessages[i], StringComparison.Ordinal), $"Expected '{expectedMessages[i]}' but got '{actualMessages[i]}'");
            }
        }

        private class LogCollector : IBrokerLogger, IEnumerable<string>
        {
            private readonly ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();


            public void Log(string message)
            {
                _messages.Enqueue(message);
            }

            public IEnumerator<string> GetEnumerator()
            {
                return _messages.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
