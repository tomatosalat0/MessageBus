using System;
using System.Threading;
using System.Threading.Tasks;
using MessageBus.Messaging;
using MessageBus.Serialization.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

namespace MessageBus.Broker.RabbitMq.Tests.SystemTests
{
    [TestClass]
    public class EventTests
    {
        [TestMethod]
        public async Task RabbitMQSendsEventToSingleConsumer()
        {
            using IMessageBroker broker = CreateBroker();

            TopicName topic = "events://-test/events/single-consumer";
            IMyEvent sendEvent = new MyEventImpl("Hello World");
            IMyEvent? receivedEvent = null;

            using ManualResetEventSlim readyEvent = new ManualResetEventSlim();
            broker.Events(topic).Subscribe<IMyEvent>((@event) =>
            {
                receivedEvent = @event.Payload;
                @event.Ack();
                readyEvent.Set();
            });

            await broker.PublishEvent(sendEvent, topic);

            Assert.IsTrue(readyEvent.Wait(TimeSpan.FromSeconds(30)));
            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(sendEvent.Message, receivedEvent.Message);
            Assert.AreEqual(sendEvent.MessageId, receivedEvent.MessageId);
        }

        [TestMethod]
        public async Task RabbitMQSendsEventToMultipleConsumer()
        {
            using IMessageBroker broker = CreateBroker();

            TopicName topic = "events://-test/events/multiple-consumer";
            IMyEvent sendEvent = new MyEventImpl("Hello World");
            IMyEvent? receivedEvent1 = null;
            IMyEvent? receivedEvent2 = null;

            using ManualResetEventSlim readyEvent1 = new ManualResetEventSlim();
            broker.Events(topic).Subscribe<IMyEvent>((@event) =>
            {
                receivedEvent1 = @event.Payload;
                @event.Ack();
                readyEvent1.Set();
            });
            using ManualResetEventSlim readyEvent2 = new ManualResetEventSlim();
            broker.Events(topic).Subscribe<IMyEvent>((@event) =>
            {
                receivedEvent2 = @event.Payload;
                @event.Ack();
                readyEvent2.Set();
            });

            await broker.PublishEvent(sendEvent, topic);

            Assert.IsTrue(readyEvent1.Wait(TimeSpan.FromSeconds(30)));
            Assert.IsTrue(readyEvent2.Wait(TimeSpan.FromSeconds(30)));
            Assert.IsNotNull(receivedEvent1);
            Assert.AreEqual(sendEvent.Message, receivedEvent1.Message);
            Assert.AreEqual(sendEvent.MessageId, receivedEvent1.MessageId);

            Assert.IsNotNull(receivedEvent2);
            Assert.AreEqual(sendEvent.Message, receivedEvent2.Message);
            Assert.AreEqual(sendEvent.MessageId, receivedEvent2.MessageId);
        }

        private IMessageBroker CreateBroker()
        {
            ConnectionFactory connection = new ConnectionFactory()
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost"
            };
            return new RabbitMqBroker(connection)
                .UseMessageSerialization(new JsonMessageSerializer().WithInterfaceDeserializer());
        }

        public interface IMyEvent : IHasMessageId
        {
            string Message { get; }
        }

        private class MyEventImpl : IMyEvent
        {
            public MyEventImpl(string message)
            {
                Message = message;
            }

            public string Message { get; }

            public MessageId MessageId { get; } = MessageId.NewId();
        }
    }
}