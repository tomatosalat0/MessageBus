namespace MessageBus.Examples.SharedMessageInterface.Common
{
    [Topic("Events/MyEvent")]
    public interface IMyEvent : IMessageEvent
    {
        string FiredOn { get; }
    }
}
