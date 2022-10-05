using System;
using System.Text.Json;

namespace MessageBus.Serialization.Json
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerOptions? _options;

        private static JsonSerializerOptions CreateDefaultOptions()
        {
            JsonSerializerOptions result = new JsonSerializerOptions();
            result.Converters.Add(new MessageIdConverter());
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
            return System.Text.Json.JsonSerializer.Deserialize<T>(data.AsSpan(), _options)!;
        }

        public object DeserializeAnonymous(byte[] data)
        {
            return System.Text.Json.JsonSerializer.Deserialize<object>(data.AsSpan(), _options)!;
        }

        public byte[] Serialize<T>(T message)
        {
            return  System.Text.Json.JsonSerializer.SerializeToUtf8Bytes<T>(message, _options);
        }
    }
}
