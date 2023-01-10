using System;

namespace MessageBus.Messaging.InProcess
{
    public interface IEventExecutor
    {
        /// <summary>
        /// Wraps the provided <paramref name="action"/>. The returned action
        /// will get called to perform the <paramref name="action"/>.
        /// </summary>
        Action Wrap(Action action);
    }
}
