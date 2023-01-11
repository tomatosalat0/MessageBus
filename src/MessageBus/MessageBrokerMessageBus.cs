using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using MessageBus.Messaging;

namespace MessageBus
{
    /// <summary>
    /// A simple event bus which uses <see cref="IMessageBroker"/> as the event transport mechanism.
    /// </summary>
    public sealed partial class MessageBrokerMessageBus : IMessageBus, IDisposable
    {
        private readonly ITopicNameProvider _topicNameProvider;
        private readonly ConcurrentDictionary<Type, ISubscriptionOptions?> _topicOptionsCache = new ConcurrentDictionary<Type, ISubscriptionOptions?>();
        private readonly IExceptionNotification _exceptionNotification;
        private readonly IMessageBroker _broker;
        private bool _disposedValue;

        public MessageBrokerMessageBus(IMessageBroker broker, IExceptionNotification exceptionNotification)
            : this(broker, exceptionNotification, new TopicNameCache(new AttributeTopicNameProvider()))
        {
        }

        public MessageBrokerMessageBus(IMessageBroker broker, IExceptionNotification exceptionNotification, ITopicNameProvider topicNameProvider)
        {
            _broker = broker;
            _exceptionNotification = exceptionNotification ?? throw new ArgumentNullException(nameof(exceptionNotification));
            _topicNameProvider = topicNameProvider ?? throw new ArgumentNullException(nameof(topicNameProvider));
        }

        private TopicName GetSuccessOutcomeTopicName<TType>(TType request)
            where TType : IHasMessageId
        {
            TopicName commandTopic = GetTopicNameFromType(typeof(TType));
            return new TopicName($"{commandTopic}.Outcome.Success.{request.MessageId}");
        }

        private TopicName GetFailureOutcomeTopicName<TType>(TType request)
            where TType : IHasMessageId
        {
            TopicName commandTopic = GetTopicNameFromType(typeof(TType));
            return new TopicName($"{commandTopic}.Outcome.Failure.{request.MessageId}");
        }

        private void ThrowDisposed()
        {
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(MessageBrokerMessageBus));
        }

        private TopicName GetTopicNameFromType(Type type)
        {
            return _topicNameProvider.GetTopic(type);
        }

        private ISubscriptionOptions? GetTopicOptionsFromType(Type type)
        {
            return _topicOptionsCache.GetOrAdd(type, ReadTopicOptionsFromAttribute);
        }

        private static ISubscriptionOptions? ReadTopicOptionsFromAttribute(Type type)
        {
            TopicOptionsAttribute? attribute = TryReadAttribute<TopicOptionsAttribute>(type);
            if (attribute is null || attribute.Type is null)
                return null;

            MethodInfo? optionsProvider = attribute.Type.GetMethod("GetTopicOptions", BindingFlags.Static | BindingFlags.Public, Array.Empty<Type>());
            if (optionsProvider is null)
                return null;

            return optionsProvider.Invoke(null, null) as ISubscriptionOptions;
        }

        private static TAttribute? TryReadAttribute<TAttribute>(Type typeToSearchIn) where TAttribute : Attribute
        {
            TAttribute? result = typeToSearchIn.GetCustomAttribute<TAttribute>(inherit: true);
            if (result is not null)
                return result;

            return typeToSearchIn.GetInterfaces()
                .Select(p => p.GetCustomAttribute<TAttribute>())
                .Where(p => p is not null)
                .FirstOrDefault();
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _broker.Dispose();
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
