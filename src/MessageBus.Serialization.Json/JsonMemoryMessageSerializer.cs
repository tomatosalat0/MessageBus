using System;
using System.Text.Json;

namespace MessageBus.Serialization.Json
{
    /// <summary>
    /// Serializes outgoing messages to a ReadOnlyMemory<byte> and deserializes incoming messages
    /// from a ReadOnlyMemory<byte> to a concrete class.
    /// </summary>
    public class JsonMemoryMessageSerializer: SystemTextJsonSerializer, ITypeMessageMemorySerializer
    {
        /// <inheritdoc/>
        public JsonMemoryMessageSerializer() : base()
        {
        }

        /// <inheritdoc/>
        public JsonMemoryMessageSerializer(Action<JsonSerializerOptions> configuration) : base(configuration) 
        {
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> data)
        {
            return JsonSerializer.Deserialize<T>(data.Span, Options)!;
        }

        public object DeserializeAnonymous(ReadOnlyMemory<byte> data)
        {
            return JsonSerializer.Deserialize<object>(data.Span, Options)!;
        }

        public ReadOnlyMemory<byte> Serialize<T>(T message)
        {
            // don't use JsonSerializer.SerializeToUtf8Bytes<T>.
            // System.Text.Json only serializes properties which are defined inside the
            // type itself. If T is an interface and that interface inherits other interfaces,
            // properties from those interfaces would not get serialized.
            return JsonSerializer.SerializeToUtf8Bytes(message, message!.GetType(), Options);
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> data, Type proxyType)
        {
            return (T)JsonSerializer.Deserialize(data.Span, proxyType, Options)!;
        }
    }

    public interface ITypeMessageMemorySerializer : IMessageMemorySerializer
    {
        /// <summary>
        /// Converts the provided <paramref name="data"/> back to an object of <typeparamref name="T"/>.
        /// The provided <paramref name="proxyType"/> is the type which should get used for deserialization.
        /// You have to make sure that <typeparamref name="T"/> can be assigned from an instance of the 
        /// provided <paramref name="proxyType"/>.
        /// </summary>
        internal T Deserialize<T>(ReadOnlyMemory<byte> data, Type proxyType);
    }
}