using System;
using System.Linq;
using System.Reflection;
using MessageBus.Messaging;

namespace MessageBus
{
    /// <summary>
    /// Returns the topic name from the <see cref="TopicAttribute"/>.
    /// </summary>
    public sealed class AttributeTopicNameProvider : ITopicNameProvider
    {
        public TopicName GetTopic(Type messageType)
        {
            return ReadTopicNameFromAttribute(messageType);
        }

        private static TopicName ReadTopicNameFromAttribute(Type type)
        {
            TopicAttribute? attribute = TryReadAttribute<TopicAttribute>(type);
            if (attribute is null)
                throw new IncompleteConfigurationException($"The event '{type.FullName}' doesn't have the required Topic attribute.");

            return attribute.Topic;
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
    }
}
