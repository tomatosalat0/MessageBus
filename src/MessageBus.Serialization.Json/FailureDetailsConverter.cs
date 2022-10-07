using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MessageBus.Serialization.Json.DynamicTypes;

namespace MessageBus.Serialization.Json
{
    public class FailureDetailsConverter : JsonConverter<IFailureDetails>
    {
        private readonly Type _implementation;

        internal FailureDetailsConverter(IIntermediateInterfaceImplementer interfaceImplementer)
        {
            _implementation = interfaceImplementer.CreateType(typeof(IFailureDetails));
        }

        public static FailureDetailsConverter Create()
        {
            return new FailureDetailsConverter(new RuntimeTypeCreator(new TypeCreationOptions()));
        }

        public override IFailureDetails? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(ref reader, _implementation, options) as IFailureDetails; 
        }

        public override void Write(Utf8JsonWriter writer, IFailureDetails value, JsonSerializerOptions options)
        {
            JsonSerializerOptions derivedOptions = new JsonSerializerOptions();
            foreach (var converter in options.Converters)
                if (converter is not FailureDetailsConverter)
                    derivedOptions.Converters.Add(converter);
            derivedOptions.DefaultIgnoreCondition = options.DefaultIgnoreCondition;
            derivedOptions.PropertyNamingPolicy = options.PropertyNamingPolicy;
            derivedOptions.DictionaryKeyPolicy = options.DictionaryKeyPolicy;
            JsonSerializer.Serialize<IFailureDetails>(writer, value, derivedOptions);
        }
    }
}
