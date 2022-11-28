using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageBus.Messaging.Hooking;
using MessageBus.Messaging.Logging;

namespace MessageBus.Messaging
{
    public static class LoggingWrapperExtensions
    {
        public static IMessageBroker WrapWithLogging(this IMessageBroker broker, IBrokerLogger logger)
        {
            return new HookedHandleMessageBroker(new LoggingBroker(broker, logger), new LoggingHook(logger)); // LoggingMessageBroker(broker, logger);
        }

        private class LoggingBroker : IMessageBroker
        {
            private readonly IBrokerLogger _logger;
            private readonly IMessageBroker _inner;
            private bool _disposedValue;

            public LoggingBroker(IMessageBroker inner, IBrokerLogger logger)
            {
                _inner = inner;
                _logger = logger;
            }

            public ISubscribable Commands(TopicName topic, ISubscriptionOptions? options) => new LogSubscribe(_inner.Commands(topic, options), _logger, topic);

            public ISubscribable Events(TopicName topic, EventsOptions options) => new LogSubscribe(_inner.Events(topic, options), _logger, topic);

            public Task PublishCommand<T>(T message, IReadOnlyList<TopicName> topics)
            {
                _logger.FormattedLog(topics, MessageHumanReadableExtensions.BuildHumanReadableDescriptionFromType(message), $"Publish command with payload: {MessageLogging.TryFormatPayload(message)}");
                return _inner.PublishCommand(message, topics);
            }

            public Task PublishEvent<T>(T message, IReadOnlyList<TopicName> topics)
            {
                _logger.FormattedLog(topics, MessageHumanReadableExtensions.BuildHumanReadableDescriptionFromType(message), $"Publish event with payload: {MessageLogging.TryFormatPayload(message)}");
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

        private readonly struct LogSubscribe : ISubscribable
        {
            private readonly TopicName _topic;
            private readonly IBrokerLogger _logger;
            private readonly ISubscribable _inner;

            public LogSubscribe(ISubscribable inner, IBrokerLogger logger, TopicName topic)
            {
                _inner = inner;
                _logger = logger;
                _topic = topic;
            }

            public IDisposable Subscribe<T>(Action<IMessage<T>> messageHandler) where T : notnull
            {
                _logger.FormattedLog(_topic, $"Adding typed subscriber with type {typeof(T).FullName}");
                return _inner.Subscribe(messageHandler);
            }

            public IDisposable Subscribe(Action<IMessage> messageHandler)
            {
                _logger.FormattedLog(_topic, $"Adding subscriber");
                return _inner.Subscribe(messageHandler);
            }
        }

        private class LoggingHook : IMessageBrokerHandleHook
        {
            private readonly IBrokerLogger _logger;

            public LoggingHook(IBrokerLogger logger)
            {
                _logger = logger;
            }

            public void AfterHandleMessage<T>(HookedMessage<T> message, TimeSpan elapsedTime)
            {
                _logger.FormattedLog(message.Topic, message.MessageDescription, $"Completed handle message in {elapsedTime}");
            }

            public void AfterHandleMessage(HookedMessage message, TimeSpan elapsedTime)
            {
                _logger.FormattedLog(message.Topic, message.MessageDescription, $"Completed handle message in {elapsedTime}");
            }

            public IMessage<T> BeforeHandleMessage<T>(HookedMessage<T> message)
            {
                _logger.FormattedLog(message.Topic, message.MessageDescription, "Begin handle message");
                return new LoggingMessage<T>(message.Message, _logger, message.Topic);
            }

            public IMessage BeforeHandleMessage(HookedMessage message)
            {
                _logger.FormattedLog(message.Topic, message.MessageDescription, "Begin handle message");
                return new LoggingAnonymousMessage(message.Message, _logger, message.Topic);
            }

            public void ExceptionDuringHandle<T>(HookedMessage<T> message, TimeSpan elapsedTime, Exception exception)
            {
                _logger.FormattedLog(message.Topic, message.MessageDescription, $"Exception in handler after {elapsedTime}: {exception}");
            }

            public void ExceptionDuringHandle(HookedMessage message, TimeSpan elapsedTime, Exception exception)
            {
                _logger.FormattedLog(message.Topic, message.MessageDescription, $"Exception in handler after {elapsedTime}: {exception}");
            }
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
                _logger.FormattedLog(_topic, _source.BuildHumanReadableDescription(), "Acknowledged");
                _source.Ack();
            }

            public void Nack()
            {
                _logger.FormattedLog(_topic, _source.BuildHumanReadableDescription(), "Not-Acknowledge");
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
                _logger.FormattedLog(_topic, _source.BuildHumanReadableDescription(), "Acknowledged");
                _source.Ack();
            }

            public void Nack()
            {
                _logger.FormattedLog(_topic, _source.BuildHumanReadableDescription(), "Not-Acknowledge");
                _source.Nack();
            }

            public void Reject()
            {
                (_source as IMessageSupportsRejection)?.Reject();
            }
        }
    }
}
