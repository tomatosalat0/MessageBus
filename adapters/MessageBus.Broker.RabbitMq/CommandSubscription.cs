using System;
using System.Collections.Generic;
using MessageBus.Messaging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace MessageBus.Broker.RabbitMq
{
    internal class CommandSubscription : ISubscribable, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly TopicName _topic;
        private bool _disposedValue;

        public CommandSubscription(TopicName topic, IConnection connection, ISubscriptionOptions? options)
        {
            _topic = topic;
            _connection = connection;
            _channel = _connection.CreateModel();
            SetupChannel(options);
        }

        private void SetupChannel(ISubscriptionOptions? options)
        {
            _channel.QueueDeclare(
                queue: _topic.ToString(),
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>()
                {
                    ["x-queue-mode"] = "lazy",
                    ["x-expires"] = 60_000
                }
            );
            _channel.BasicQos(prefetchSize: 0, prefetchCount: ReadPrefetchCount(options), global: false);
        }

        private ushort ReadPrefetchCount(ISubscriptionOptions? options)
        {
            const ushort defaultValue = 1;

            if (options is null || !options.Attributes.TryGetValue("-rabbitmq-qos-prefetchcount", out object? value))
                return defaultValue;

            switch (value)
            {
                case ushort casted: return casted;
                case int casted: return (ushort)casted;
                case uint casted: return (ushort)casted;
                case long casted: return (ushort)casted;
                case ulong casted: return (ushort)casted;
            }
            return defaultValue;
        }

        public IDisposable Subscribe<T>(Action<IMessage<T>> messageHandler) where T : notnull
        {
            if (typeof(T) != typeof(byte[]))
                throw new NotSupportedException($"The payload must be of the type byte[], but got '{typeof(T)}'");

            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
            EventHandler<BasicDeliverEventArgs> handler = CreateManualAckHandler<T>(_channel, messageHandler);
            consumer.Received += handler;

            _channel.BasicConsume(
                queue: _topic.ToString(),
                autoAck: false,
                consumer: consumer
            );

            return new EventHandlerDisposable(consumer, handler);
        }

        public IDisposable Subscribe(Action<IMessage> messageHandler)
        {
            throw new NotSupportedException();
        }

        private static EventHandler<BasicDeliverEventArgs> CreateManualAckHandler<T>(IModel channel, Action<IMessage<T>> messageHandler)
        {
            return (model, ea) =>
            {
                RabbitMQMessage<T> message = new RabbitMQMessage<T>(ea, channel);
                messageHandler(message);
            };
        }

        private sealed class EventHandlerDisposable : IDisposable
        {
            private readonly EventHandler<BasicDeliverEventArgs> _handler;
            private readonly EventingBasicConsumer _consumer;

            public EventHandlerDisposable(EventingBasicConsumer consumer, EventHandler<BasicDeliverEventArgs> handler)
            {
                _consumer = consumer;
                _handler = handler;
            }

            private bool disposedValue;

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _consumer.Received -= _handler;
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

        private class RabbitMQMessage<T> : IMessage<T>, IMessageSupportsRejection
        {
            private readonly BasicDeliverEventArgs _args;
            private readonly IModel _channel;

            public RabbitMQMessage(BasicDeliverEventArgs args, IModel channel)
            {
                _args = args;
                _channel = channel;
            }

            public T Payload => (T)(object)(_args.Body.ToArray());

            public MessageState State { get; private set; }

            public void Ack()
            {
                if (State != MessageState.Initial && State != MessageState.Acknowledged)
                    throw new InvalidOperationException($"The message state can only be set once, current state: '{State}'");
                if (State != MessageState.Acknowledged)
                {
                    State = MessageState.Acknowledged;
                    _channel.BasicAck(_args.DeliveryTag, multiple: false);
                }
            }

            public void Nack()
            {
                if (State != MessageState.Initial && State != MessageState.NotAcknowledged)
                    throw new InvalidOperationException($"The message state can only be set once, current state: '{State}'");
                if (State != MessageState.NotAcknowledged)
                {
                    State = MessageState.NotAcknowledged;
                    _channel.BasicNack(_args.DeliveryTag, multiple: false, requeue: false);
                }
            }

            public void Reject()
            {
                _channel.BasicReject(_args.DeliveryTag, requeue: true);
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
