using System;
using System.Collections.Concurrent;
using MessageBus.Messaging;

namespace MessageBus
{
    /// <summary>
    /// Uses the provided underlying provider to read the topic names for a given
    /// type. The result is cached. This class is thread safe.
    /// </summary>
    public sealed class TopicNameCache : ITopicNameProvider
    {
        private readonly ITopicNameProvider _inner;
        private readonly ConcurrentDictionary<Type, TopicName> _topicNameCache = new ConcurrentDictionary<Type, TopicName>();

        public TopicNameCache(ITopicNameProvider inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public TopicName GetTopic(Type messageType)
        {
            return _topicNameCache.GetOrAdd(messageType, ReadFromInner);
        }

        private TopicName ReadFromInner(Type messageType)
        {
            return _inner.GetTopic(messageType);
        }
    }
}
