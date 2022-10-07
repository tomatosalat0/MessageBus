using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBus.Messaging.Logging
{
    internal class LoggingMessageBroker : IMessageBroker
    {
        private readonly IMessageBroker _inner;
        private readonly IBrokerLogger _logger;
        private bool _disposedValue;

        public LoggingMessageBroker(IMessageBroker inner, IBrokerLogger logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ISubscribable Commands(TopicName topic, ISubscriptionOptions? options)
        {
            return new LoggingSubscribable(_inner.Commands(topic, options), _logger, topic);
        }

        public ISubscribable Events(TopicName topic, EventsOptions options)
        {
            return new LoggingSubscribable(_inner.Events(topic, options), _logger, topic);
        }

        public Task PublishEvent<T>(T message, IReadOnlyList<TopicName> topics)
        {
            _logger.FormattedLog(topics, MessageLogging.GetPayloadType(message), $"Publish event with payload: {MessageLogging.TryFormatPayload(message)}");
            return _inner.PublishEvent(message, topics);
        }

        public Task PublishCommand<T>(T message, IReadOnlyList<TopicName> topics)
        {
            _logger.FormattedLog(topics, MessageLogging.GetPayloadType(message), $"Publish command with payload: {MessageLogging.TryFormatPayload(message)}");
            return _inner.PublishCommand(message, topics);
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
