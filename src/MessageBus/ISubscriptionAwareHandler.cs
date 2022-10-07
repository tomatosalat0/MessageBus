using System;

namespace MessageBus
{
    /// <summary>
    /// If a handler, which gets passed to any of the <see cref="IMessageBusHandler"/>
    /// register methods, implements this interface, the created subscription
    /// <see cref="IDisposable"/> will get passed to that handler by calling the
    /// <see cref="RegisterSubscription(IDisposable)"/> method. That way the handler
    /// can unregister itself from the message bus in case it gets disposed.
    /// </summary>
    public interface ISubscriptionAwareHandler
    {
        void RegisterSubscription(IDisposable subscription);
    }
}
