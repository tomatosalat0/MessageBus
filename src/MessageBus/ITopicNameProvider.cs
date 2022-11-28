using System;
using MessageBus.Messaging;

namespace MessageBus
{
    /// <summary>
    /// Returns the topic name for a given type.
    /// </summary>
    public interface ITopicNameProvider
    {
        /// <summary>
        /// Returns the topic name for the given type.
        /// </summary>
        /// <exception cref="IncompleteConfigurationException">Is thrown if the topic name could not be found.</exception>
        TopicName GetTopic(Type messageType);
    }
}
