using System;
using System.Text.Json;

namespace MessageBus.Serialization.Json
{
    public abstract class SystemTextJsonSerializer
    {
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
        /// Initializes the serializer with the default <see cref="JsonSerializerOptions"/>.
        /// </summary>
        protected SystemTextJsonSerializer()
        {
            Options = CreateDefaultOptions();
        }

        /// <summary>
        /// Initializes the serializer with the possibility to customize the json
        /// configuration within the <paramref name="configuration"/> callback.
        /// </summary>
        protected SystemTextJsonSerializer(Action<JsonSerializerOptions> configuration)
        {
            Options = CreateDefaultOptions();
            configuration(Options);
        }
        
        protected JsonSerializerOptions Options { get; }
        
    }
}