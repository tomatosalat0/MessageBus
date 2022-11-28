using System;
using System.Text;

namespace MessageBus.Messaging.Logging
{
    internal static class MessageLogging
    {
        public static string TryFormatPayload(object payload)
        {
            return TryFormatPayload<object>(payload);
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
