using System;
using System.Text.Json;

namespace MessageBus.Serialization.Json
{
    public class JsonMessageSerializer : ITypeMessageSerializer
    {
        private readonly JsonSerializerOptions? _options;

        private static JsonSerializerOptions CreateDefaultOptions()
        {
            JsonSerializerOptions result = new JsonSerializerOptions();
            result.Converters.Add(new MessageIdConverter());
            result.Converters.Add(FailureDetailsConverter.Create());
            result.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            result.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            return result;
        }

        /// <summary>
        /// Initializes the <see cref="JsonMessageSerializer"/> with the default <see cref="JsonSerializerOptions"/>.
        /// </summary>
        public JsonMessageSerializer()
        {
            _options = CreateDefaultOptions();
        }

        /// <summary>
        /// Initializes the <see cref="JsonMessageSerializer"/> with the possibility to customize the json
        /// configuration within the <paramref name="configuration"/> callback.
        /// </summary>
        public JsonMessageSerializer(Action<JsonSerializerOptions> configuration)
        {
            _options = CreateDefaultOptions();
            configuration(_options);
        }

        public T Deserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data.AsSpan(), _options)!;
        }

        public object DeserializeAnonymous(byte[] data)
        {
            return JsonSerializer.Deserialize<object>(data.AsSpan(), _options)!;
        }

        public byte[] Serialize<T>(T message)
        {
            // don't use JsonSerializer.SerializeToUtf8Bytes<T>.
            // System.Text.Json only serializes properties which are defined inside the
            // type itself. If T is an interface and that interface inherits other interfaces,
            // properties from those interfaces would not get serialized.
            return JsonSerializer.SerializeToUtf8Bytes(message, message!.GetType(), _options);
        }

        public T Deserialize<T>(byte[] data, Type proxyType)
        {
            return (T)JsonSerializer.Deserialize(data.AsSpan(), proxyType, _options)!;
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
