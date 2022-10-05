using System.Text.Json.Serialization;

namespace MessageBus.Examples.CrossModulesCommunication.ModuleA
{
    [Topic("Events/SomeStateChanged")]
    internal class SomeStateChangedEvent : IMessageEvent
    {
        public SomeStateChangedEvent(string latestState)
        {
            LatestState = latestState;
        }

        [JsonInclude]
        public string LatestState { get; }

        [JsonInclude]
        public MessageId MessageId { get; private init; } = MessageId.NewId();
    }
}
