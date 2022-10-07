using System.Text.Json.Serialization;

namespace MessageBus.Examples.CrossModulesCommunication.ModuleB
{
    [Topic("Events/SomeStateChanged")]
    internal class StateChangedEvent : IMessageEvent
    {
        [JsonInclude]
        public string LatestState { get; private init; } = null!;

        [JsonInclude]
        public MessageId MessageId { get; private init; }
    }
}
