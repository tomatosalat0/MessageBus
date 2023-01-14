using System;
using MessageBus.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus.Broker.RabbitMq
{
    internal sealed class EventSubscription : ISubscribable, IDisposable
    {
        private readonly IModel _channel;
        private readonly TopicName _topic;
        private readonly bool _disposeOnDisconnect;
        private bool _disposedValue;

        public EventSubscription(TopicName topic, IConnection connection, bool disposeOnDisconnect)
        {
            _topic = topic;
            _disposeOnDisconnect = disposeOnDisconnect;
            _channel = connection.CreateModel();
        }

        public IDisposable Subscribe<T>(Action<IMessage<T>> messageHandler) where T : notnull
        {
            Type messageType = typeof(T);
            if (messageType == typeof(byte[]))
                return ExecuteSubscribe(() => CreateAutoAckByteArrayHandler(messageHandler));
            if (messageType == typeof(ReadOnlyMemory<byte>))
                return ExecuteSubscribe(() => CreateAutoAckMemoryHandler(messageHandler));
            
            throw new NotSupportedException($"The payload must be of the type byte[] or ReadOnlyMemory<byte>, but got '{typeof(T)}'");
        }

        private IDisposable ExecuteSubscribe(Func<EventHandler<BasicDeliverEventArgs>> createEventHandler) 
        {
            string queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(
                queue: queueName,
                exchange: "amq.direct",
                routingKey: _topic.ToString()
            );

            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
            EventHandler<BasicDeliverEventArgs> handler = createEventHandler();
            consumer.Received += handler;

            _channel.BasicConsume(
                queue: queueName,
                autoAck: true,
                consumer: consumer
            );

            return new EventHandlerDisposable(consumer, handler, _disposeOnDisconnect ? this : null);
        }

        public IDisposable Subscribe(Action<IMessage> messageHandler)
        {
            throw new NotSupportedException();
        }

        private static EventHandler<BasicDeliverEventArgs> CreateAutoAckMemoryHandler<T>(Action<IMessage<T>> messageHandler)
        {
            return (_, ea) =>
            {
                RabbitMQMemoryMessage<T> memoryMessage = new RabbitMQMemoryMessage<T>(ea);
                messageHandler(memoryMessage);
            };
        }

        private static EventHandler<BasicDeliverEventArgs> CreateAutoAckByteArrayHandler<T>(Action<IMessage<T>> messageHandler)
        {
            return (_, ea) =>
            {
                RabbitMQByteArrayMessage<T> byteArrayMessage = new RabbitMQByteArrayMessage<T>(ea);
                messageHandler(byteArrayMessage);
            };
        }

        private sealed class EventHandlerDisposable : IDisposable
        {
            private readonly EventHandler<BasicDeliverEventArgs> _handler;
            private readonly EventingBasicConsumer _consumer;
            private readonly IDisposable? _parentDispose;
            private bool _disposedValue;

            public EventHandlerDisposable(EventingBasicConsumer consumer, EventHandler<BasicDeliverEventArgs> handler, IDisposable? parentDispose)
            {
                _consumer = consumer;
                _handler = handler;
                _parentDispose = parentDispose;
            }

            private void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                        _consumer.Received -= _handler;
                        _parentDispose?.Dispose();
                    }
                    _disposedValue = true;
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        private abstract class RabbitMQMessage
        {
            protected readonly BasicDeliverEventArgs _args;
            
            protected RabbitMQMessage(BasicDeliverEventArgs args)
            {
                _args = args;
            }
            
            public MessageState State => MessageState.Acknowledged;

            public void Ack()
            {
                // intentionally left blank
            }

            public void Nack()
            {
                // intentionally left blank
            }
        }

        private sealed class RabbitMQByteArrayMessage<T> : RabbitMQMessage, IMessage<T>
        {
            public RabbitMQByteArrayMessage(BasicDeliverEventArgs args) : base(args)
            {
            }

            public T Payload => (T)(object)(_args.Body.ToArray());
        }

        private sealed class RabbitMQMemoryMessage<T> : RabbitMQMessage, IMessage<T>
        {
            public RabbitMQMemoryMessage(BasicDeliverEventArgs args) : base(args)
            {
            }

            public T Payload => (T)(object)(_args.Body);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _channel.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
