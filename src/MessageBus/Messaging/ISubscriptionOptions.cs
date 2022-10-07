using System.Collections.Generic;

namespace MessageBus.Messaging
{
    public interface ISubscriptionOptions
    {
        /// <summary>
        /// Returns a dictionary with broker implementation specific
        /// properties.
        /// </summary>
        IReadOnlyDictionary<string, object> Attributes { get; }
    }
}
