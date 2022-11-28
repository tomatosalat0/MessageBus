using MessageBus.Messaging.Hooking;

namespace MessageBus.Messaging
{
    public static class HookingWrapperExtensions
    {
        public static IMessageBroker HookHandling(this IMessageBroker broker, IMessageBrokerHandleHook hook)
        {
            return new HookedHandleMessageBroker(broker, hook);
        }
    }
}
