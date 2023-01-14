using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageBus.Messaging;
using RabbitMQ.Client;

namespace MessageBus.Broker.RabbitMq
{
    public class RabbitMqBroker : IMessageBroker
    {
        private readonly ConcurrentDictionary<TopicName, EventSubscription> _activeEventSubs = new ConcurrentDictionary<TopicName, EventSubscription>();
        private readonly ConcurrentDictionary<TopicName, CommandSubscription> _activeCommandSubs = new ConcurrentDictionary<TopicName, CommandSubscription>();
        private readonly MessagePublishing _eventPublisher;
        private readonly MessagePublishing _commandPublisher;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IConnection _publishConnection;
        private readonly IConnection _receiveConnection;
        private bool _disposedValue;

        public RabbitMqBroker(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _receiveConnection = _connectionFactory.CreateConnection();
            _publishConnection = _connectionFactory.CreateConnection();
            _eventPublisher = new MessagePublishing(_publishConnection);
            _commandPublisher = new MessagePublishing(_publishConnection);
        }

        public ISubscribable Commands(TopicName topic, ISubscriptionOptions? options)
        {
            return _activeCommandSubs.GetOrAdd(topic, (t) => new CommandSubscription(t, _receiveConnection, options));
        }

        public ISubscribable Events(TopicName topic, EventsOptions options)
        {
            if ((options & EventsOptions.Temporary) == EventsOptions.Temporary)
                return new EventSubscription(topic, _receiveConnection, disposeOnDisconnect: true);

            return _activeEventSubs.GetOrAdd(topic, (t) => new EventSubscription(t, _receiveConnection, disposeOnDisconnect: false));
        }

        public Task PublishEvent<T>(T message, IReadOnlyList<TopicName> topics)
        {
            string exchange = "amq.direct";
            switch (message)
            {
                case ReadOnlyMemory<byte> readOnlyMemory:
                    return PublishReadOnlyMemory(readOnlyMemory, topics, exchange, _eventPublisher);
                case byte[] byteArray:
                    return PublishByteArray(byteArray, topics, exchange, _eventPublisher);
                default:
                    throw new NotSupportedException($"The payload must be of the type byte[] or ReadOnlyMemory<byte>, but got '{typeof(T)}'");
            }
        }

        public Task PublishCommand<T>(T message, IReadOnlyList<TopicName> topics)
        {
            string exchange = string.Empty;
            switch (message)
            {
                case ReadOnlyMemory<byte> readOnlyMemory:
                    return PublishReadOnlyMemory(readOnlyMemory, topics, exchange, _commandPublisher);
                case byte[] byteArray:
                    return PublishByteArray(byteArray, topics, exchange, _commandPublisher);
                default:
                    throw new NotSupportedException($"The payload must be of the type byte[] or ReadOnlyMemory<byte>, but got '{typeof(T)}'");
            }
        }

        private static Task PublishReadOnlyMemory(ReadOnlyMemory<byte> payload, IReadOnlyList<TopicName> topics, string exchange, MessagePublishing publisher)
        {
            if (topics.Count == 1)
                return publisher.PublishReadOnlyMemory(exchange, topics[0], payload);
            
            return Task.WhenAll(
                topics.Select(topic => publisher.PublishReadOnlyMemory(exchange, topic, payload))    
            );
        }

        private static Task PublishByteArray(byte[] payload, IReadOnlyList<TopicName> topics, string exchange, MessagePublishing publisher)
        {
            if (topics.Count == 1)
                return publisher.PublishByteArray(exchange, topics[0], payload);
            
            return Task.WhenAll(
                topics.Select(topic => publisher.PublishByteArray(exchange, topic, payload))    
            );
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var pair in _activeEventSubs.ToArray())
                        pair.Value.Dispose();
                    foreach (var pair in _activeCommandSubs.ToArray())
                        pair.Value.Dispose();
                    _eventPublisher.Dispose();
                    _commandPublisher.Dispose();
                    _publishConnection.Dispose();
                    _receiveConnection.Dispose();
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
