﻿using System;
using System.Text.Json;

namespace MessageBus.Serialization.Json
{
    /// <summary>
    /// Serializes outgoing messages to a byte array and deserializes incoming messages
    /// from a byte array to a concrete class.
    /// </summary>
    public class JsonMessageSerializer : SystemTextJsonSerializer, ITypeMessageSerializer
    {
        /// <inheritdoc/>
        public JsonMessageSerializer() : base()
        {
        }

        /// <inheritdoc/>
        public JsonMessageSerializer(Action<JsonSerializerOptions> configuration) : base(configuration) 
        {
        }

        public T Deserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data.AsSpan(), Options)!;
        }

        public object DeserializeAnonymous(byte[] data)
        {
            return JsonSerializer.Deserialize<object>(data.AsSpan(), Options)!;
        }

        public byte[] Serialize<T>(T message)
        {
            // don't use JsonSerializer.SerializeToUtf8Bytes<T>.
            // System.Text.Json only serializes properties which are defined inside the
            // type itself. If T is an interface and that interface inherits other interfaces,
            // properties from those interfaces would not get serialized.
            return JsonSerializer.SerializeToUtf8Bytes(message, message!.GetType(), Options);
        }

        public T Deserialize<T>(byte[] data, Type proxyType)
        {
            return (T)JsonSerializer.Deserialize(data.AsSpan(), proxyType, Options)!;
        }
    }

    public interface ITypeMessageSerializer : IMessageSerializer
    {
        /// <summary>
        /// Converts the provided <paramref name="data"/> back to an object of <typeparamref name="T"/>.
        /// The provided <paramref name="proxyType"/> is the type which should get used for deserialization.
        /// You have to make sure that <typeparamref name="T"/> can be assigned from an instance of the 
        /// provided <paramref name="proxyType"/>.
        /// </summary>
        internal T Deserialize<T>(byte[] data, Type proxyType);
    }
}
