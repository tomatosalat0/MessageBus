using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace MessageBus
{
    /// <summary>
    /// Throw this exception within a command/query/rpc handler if you want to indicate that the handler
    /// itself is currently not able to process the incoming message.
    /// If this exception gets thrown, the message will get rescheduled.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class HandlerUnavailableException : Exception
    {
        public HandlerUnavailableException()
        {
        }

        public HandlerUnavailableException(string? message) : base(message)
        {
        }

        public HandlerUnavailableException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected HandlerUnavailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
