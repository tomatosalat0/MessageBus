using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageBus.Messaging;

namespace MessageBus.Serialization
{
    public class MemorySerializationMessageBroker: IMessageBroker
    {
        private readonly IMessageBroker _inner;
        private readonly IMessageMemorySerializer _serializer;
        private bool _disposedValue;

        public MemorySerializationMessageBroker(IMessageBroker inner, IMessageMemorySerializer serializer)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public ISubscribable Commands(TopicName topic, ISubscriptionOptions? options)
        {
            return new MemorySerializableSubscribable(_inner.Commands(topic, options), _serializer);
        }

        public ISubscribable Events(TopicName topic, EventsOptions options)
        {
            return new MemorySerializableSubscribable(_inner.Events(topic, options), _serializer);
        }

        public Task PublishEvent<T>(T message, IReadOnlyList<TopicName> topics)
        {
            return _inner.PublishEvent(_serializer.Serialize(message), topics);
        }

        public Task PublishCommand<T>(T message, IReadOnlyList<TopicName> topics)
        {
            return _inner.PublishCommand(_serializer.Serialize(message), topics);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _inner.Dispose();
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