using System;
using System.Diagnostics;

namespace MessageBus.Messaging.Logging
{
    internal readonly struct LoggingSubscribable : ISubscribable
    {
        private readonly ISubscribable _inner;
        private readonly IBrokerLogger _logger;
        private readonly TopicName _topic;

        public LoggingSubscribable(ISubscribable inner, IBrokerLogger logger, TopicName topic)
        {
            _inner = inner;
            _logger = logger;
            _topic = topic;
        }

        public IDisposable Subscribe<T>(Action<IMessage<T>> messageHandler) where T : notnull
        {
            _logger.FormattedLog(_topic, $"Adding typed subscriber with type {typeof(T).FullName}");
            IBrokerLogger logger = _logger;
            TopicName topic = _topic;
            return _inner.Subscribe<T>((data) =>
            {
                string messageType = MessageLogging.GetMessageType(data);
                Stopwatch watch = Stopwatch.StartNew();
                logger.FormattedLog(topic, messageType, "Begin handle message");
                try
                {
                    messageHandler(new LoggingMessage<T>(data, logger, topic));
                    logger.FormattedLog(topic, messageType, $"Completed handle message in {watch.Elapsed}");
                }
                catch (Exception ex)
                {
                    logger.FormattedLog(topic, messageType, $"Exception in handler after {watch.Elapsed}: {ex}");
                    throw;
                }
            });
        }

        public IDisposable Subscribe(Action<IMessage> messageHandler)
        {
            _logger.FormattedLog(_topic, $"Adding subscriber");
            IBrokerLogger logger = _logger;
            TopicName topic = _topic;
            return _inner.Subscribe((data) =>
            {
                string messageType = MessageLogging.GetMessageType(data);
                Stopwatch watch = Stopwatch.StartNew();
                logger.FormattedLog(topic, messageType, "Begin handle message");
                try
                {
                    messageHandler(new LoggingAnonymousMessage(data, logger, topic));
                    logger.FormattedLog(topic, messageType, $"Completed handle message in {watch.Elapsed}");
                }
                catch (Exception ex)
                {
                    logger.FormattedLog(topic, messageType, $"Exception in handler after {watch.Elapsed}: {ex}");
                    throw;
                }
            });
        }

        private readonly struct LoggingAnonymousMessage : IMessage
        {
            private readonly IMessage _source;
            private readonly IBrokerLogger _logger;
            private readonly TopicName _topic;

            public LoggingAnonymousMessage(IMessage source, IBrokerLogger logger, TopicName topic)
            {
                _source = source;
                _logger = logger;
                _topic = topic;
            }

            public object Payload => _source.Payload;

            public MessageState State => _source.State;

            public void Ack()
            {
                _logger.FormattedLog(_topic, MessageLogging.GetMessageType(_source), "Acknowledged");
                _source.Ack();
            }

            public void Nack()
            {
                _logger.FormattedLog(_topic, MessageLogging.GetMessageType(_source), "Not-Acknowledge");
                _source.Nack();
            }
        }

        private readonly struct LoggingMessage<T> : IMessage<T>, IMessageSupportsRejection
        {
            private readonly IMessage<T> _source;
            private readonly IBrokerLogger _logger;
            private readonly TopicName _topic;

            public LoggingMessage(IMessage<T> source, IBrokerLogger logger, TopicName topic)
            {
                _source = source;
                _logger = logger;
                _topic = topic;
            }

            public T Payload => _source.Payload;

            public MessageState State => _source.State;

            public void Ack()
            {
                _logger.FormattedLog(_topic, MessageLogging.GetMessageType(_source), "Acknowledged");
                _source.Ack();
            }

            public void Nack()
            {
                _logger.FormattedLog(_topic, MessageLogging.GetMessageType(_source), "Not-Acknowledge");
                _source.Nack();
            }

            public void Reject()
            {
                (_source as IMessageSupportsRejection)?.Reject();
            }
        }
    }
}
