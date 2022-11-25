using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace MessageBus.Decorations.Validations
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class MessageValidationException : Exception
    {
        public MessageValidationException()
        {
        }

        public MessageValidationException(string? message) : base(message)
        {
        }

        public MessageValidationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MessageValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
