using System;
using System.Text;

namespace MessageBus.Messaging.Logging
{
    internal static class MessageLogging
    {
        public static string GetMessageType<T>(IMessage<T> message)
        {
            return GetPayloadType(message.Payload);
        }

        public static string GetPayloadType<T>(T payload)
        {
            Type type = payload?.GetType() ?? typeof(T);
            return BuildMessageIdentification(type, payload);
        }

        public static string GetMessageType(IMessage message)
        {
            Type? type = message.Payload?.GetType();
            return BuildMessageIdentification(type, message.Payload);
        }

        private static string BuildMessageIdentification(Type? type, object? message)
        {
            string typeName = type?.FullName ?? type?.Name ?? "<unknown type>";
            if (message is IHasMessageId hasMessageId)
                return $"{typeName}::{hasMessageId.MessageId}";
            else
                return typeName;
        }

        public static string TryFormatPayload(object payload)
        {
            if (payload is null)
                return "<null>";

            if (payload is byte[] byteArray)
                return Encoding.UTF8.GetString(byteArray);

            Type type = payload.GetType();
            return type.FullName ?? type.Name;
        }

        public static string TryFormatPayload<T>(T payload)
        {
            if (payload is null)
                return "<null>";

            switch (payload)
            {
                case byte[] byteArray: return Encoding.UTF8.GetString(byteArray);
                case string stringValue: return stringValue;
                case IFormattable formattable: return formattable.ToString() ?? string.Empty;
            }

            Type type = typeof(T);
            return type.FullName ?? type.Name;
        }
    }
}
