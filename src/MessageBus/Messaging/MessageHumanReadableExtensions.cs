using System;

namespace MessageBus.Messaging
{
    internal static class MessageHumanReadableExtensions
    {
        public static string BuildHumanReadableDescription<T>(this IMessage<T> message)
        {
            return BuildHumanReadableDescriptionFromType(message.Payload);
        }

        public static string BuildHumanReadableDescription(this IMessage message)
        {
            Type? type = message.Payload?.GetType();
            return BuildMessageIdentification(type, message.Payload);
        }

        public static string BuildHumanReadableDescriptionFromType<T>(T payload)
        {
            Type type = payload?.GetType() ?? typeof(T);
            return BuildMessageIdentification(type, payload);
        }

        private static string BuildMessageIdentification(Type? type, object? message)
        {
            string typeName = type?.FullName ?? type?.Name ?? "<unknown type>";
            if (message is IHasMessageId hasMessageId)
                return $"{typeName}::{hasMessageId.MessageId}";
            else
                return typeName;
        }
    }
}
