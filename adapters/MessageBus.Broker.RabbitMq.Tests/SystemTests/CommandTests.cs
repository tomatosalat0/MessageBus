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
    public class CommandTests
    {
        [TestMethod]
        public async Task RabbitMQSendsCommandToSingleConsumer()
        {
            using IMessageBroker broker = CreateBroker();

            TopicName topic = "command://-test/commands/single-consumer";
            IMyCommand sendCommand = new MyCommandImpl("Hello World");
            IMyCommand? receivedCommand = null;

            using ManualResetEventSlim readyEvent = new ManualResetEventSlim();
            broker.Commands(topic).Subscribe<IMyCommand>((command) =>
            {
                receivedCommand = command.Payload;
                command.Ack();
                readyEvent.Set();
            });

            await broker.PublishCommand(sendCommand, topic);

            Assert.IsTrue(readyEvent.Wait(TimeSpan.FromSeconds(2)));
            Assert.IsNotNull(receivedCommand);
            Assert.AreEqual(sendCommand.Message, receivedCommand.Message);
            Assert.AreEqual(sendCommand.MessageId, receivedCommand.MessageId);
        }

        [TestMethod]
        public async Task RabbitMQSendsCommandToOnlyOneConsumer()
        {
            using IMessageBroker broker = CreateBroker();

            TopicName topic = "command://-test/commands/multiple-consumer";
            IMyCommand sendCommand = new MyCommandImpl("Hello World");
            IMyCommand? receivedCommand1 = null;
            IMyCommand? receivedCommand2 = null;

            using ManualResetEventSlim readyEvent1 = new ManualResetEventSlim();
            broker.Commands(topic).Subscribe<IMyCommand>((command) =>
            {
                receivedCommand1 = command.Payload;
                command.Ack();
                readyEvent1.Set();
            });
            using ManualResetEventSlim readyEvent2 = new ManualResetEventSlim();
            broker.Commands(topic).Subscribe<IMyCommand>((command) =>
            {
                receivedCommand2 = command.Payload;
                command.Ack();
                readyEvent2.Set();
            });

            await broker.PublishCommand(sendCommand, topic);

            bool wait1 = false;
            bool wait2 = false;
            await Task.WhenAll(
                Task.Run(() => { wait1 = readyEvent1.Wait(TimeSpan.FromSeconds(2)); }),
                Task.Run(() => { wait2 = readyEvent2.Wait(TimeSpan.FromSeconds(2)); })
            );
            Assert.IsTrue(wait1 ^ wait2);
            if (wait1)
            {
                Assert.IsNotNull(receivedCommand1);
                Assert.AreEqual(sendCommand.Message, receivedCommand1.Message);
                Assert.AreEqual(sendCommand.MessageId, receivedCommand1.MessageId);
            }
            if (wait2)
            {
                Assert.IsNotNull(receivedCommand2);
                Assert.AreEqual(sendCommand.Message, receivedCommand2.Message);
                Assert.AreEqual(sendCommand.MessageId, receivedCommand2.MessageId);
            }
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

        public interface IMyCommand : IHasMessageId
        {
            string Message { get; }
        }

        private class MyCommandImpl : IMyCommand
        {
            public MyCommandImpl(string message)
            {
                Message = message;
            }

            public string Message { get; }

            public MessageId MessageId { get; } = MessageId.NewId();
        }
    }
}
