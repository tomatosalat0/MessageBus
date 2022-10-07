using System;
using MessageBus.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBus.Broker.RabbitMq
{
    internal class EventSubscription : ISubscribable, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly TopicName _topic;
        private readonly bool _disposeOnDisconnect;
        private bool _disposedValue;

        public EventSubscription(TopicName topic, IConnection connection, bool disposeOnDisconnect)
        {
            _topic = topic;
            _connection = connection;
            _disposeOnDisconnect = disposeOnDisconnect;
            _channel = _connection.CreateModel();
        }

        public IDisposable Subscribe<T>(Action<IMessage<T>> messageHandler) where T : notnull
        {
            if (typeof(T) != typeof(byte[]))
                throw new NotSupportedException($"The payload must be of the type byte[], but got '{typeof(T)}'");

            string queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(
                queue: queueName,
                exchange: "amq.direct",
                routingKey: _topic.ToString()
            );

            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
            EventHandler<BasicDeliverEventArgs> handler = CreateAutoAckHandler<T>(messageHandler);
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

        private static EventHandler<BasicDeliverEventArgs> CreateAutoAckHandler<T>(Action<IMessage<T>> messageHandler)
        {
            return (model, ea) =>
            {
                RabbitMQMessage<T> message = new RabbitMQMessage<T>(ea);
                messageHandler(message);
            };
        }

        private sealed class EventHandlerDisposable : IDisposable
        {
            private readonly EventHandler<BasicDeliverEventArgs> _handler;
            private readonly EventingBasicConsumer _consumer;
            private readonly IDisposable? _parentDispose;

            public EventHandlerDisposable(EventingBasicConsumer consumer, EventHandler<BasicDeliverEventArgs> handler, IDisposable? parentDispose)
            {
                _consumer = consumer;
                _handler = handler;
                _parentDispose = parentDispose;
            }

            private bool disposedValue;

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _consumer.Received -= _handler;
                        _parentDispose?.Dispose();
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        private class RabbitMQMessage<T> : IMessage<T>
        {
            private readonly BasicDeliverEventArgs _args;

            public RabbitMQMessage(BasicDeliverEventArgs args)
            {
                _args = args;
            }

            public T Payload => (T)(object)(_args.Body.ToArray());

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

        protected virtual void Dispose(bool disposing)
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
