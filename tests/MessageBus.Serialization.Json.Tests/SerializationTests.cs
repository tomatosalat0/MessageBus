using System.Text.Json.Serialization;
using MessageBus.Messaging;
using MessageBus.Messaging.InProcess;
using MessageBus.Messaging.InProcess.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Serialization.Json.Tests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void MessageWillGetConvertedToAndFromJson()
        {
            ManualScheduler scheduler = new ManualScheduler();
            using IMessageBroker broker = new InProcessMessageBroker(MessageBrokerOptions.BlockingManual(scheduler))
                .UseMessageSerialization(new JsonMessageSerializer());

            PlainMessage sendMessage = new PlainMessage("Hello World");

            bool hasBeenFired = false;
            bool hasSameMessageBody = false;
            bool isSameObjectReference = false;

            broker.Events("TestTopic").Subscribe<PlainMessage>((message) =>
            {
                hasBeenFired = true;
                hasSameMessageBody = message.Payload.Message == sendMessage.Message;
                isSameObjectReference = object.ReferenceEquals(message.Payload, sendMessage);
            });
            broker.Publish(sendMessage, "TestTopic");
            scheduler.Drain();

            Assert.IsTrue(hasBeenFired);
            Assert.IsTrue(hasSameMessageBody);
            Assert.IsFalse(isSameObjectReference);
        }

        public class PlainMessage
        {
            public PlainMessage(string message)
            {
                Message = message;
            }

            public string Message { get; internal set; }
        }

        [TestMethod]
        public void MessageWithMessageIdWillGetConvertedToAndFromJson()
        {
            ManualScheduler scheduler = new ManualScheduler();
            using IMessageBroker broker = new InProcessMessageBroker(MessageBrokerOptions.BlockingManual(scheduler))
                .UseMessageSerialization(new JsonMessageSerializer());

            WithMessageId sendMessage = new WithMessageId("Hello World");

            bool hasBeenFired = false;
            bool hasSameMessageBody = false;
            bool isSameMessageId = false;

            broker.Events("TestTopic").Subscribe<WithMessageId>((message) =>
            {
                hasBeenFired = true;
                hasSameMessageBody = message.Payload.Message == sendMessage.Message;
                isSameMessageId = message.Payload.MessageId == sendMessage.MessageId;
            });
            broker.Publish(sendMessage, "TestTopic");
            scheduler.Drain();

            Assert.IsTrue(hasBeenFired);
            Assert.IsTrue(hasSameMessageBody);
            Assert.IsTrue(isSameMessageId);
        }


        public class WithMessageId
        {
            public WithMessageId(string message)
            {
                Message = message;
            }

            [JsonInclude]
            public MessageId MessageId { get; private init; } = MessageId.NewId();

            public string Message { get; internal set; }
        }
    }
}