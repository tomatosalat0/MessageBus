using MessageBus.Messaging;
using MessageBus.Messaging.Logging;

namespace MessageBus
{
    public static class LoggingWrapperExtensions
    {
        public static IMessageBroker WrapWithLogging(this IMessageBroker broker, IBrokerLogger logger)
        {
            return new LoggingMessageBroker(broker, logger);
        }
    }
}
