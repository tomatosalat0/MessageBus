using System;

namespace MessageBus.Decorators
{
    public abstract class BaseDecoratedHandler : ISubscriptionAwareHandler
    {
        private readonly object _inner;

        internal protected BaseDecoratedHandler(object inner)
        {
            _inner = inner;
        }

        public void RegisterSubscription(IDisposable subscription)
        {
            (_inner as ISubscriptionAwareHandler)?.RegisterSubscription(subscription);
        }
    }
}
