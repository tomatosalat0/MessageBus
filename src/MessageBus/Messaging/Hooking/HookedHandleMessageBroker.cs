using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Messaging.Hooking
{
    internal class HookedHandleMessageBroker : IMessageBroker
    {
        private readonly IMessageBroker _inner;
        private readonly IMessageBrokerHandleHook _hook;
        private bool _disposedValue;

        public HookedHandleMessageBroker(IMessageBroker inner, IMessageBrokerHandleHook hook)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _hook = hook ?? throw new ArgumentNullException(nameof(hook));
        }

        public ISubscribable Commands(TopicName topic, ISubscriptionOptions? options)
        {
            return new HookedSubscribable(_inner.Commands(topic, options), topic, _hook);
        }

        public ISubscribable Events(TopicName topic, EventsOptions options)
        {
            return new HookedSubscribable(_inner.Events(topic, options), topic, _hook);
        }

        public Task PublishCommand<T>(T message, IReadOnlyList<TopicName> topics)
        {
            return _inner.PublishCommand(message, topics);
        }

        public Task PublishEvent<T>(T message, IReadOnlyList<TopicName> topics)
        {
            return _inner.PublishEvent(message, topics);
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
