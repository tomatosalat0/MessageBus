using MessageBus.Messaging;
using MessageBus.Serialization;

namespace MessageBus
{
    public static class MessageBrokerExtensions
    {
        /// <summary>
        /// Extends the provided <paramref name="broker"/> and to use the provided <paramref name="serializer"/>
        /// to convert messages before getting transported.
        /// </summary>
        public static IMessageBroker UseMessageSerialization(this IMessageBroker broker, IMessageSerializer serializer)
        {
            return new SerializationMessageBroker(broker, serializer);
        }

        /// <summary>
        /// Extends the provided <paramref name="broker"/> and to use the provided <paramref name="serializer"/>
        /// to convert messages before getting transported.
        /// </summary>
        public static IMessageBroker UseMessageSerialization(this IMessageBroker broker, IMessageMemorySerializer serializer)
        {
            return new MemorySerializationMessageBroker(broker, serializer);
        }
    }
}
