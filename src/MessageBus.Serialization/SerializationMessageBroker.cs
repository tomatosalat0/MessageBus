using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageBus.Messaging;

namespace MessageBus.Serialization
{
    public class SerializationMessageBroker : IMessageBroker
    {
        private readonly IMessageBroker _inner;
        private readonly IMessageSerializer _serializer;
        private bool _disposedValue;

        public SerializationMessageBroker(IMessageBroker inner, IMessageSerializer serializer)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public ISubscribable Commands(TopicName topic)
        {
            return new SerializeableSubscribable(_inner.Commands(topic), _serializer);
        }

        public ISubscribable Events(TopicName topic, EventsOptions options)
        {
            return new SerializeableSubscribable(_inner.Events(topic, options), _serializer);
        }

        public Task Publish<T>(T message, IReadOnlyList<TopicName> topics)
        {
            return _inner.Publish(_serializer.Serialize(message), topics);
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
