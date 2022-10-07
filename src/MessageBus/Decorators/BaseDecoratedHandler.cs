using System;

namespace MessageBus.Decorators
{
    internal abstract class BaseDecoratedHandler : ISubscriptionAwareHandler
    {
        private readonly object _inner;

        protected BaseDecoratedHandler(object inner)
        {
            _inner = inner;
        }

        public void RegisterSubscription(IDisposable subscription)
        {
            (_inner as ISubscriptionAwareHandler)?.RegisterSubscription(subscription);
        }
    }
}
