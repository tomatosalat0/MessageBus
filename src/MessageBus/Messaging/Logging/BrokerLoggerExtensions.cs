using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Messaging.Logging
{
    internal static class BrokerLoggerExtensions
    {
        public static void FormattedLog(this IBrokerLogger logger, TopicName topic, string message)
        {
            logger.Log($"[{topic}]: {message}");
        }

        public static void FormattedLog(this IBrokerLogger logger, IReadOnlyList<TopicName> topics, string message)
        {
            if (topics.Count == 1)
                logger.FormattedLog(topics[0], message);
            else
                logger.Log($"[{ToSingleString(topics)}]: {message}");
        }

        public static void FormattedLog(this IBrokerLogger logger, TopicName topic, string messageType, string message)
        {
            logger.Log($"[{topic}] type {{{messageType}}}: {message}");
        }

        public static void FormattedLog(this IBrokerLogger logger, IReadOnlyList<TopicName> topics, string messageType, string message)
        {
            if (topics.Count == 1)
                logger.FormattedLog(topics[0], messageType, message);
            else
                logger.Log($"[{ToSingleString(topics)}] type {{{messageType}}}: {message}");
        }

        private static string ToSingleString(IReadOnlyList<TopicName> topics)
        {
            return string.Join('|', topics.Select(p => p.ToString()));
        }
    }
}
