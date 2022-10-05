using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageBus.Serialization.Json
{
    public class MessageIdConverter : JsonConverter<MessageId>
    {
        public override MessageId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return MessageId.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, MessageId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
