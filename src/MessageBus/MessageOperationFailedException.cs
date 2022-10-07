using System;
using System.Runtime.Serialization;

namespace MessageBus
{
    [Serializable]
    public class MessageOperationFailedException : Exception
    {
        public MessageOperationFailedException()
        {
        }

        public MessageOperationFailedException(string? message) : base(message)
        {
        }

        public MessageOperationFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public MessageOperationFailedException(IFailureDetails details)
            : this(details.Message)
        {
            HResult = details.StatusCode;
        }

        protected MessageOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IFailureDetails? Details { get; private set; } = null!;
    }
}
