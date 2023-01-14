using System;
using System.Collections.Generic;
using MessageBus.Messaging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace MessageBus.Broker.RabbitMq
{
    internal sealed class CommandSubscription : ISubscribable, IDisposable
    {
        private readonly IModel _channel;
        private readonly TopicName _topic;
        private bool _disposedValue;

        public CommandSubscription(TopicName topic, IConnection connection, ISubscriptionOptions? options)
        {
            _topic = topic;
            _channel = connection.CreateModel();
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
            Type messageType = typeof(T);
            if (messageType == typeof(byte[]))
                return ExecuteSubscribe(() => CreateManualAckByteArrayHandler(_channel, messageHandler));
            if (messageType == typeof(ReadOnlyMemory<byte>))
                return ExecuteSubscribe(() => CreateManualAckMemoryHandler(_channel, messageHandler));
            
            throw new NotSupportedException($"The payload must be of the type byte[] or ReadOnlyMemory<byte>, but got '{typeof(T)}'");
        }

        private IDisposable ExecuteSubscribe(Func<EventHandler<BasicDeliverEventArgs>> createHandler)
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
            EventHandler<BasicDeliverEventArgs> handler = createHandler();
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

        private static EventHandler<BasicDeliverEventArgs> CreateManualAckMemoryHandler<T>(IModel channel, Action<IMessage<T>> messageHandler)
        {
            return (_, ea) =>
            {
                RabbitMQMemoryMessage<T> message = new RabbitMQMemoryMessage<T>(ea, channel);
                messageHandler(message);
            };
        }

        private static EventHandler<BasicDeliverEventArgs> CreateManualAckByteArrayHandler<T>(IModel channel, Action<IMessage<T>> messageHandler)
        {
            return (_, ea) =>
            {
                RabbitMQByteArrayMessage<T> message = new RabbitMQByteArrayMessage<T>(ea, channel);
                messageHandler(message);
            };
        }

        private sealed class EventHandlerDisposable : IDisposable
        {
            private readonly EventHandler<BasicDeliverEventArgs> _handler;
            private readonly EventingBasicConsumer _consumer;
            private bool _disposedValue;

            public EventHandlerDisposable(EventingBasicConsumer consumer, EventHandler<BasicDeliverEventArgs> handler)
            {
                _consumer = consumer;
                _handler = handler;
            }

            private void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                        _consumer.Received -= _handler;
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

        private abstract class RabbitMQMessage : IMessageSupportsRejection
        {
            protected readonly BasicDeliverEventArgs _args;
            private readonly IModel _channel;

            protected RabbitMQMessage(BasicDeliverEventArgs args, IModel channel)
            {
                _args = args;
                _channel = channel;
            }

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

        private sealed class RabbitMQByteArrayMessage<T> : RabbitMQMessage, IMessage<T>
        {
            public RabbitMQByteArrayMessage(BasicDeliverEventArgs args, IModel channel) : base(args, channel)
            {
            }

            public T Payload => (T)(object)(_args.Body.ToArray());
        }

        private sealed class RabbitMQMemoryMessage<T> : RabbitMQMessage, IMessage<T>
        {
            public RabbitMQMemoryMessage(BasicDeliverEventArgs args, IModel channel) : base(args, channel)
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
