using System.Collections.Generic;

namespace MessageBus
{
    /// <summary>
    /// This class contains details about errors which occured when handling a command/message.
    /// </summary>
    public interface IFailureDetails
    {
        /// <summary>
        /// Defines the failure category.
        /// </summary>
        string Category { get; }

        /// <summary>
        /// Gets the message which describes the failure.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// An error code for the problem which occured.
        /// </summary>
        int StatusCode { get; }

        /// <summary>
        /// Additional data added to the failure details. The content
        /// will vary depending on the error.
        /// </summary>
        IReadOnlyDictionary<string, object?> Details { get; }
    }
}
